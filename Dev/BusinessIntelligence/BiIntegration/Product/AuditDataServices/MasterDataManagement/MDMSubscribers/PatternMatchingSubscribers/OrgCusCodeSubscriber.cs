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
	public class OrgCusCodeSubscriber : PatternMatchingSubscriber<OrgCusCode>
	{
		protected override string PKColumn
		{
			get { return OrgCusCodeSchema.Constants.PK; }
		}

		public override string Code
		{
			get { return "OCC"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Cus Code Subscriber"; }
		}

		public override ITableSchema Table
		{
			get { return OrgCusCodeSchema.Instance; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				return new SchemaColumn[] { OrgCusCodeSchema.OK_CustomsRegNo, OrgCusCodeSchema.OK_RN_NKCodeCountry };
			}
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgCusCodeSchema.Constants.OK_CustomsRegNo };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgCusCodeSchema.OK_CustomsRegNo.Name] == DBNull.Value || Convert.ToString(row[OrgCusCodeSchema.OK_CustomsRegNo.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();
			var regCode = changeRow[changeRowField, rowVersion].ToStringSafe();

			var codeToHash = TextStandardizerHelper.StandardizeRegCode(regCode);

			valuesToHash.Add(codeToHash);
			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
					"{0} - {1} - {2}",
					changeRow.RowState.ToString(),
					changeRow[OrgCusCodeSchema.Constants.PK].ToString(),
					changeRow[OrgCusCodeSchema.Constants.OK_CustomsRegNo].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgCusCodeSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesRegCode = new OrgPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();
			return subscriberUtilitiesRegCode.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgCusCodeSchema.Constants.Prefix, PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK, BizO.Header.CountryCode);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var subscriberUtilitiesRegCode = new OrgPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();
			return subscriberUtilitiesRegCode.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgCusCodeSchema.Constants.Prefix, BizO.Header.CountryCode);
		}

		protected override IEnumerable<SchemaColumn> GetCountryCodeColumns
		{
			get
			{
				return new SchemaColumn[] { OrgCusCodeSchema.OK_RN_NKCodeCountry };
			}
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesRegCode = new OrgPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();
			return subscriberUtilitiesRegCode.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO.Header, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.Header.PK, queued);
		}
	}
}
