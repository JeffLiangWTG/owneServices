using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class EntryNumbersBO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string PreEntryNumber = "PreEntryNumber";
			public const string DeclarationUnifiedNumber = "DeclarationUnifiedNumber";
			public const string MovementReferenceNumber = "MovementReferenceNumber";
			public const string MovementReferenceNumberIssueDate = "MovementReferenceNumberIssueDate";
			public const string CIQNumber = "CIQNumber";

			public const int CE_EntryNumMaxLength = 18;
		}

		public EntryNumbersBO(CusEntryHeader header) : base(header.Factory)
		{
			cusEntryHeader = header;
			preEntryNumber = cusEntryHeader.PreEntryNumber;
			declarationUnifiedNumber = cusEntryHeader.DeclarationUnifiedNumber;
			movementReferenceNumber = cusEntryHeader.MovementReferenceNumber;
			movementReferenceNumberIssueDate = cusEntryHeader.MovementReferenceNumberIssueDate;
			cIQNumber = cusEntryHeader.CIQNumber;
		}

		internal IValidationModeProvider ValidationModeProvider => cusEntryHeader?.Declaration;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			checkExistingInOtherEntry = true;
			ValidateEntryNumber();
			checkExistingInOtherEntry = false;
		}

		bool checkExistingInOtherEntry;
		readonly CusEntryHeader cusEntryHeader;

		public void SetEntryNumbers()
		{
			if (!PreEntryNumber_ReadOnly)
			{
				cusEntryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, PreEntryNumber);
			}
			if (!DeclarationUnifiedNumber_ReadOnly)
			{
				cusEntryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, DeclarationUnifiedNumber);
			}
			if (!CIQNumber_ReadOnly)
			{
				cusEntryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, CIQNumber);
			}
			if (!MovementReferenceNumber_ReadOnly)
			{
				cusEntryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, MovementReferenceNumber, MovementReferenceNumberIssueDate);
			}
		}

		ZString preEntryNumber;
		[MaxLength(Schema.CE_EntryNumMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.CN.Business.EntryNumbersBO|PreEntryNumber", Caption = "PRE Entry Number")]
		public ZString PreEntryNumber
		{
			get => preEntryNumber;
			set
			{
				CheckMaximumLength(PreEntryNumberInfo, value);
				SetNonPersistentPropertyValue(PreEntryNumberInfo, ref preEntryNumber, value);
				PreEntryNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PreEntryNumberInfo => GetZPropertyInfo(Schema.PreEntryNumber);

		public bool PreEntryNumber_ReadOnly => cusEntryHeader?.IsEntryNumberSystemGernerated(CusEntryNumberTypes.China.PreEntryNumber) ?? false;

		ZString declarationUnifiedNumber;

		[MaxLength(Schema.CE_EntryNumMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.CN.Business.EntryNumbersBO|DeclarationUnifiedNumber", Caption = "Declaration Unified Number")]
		public ZString DeclarationUnifiedNumber
		{
			get => declarationUnifiedNumber;
			set
			{
				CheckMaximumLength(DeclarationUnifiedNumberInfo, value);
				SetNonPersistentPropertyValue(DeclarationUnifiedNumberInfo, ref declarationUnifiedNumber, value);
				DeclarationUnifiedNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeclarationUnifiedNumberInfo => GetZPropertyInfo(Schema.DeclarationUnifiedNumber);

		public bool DeclarationUnifiedNumber_ReadOnly => cusEntryHeader?.IsEntryNumberSystemGernerated(CusEntryNumberTypes.China.DeclarationUnifiedNumber) ?? false;

		ZString movementReferenceNumber;

		[MaxLength(Schema.CE_EntryNumMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.CN.Business.EntryNumbersBO|MovementReferenceNumber", Caption = "Customs Entry Number")]
		public ZString MovementReferenceNumber
		{
			get => movementReferenceNumber;
			set
			{
				var targetInfo = MovementReferenceNumberInfo;
				CheckMaximumLength(targetInfo, value);
				SetNonPersistentPropertyValue(targetInfo, ref movementReferenceNumber, value);
				targetInfo.RefreshBinding();
				ValidateMovementReferenceNumberIssueDate();
			}
		}

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(Schema.MovementReferenceNumber);

		public bool MovementReferenceNumber_ReadOnly => cusEntryHeader?.IsEntryNumberSystemGernerated(CusEntryNumberTypes.Standard.MovementReferenceNumber) ?? false;

		ZDateTime movementReferenceNumberIssueDate;

		public ZDateTime MovementReferenceNumberIssueDate
		{
			get => movementReferenceNumberIssueDate;
			set => SetNonPersistentPropertyValue(MovementReferenceNumberIssueDateInfo, ref movementReferenceNumberIssueDate, value);
		}

		public ZPropertyInfo MovementReferenceNumberIssueDateInfo => GetZPropertyInfo(Schema.MovementReferenceNumberIssueDate);

		public bool MovementReferenceNumberIssueDate_ReadOnly => cusEntryHeader?.IsEntryNumberSystemGernerated(CusEntryNumberTypes.Standard.MovementReferenceNumber) ?? false;

		ZString cIQNumber;

		[MaxLength(Schema.CE_EntryNumMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.CN.Business.EntryNumbersBO|CIQNumber", Caption = "CIQ Number")]
		public ZString CIQNumber
		{
			get => cIQNumber;
			set
			{
				CheckMaximumLength(CIQNumberInfo, value);
				SetNonPersistentPropertyValue(CIQNumberInfo, ref cIQNumber, value);
				CIQNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CIQNumberInfo => GetZPropertyInfo(Schema.CIQNumber);

		public bool CIQNumber_ReadOnly => cusEntryHeader?.IsEntryNumberSystemGernerated(CusEntryNumberTypes.China.CIQNumber) ?? false;

		void ValidateEntryNumber()
		{
			ValidatePreEntryNumber();
			ValidateDeclarationUnifiedNumber();
			ValidateMovementReferenceNumber();
			ValidateCIQNumber();
			ValidateMovementReferenceNumberIssueDate();
		}

		#region Validations

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular expression pattern")]
		const string regex15Digits = @"^\d{15}$";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular expression pattern")]
		const string regex18Digits = @"^\d{18}$";

		string EntryNumberAlreadyExistsInThisJob => Res.GetString(
			"727C08F0-AC6C-48E3-9EA4-B4DC24A58EC5",
			"This number is the same as another number on this job, please make sure a correct number is entered."
		);

		string EntryNumberAlreadyExistsInAnotherJob => Res.GetString(
			"362AC09F-7B5D-4F58-A811-A44976E9E1B7",
			"This number has already been used by another job in the system, please make sure you type in the correct number."
		);

		string CustomsOfficeIsEmpty => Res.GetString(
			"DA23C847-6C7F-471B-9CC9-26DDB59565B0",
			"The Customs Office of this job is empty."
		);

		string MRNPrefix
		{
			get
			{
				var customsOfice = cusEntryHeader.Declaration.JE_CustomsOffice;
				var yearOfValidation = cusEntryHeader.DateOfValuation.ToString("yyyy", CultureInfo.InvariantCulture);
				var ieFlag = cusEntryHeader.IsEntering ? 1 : 0;
				return customsOfice + yearOfValidation + ieFlag.ToString(CultureInfo.InvariantCulture);
			}
		}

		string regexCUSMRN
		{
			get
			{
				var result = string.Empty;
				if (cusEntryHeader.Declaration != null && !cusEntryHeader.Declaration.JE_CustomsOffice.IsEmpty && !cusEntryHeader.DateOfValuation.IsEmpty)
				{
					FormattableString format = $@"^{MRNPrefix}\d{{{9}}}$";
					result = format.ToString(CultureInfo.InvariantCulture);
				}
				return result;
			}
		}

		string UNIPrefix
		{
			get
			{
				var yearOfValidation = cusEntryHeader.DateOfValuation.ToString("yyyy", CultureInfo.InvariantCulture);
				return $@"^[I|E]{yearOfValidation}\d{{13}}$";
			}
		}

		public void ValidatePreEntryNumber()
		{
			PreEntryNumberInfo.ClearAllNotifications();

			if (cusEntryHeader?.Declaration != null && cusEntryHeader.Declaration.JE_CustomsOffice.IsEmpty)
			{
				PreEntryNumberInfo.AddNotification(CustomsOfficeIsEmpty, ValidationModeProvider);
			}

			if (!PreEntryNumber.IsEmpty)
			{
				if (checkExistingInOtherEntry && checkEntryNumberExistingInOtherEntry(PreEntryNumber, CusEntryNumberTypes.China.PreEntryNumber))
				{
					PreEntryNumberInfo.AddError(EntryNumberAlreadyExistsInAnotherJob);
				}

				if (!Regex.IsMatch(PreEntryNumber, regex18Digits))
				{
					PreEntryNumberInfo.AddNotification(Res.GetString("DABB4F9E-CA3F-4BB2-AA6A-3E517BA5E685", "Pre Entry Number should be 18 digital characters."), ValidationModeProvider);
				}
			}
		}

		public void ValidateDeclarationUnifiedNumber()
		{
			DeclarationUnifiedNumberInfo.ClearAllNotifications();

			if (cusEntryHeader?.Declaration != null && cusEntryHeader.Declaration.JE_CustomsOffice.IsEmpty)
			{
				DeclarationUnifiedNumberInfo.AddNotification(CustomsOfficeIsEmpty, ValidationModeProvider);
			}

			if (!DeclarationUnifiedNumber.IsEmpty)
			{
				if (DeclarationUnifiedNumber == PreEntryNumber || DeclarationUnifiedNumber == MovementReferenceNumber || DeclarationUnifiedNumber == CIQNumber)
				{
					DeclarationUnifiedNumberInfo.AddNotification(EntryNumberAlreadyExistsInThisJob, ValidationModeProvider);
				}
				else if (checkExistingInOtherEntry && checkEntryNumberExistingInOtherEntry(DeclarationUnifiedNumber, CusEntryNumberTypes.China.DeclarationUnifiedNumber))
				{
					DeclarationUnifiedNumberInfo.AddError(EntryNumberAlreadyExistsInAnotherJob);
				}

				if (!Regex.IsMatch(DeclarationUnifiedNumber, UNIPrefix))
				{
					DeclarationUnifiedNumberInfo.AddNotification(Res.GetString("61D14396-DC4D-49EE-9131-ABE830CBFC8C", "Declaration Unified Number should be 18 digital characters and start with I or O and then 4 digital year(e.g I20190000123456789)"), ValidationModeProvider);
				}
			}
		}

		public void ValidateMovementReferenceNumber()
		{
			var targetInfo = MovementReferenceNumberInfo;
			targetInfo.ClearAllNotifications();

			if (cusEntryHeader?.Declaration != null && cusEntryHeader.Declaration.JE_CustomsOffice.IsEmpty)
			{
				targetInfo.AddNotification(CustomsOfficeIsEmpty, ValidationModeProvider);
			}

			if (!MovementReferenceNumber.IsEmpty)
			{
				if (MovementReferenceNumber == DeclarationUnifiedNumber || MovementReferenceNumber == CIQNumber)
				{
					targetInfo.AddNotification(EntryNumberAlreadyExistsInThisJob, ValidationModeProvider);
				}
				else if (checkExistingInOtherEntry && checkEntryNumberExistingInOtherEntry(MovementReferenceNumber, CusEntryNumberTypes.Standard.MovementReferenceNumber))
				{
					targetInfo.AddError(EntryNumberAlreadyExistsInAnotherJob);
				}

				if (!Regex.IsMatch(MovementReferenceNumber, regexCUSMRN))
				{
					targetInfo.AddNotification(Res.GetString("37269E95-702E-4E31-8A23-7E08EB80BCE3", "Customs Entry Number should start with {0} and followed by a 9-digit sequence number.", MRNPrefix), ValidationModeProvider);
				}
			}
		}

		public void ValidateMovementReferenceNumberIssueDate()
		{
			MovementReferenceNumberIssueDateInfo.ClearAllNotifications();
			if (!MovementReferenceNumber.IsEmpty)
			{
				MandatoryValidation.CheckEntered(MovementReferenceNumberIssueDateInfo);
				TypeValidation.CheckValidSmallDateTime(MovementReferenceNumberIssueDateInfo);
			}
		}

		public void ValidateCIQNumber()
		{
			var targetInfo = CIQNumberInfo;
			targetInfo.ClearAllNotifications();

			if (cusEntryHeader?.Declaration != null && cusEntryHeader.Declaration.JE_CustomsOffice.IsEmpty)
			{
				targetInfo.AddNotification(CustomsOfficeIsEmpty, ValidationModeProvider);
			}

			if (!CIQNumber.IsEmpty)
			{
				if (CIQNumber == DeclarationUnifiedNumber || CIQNumber == PreEntryNumber || CIQNumber == MovementReferenceNumber)
				{
					targetInfo.AddNotification(EntryNumberAlreadyExistsInThisJob, ValidationModeProvider);
				}
				else if (checkExistingInOtherEntry && checkEntryNumberExistingInOtherEntry(CIQNumber, CusEntryNumberTypes.China.CIQNumber))
				{
					targetInfo.AddError(EntryNumberAlreadyExistsInAnotherJob);
				}

				if (!Regex.IsMatch(CIQNumber, regex15Digits) && !Regex.IsMatch(CIQNumber, regex18Digits))
				{
					targetInfo.AddNotification(Res.GetString("7EE206EF-0B66-4A1F-A6CA-4531CCBFB888", "CIQ Number should be 15 digital or 18 digital characters."), ValidationModeProvider);
				}
			}
		}

		bool checkEntryNumberExistingInOtherEntry(ZString entryNumber, ZString numberType)
		{
			var result = false;
			if (!entryNumber.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusEntryNumber)) { MaximumRows = 1 };
				query.AddToFilter(CusEntryNumSchema.CE_EntryType, numberType);
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.China);
				query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				query.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
				query.AddToFilter(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, cusEntryHeader.PK);
				var existingNumbers = cusEntryHeader.Factory.Load<CusEntryNumber>(query);

				result = existingNumbers.Length > 0;
			}
			return result;
		}

		#endregion
	}
}
