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
	public class GlbStaffPhoneNumberSubscriber : GlbStaffSubscriber
	{
		public override string Code => "GSP";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Staff Phone Number Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbStaffSchema.GS_FaxNum, GlbStaffSchema.GS_HomePhone, GlbStaffSchema.GS_MobilePhone, GlbStaffSchema.GS_WorkPhone };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if ((row[GlbStaffSchema.GS_FaxNum.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_FaxNum.Name]).Length <= 0)
				&& (row[GlbStaffSchema.GS_HomePhone.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_HomePhone.Name]).Length <= 0)
				&& (row[GlbStaffSchema.GS_MobilePhone.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_MobilePhone.Name]).Length <= 0)
				&& (row[GlbStaffSchema.GS_WorkPhone.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_WorkPhone.Name]).Length <= 0))
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbStaffSchema.GS_FaxNum.Name, GlbStaffSchema.GS_HomePhone.Name, GlbStaffSchema.GS_MobilePhone.Name, GlbStaffSchema.GS_WorkPhone.Name };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone> subscriberUtilitiesPhone = new PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesPhone.CreatePatternMatchingBusinessObject(factory, BizO.GS_PER, hashedValue, GlbStaffSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesPhone.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue);
		}

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
				"{0} - {1} - {2} - {3} - {4} - {5}",
				changeRow.RowState.ToString(),
				changeRow[GlbStaffSchema.Constants.PK].ToString(),
				changeRow[GlbStaffSchema.Constants.GS_FaxNum].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_HomePhone].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_MobilePhone].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_WorkPhone].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbStaffPhoneNumberSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesPhone.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue, originalHashedValue, BizO.GS_PER, GlbStaffSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			if (BizO.Person != null)
			{
				var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO.Person, factory);
				yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.Person.PK, queued);
			}
		}
	}
}
