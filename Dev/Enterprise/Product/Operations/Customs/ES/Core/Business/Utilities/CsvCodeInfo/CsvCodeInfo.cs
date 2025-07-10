using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public class CsvCodeInfo : NonPersistentBusinessObject
	{
		CsvCodeInfo(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			this.entryHeader = entryHeader;
			Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}
		public readonly CusEntryHeader entryHeader;

		public static class Schema
		{
			public const string CsvCodeFromUser = "CsvCodeFromUser";
			public const string SecondaryCsvCodeFromUser = "SecondaryCsvCodeFromUser";
			public const string ThirdCsvCodeFromUser = "ThirdCsvCodeFromUser";
			public const string ClearanceDateFromUser = "ClearanceDateFromUser";
		}

		public static CsvCodeInfo LoadNew(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			var updateCSVClearance = new CsvCodeInfo(entryHeader);
			updateCSVClearance.CsvCodeFromUser = entryHeader.CSVClearance;
			updateCSVClearance.SecondaryCsvCodeFromUser = entryHeader.IsImport ? entryHeader.ZG_CSVImportCertificate : entryHeader.ZG_CSVT2L;
			updateCSVClearance.ThirdCsvCodeFromUser = entryHeader.ZG_CSVExitCertificate;
			updateCSVClearance.ClearanceDateFromUser = entryHeader.CH_EntryReleaseDate;
			return updateCSVClearance;
		}

		public ZString CsvCodeFromUser
		{
			get => csvCodeFromUser;
			set
			{
				SetNonPersistentPropertyValue(CsvCodeFromUserInfo, ref csvCodeFromUser, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCsvCodeFromUser();
					Validation.ValidateClearanceDateFromUser();
				}
			}
		}
		ZString csvCodeFromUser;
		public ZPropertyInfo CsvCodeFromUserInfo => GetZPropertyInfo(Schema.CsvCodeFromUser);

		public ZString SecondaryCsvCodeFromUser
		{
			get => secondaryCsvCodeFromUser;
			set
			{
				SetNonPersistentPropertyValue(SecondaryCsvCodeFromUserInfo, ref secondaryCsvCodeFromUser, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecondaryCsvCodeFromUser();
				}
			}
		}
		ZString secondaryCsvCodeFromUser;
		public ZPropertyInfo SecondaryCsvCodeFromUserInfo => GetZPropertyInfo(Schema.SecondaryCsvCodeFromUser);

		public ZString ThirdCsvCodeFromUser
		{
			get => thirdCsvCodeFromUser;
			set
			{
				SetNonPersistentPropertyValue(ThirdCsvCodeFromUserInfo, ref thirdCsvCodeFromUser, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateThirdCsvCodeFromUser();
				}
			}
		}
		ZString thirdCsvCodeFromUser;
		public ZPropertyInfo ThirdCsvCodeFromUserInfo => GetZPropertyInfo(Schema.ThirdCsvCodeFromUser);

		public ZDateTime ClearanceDateFromUser
		{
			get => clearanceDateFromUser;
			set
			{
				SetNonPersistentPropertyValue(ClearanceDateFromUserInfo, ref clearanceDateFromUser, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateClearanceDateFromUser();
				}
			}
		}
		ZDateTime clearanceDateFromUser;

		public ZPropertyInfo ClearanceDateFromUserInfo => GetZPropertyInfo(Schema.ClearanceDateFromUser);

		public CsvCodeInfoValidation Validation => new CsvCodeInfoValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}
	}
}
