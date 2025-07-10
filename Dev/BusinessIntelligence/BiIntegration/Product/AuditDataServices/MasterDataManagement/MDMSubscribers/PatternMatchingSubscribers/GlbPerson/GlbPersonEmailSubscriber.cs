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
	public class GlbPersonEmailSubscriber : GlbPersonSubscriber
	{
		public override string Code => "GPE";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbPerson Email Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbPersonSchema.PER_EmailAddress, GlbPersonSchema.PER_EmailAddress2 };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if ((row[GlbPersonSchema.PER_EmailAddress.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_EmailAddress.Name]).Length <= 0)
				&& (row[GlbPersonSchema.PER_EmailAddress2.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_EmailAddress2.Name]).Length <= 0))
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbPersonSchema.Constants.PER_EmailAddress, GlbPersonSchema.Constants.PER_EmailAddress2 };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingEmail> subscriberUtilitiesEmail = new PersonPatternMatchingSubscriberUtilities<PatternMatchingEmail>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesEmail.CreatePatternMatchingBusinessObject(factory, BizO.PK, hashedValue, GlbPersonSchema.Constants.Prefix, PatternMatchingEmailSchema.PME_ParentId, BizO.PK, BizO.PER_RN_NKCountry);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesEmail.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue, originalHashedValue, BizO.PK, GlbPersonSchema.Constants.Prefix, BizO.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesEmail.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var emailAddress = changeRow[changeRowField, rowVersion].ToStringSafe();
			var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);

			return new List<string> { emailToHash };
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3}",
				changeRow.RowState.ToString(),
				changeRow[GlbPersonSchema.Constants.PK].ToString(),
				changeRow[GlbPersonSchema.Constants.PER_EmailAddress].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_EmailAddress2].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbPersonEmailSubscriber);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO, factory);
			yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.PK, queued);
		}
	}
}
