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
	public class GlbPersonRegCodeSubscriber : GlbPersonSubscriber
	{
		public override string Code => "GPR";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbPerson Reg Code Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbPersonSchema.PER_Passport, GlbPersonSchema.PER_DriversLicenseNumber, GlbPersonSchema.PER_BirthDate };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if ((row[GlbPersonSchema.PER_Passport.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_Passport.Name]).Length <= 0)
				&& (row[GlbPersonSchema.PER_DriversLicenseNumber.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_DriversLicenseNumber.Name]).Length <= 0)
				&& row[GlbPersonSchema.PER_BirthDate.Name] == DBNull.Value)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { GlbPersonSchema.Constants.PER_Passport, GlbPersonSchema.Constants.PER_DriversLicenseNumber, GlbPersonSchema.Constants.PER_BirthDate };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode> subscriberUtilitiesRegCode = new PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.CreatePatternMatchingBusinessObject(factory, BizO.PK, hashedValue, GlbPersonSchema.Constants.Prefix, PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK, BizO.PER_RN_NKCountry);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesRegCode.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue, originalHashedValue, BizO.PK, GlbPersonSchema.Constants.Prefix, BizO.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue);
		}

		static readonly DateTime beginDate = new DateTime(1753, 1, 1); // minimum date in SQL Server

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			if (changeRowField == GlbPersonSchema.Constants.PER_BirthDate)
			{
				if (changeRow[changeRowField, rowVersion] is DateTime birthdate)
				{
					var birthdateStr = (birthdate - beginDate).Days.ToString(CultureInfo.InvariantCulture);
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
				case GlbPersonSchema.Constants.PER_Passport:
					prefix = CodeTypeIdentifier.Passport;
					break;
				case GlbPersonSchema.Constants.PER_DriversLicenseNumber:
					prefix = CodeTypeIdentifier.License;
					break;
			}

			return string.IsNullOrEmpty(codeToHash) ? string.Empty : prefix + codeToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			var birthdateStr = string.Empty;
			if (changeRow[GlbPersonSchema.Constants.PER_BirthDate] is DateTime birthdate)
			{
				birthdateStr = birthdate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
			}

			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3} - {4}",
				changeRow.RowState.ToString(),
				changeRow[GlbPersonSchema.Constants.PK].ToString(),
				changeRow[GlbPersonSchema.Constants.PER_Passport].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_DriversLicenseNumber].ToString().Trim(),
				birthdateStr
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbPersonRegCodeSubscriber);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO, factory);
			yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.PK, queued);
		}
	}
}
