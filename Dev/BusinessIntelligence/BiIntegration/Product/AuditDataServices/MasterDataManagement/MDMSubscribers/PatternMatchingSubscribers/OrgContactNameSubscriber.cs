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
	public class OrgContactNameSubscriber : OrgContactSubscriber
	{
		public override string Code => "OCN";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Org Contact Name Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { OrgContactSchema.OC_ContactName };

		protected override ICollection<string> ColumnsToHash => new[] { OrgContactSchema.Constants.OC_ContactName };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL Value Comparison")]
		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgContactSchema.OC_ContactName.Name] == DBNull.Value
				|| Convert.ToString(row[OrgContactSchema.OC_ContactName.Name]).Length <= 0
				|| Convert.ToString(row[OrgContactSchema.OC_ContactName.Name]) == "DUMMY CONTACT TO SUPPRESS DOCS")
			{
				row.Delete();
			}
		};

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingName> subscriberUtilitiesName = new PersonPatternMatchingSubscriberUtilities<PatternMatchingName>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesName.CreatePatternMatchingBusinessObject(factory, BizO.OC_PER, hashedValue, OrgContactSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesName.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var contactName = changeRow[changeRowField, rowVersion].ToStringSafe();
			var valuesToHash = new List<string>();

			if (!TextStandardizerHelper.IsPlaceholderPersonName(contactName) && contactName.Trim().Contains(" "))
			{
				var standardizeFullName = TextStandardizerHelper.StandardizePersonName(contactName);
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
				changeRow[OrgContactSchema.Constants.PK].ToString(),
				changeRow[OrgContactSchema.Constants.OC_ContactName].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgContactNameSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesName.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue, originalHashedValue, BizO.OC_PER, OrgContactSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var orgQueued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO.Header, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.Header.PK, orgQueued);

			if (BizO.Person != null)
			{
				var personQueued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO.Person, factory);
				yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.Person.PK, personQueued);
			}
		}
	}
}
