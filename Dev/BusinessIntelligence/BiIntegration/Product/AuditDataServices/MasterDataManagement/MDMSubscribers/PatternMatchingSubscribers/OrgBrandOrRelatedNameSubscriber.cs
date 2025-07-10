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
	public class OrgBrandOrRelatedNameSubscriber : PatternMatchingSubscriber<OrgBrandOrRelatedName>
	{
		protected override string PKColumn
		{
			get { return OrgBrandOrRelatedNameSchema.Constants.PK; }
		}

		public override string Code
		{
			get { return "OBR"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Brand Or Related Name Subscriber"; }
		}

		public override ITableSchema Table
		{
			get { return OrgBrandOrRelatedNameSchema.Instance; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				return new SchemaColumn[] { OrgBrandOrRelatedNameSchema.P1_RelatedName };
			}
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgBrandOrRelatedNameSchema.Constants.P1_RelatedName };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgBrandOrRelatedNameSchema.P1_RelatedName.Name] == DBNull.Value || Convert.ToString(row[OrgBrandOrRelatedNameSchema.P1_RelatedName.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var orgBrandOrRelatedNameRelatedName = changeRow[changeRowField, rowVersion].ToStringSafe();
			return new List<string> { TextStandardizerHelper.StandardizeCompanyName(orgBrandOrRelatedNameRelatedName, BizO.Header.CountryCode) };
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[OrgBrandOrRelatedNameSchema.Constants.PK].ToString(),
				changeRow[OrgBrandOrRelatedNameSchema.Constants.P1_RelatedName].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgBrandOrRelatedNameSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesName = new OrgPatternMatchingSubscriberUtilities<PatternMatchingName>();
			return subscriberUtilitiesName.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgBrandOrRelatedNameSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, BizO.PK, BizO.Header.CountryCode);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var subscriberUtilitiesName = new OrgPatternMatchingSubscriberUtilities<PatternMatchingName>();
			return subscriberUtilitiesName.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgBrandOrRelatedNameSchema.Constants.Prefix, BizO.Header.CountryCode);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesName = new OrgPatternMatchingSubscriberUtilities<PatternMatchingName>();
			return subscriberUtilitiesName.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO.Header, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.Header.PK, queued);
		}
	}
}
