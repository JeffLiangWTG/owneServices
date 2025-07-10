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
	public class HRJobApplicantPhoneNumberSubscriber : HRJobApplicantSubscriber
	{
		public override string Code => "HJP";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "HR Job Applicant Phone Number Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { HRJobApplicantSchema.HA_WorkPhone };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[HRJobApplicantSchema.HA_WorkPhone.Name] == DBNull.Value || Convert.ToString(row[HRJobApplicantSchema.HA_WorkPhone.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { HRJobApplicantSchema.HA_WorkPhone.Name };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone> subscriberUtilitiesPhone = new PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesPhone.CreatePatternMatchingBusinessObject(factory, BizO.Person.PK, hashedValue, HRJobApplicantSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
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
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[HRJobApplicantSchema.Constants.PK].ToString(),
				changeRow[HRJobApplicantSchema.Constants.HA_WorkPhone].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(HRJobApplicantPhoneNumberSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesPhone.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue, originalHashedValue, BizO.Person.PK, HRJobApplicantSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
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
