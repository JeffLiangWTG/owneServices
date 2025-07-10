using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TransactionNumberValidation : AutoTransactionNumberValidation
	{
		public TransactionNumberValidation(AutoTransactionNumber parent)
			: base(parent) { }

		#region Implementation

		public new TransactionNumber Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (TransactionNumber)base.Parent; }
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFormattedTransactionNumber();
		}

		#region CheckAccountSecurityCode

		protected override void CheckAccountSecurityCode()
		{
			base.CheckAccountSecurityCode();
			var declaration = Parent.declaration;
			if (declaration != null && declaration.IsImportIncludingB2 && declaration.DisplaySequentialOfTransactionNumberSeparately)
			{
				ValidateAccountSecurityCode(Parent.AccountSecurityCodeInfo, Parent.AccountSecurityCode);
			}
		}

		#endregion

		#region CheckSequentialNumber

		protected override void CheckSequentialNumber()
		{
			base.CheckSequentialNumber();
			var declaration = Parent.declaration;
			if (declaration != null)
			{
				if (!Parent.SequentialNumber_ReadOnly && declaration.IsImportIncludingB2 && declaration.DisplaySequentialOfTransactionNumberSeparately)
				{
					ValidateWHSTransactionExists(Parent.SequentialNumberInfo, declaration, Parent.ToString());

					if (!Parent.SequentialNumberInfo.HasErrors())
					{
						long fountainMin, fountainMax;
						if (Parent.SequentialNumber == TransactionNumber.SequentialNumberDefault)
						{
							if (declaration.ForceManualInputOfTransactionNumber)
							{
								Parent.SequentialNumberInfo.AddError(MandatoryValidation.MustBeEnteredMessage(Parent.SequentialNumberInfo.HumanReadableName));
							}
						}
						else if (!Regex.IsMatch(Parent.SequentialNumber, @"^[0-9]{8}$"))
						{
							Parent.SequentialNumberInfo.AddError(Res.GetString("a25c96a6-f5f7-4555-99fd-5dddec53e134", "Sequential Number should be composed of 8 digits."));
						}
						else if (Parent.SequentialNumberHasChanges && !Parent.CheckNumberFountainRange(Parent.SequentialNumber, out fountainMin, out fountainMax))
						{
							Parent.SequentialNumberInfo.AddError(Res.GetString("15b46c77-4336-42c7-9096-56577770caa5", "Sequential Number should be between {0} and {1}.", fountainMin, fountainMax));
						}
						else
						{
							ValidateTransactionNumberIsUnique(Parent.SequentialNumberInfo);
						}
					}

					ValidateHasTransactionNumbersAvailable(Parent.SequentialNumberInfo);
				}
			}
		}

		#endregion

		#region CheckFormattedTransactionNumber

		public void ValidateFormattedTransactionNumber()
		{
			ValidateCalculatedProperty(Parent.FormattedTransactionNumberInfo);
		}

		protected void CheckFormattedTransactionNumber()
		{
			var declaration = Parent.declaration;
			if (declaration != null && declaration.IsImportIncludingB2 && !declaration.DisplaySequentialOfTransactionNumberSeparately)
			{
				var transactionNumber = Parent.FormattedTransactionNumber;
				if (transactionNumber.IsNumbersOnlyOrEmpty && transactionNumber.Length == TransactionNumber.Schema.FormattedTransactionNumberMaxLength)
				{
					ValidateWHSTransactionExists(Parent.FormattedTransactionNumberInfo, declaration, transactionNumber);
				}

				var accountSecurityNo = Parent.GetAccountSecurityNo();
				ValidateAccountSecurityCode(Parent.FormattedTransactionNumberInfo, accountSecurityNo);

				if (!accountSecurityNo.IsEmpty
					&& !Parent.FormattedTransactionNumberInfo.HasErrors()
					&& !Parent.SequentialNumber.IsEmpty
					&& Parent.SequentialNumber != TransactionNumber.SequentialNumberDefault
					&& Parent.EntryNumberChanged
					&& !Parent.CanBeChangedOrDeleted(out var errMsg))
				{
					Parent.FormattedTransactionNumberInfo.AddError(Res.GetString("d9c0c932-09e2-451e-9eea-0f444919cc4f", "Transaction Number can not be changed as message(s) has been sent.\r\n{0}", errMsg));
				}

				if (!Parent.FormattedTransactionNumberInfo.HasErrors())
				{
					if (!transactionNumber.IsEmpty || declaration.ForceManualInputOfTransactionNumber)
					{
						long fountainMin, fountainMax;
						if (!transactionNumber.IsNumbersOnlyOrEmpty || transactionNumber.Length != TransactionNumber.Schema.FormattedTransactionNumberMaxLength)
						{
							Parent.FormattedTransactionNumberInfo.AddError(Res.GetString("6ee4f0d4-ede9-4cb5-887f-1d7cc1c2ae17", "Transaction Number should be composed of 14 digits numbers."));
						}
						else if (!accountSecurityNo.IsEmpty && accountSecurityNo != TransactionNumber.AccountSecurityNoDefault && transactionNumber.Substring(0, 5) != accountSecurityNo)
						{
							Parent.FormattedTransactionNumberInfo.AddError(Res.GetString("254e007b-5ff6-4a1b-b228-9e699c6f8268", "The first 5 digits should be the Account Security Code '{0}'.", accountSecurityNo));
						}
						else if (transactionNumber.Substring(5, 8) == TransactionNumber.SequentialNumberDefault)
						{
							Parent.FormattedTransactionNumberInfo.AddError(Res.GetString("41788306-df2e-4cc3-8717-64c62d9c41bd", "Transaction Number cannot end with all zeros."));
						}
						else if (Parent.SequentialNumberHasChanges && !Parent.CheckNumberFountainRange(transactionNumber.Substring(5, 8), out fountainMin, out fountainMax))
						{
							Parent.FormattedTransactionNumberInfo.AddError(Res.GetString("15b46c77-4336-42c7-9096-56577770caa5", "Sequential Number should be between {0} and {1}.", fountainMin, fountainMax));
						}
						else
						{
							var checkDigit = TransactionNumber.GetCheckDigit(transactionNumber.Substring(0, 13));
							if (ZInt.Parse(transactionNumber.Substring(13)) != checkDigit)
							{
								Parent.FormattedTransactionNumberInfo.AddError(Res.GetString("5166e6aa-9d16-4dda-a3ad-71d3420a5b85", "Transaction Number does not have a valid check (last) digit. The check digit should be {0}.", checkDigit));
							}
							else
							{
								ValidateTransactionNumberIsUnique(Parent.FormattedTransactionNumberInfo);
							}
						}
					}
				}

				ValidateHasTransactionNumbersAvailable(Parent.FormattedTransactionNumberInfo);
			}
		}

		#endregion

		#region Common Validation Methods

		void ValidateAccountSecurityCode(ZPropertyInfo propertyInfo, ZString accountSecurityCode)
		{
			if (!propertyInfo.HasErrors())
			{
				if ((accountSecurityCode == TransactionNumber.AccountSecurityNoDefault || !Regex.IsMatch(accountSecurityCode, @"^[0-9]{5}$")))
				{
					propertyInfo.AddError(Res.GetString("3a9e98f6-1e07-4761-be41-0b85d73cfd4f", "Account Security Number is not specified properly.\r\nPlease follow Registry -> {0} and specify 5 digit code.", ((IRegistryItemInternals)Registry.CACustomsDataRegistry.Instance.AccountSecurityNo).Location));
				}
			}
		}

		void ValidateHasTransactionNumbersAvailable(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors())
			{
				if (Parent.AccountSecurityCode.IsEmpty)
				{
					// nothing to report, error will be detected for security-code property
				}
				else if (Parent.EffectiveNumberFountain == null)
				{
					propertyInfo.AddError(TransactionNumberMessages.TransactionNumberRangeNotConfigured(Parent.AccountSecurityCode));
				}
				else if (Parent.NumberFountainHasDriedUp && (Parent.SequentialNumberHasChanges || Parent.SequentialNumber == TransactionNumber.SequentialNumberDefault))
				{
					propertyInfo.AddError(TransactionNumberMessages.TransactionNumberRangeIsEmpty());
				}
			}
		}

		void ValidateWHSTransactionExists(ZPropertyInfo propertyInfo, JobDeclaration declaration, ZString transactionNum)
		{
			declaration.Validation.CheckWHSTransactionExists(propertyInfo, () =>
			{
				var transactionNumber = CusEntryNumber.Load(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
				return transactionNumber != null && !transactionNumber.CE_EntryNumInfo.OriginalValue.Equals(transactionNum);
			});
		}

		void ValidateTransactionNumberIsUnique(ZPropertyInfo propertyInfo)
		{
			if (!Parent.IsUnique)
			{
				propertyInfo.AddError(TransactionNumberMessages.TransactionNumberAlreadyUsed(Parent.existingDeclaration));
			}
		}

		#endregion
	}
}
