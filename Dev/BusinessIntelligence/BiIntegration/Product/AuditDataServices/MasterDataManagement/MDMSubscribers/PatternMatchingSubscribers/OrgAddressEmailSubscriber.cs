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
	public class OrgAddressEmailSubscriber : OrgAddressSubscriber
	{
		public override string Code
		{
			get { return "OAE"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Address Email Subscriber"; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get { return new SchemaColumn[] { OrgAddressSchema.OA_Email }; }
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgAddressSchema.Constants.OA_Email };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgAddressSchema.OA_Email.Name] == DBNull.Value || Convert.ToString(row[OrgAddressSchema.OA_Email.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			var emailAddress = changeRow[changeRowField, rowVersion].ToStringSafe();
			var domain = TextStandardizerHelper.ExtractEmailDomain(emailAddress);
			var domainToHash = !TextStandardizerHelper.IsGenericDomain(domain) ? domain : string.Empty;

			valuesToHash.Add(domainToHash);

			var emailToHash = TextStandardizerHelper.StandardizeEmail(emailAddress);

			valuesToHash.Add(emailToHash);

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[OrgAddressSchema.Constants.PK].ToString(),
				changeRow[OrgAddressSchema.Constants.OA_Email].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgAddressEmailSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			if (index == 0)
			{
				var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
				return subscriberUtilitiesDomain.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgAddressSchema.Constants.Prefix, PatternMatchingDomainSchema.PMD_ParentId, BizO.PK, BizO.Header.CountryCode);
			}
			else if (index == 1)
			{
				var subscriberUtilitiesEmail = new OrgPatternMatchingSubscriberUtilities<PatternMatchingEmail>();
				return subscriberUtilitiesEmail.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgAddressSchema.Constants.Prefix, PatternMatchingEmailSchema.PME_ParentId, BizO.PK, BizO.Header.CountryCode);
			}

			return false;
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			if (index == 0)
			{
				var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
				return subscriberUtilitiesDomain.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingDomainSchema.PMD_ParentId, PatternMatchingDomainSchema.PMD_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgAddressSchema.Constants.Prefix, BizO.Header.CountryCode);
			}
			else if (index == 1)
			{
				var subscriberUtilitiesEmail = new OrgPatternMatchingSubscriberUtilities<PatternMatchingEmail>();
				return subscriberUtilitiesEmail.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgAddressSchema.Constants.Prefix, BizO.Header.CountryCode);
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
			else if (index == 1)
			{
				var subscriberUtilitiesEmail = new OrgPatternMatchingSubscriberUtilities<PatternMatchingEmail>();
				return subscriberUtilitiesEmail.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue);
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
