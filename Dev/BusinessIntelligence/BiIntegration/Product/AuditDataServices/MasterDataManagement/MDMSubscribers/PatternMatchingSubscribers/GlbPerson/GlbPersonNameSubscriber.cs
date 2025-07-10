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
	public class GlbPersonNameSubscriber : GlbPersonSubscriber
	{
		public override string Code => "GPN";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbPerson Name Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbPersonSchema.PER_FullName };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL value comparison")]
		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GlbPersonSchema.PER_FullName.Name] == DBNull.Value
				|| Convert.ToString(row[GlbPersonSchema.PER_FullName.Name]).Length <= 0
				|| Convert.ToString(row[GlbPersonSchema.PER_FullName.Name]) == "DUMMY CONTACT TO SUPPRESS DOCS")
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbPersonSchema.Constants.PER_FullName };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingName> subscriberUtilitiesName = new PersonPatternMatchingSubscriberUtilities<PatternMatchingName>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesName.CreatePatternMatchingBusinessObject(factory, BizO.PK, hashedValue, GlbPersonSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, BizO.PK, BizO.PER_RN_NKCountry);
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
				changeRow[GlbPersonSchema.Constants.PK].ToString(),
				changeRow[GlbPersonSchema.Constants.PER_FullName].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbPersonNameSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesName.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue, originalHashedValue, BizO.PK, GlbPersonSchema.Constants.Prefix, BizO.PER_RN_NKCountry);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO, factory);
			yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.PK, queued);
		}
	}
}
