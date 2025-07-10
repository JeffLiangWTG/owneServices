using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class OrgContactItemSubscriber : PatternMatchingSubscriber<OrgContactItem>
	{
		public override string Code => "OCI";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Org Contact Item Subscriber";

		public override ITableSchema Table => OrgContactItemSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { OrgContactItemSchema.OI_Address };

		protected override string PKColumn => OrgContactItemSchema.Constants.PK;

		protected override ICollection<string> ColumnsToHash => new[] { OrgContactItemSchema.Constants.OI_Address };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgContactItemSchema.OI_Address.Name] == DBNull.Value || Convert.ToString(row[OrgContactItemSchema.OI_Address.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		string contactItemType;

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			contactItemType = changeRow[OrgContactItemSchema.Constants.OI_ContactItemType, rowVersion].ToStringSafe();

			var valuesToHash = new List<string>();
			var rowValue = changeRow[changeRowField, rowVersion].ToStringSafe();

			if (contactItemType == OrgContactItemTypes.Codes.Phone)
			{
				var phoneToHash = TextStandardizerHelper.StandardizePhone(rowValue);
				valuesToHash.Add(phoneToHash);
			}
			else if (contactItemType == OrgContactItemTypes.Codes.Email)
			{
				var emailToHash = TextStandardizerHelper.StandardizeEmail(rowValue);
				valuesToHash.Add(emailToHash);
			}

			return valuesToHash;
		}

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone> subscriberUtilitiesPhone = new PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone>();
		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingEmail> subscriberUtilitiesEmail = new PersonPatternMatchingSubscriberUtilities<PatternMatchingEmail>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var result = false;

			if (contactItemType == OrgContactItemTypes.Codes.Phone)
			{
				result = subscriberUtilitiesPhone.CreatePatternMatchingBusinessObject(factory, BizO.Contact.OC_PER, hashedValue, OrgContactItemSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK, BizO.Contact.Person.PER_RN_NKCountry);
			}
			else if (contactItemType == OrgContactItemTypes.Codes.Email)
			{
				result = subscriberUtilitiesEmail.CreatePatternMatchingBusinessObject(factory, BizO.Contact.OC_PER, hashedValue, OrgContactItemSchema.Constants.Prefix, PatternMatchingEmailSchema.PME_ParentId, BizO.PK, BizO.Contact.Person.PER_RN_NKCountry);
			}

			return result;
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var result = false;

			if (contactItemType == OrgContactItemTypes.Codes.Phone)
			{
				result = subscriberUtilitiesPhone.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue, originalHashedValue, BizO.Contact.OC_PER, OrgContactItemSchema.Constants.Prefix, BizO.Contact.Person.PER_RN_NKCountry);
			}
			else if (contactItemType == OrgContactItemTypes.Codes.Email)
			{
				result = subscriberUtilitiesEmail.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue, originalHashedValue, BizO.Contact.OC_PER, OrgContactItemSchema.Constants.Prefix, BizO.Contact.Person.PER_RN_NKCountry);
			}

			return result;
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var result = false;

			if (contactItemType == OrgContactItemTypes.Codes.Phone)
			{
				result = subscriberUtilitiesPhone.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue);
			}
			else if (contactItemType == OrgContactItemTypes.Codes.Email)
			{
				result = subscriberUtilitiesEmail.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue);
			}

			return result;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3}",
				changeRow.RowState.ToString(),
				changeRow[OrgContactItemSchema.Constants.PK].ToString(),
				changeRow[OrgContactItemSchema.Constants.OI_ContactItemType].ToString().Trim(),
				changeRow[OrgContactItemSchema.Constants.OI_Address].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgContactItemSubscriber);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			if (BizO.Contact.Person != null)
			{
				var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO.Contact.Person, factory);
				yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.Contact.Person.PK, queued);
			}
		}
	}
}
