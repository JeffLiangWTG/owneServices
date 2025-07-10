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
	public class OrgWebURLSubscriber : PatternMatchingSubscriber<OrgWebURL>
	{
		protected override string PKColumn
		{
			get { return OrgWebURLSchema.Constants.PK; }
		}

		public override string Code
		{
			get { return "OWU"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Web URL Subscriber"; }
		}

		public override ITableSchema Table
		{
			get { return OrgWebURLSchema.Instance; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				return new SchemaColumn[] { OrgWebURLSchema.PU_URL };
			}
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgWebURLSchema.Constants.PU_URL };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgWebURLSchema.PU_URL.Name] == DBNull.Value
				|| Convert.ToString(row[OrgWebURLSchema.PU_URL.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();
			var url = changeRow[changeRowField, rowVersion].ToStringSafe();
			var domainToHash = TextStandardizerHelper.StandardizeUrlDomain(url);

			valuesToHash.Add(domainToHash);

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[OrgWebURLSchema.Constants.PK].ToString(),
				changeRow[OrgWebURLSchema.Constants.PU_URL].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgWebURLSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			if (index == 0)
			{
				var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
				return subscriberUtilitiesDomain.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgWebURLSchema.Constants.Prefix, PatternMatchingDomainSchema.PMD_ParentId, BizO.PK, BizO.Header.CountryCode);
			}

			return false;
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			if (index == 0)
			{
				var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
				return subscriberUtilitiesDomain.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingDomainSchema.PMD_ParentId, PatternMatchingDomainSchema.PMD_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgWebURLSchema.Constants.Prefix, BizO.Header.CountryCode);
			}

			return false;
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			if (index == 0)
			{
				var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
				return subscriberUtilitiesDomain.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingDomainSchema.PMD_ParentId, PatternMatchingDomainSchema.PMD_HashedValue, hashedValue);
			}

			return false;
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO.Header, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.Header.PK, queued);
		}
	}
}
