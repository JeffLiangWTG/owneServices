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
	public class GlbStaffRegCodeSubscriber : GlbStaffSubscriber
	{
		public override string Code => "GSR";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Staff Reg Code Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbStaffSchema.GS_Passport, GlbStaffSchema.GS_EnterpriseCertificationID, GlbStaffSchema.GS_Birthdate };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if ((row[GlbStaffSchema.GS_Passport.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_Passport.Name]).Length <= 0)
				&& (row[GlbStaffSchema.GS_EnterpriseCertificationID.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_EnterpriseCertificationID.Name]).Length <= 0)
				&& row[GlbStaffSchema.GS_Birthdate.Name] == DBNull.Value)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbStaffSchema.Constants.GS_Passport, GlbStaffSchema.Constants.GS_EnterpriseCertificationID, GlbStaffSchema.Constants.GS_Birthdate };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode> subscriberUtilitiesRegCode = new PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesRegCode.CreatePatternMatchingBusinessObject(factory, BizO.GS_PER, hashedValue, GlbStaffSchema.Constants.Prefix, PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue);
		}

		static readonly DateTime baseLineDate = new DateTime(1753, 1, 1); // minimum date in SQL Server

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			if (changeRowField == GlbStaffSchema.Constants.GS_Birthdate)
			{
				if (changeRow[changeRowField, rowVersion] is DateTime birthdate)
				{
					var birthdateStr = (birthdate - baseLineDate).Days.ToString(CultureInfo.InvariantCulture);
					valuesToHash.Add(birthdateStr);
				}
				else
				{
					valuesToHash.Add(string.Empty);
				}
			}
			else
			{
				var regCode = changeRow[changeRowField, rowVersion].ToStringSafe();
				var codeToHash = TextStandardizerHelper.StandardizeRegCode(regCode);
				codeToHash = GetCodeToHashWithPrefix(changeRowField, codeToHash);
				valuesToHash.Add(codeToHash);
			}

			return valuesToHash;
		}

		string GetCodeToHashWithPrefix(string changeRowField, string codeToHash)
		{
			var prefix = string.Empty;
			switch (changeRowField)
			{
				case GlbStaffSchema.Constants.GS_Passport:
					prefix = CodeTypeIdentifier.Passport;
					break;
				case GlbStaffSchema.Constants.GS_EnterpriseCertificationID:
					prefix = CodeTypeIdentifier.Certificate;
					break;
			}

			return string.IsNullOrEmpty(codeToHash) ? string.Empty : prefix + codeToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			var birthdateStr = string.Empty;
			if (changeRow[GlbStaffSchema.Constants.GS_Birthdate] is DateTime birthdate)
			{
				birthdateStr = birthdate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
			}

			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3} - {4}",
				changeRow.RowState.ToString(),
				changeRow[GlbStaffSchema.Constants.PK].ToString(),
				changeRow[GlbStaffSchema.Constants.GS_Passport].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_EnterpriseCertificationID].ToString().Trim(),
				birthdateStr
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbStaffRegCodeSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesRegCode.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue, originalHashedValue, BizO.GS_PER, GlbStaffSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
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
