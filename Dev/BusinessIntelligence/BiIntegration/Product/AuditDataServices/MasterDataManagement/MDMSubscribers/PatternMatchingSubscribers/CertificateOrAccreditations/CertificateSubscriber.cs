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
	public abstract class CertificateSubscriber : PatternMatchingSubscriber<GenRegCertAccredMaintList>
	{
		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode> subscriberUtilitiesRegCode = new PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();

		protected abstract string ParentTableCode { get; }

		protected abstract GlbPerson GetPerson(BusinessObjectFactory factory, GenRegCertAccredMaintList certificate);

		protected override string PKColumn => GenRegCertAccredMaintListSchema.Constants.PK;

		public override ITableSchema Table => GenRegCertAccredMaintListSchema.Instance;

		protected override PatternMasterType MasterType => PatternMasterType.GlbPerson;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Person Certificate or Accreditation Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GenRegCertAccredMaintListSchema.XZ_Type, GenRegCertAccredMaintListSchema.XZ_RefNumber };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GenRegCertAccredMaintListSchema.XZ_Type.Name] == DBNull.Value
				|| row[GenRegCertAccredMaintListSchema.XZ_RefNumber.Name] == DBNull.Value
				|| row[GenRegCertAccredMaintListSchema.XZ_ParentTableCode.Name] == DBNull.Value
				|| Convert.ToString(row[GenRegCertAccredMaintListSchema.XZ_Type.Name]).Length <= 0
				|| Convert.ToString(row[GenRegCertAccredMaintListSchema.XZ_RefNumber.Name]).Length <= 0
				|| Convert.ToString(row[GenRegCertAccredMaintListSchema.XZ_ParentTableCode.Name]) != ParentTableCode)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { string.Empty }; // Placeholder to ensure each certificate row is hashed once, but we want to manually extract columns

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var person = GetPerson(factory, BizO);
			return person != null && subscriberUtilitiesRegCode.CreatePatternMatchingBusinessObject(factory, person.PK, hashedValue, GenRegCertAccredMaintListSchema.Constants.Prefix, PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK, person.PER_RN_NKCountry);
		}
		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var person = GetPerson(factory, BizO);
			return person != null && subscriberUtilitiesRegCode.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue, originalHashedValue, person.PK, GenRegCertAccredMaintListSchema.Constants.Prefix, person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			var refNumber = changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_RefNumber, rowVersion].ToStringSafe();
			var type = changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_Type, rowVersion].ToStringSafe();
			var regCodeToHash = TextStandardizerHelper.StandardizeCertificate(refNumber, type);

			valuesToHash.Add(regCodeToHash);

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}_{3}",
				changeRow.RowState.ToString(),
				changeRow[GenRegCertAccredMaintListSchema.Constants.PK].ToString(),
				changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_Type].ToString(),
				changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_RefNumber].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(CertificateSubscriber);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var person = GetPerson(factory, BizO);

			if (person != null)
			{
				var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(person, factory);
				yield return new PatternMasterDetail(PatternMasterType.GlbPerson, person.PK, queued);
			}
		}
	}
}
