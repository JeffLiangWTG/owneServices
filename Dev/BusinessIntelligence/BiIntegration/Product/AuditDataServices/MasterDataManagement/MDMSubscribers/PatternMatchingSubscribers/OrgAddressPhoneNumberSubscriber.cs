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
	public class OrgAddressPhoneNumberSubscriber : OrgAddressSubscriber
	{
		public override string Code
		{
			get { return "OAP"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Address Phone Number Subscriber"; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get { return new SchemaColumn[] { OrgAddressSchema.OA_Phone, OrgAddressSchema.OA_Fax, OrgAddressSchema.OA_Mobile }; }
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgAddressSchema.Constants.OA_Phone, OrgAddressSchema.Constants.OA_Fax, OrgAddressSchema.Constants.OA_Mobile };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if ((row[OrgAddressSchema.OA_Phone.Name] == DBNull.Value || Convert.ToString(row[OrgAddressSchema.OA_Phone.Name]).Length <= 0)
				&& (row[OrgAddressSchema.OA_Fax.Name] == DBNull.Value || Convert.ToString(row[OrgAddressSchema.OA_Fax.Name]).Length <= 0)
				&& (row[OrgAddressSchema.OA_Mobile.Name] == DBNull.Value || Convert.ToString(row[OrgAddressSchema.OA_Mobile.Name]).Length <= 0))
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();
			var orgPhone = changeRow[changeRowField, rowVersion].ToStringSafe();

			var phoneToHash = TextStandardizerHelper.StandardizePhone(orgPhone);

			valuesToHash.Add(phoneToHash);

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3} - {4}",
				changeRow.RowState.ToString(),
				changeRow[OrgAddressSchema.Constants.PK].ToString(),
				changeRow[OrgAddressSchema.Constants.OA_Phone].ToString().Trim(),
				changeRow[OrgAddressSchema.Constants.OA_Fax].ToString().Trim(),
				changeRow[OrgAddressSchema.Constants.OA_Mobile].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgAddressPhoneNumberSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesPhone = new OrgPatternMatchingSubscriberUtilities<PatternMatchingPhone>();
			return subscriberUtilitiesPhone.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgAddressSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK, BizO.Header.CountryCode);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var subscriberUtilitiesPhone = new OrgPatternMatchingSubscriberUtilities<PatternMatchingPhone>();
			return subscriberUtilitiesPhone.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgAddressSchema.Constants.Prefix, BizO.Header.CountryCode);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesPhone = new OrgPatternMatchingSubscriberUtilities<PatternMatchingPhone>();
			return subscriberUtilitiesPhone.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO.Header, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.Header.PK, queued);
		}
	}
}
