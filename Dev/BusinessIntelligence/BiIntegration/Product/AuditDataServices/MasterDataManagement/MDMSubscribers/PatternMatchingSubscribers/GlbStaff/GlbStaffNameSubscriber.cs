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
	public class GlbStaffNameSubscriber : GlbStaffSubscriber
	{
		public override string Code => "GSN";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Staff Name Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbStaffSchema.GS_FullName };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GlbStaffSchema.GS_FullName.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_FullName.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbStaffSchema.Constants.GS_FullName };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingName> subscriberUtilitiesName = new PersonPatternMatchingSubscriberUtilities<PatternMatchingName>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesName.CreatePatternMatchingBusinessObject(factory, BizO.GS_PER, hashedValue, GlbStaffSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesName.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var fullName = changeRow[changeRowField, rowVersion].ToStringSafe();
			var valuesToHash = new List<string>();

			if (!TextStandardizerHelper.IsPlaceholderPersonName(fullName) && fullName.Trim().Contains(" "))
			{
				var standardizeFullName = TextStandardizerHelper.StandardizePersonName(fullName);
				valuesToHash.Add(standardizeFullName);
			}
			else
			{
				valuesToHash.Add(string.Empty);
			}

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[GlbStaffSchema.Constants.PK].ToString(),
				changeRow[GlbStaffSchema.Constants.GS_FullName].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbStaffNameSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesName.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue, originalHashedValue, BizO.GS_PER, GlbStaffSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
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
