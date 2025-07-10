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
	public class GlbStaffEmailSubscriber : GlbStaffSubscriber
	{
		public override string Code => "GDE";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Staff Email Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbStaffSchema.GS_EmailAddress };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GlbStaffSchema.GS_EmailAddress.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_EmailAddress.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbStaffSchema.GS_EmailAddress.Name };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingEmail> subscriberUtilitiesEmail = new PersonPatternMatchingSubscriberUtilities<PatternMatchingEmail>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesEmail.CreatePatternMatchingBusinessObject(factory, BizO.GS_PER, hashedValue, GlbStaffSchema.Constants.Prefix, PatternMatchingEmailSchema.PME_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesEmail.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var value = changeRow[changeRowField, rowVersion].ToStringSafe();
			var emailToHash = TextStandardizerHelper.StandardizeEmail(value);
			return new List<string> { emailToHash };
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[GlbStaffSchema.Constants.PK].ToString(),
				changeRow[GlbStaffSchema.Constants.GS_EmailAddress].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbStaffEmailSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesEmail.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue, originalHashedValue, BizO.GS_PER, GlbStaffSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
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
