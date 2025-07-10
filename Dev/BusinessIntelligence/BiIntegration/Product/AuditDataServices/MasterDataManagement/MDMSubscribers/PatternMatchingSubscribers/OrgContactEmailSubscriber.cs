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
	public class OrgContactEmailSubscriber : OrgContactSubscriber
	{
		public override string Code
		{
			get { return "OCE"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Contact Email Subscriber"; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				return new SchemaColumn[] { OrgContactSchema.OC_Email };
			}
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgContactSchema.Constants.OC_Email };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgContactSchema.OC_Email.Name] == DBNull.Value || Convert.ToString(row[OrgContactSchema.OC_Email.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			var orgContactEmail = changeRow[changeRowField, rowVersion].ToStringSafe();
			var domain = TextStandardizerHelper.ExtractEmailDomain(orgContactEmail);

			var domainToHash = !TextStandardizerHelper.IsGenericDomain(domain) ? domain : string.Empty;
			valuesToHash.Add(domainToHash);

			var emailToHash = TextStandardizerHelper.StandardizeEmail(orgContactEmail);

			valuesToHash.Add(emailToHash);

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[OrgContactSchema.Constants.PK].ToString(),
				changeRow[OrgContactSchema.Constants.OC_Email].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgContactEmailSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			if (index == 0)
			{
				var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
				return subscriberUtilitiesDomain.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgContactSchema.Constants.Prefix, PatternMatchingDomainSchema.PMD_ParentId, BizO.PK, BizO.Header.CountryCode);
			}
			else if (index == 1)
			{
				var subscriberUtilitiesEmail = new OrgContactPatternMatchingSubscriberUtilities<PatternMatchingEmail>(BizO.OC_PER);
				return subscriberUtilitiesEmail.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgContactSchema.Constants.Prefix, PatternMatchingEmailSchema.PME_ParentId, BizO.PK, BizO.Header.CountryCode);
			}

			return false;
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			if (index == 0)
			{
				var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
				return subscriberUtilitiesDomain.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingDomainSchema.PMD_ParentId, PatternMatchingDomainSchema.PMD_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgContactSchema.Constants.Prefix, BizO.Header.CountryCode);
			}
			else if (index == 1)
			{
				var subscriberUtilitiesEmail = new OrgContactPatternMatchingSubscriberUtilities<PatternMatchingEmail>(BizO.OC_PER);
				return subscriberUtilitiesEmail.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgContactSchema.Constants.Prefix, BizO.Header.CountryCode);
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
				var subscriberUtilitiesEmail = new OrgContactPatternMatchingSubscriberUtilities<PatternMatchingEmail>(BizO.OC_PER);
				return subscriberUtilitiesEmail.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_HashedValue, hashedValue);
			}

			return false;
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
