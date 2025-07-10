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
	public class OrgContactPhoneNumberSubscriber : OrgContactSubscriber
	{
		public override string Code
		{
			get { return "OCP"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Contact Phone Number Subscriber"; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				return new SchemaColumn[] { OrgContactSchema.OC_Phone, OrgContactSchema.OC_HomePhone, OrgContactSchema.OC_Mobile, OrgContactSchema.OC_OtherPhone, OrgContactSchema.OC_Fax };
			}
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgContactSchema.Constants.OC_Phone, OrgContactSchema.Constants.OC_HomePhone, OrgContactSchema.Constants.OC_Mobile, OrgContactSchema.Constants.OC_OtherPhone, OrgContactSchema.Constants.OC_Fax };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if ((row[OrgContactSchema.OC_Phone.Name] == DBNull.Value ||  Convert.ToString(row[OrgContactSchema.OC_Phone.Name]).Length <= 0)
				&& (row[OrgContactSchema.OC_HomePhone.Name] == DBNull.Value || Convert.ToString(row[OrgContactSchema.OC_HomePhone.Name]).Length <= 0)
				&& (row[OrgContactSchema.OC_Mobile.Name] == DBNull.Value || Convert.ToString(row[OrgContactSchema.OC_Mobile.Name]).Length <= 0)
				&& (row[OrgContactSchema.OC_OtherPhone.Name] == DBNull.Value || Convert.ToString(row[OrgContactSchema.OC_OtherPhone.Name]).Length <= 0)
				&& (row[OrgContactSchema.OC_Fax.Name] == DBNull.Value || Convert.ToString(row[OrgContactSchema.OC_Fax.Name]).Length <= 0))
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();
			var phoneNumber = changeRow[changeRowField, rowVersion].ToStringSafe();

			var phoneToHash = TextStandardizerHelper.StandardizePhone(phoneNumber);

			valuesToHash.Add(phoneToHash);

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3} - {4} - {5} - {6}",
				changeRow.RowState.ToString(),
				changeRow[OrgContactSchema.Constants.PK].ToString(),
				changeRow[OrgContactSchema.Constants.OC_Phone].ToString().Trim(),
				changeRow[OrgContactSchema.Constants.OC_HomePhone].ToString().Trim(),
				changeRow[OrgContactSchema.Constants.OC_Mobile].ToString().Trim(),
				changeRow[OrgContactSchema.Constants.OC_OtherPhone].ToString().Trim(),
				changeRow[OrgContactSchema.Constants.OC_Fax].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgContactPhoneNumberSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesPhone = new OrgContactPatternMatchingSubscriberUtilities<PatternMatchingPhone>(BizO.OC_PER);
			return subscriberUtilitiesPhone.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgContactSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK, BizO.Header.CountryCode);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var subscriberUtilitiesPhone = new OrgContactPatternMatchingSubscriberUtilities<PatternMatchingPhone>(BizO.OC_PER);
			return subscriberUtilitiesPhone.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgContactSchema.Constants.Prefix, BizO.Header.CountryCode);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesPhone = new OrgContactPatternMatchingSubscriberUtilities<PatternMatchingPhone>(BizO.OC_PER);
			return subscriberUtilitiesPhone.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var orgQueued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO.Header, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.Header.PK, orgQueued);

			if (BizO.Person != null)
			{
				var personQueued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO.Person, factory);
				yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.Person.PK, personQueued);
			}
		}
	}
}
