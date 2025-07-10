using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Schema;
using INumberFountainProxy = Enterprise.ZArchitecture.Environment.INumberFountainProxy;

namespace Enterprise.Customs.CA.Business
{
	public sealed class TransactionNumber : AutoTransactionNumber
	{
		public const long MaxMaximumNumber = 99_999_999;
		public const long MinRemainingCapacity = 1000;
		public const long MinDistanceFromDIFToPreviousRange = 1000000;
		public const long MinDistanceFromDIFToNextRange = 1000000;

		public const string DIFNumberDeclarationType = "DIF";

		#region Constructors

		public TransactionNumber(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				this.declaration = declaration;
				declaration.RegisterEditableChildObject(this);
				InitializeTransactionNumber();
				OriginalSequentialNumber = SequentialNumber;
			}
		}

		public TransactionNumber()
			: this(ZString.Empty, ZString.Empty)
		{
		}

		TransactionNumber(ZString securityCode, ZString sequentialNumber)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				base.AccountSecurityCode = securityCode;
				base.SequentialNumber = sequentialNumber;
				OriginalSequentialNumber = SequentialNumber;
				UpdateFormattedTransactionNumberFromSequentialNumber();
			}
		}

		void InitializeTransactionNumber()
		{
			transactionNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumber.EntryType.CATransactionNumber, Constants.CountryCodes.Canada);
			var match = Regex.Match(transactionNumber.CE_EntryNum, @"^(?<SecurityCode>[0-9]{5})(?<SequentialNumber>[0-9]{8})[0-9]$");
			if (match.Success)
			{
				base.AccountSecurityCode = match.Groups["SecurityCode"].Value;
				base.SequentialNumber = match.Groups["SequentialNumber"].Value;
				UpdateFormattedTransactionNumberFromSequentialNumber();
			}
			else
			{
				SetAccountSecurityNo();
				SequentialNumber = SequentialNumberDefault;
			}
		}

		#endregion

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoTransactionNumber.Schema
		{
			public const string FormattedTransactionNumber = "FormattedTransactionNumber";
			public const int FormattedTransactionNumberMaxLength = 14;
		}

		#endregion

		#region GenerateTransactionNumber

		public void SetAccountSecurityNo()
		{
			var accountSecurityCode = GetAccountSecurityNo();
			if (!accountSecurityCode.IsEmpty && accountSecurityCode != AccountSecurityCode)
			{
				AccountSecurityCode = accountSecurityCode;
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (declaration != null && !declaration.IsDeleted && declaration.IsPersistent)
			{
				if (declaration.IsImportIncludingB2 && !declaration.ForceManualInputOfTransactionNumber && CanChange)
				{
					SequentialNumber = GetSequentialNumber();
				}
				else if (!declaration.IsImportIncludingB2 && transactionNumber != null)
				{
					Delete();
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				OriginalSequentialNumber = SequentialNumber;
			}
		}

		public override void Delete()
		{
			base.Delete();
			if (transactionNumber != null)
			{
				transactionNumber.Delete();
			}
		}

		#region GetAccountSecurityNo

		internal bool CanBeChangedOrDeleted(out string errMsg)
		{
			return ((ICusEntryNumberParent)declaration).CanBeChangedOrDeleted(transactionNumber, out errMsg);
		}

		internal bool EntryNumberChanged
		{
			get
			{
				return transactionNumber.IsInDatabase && transactionNumber.CE_EntryNumInfo.OriginalValue.ToString() != FormattedTransactionNumber;
			}
		}

		internal ZString GetAccountSecurityNo()
		{
			return GetImporterAccountSecurityNo() ?? CACustomsDataRegistry.Instance.AccountSecurityNo.Value;
		}

		string GetImporterAccountSecurityNo()
		{
			string result = null;
			if (declaration != null && declaration.CA_UseImporterAccountSecurityNumber)
			{
				var importerAddInfo = declaration.ImporterOfRecordAddInfo ?? declaration.ImporterAddInfo;
				if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
				{
					result = importerAddInfo.ZO_AccountSecurityNumber;
				}
			}
			return result;
		}

		#endregion

		#region GetSequentialNumber

		/// <summary>
		/// This method must be called in transaction.
		/// </summary>
		internal ZString GetSequentialNumber()
		{
			var securityCode = AccountSecurityCode;
			var sequentialNumber = SequentialNumber;

			if (sequentialNumber != SequentialNumberDefault)
			{
				if (!securityCode.IsEmpty && !sequentialNumber.IsEmpty && !IsTransactionNumberUnique(new TransactionNumber(securityCode, sequentialNumber)))
				{
					throw new ZCannotSaveException(TransactionNumberMessages.TransactionNumberAlreadyUsed(existingDeclaration), TransactionNumberMessages.CannotAllocateTransactionNumber());
				}
				return sequentialNumber;
			}

			if (securityCode.IsEmpty)
			{
				return SequentialNumberDefault;
			}

			var numberFountain = EffectiveNumberFountain
				?? throw new ZCannotSaveException(TransactionNumberMessages.TransactionNumberRangeNotConfigured(securityCode), TransactionNumberMessages.CannotAllocateTransactionNumber());

			bool isDuplicateNumber = true;
			while (isDuplicateNumber)
			{
				try
				{
					sequentialNumber = numberFountain.GetNextFormatted(declaration.Factory);
				}
				catch (NumberFountainMaximumValueReachedException)
				{
					throw new ZCannotSaveException(TransactionNumberMessages.TransactionNumberRangeIsEmpty(), TransactionNumberMessages.CannotAllocateTransactionNumber());
				}
				isDuplicateNumber = !IsTransactionNumberUnique(new TransactionNumber(securityCode, sequentialNumber));
			}

			return sequentialNumber;
		}

		bool IsTransactionNumberUnique(TransactionNumber tranNumber)
		{
			var existingTranNumber = LoadTransactionNumber(tranNumber.AccountSecurityCode + tranNumber.UniqueIdentifier);
			if (existingTranNumber != null)
			{
				this.existingDeclaration = Factory.Load<JobDeclaration>(existingTranNumber.CE_ParentID);
				return false;
			}
			this.existingDeclaration = null;
			return true;
		}

		CusEntryNumber LoadTransactionNumber(ZString tranNumber)
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.CATransactionNumber);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Constants.CountryCodes.Canada);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, tranNumber);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, JobDeclarationSchema.Constants.TableName);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, this.declaration.PK);
			filter.TableIndexHints.Add(new TableIndexHint("NR_RX__CE_EntryNum"));
			filter.TableHints = TableHints.FORCESEEK;
			return new BusinessObjectFactory().LoadTop1<CusEntryNumber>(filter);
		}

		#endregion

		#endregion

		#region Properties

		#region NumberFountainHasDriedUp

		internal bool NumberFountainHasDriedUp
		{
			get
			{
				var numberFountain = EffectiveNumberFountain;
				if (numberFountain == null)
				{
					return false;
				}
				numberFountain.GetMinAndMaxValues(Factory, out long minValue, out long maxValue);
				return numberFountain.PeekPreliminary(Factory) > maxValue;
			}
		}

		#endregion

		#region AccountSecurityPassword

		public ZString AccountSecurityPassword
		{
			get
			{
				var result = ZString.Empty;
				if (AccountSecurityCode == CACustomsDataRegistry.Instance.AccountSecurityNo.Value)
				{
					result = CACustomsDataRegistry.Instance.AccountSecurityNoPassword.Value;
				}
				else
				{
					var addInfo = declaration == null ? null : declaration.ImporterOfRecordAddInfo ?? declaration.ImporterAddInfo;
					if (addInfo != null && AccountSecurityCode == addInfo.ZO_AccountSecurityNumber)
					{
						result = addInfo.ZO_AccountSecirityPassword;
					}
				}
				return result;
			}
		}

		#endregion

		#region AccountSecurityCode

		[ReadOnlyMember(nameof(AccountSecurityCode_ReadOnly))]
		public override ZString AccountSecurityCode
		{
			get { return base.AccountSecurityCode; }
			set
			{
				if (CanChange)
				{
					var oldValue = base.AccountSecurityCode;
					base.AccountSecurityCode = value.IsEmpty ? GetAccountSecurityNo() : value;
					if (oldValue != base.AccountSecurityCode)
					{
						SequentialNumber = SequentialNumberDefault;
					}
					Validation.ValidateSequentialNumber();
					transactionNumber.CE_EntryNum = ToString();

					if (oldValue != AccountSecurityCode)
					{
						UpdateFormattedTransactionNumberFromSequentialNumber();
					}
				}
			}
		}

		bool AccountSecurityCode_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region SequentialNumber

		[BusinessObjectMaxLengthTestExclude]
		[ReadOnlyMember(nameof(SequentialNumber_ReadOnly))]
		public override ZString SequentialNumber
		{
			get { return base.SequentialNumber; }
			set
			{
				if (CanChange)
				{
					var oldValue = base.SequentialNumber;
					base.SequentialNumber = value.PadLeft(8, '0');
					transactionNumber.CE_EntryNum = ToString();

					if (oldValue != SequentialNumber)
					{
						UpdateFormattedTransactionNumberFromSequentialNumber();
					}
				}
			}
		}

		internal bool SequentialNumber_ReadOnly
		{
			get { return !CanChange; }
		}

		public bool CanChange
		{
			get
			{
				if (declaration != null)
				{
					var releaseEntry = declaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
					var b3Entry = declaration.B3EntryHeader;
					return (releaseEntry == null || releaseEntry.Messages.Count == 0) && (b3Entry == null || b3Entry.Messages.Count == 0);
				}
				else
				{
					return false;
				}
			}
		}

		ZString OriginalSequentialNumber { get; set; }

		internal bool SequentialNumberHasChanges => OriginalSequentialNumber != SequentialNumber;

		#endregion

		#region CheckDigit

		public override ZInt CheckDigit
		{
			get
			{
				var result = 0;
				var code = AccountSecurityCode + SequentialNumber;
				if (!SequentialNumber.IsEmpty && SequentialNumber != SequentialNumberDefault)
				{
					result = GetCheckDigit(code);
				}
				return result;
			}
		}

		internal static ZInt GetCheckDigit(ZString code)
		{
			var sum = 0;
			if (Regex.IsMatch(code, @"^[0-9]{13}$"))
			{
				for (var i = 0; i < code.Length; i++)
				{
					var digit = int.Parse(code[i].ToString());
					if (i % 2 != 0)
					{
						if ((digit *= 2) >= 10)
						{
							digit = (digit / 10) + (digit % 10);
						}
					}
					sum += digit;
				}
			}
			return sum % 10;
		}

		#endregion

		#region IsUnique

		public bool IsUnique
		{
			get { return IsTransactionNumberUnique(this); }
		}

		#endregion

		#region UniqueIdentifier

		public ZString UniqueIdentifier
		{
			get { return SequentialNumber + CheckDigit; }
		}

		#endregion

		#region FormattedTransactionNumber

		ZString formattedTransactionNumber;

		[MaxLength(Schema.FormattedTransactionNumberMaxLength)]
		[ReadOnlyMember(nameof(FormattedTransactionNumber_ReadOnly))]
		public ZString FormattedTransactionNumber
		{
			get { return formattedTransactionNumber; }
			set
			{
				var oldValue = FormattedTransactionNumber;
				CheckMaximumLength(FormattedTransactionNumberInfo, value);
				formattedTransactionNumber = value;

				if (oldValue != FormattedTransactionNumber && !IsCopying && declaration != null)
				{
					UpdateSequentialNumberFromFormattedTransactionNumber();

					if (!IsValidationSuspended)
					{
						Validation.ValidateFormattedTransactionNumber();
					}
				}

				FormattedTransactionNumberInfo.RefreshBinding();
			}
		}

		void UpdateSequentialNumberFromFormattedTransactionNumber()
		{
			if (isFormattedNumberRecalculationSuppressed)
			{
				return;
			}
			using (SuppressFormattedNumberRecalculation())
			{
				var sequentialNumber = ZString.Empty;

				if (FormattedTransactionNumber.IsNumbersOnlyOrEmpty && FormattedTransactionNumber.Length == Schema.FormattedTransactionNumberMaxLength)
				{
					sequentialNumber = FormattedTransactionNumber.SubstringSafe(5, 8);
				}

				SequentialNumber = sequentialNumber;
			}
		}

		void UpdateFormattedTransactionNumberFromSequentialNumber()
		{
			if (isFormattedNumberRecalculationSuppressed)
			{
				return;
			}
			using (SuppressFormattedNumberRecalculation())
			{
				if (AccountSecurityCode.IsEmpty || SequentialNumber.IsEmpty || SequentialNumber == SequentialNumberDefault)
				{
					FormattedTransactionNumber = ZString.Empty;
				}
				else
				{
					FormattedTransactionNumber = ToString();
				}
			}
		}

		IDisposable SuppressFormattedNumberRecalculation()
		{
			bool oldValue = isFormattedNumberRecalculationSuppressed;
			isFormattedNumberRecalculationSuppressed = true;
			return new DisposableAction(() => { isFormattedNumberRecalculationSuppressed = oldValue; });
		}

		bool isFormattedNumberRecalculationSuppressed;

		bool FormattedTransactionNumber_ReadOnly
		{
			get { return !CanChange; }
		}

		public ZPropertyInfo FormattedTransactionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.FormattedTransactionNumber); }
		}

		#endregion

		#endregion

		#region Implementation

		#region ToString

		public static implicit operator ZString(TransactionNumber number)
		{
			return number.ToString();
		}

		public new ZString ToString()
		{
			return AccountSecurityCode.IsEmpty ? string.Empty : AccountSecurityCode + UniqueIdentifier;
		}

		#endregion

		internal readonly JobDeclaration declaration;
		internal JobDeclaration existingDeclaration;
		CusEntryNumber transactionNumber;
		public const string SequentialNumberDefault = "00000000";
		public const string AccountSecurityNoDefault = "00000";

		#endregion

		#region CheckNumberFountainRange

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public bool CheckNumberFountainRange(string sequentialNumber, out long minValue, out long maxValue)
		{
			// the caller should make sure that SequentialNumber has correct format
			long sequentialNumberLong;
			if (!long.TryParse(sequentialNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out sequentialNumberLong))
			{
				// There is no number to validate. Error will be reported by another check.
				minValue = -1;
				maxValue = -1;
				return true;
			}

			INumberFountainProxy fountain = EffectiveNumberFountain;

			if (fountain == null)
			{
				// There is no number number fountain to check. Error will be reported by another check.
				minValue = -1;
				maxValue = -1;
				return true;
			}

			//long minValue;
			fountain.GetMinAndMaxValues(declaration.Factory, out minValue, out maxValue);

			if (sequentialNumberLong < minValue || sequentialNumberLong > maxValue)
			{
				return false;
			}

			return true;
		}

		#endregion

		#region Number Fountain

		/// <returns>
		/// Either branch / declaration type specific number fountain;
		/// or generic number fountain for given security number;
		/// or null if neither of those are set up.
		/// </returns>
		internal INumberFountainProxy EffectiveNumberFountain
		{
			get
			{
				INumberFountainProxy result = null;
				if (declaration != null)
				{
					var messageType = declaration.IsIM2 || declaration.IsB3X ? JobMessageTypeList.Codes.B2Adjustments : declaration.JE_MessageType.ToString();
					result = GetEffectiveNumberFountain(AccountSecurityCode, declaration.Branch, messageType, declaration.Factory);
				}
				return result;
			}
		}

		/// <returns>
		/// Either branch / declaration type specific number fountain;
		/// or generic number fountain for given security number;
		/// or null if neither of those are set up.
		/// </returns>
		internal static INumberFountainProxy GetEffectiveNumberFountain(ZString securityCode, GlbBranch branch, ZString messagetype, BusinessObjectFactory factory)
		{
			if (securityCode.IsEmpty)
			{
				return null;
			}

			var fountainKey = GetNumberFountainKey(GetRangeSeparator(securityCode, branch, messagetype));
			var numberFountain = GetNumberFountain(fountainKey);
			numberFountain.GetMinAndMaxValues(factory, out long minValue, out long maxValue);
			var preliminaryValue = numberFountain.PeekPreliminary(factory);

			if (numberFountain.PeekPreliminaryOrDefault(factory, 0) != 0 && preliminaryValue >= minValue && preliminaryValue <= maxValue)
			{
				return numberFountain;
			}

			fountainKey = GetNumberFountainKey(GetRangeSeparator(securityCode, null, ""));
			numberFountain = GetNumberFountain(fountainKey);
			if (numberFountain.PeekPreliminaryOrDefault(factory, 0) != 0)
			{
				return numberFountain;
			}

			return null;
		}

		internal static INumberFountainProxy GetNumberFountain(string fountainKey)
		{
			return Env.NumberFountains.GetCAEntryNumberGeneratorFountain(fountainKey);
		}

		internal static ZString GetRangeSeparator(ZString securityCode, GlbBranch branch, ZString messageType)
		{
			if (branch != null)
			{
				return securityCode + branch.PK + messageType;
			}
			return securityCode + messageType;
		}

		internal static string GetRangeName(ZString securityCode, GlbBranch branch, ZString messageType)
		{
			if (securityCode.IsEmpty)
			{
				return TransactionNumberSetting.DefaultAccountSecurityCode;
			}
			List<string> parts = new List<string>();
			if (!securityCode.IsEmpty)
			{
				parts.Add(securityCode);
			}
			if (branch != null)
			{
				parts.Add(branch.GB_Code);
			}
			if (!messageType.IsEmpty)
			{
				parts.Add(messageType);
			}
			return string.Join(" ", parts);
		}

		internal static ZString GetNumberFountainKey(ZString separator)
		{
			return DefaultNumberFountainKey + separator.Trim();
		}

		internal static ZString DefaultNumberFountainKey
		{
			get { return "CADeclarationTransactionNumber"; }
		}

		#endregion
	}
}
