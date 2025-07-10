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
	public class HRJobApplicantRegCodeSubscriber : HRJobApplicantSubscriber
	{
		public override string Code => "HJR";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "HR Job Applicant Reg Code Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { HRJobApplicantSchema.HA_OtherIdentityDocument };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[HRJobApplicantSchema.HA_OtherIdentityDocument.Name] == DBNull.Value || Convert.ToString(row[HRJobApplicantSchema.HA_OtherIdentityDocument.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { HRJobApplicantSchema.Constants.HA_OtherIdentityDocument };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode> subscriberUtilitiesRegCode = new PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.CreatePatternMatchingBusinessObject(factory, BizO.Person.PK, hashedValue, HRJobApplicantSchema.Constants.Prefix, PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			var regCode = changeRow[changeRowField, rowVersion].ToStringSafe();
			var codeToHash = TextStandardizerHelper.StandardizeRegCode(regCode);
			codeToHash = GetCodeToHashWithPrefix(changeRowField, codeToHash);
			valuesToHash.Add(codeToHash);

			return valuesToHash;
		}

		string GetCodeToHashWithPrefix(string changeRowField, string codeToHash)
		{
			var prefix = string.Empty;
			if (changeRowField == HRJobApplicantSchema.Constants.HA_OtherIdentityDocument)
			{
				prefix = CodeTypeIdentifier.Other;
			}

			return string.IsNullOrEmpty(codeToHash) ? string.Empty : prefix + codeToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[HRJobApplicantSchema.Constants.PK].ToString(),
				changeRow[HRJobApplicantSchema.Constants.HA_OtherIdentityDocument].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(HRJobApplicantRegCodeSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesRegCode.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue, originalHashedValue, BizO.Person.PK, HRJobApplicantSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
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
