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
	public class GlbPersonPhoneNumberSubscriber : GlbPersonSubscriber
	{
		public override string Code => "GPP";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbPerson Phone Number Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbPersonSchema.PER_FaxNumber, GlbPersonSchema.PER_HomePhone, GlbPersonSchema.PER_MobilePhone, GlbPersonSchema.PER_MobilePhone2 };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if ((row[GlbPersonSchema.PER_FaxNumber.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_FaxNumber.Name]).Length <= 0)
				&& (row[GlbPersonSchema.PER_HomePhone.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_HomePhone.Name]).Length <= 0)
				&& (row[GlbPersonSchema.PER_MobilePhone.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_MobilePhone.Name]).Length <= 0)
				&& (row[GlbPersonSchema.PER_MobilePhone2.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_MobilePhone2.Name]).Length <= 0))
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbPersonSchema.PER_FaxNumber.Name, GlbPersonSchema.PER_HomePhone.Name, GlbPersonSchema.PER_MobilePhone.Name, GlbPersonSchema.PER_MobilePhone2.Name };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone> subscriberUtilitiesPhone = new PersonPatternMatchingSubscriberUtilities<PatternMatchingPhone>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesPhone.CreatePatternMatchingBusinessObject(factory, BizO.PK, hashedValue, GlbPersonSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK, BizO.PER_RN_NKCountry);
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
				changeRow[GlbPersonSchema.Constants.PK].ToString(),
				changeRow[GlbPersonSchema.Constants.PER_FaxNumber].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_HomePhone].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_MobilePhone].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_MobilePhone2].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbPersonPhoneNumberSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesPhone.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_HashedValue, hashedValue, originalHashedValue, BizO.PK, GlbPersonSchema.Constants.Prefix, BizO.PER_RN_NKCountry);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO, factory);
			yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.PK, queued);
		}
	}
}
