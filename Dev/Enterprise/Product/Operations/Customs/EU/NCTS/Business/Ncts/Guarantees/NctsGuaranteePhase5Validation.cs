using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using GuaranteeCodes = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuaranteePhase5Validation : EU.Business.Declaration.CommonGuaranteeValidation
	{
		public NctsGuaranteePhase5Validation(NctsGuarantee cusBondDetail)
			: base(cusBondDetail)
		{
		}

		const decimal MaximumInAmountInGuaranteeTypeFlatRateVoucher = 10000;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGuaranteesRuleNR0005();
			ValidateGuaranteesRuleTR0093();
			CheckRuleGuaranteeTR0096();
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();

			var parent = Parent;
			var movementHeader = parent.NctsHeader.MovementHeader;
			var bondTypeInfo = parent.PW_BondTypeInfo;
			var bondType = parent.PW_BondType;

			if (!IsBondTypePR15() && NctsHeaderValidationHelper.IsConditionRP16(parent.NctsHeader))
			{
				bondTypeInfo.AddError(Res.GetString("5C4FD663-7E56-4195-A3C2-2AEDE4BF98C3", "[RP16] When Authorization Code is ACR and Declaration Type is not equal to TIR, Then Guarantee Type (0 or 1 or 2 or 4 or 9) is required."));
			}
			else if (CheckBondTypeIsEmptyIsActive)
			{
				MandatoryValidation.MessageErrorIfNotEntered(bondTypeInfo);
			}

			CheckRuleR0900();
			CheckPW_BondType_R0900_2And3();

			bool IsBondTypePR15()
			{
				return bondType == GuaranteeCodes.GuaranteeWaiver
				|| bondType == GuaranteeCodes.ComprehensiveGuarantee
				|| bondType == GuaranteeCodes.IndividualGuaranteeByGuarantor
				|| bondType == GuaranteeCodes.FlatRateVoucher
				|| bondType == GuaranteeCodes.IndividualGuaranteeWithMultipleUsage;
			}

			void CheckRuleR0900()
			{
				if (Parent.ValidationDecider is not INctsGuaranteeDeparturePhase5ValidationDecider { IsRuleR0900Active: true })
				{
					return;
				}

				if (movementHeader.IsTIRDeclaration)
				{
					ValidateTIRDeclaration();
				}
				else
				{
					ValidateNonTIRDeclaration();
				}
			}

			void ValidateTIRDeclaration()
			{
				if (bondType != GuaranteeCodes.MovementsCarriedUnderTheTirConvention)
				{
					bondTypeInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0900aMessage);
				}
			}

			void ValidateNonTIRDeclaration()
			{
				var customsOfficeOfDep = movementHeader.DepartureCustomsOfficeCodeCountry;
				if (customsOfficeOfDep.IsEmpty)
				{
					return;
				}
				if (UniversalLookupsHelper.GetCL010CountryCodes(movementHeader.Factory).ContainsCode(customsOfficeOfDep) || NctsHeaderValidationHelper.IsCountryAndorraOrSanMarino(customsOfficeOfDep))
				{
					var cl230Code = UniversalLookupsHelper.GetCL230EUGuaranteeTypeEUNonTIR(movementHeader.Factory);
					if (!cl230Code.ContainsCode(bondType))
					{
						bondTypeInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0900bMessage);
					}
				}
				else
				{
					var cl229Code = UniversalLookupsHelper.GetCL229EUGuaranteeTypeCTC(movementHeader.Factory);
					if (!cl229Code.ContainsCode(bondType))
					{
						bondTypeInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0900cMessage);
					}
				}
			}

			void CheckPW_BondType_R0900_2And3()
			{
				if (parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider)
				{
					if (departureValidationDecider.IsRuleR0900_2Active && (movementHeader?.BM_InBondEntryType ?? ZString.Empty) == NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration && bondType != GuaranteeCodes.MovementsCarriedUnderTheTirConvention)
					{
						bondTypeInfo.AddMessageError(Res.GetString("8D3BD131-4C8F-41D4-9CB0-DA3363C3ACB3", "[R0900-2] Guarantee Type B must be declared when Declaration Type = TIR"));
					}
					else if (departureValidationDecider.IsRuleR0900_3Active && (movementHeader?.BM_InBondEntryType ?? ZString.Empty) != NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration && bondType == GuaranteeCodes.MovementsCarriedUnderTheTirConvention)
					{
						bondTypeInfo.AddMessageError(Res.GetString("9899B768-63BD-4C46-9779-6002067C86D2", "[R0900-3] Guarantee Type B must be only declared if Declaration Type = TIR"));
					}
				}
			}
		}

		protected virtual bool CheckBondTypeIsEmptyIsActive => true;

		protected override void CheckPW_BondAmount()
		{
			ExecuteActionBasedOnRuleC0085_2(() =>
			{
				base.CheckPW_BondAmount();

				var parent = Parent;
				var header = parent.NctsHeader;

				MandatoryValidation.CheckNotNegative(Parent.PW_BondAmountInfo);

				if ((ValidationRuleConfiguration?.IsRuleTR0055Active ?? false) && parent.PW_BondType == GuaranteeCodes.FlatRateVoucher && parent.PW_BondAmount > MaximumInAmountInGuaranteeTypeFlatRateVoucher)
				{
					parent.PW_BondAmountInfo.AddMessageError(Res.GetString("276BEDCB-CB8A-4F30-80BF-FD7F986543EA", "[TR0055] For each guarantee of type 4, a maximum of 10,000€ can be covered"));
				}

				if (parent.PW_BondAmount.IsEmpty && NctsHeaderValidationHelper.IsConditionRP16(header))
				{
					parent.PW_BondAmountInfo.AddWarning(Res.GetString("4C7B8AC8-158A-4831-B5EF-F7C168AB02D0", "[RP16] You have not entered Liability Amount (Guarantee Amount)."));
				}

				NctsValidationHelper.CheckPermitRuleNotFound(parent);
				CheckPW_BondAmount_NR0067();
			});

			CheckPW_BondAmount_NR0064();
		}

		void CheckPW_BondAmount_NR0064()
		{
			var parent = Parent;

			if (parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider)
			{
				if (departureValidationDecider.IsRuleNR0064Active && IsGuaranteeTypeWithReference(parent.PW_BondType))
				{
					var rulePrefix = ValidationRuleConfiguration.Messages.NR0064RuleCode.GetRuleCodeMessagePrefix(true);
					MandatoryValidation.MessageErrorIfNotEntered(parent.PW_BondAmountInfo, Res.GetString("9069D6F6-584B-42D9-ACD4-9723BCD1E596", "Liability Amount"), messagePrefix: rulePrefix);
				}
			}
		}

		protected new NctsGuarantee Parent => (NctsGuarantee)base.Parent;

		protected virtual bool IsGuaranteeTypeWithReferenceC0085(ZString guaranteeType)
		{
			switch (guaranteeType)
			{
				case GuaranteeCodes.GuaranteeWaiver:
				case GuaranteeCodes.ComprehensiveGuarantee:
				case GuaranteeCodes.IndividualGuaranteeByGuarantor:
				case GuaranteeCodes.CashDepositGuarantee:
				case GuaranteeCodes.FlatRateVoucher:
				case GuaranteeCodes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur:
				case GuaranteeCodes.IndividualGuaranteeWithMultipleUsage:
					return true;
				default:
					return false;
			}
		}

		bool IsGuaranteeTypeWithReferenceC0085_1(ZString guaranteeType)
		{
			switch (guaranteeType)
			{
				case GuaranteeCodes.GuaranteeWaiver:
				case GuaranteeCodes.ComprehensiveGuarantee:
				case GuaranteeCodes.IndividualGuaranteeByGuarantor:
				case GuaranteeCodes.FlatRateVoucher:
				case GuaranteeCodes.IndividualGuaranteeWithMultipleUsage:
					return true;
				default:
					return false;
			}
		}

		bool IsGuaranteeTypeWithReference(ZString guaranteeType)
		{
			switch (guaranteeType)
			{
				case GuaranteeCodes.GuaranteeWaiver:
				case GuaranteeCodes.ComprehensiveGuarantee:
				case GuaranteeCodes.IndividualGuaranteeByGuarantor:
				case GuaranteeCodes.CashDepositGuarantee:
				case GuaranteeCodes.FlatRateVoucher:
					return true;
				default:
					return false;
			}
		}

		protected override void CheckPW_BondNumber()
		{
			ExecuteActionBasedOnRuleC0085_2(() =>
			{
				base.CheckPW_BondNumber();

				var parent = Parent;
				var header = parent.NctsHeader;

				if (header != null && header.IsDepartureMovement)
				{
					var bondNumber = parent.PW_BondNumber;
					var departureValidationDecider = parent.ValidationDecider as INctsGuaranteeDeparturePhase5ValidationDecider;
					if (bondNumber.IsEmpty)
					{
						if ((departureValidationDecider?.IsRuleC0085Active ?? false) && IsGuaranteeTypeWithReferenceC0085(parent.PW_BondType))
						{
							parent.PW_BondNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0085Message);
						}

						if ((departureValidationDecider?.IsRuleC0085_1Active ?? false) && IsGuaranteeTypeWithReferenceC0085_1(parent.PW_BondType))
						{
							parent.PW_BondNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0085_1Message);
						}

						if (ShouldCheckRuleC0086())
						{
							parent.PW_BondNumberInfo.AddMessageError(Res.GetString("B0F163E5-B520-4270-8115-1E48AB723C1D", "[C0086] You have not entered a Guarantee Reference Number (GRN)."));
						}
					}
					else
					{
						CheckRuleTR0065();
						if (departureValidationDecider?.IsRuleR0318Active ?? false)
						{
							bondNumber = BondNumberForR0318Check;

							var isNotAlphanumeric = !IsBondNumberAlphanumericData(bondNumber);
							if (parent.PW_BondType == GuaranteeCodes.FlatRateVoucher)
							{
								if (isNotAlphanumeric || bondNumber.Length != 24)
								{
									parent.PW_BondNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0318aMessage);
								}
							}
							else if (isNotAlphanumeric || bondNumber.Length != 17)
							{
								parent.PW_BondNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0318bMessage);
							}
						}
						var departureMovementHeader = header.MovementHeader;

						if ((departureValidationDecider?.IsRuleR0900_1Active ?? false)
						&& departureMovementHeader.BM_InBondEntryType == NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration && departureMovementHeader.Guarantees.Count > 1)
						{
							parent.PW_BondNumberInfo.AddMessageError(Res.GetString("15B34EBA-C5FB-4490-BA7B-B11C7D797DDD", "[R0900-1] Only one Guarantee must be declared when Declaration Type = TIR"));
						}

						if ((departureValidationDecider?.IsRuleTR0019Active ?? false) &&
							departureMovementHeader.Guarantees.Except(parent).Cast<NctsGuarantee>().Any(x => x.PW_BondNumber.Equals(bondNumber)))
						{
							parent.PW_BondNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.TR0019Message);
						}
					}
				}
			});
		}

		protected virtual ZString BondNumberForR0318Check => Parent.PW_BondNumber;

		protected override void CheckPW_RX_NKCurrency()
		{
			base.CheckPW_RX_NKCurrency();

			if (Parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider
				&& departureValidationDecider.IsRuleB1898_1ActiveForPW_RX_NKCurrency
				&& Parent.IsInPhase5TransitionPeriod
				&& Parent.PW_BondAmount > 0
				&& Parent.PW_RX_NKCurrency.IsEmpty)
			{
				Parent.PW_RX_NKCurrencyInfo.AddMessageError(ValidationRuleConfiguration.Messages.B1898_1Message);
			}

			CheckPW_RX_NKCurrency_B2101();
			CheckPW_RX_NKCurrency_NR0065();
			CheckPW_RX_NKCurrency_TR0089();
		}

		void CheckPW_RX_NKCurrency_B2101()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider
				&& departureValidationDecider.IsRuleB2101Active
				&& !parent.IsInPhase5TransitionPeriod
				&& parent.PW_RX_NKCurrency.IsEmpty)
			{
				parent.PW_RX_NKCurrencyInfo.AddMessageError(ValidationRuleConfiguration.Messages.B2101CurrencyMessage);
			}
		}

		void CheckPW_RX_NKCurrency_NR0065()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider)
			{
				if (!parent.PW_BondAmount.IsEmpty
					&& departureValidationDecider.IsRuleNR0065Active
					&& IsGuaranteeTypeWithReference(parent.PW_BondType))
				{
					var rulePrefix = ValidationRuleConfiguration.Messages.NR0065RuleCode.GetRuleCodeMessagePrefix(true);
					MandatoryValidation.MessageErrorIfNotEntered(parent.PW_RX_NKCurrencyInfo, messagePrefix: rulePrefix);
				}
			}
		}

		void CheckRuleTR0065()
		{
			if (Parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider
				&& departureValidationDecider.IsRuleTR0065Active)
			{
				var grn = Parent.PW_BondNumber;
				const string regexString = "^[0-9]{2}(?<CountryCode>[A-Z]{2})([A-Z0-9]{12}|[A-Z0-9]{19})(?<CheckDigit>[0-9])$";

				if (Regex.IsMatch(grn, regexString))
				{
					var match = Regex.Match(grn, regexString);
					if (!MRNAndGRNFormatValidatorHelper.IsCountryCodeValid(match.Groups["CountryCode"].Value, Parent.Factory))
					{
						Parent.PW_BondNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.TR0065NotValidMessage);
					}
					else if (!MRNAndGRNFormatValidatorHelper.IsGRNDigitValid(grn.SubstringSafe(0, 17)).isGRNDigitValid)
					{
						Parent.PW_BondNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.TR0065CheckDigitNotValidMessage);
					}
				}
			}
		}

		protected override void CheckPW_BondNumber2()
		{
			base.CheckPW_BondNumber2();

			var parent = Parent;
			var header = parent.NctsHeader;

			if (header != null)
			{
				var bondNumber2 = parent.PW_BondNumber2;
				var departureValidationDecider = parent.ValidationDecider as INctsGuaranteeDeparturePhase5ValidationDecider;

				if ((departureValidationDecider?.IsRuleC0130Active ?? false) &&
					parent.PW_BondType == GuaranteeCodes.GuaranteeNotRequiredForCertainPublicBodies)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.PW_BondNumber2Info, messagePrefix: "[C0130] ", propertyDescription: Res.GetString("0FC5DA38-1754-4340-A169-9645E84EE474", "Other Guarantee Reference Number"));
				}

				if (!bondNumber2.IsEmpty &&
					(departureValidationDecider?.IsRuleTR0019Active ?? false) &&
					header.IsDepartureMovement &&
					header.MovementHeader.Guarantees.Except(parent).Cast<NctsGuarantee>().Any(x => x.PW_BondNumber2.Equals(bondNumber2)))
				{
					parent.PW_BondNumber2Info.AddMessageError(Res.GetString("09A6D675-C7A2-43B3-AFB5-E8AFAA471AE1", "[TR0019] Duplicate Other Guarantee Ref. No. is entered."));
				}
			}
		}

		protected override void CheckPW_Password()
		{
			ExecuteActionBasedOnRuleC0085_2(() =>
			{
				base.CheckPW_Password();
				var parent = Parent;
				var header = parent.NctsHeader;
				if (parent.PW_Password.IsEmpty
					&& (header?.IsPhase5Departure ?? false)
					&& ShouldCheckRuleC0086())
				{
					parent.PW_PasswordInfo.AddMessageError(Res.GetString("F860966F-D3B7-4C56-BF95-CA4A5E97858E", "[C0086] You have not entered a Access code (GAC)."));
				}

				if ((ValidationRuleConfiguration?.IsRuleTR0056Active ?? false) && parent.PW_BondType == GuaranteeCodes.FlatRateVoucher && parent.PW_Password.IsEmpty)
				{
					parent.PW_PasswordInfo.AddMessageError(Res.GetString("D6349046-0B0E-4D5C-8F91-2F25A329E009", "[TR0056] You have not entered an Access Code (GAC)"));
				}

				CheckPW_Password_C0086_1();
			});
		}

		protected override void CheckPW_BondFiledPort()
		{
			ExecuteActionBasedOnRuleC0085_2(() =>
			{
				base.CheckPW_BondFiledPort();
				ValidateBondFiledPortForRuleNR0014();
			});
		}

		protected override void CheckPW_Status()
		{
			base.CheckPW_Status();

			var parent = Parent;
			if (parent.NctsHeader?.IsPhase5Departure ?? false)
			{
				var messageError = Res.GetString("E0E22393-840E-4AFD-AFBC-D6BE15FC9BE8", "Since creation of this Declaration one or more Exchange Rate(s) of Currencies in Goods Items might have changed. Please calculate Liability Amount with untick and tick on checkbox \"Override\" again.");
				parent.RemoveRowMessageError(messageError);

				if (parent.PW_Status == NctsGuarantee.DirtyStatus)
				{
					parent.AddRowMessageError(messageError);
				}
			}
		}

		void ExecuteActionBasedOnRuleC0085_2(Action action)
		{
			if (!Parent.IsGuaranteeReferenceFieldsReadOnlyDueToRuleC0085_2)
			{
				action();
			}
		}

		void ValidateBondFiledPortForRuleNR0014()
		{
			if (Parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider && departureValidationDecider.IsRuleNR0014Active)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondFiledPortInfo, messagePrefix: "[NR0014] ");
			}
		}

		void ValidateGuaranteesRuleTR0093()
		{
			var parent = Parent;
			var ruleConfiguration = ValidationRuleConfiguration;

			if (ruleConfiguration == null)
			{
				return;
			}

			var error = ruleConfiguration.Messages.TR0093RuleCode;
			parent.ClearRowNotificationsContaining(error);

			if (Parent.ValidationDecider is not INctsGuaranteeDeparturePhase5ValidationDecider
				{
					IsRuleTR0093Active: true,
				})
			{
				return;
			}

			var guaranteeHeader = parent.CusGuarantee;

			if (guaranteeHeader is null)
			{
				return;
			}

			var totalBalanceIncludingPendingAndBondAmount = guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal - parent.PW_BondAmount;
			var openingBalance = guaranteeHeader.CPH_Calc_OpeningBalance;
			if (totalBalanceIncludingPendingAndBondAmount < 0)
			{
				parent.AddRowWarning(ruleConfiguration.Messages.TR0093Message(totalBalanceIncludingPendingAndBondAmount, guaranteeHeader.CPH_UnitOfMeasure));
			}
		}

		void ValidateGuaranteesRuleNR0005()
		{
			var parent = Parent;
			var ruleConfiguration = ValidationRuleConfiguration;
			if (ruleConfiguration != null)
			{
				var error = ruleConfiguration.Messages.NR0005Message;
				parent.ClearRowNotificationsContaining(error);
				if (Parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider
					&& departureValidationDecider.IsRuleNR0005Active
					&& parent.NctsHeader.MovementHeader is NctsDepartureMovementHeader deparetureMovementHeader)
				{
					var representative = deparetureMovementHeader.Representative;
					var hasRepresentative = !representative.IsEmpty;
					if (hasRepresentative && ValidationExtendMethods.GuaranteeTypesApplicable.Contains(parent.PW_BondType))
					{
						if (parent.CusGuaranteeWithSubTypeFilter is CusGuaranteeHeader cusGuarantee)
						{
							var accessCode = parent.PW_Password;
							var activeContacts = representative.Organisation.GetActiveContacts().Cast<OrgContact>().ToArray();
							if (!activeContacts.Any(ac => (ac.OC_ContactName.EqualsIgnoringCase(cusGuarantee.MainAccessPersonName) && cusGuarantee.MainAccessCode == accessCode) ||
														  cusGuarantee.AdditionalAccessCodes.Any(x => x.CPR_Description.EqualsIgnoringCase(ac.OC_ContactName) && x.CPR_ValueFrom == accessCode)))
							{
								parent.AddRowMessageError(error);
							}
						}
					}
				}
			}
		}

		ValidationRuleConfiguration ValidationRuleConfiguration => Parent?.NctsHeader?.Configuration?.ValidationRuleConfiguration;

		bool ShouldCheckRuleC0086()
		{
			var bondType = Parent.PW_BondType;
			return (Parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider
					&& departureValidationDecider.IsRuleC0086Active)
					&& (bondType == GuaranteeCodes.GuaranteeWaiver
					|| bondType == GuaranteeCodes.ComprehensiveGuarantee
					|| bondType == GuaranteeCodes.IndividualGuaranteeByGuarantor
					|| bondType == GuaranteeCodes.FlatRateVoucher
					|| bondType == GuaranteeCodes.IndividualGuaranteeWithMultipleUsage
					);
		}

		bool IsBondNumberAlphanumericData(ZString bondNumber)
		{
			var pattern = (NoResString)@"^[A-Za-z0-9]+$";
			var regex = new Regex(pattern);
			return bondNumber.IsEmpty || regex.IsMatch(bondNumber);
		}

		void CheckPW_BondAmount_NR0067()
		{
			var parent = Parent;
			if (parent.ValidationDecider is IRuleNR0067Decider decider && decider.IsActive && parent.PW_BondAmount.IsEmpty && parent.IsGuaranteeTypeWithAmountNR0067)
			{
				parent.PW_BondAmountInfo.AddMessageError(parent.NctsHeader.Configuration.ValidationRuleConfiguration.Messages.NR0067Message);
			}
		}

		void CheckPW_RX_NKCurrency_TR0089()
		{
			var parent = Parent;
			if (parent.ValidationDecider is IRuleTR0089Decider decider && decider.IsActive && parent.NctsHeader is NctsHeader header)
			{
				var targetInfo = parent.PW_RX_NKCurrencyInfo;
				var currency = parent.PW_RX_NKCurrency;
				if (!currency.IsEmpty && currency != header.LocalCurrency
					&& header.EffectiveMessageStatus.In(new ZString[] { NctsMessageStatusList.Codes.DepartureDeclarationNotSent, NctsMessageStatusList.Codes.Rejected, LogicalStatusList.Codes.Invalid, LogicalStatusList.Codes.Failed, LogicalStatusList.Codes.Error, ZString.Empty })
					&& GetEffectiveExchangeRate(currency) == null)
				{
					targetInfo.AddMessageError(parent.NctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0089Message(currency));
				}
			}

			RefExchangeRate GetEffectiveExchangeRate(ZString currencyCode)
			{
				return new RefExchangeRate.Loader(parent.Factory).GetEffectiveRateOn(ZDate.Today, currencyCode, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			}
		}

		void CheckPW_Password_C0086_1()
		{
			var parent = Parent;
			var header = parent.NctsHeader;

			if (Parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider && departureValidationDecider.IsRuleC0086_1Active)
			{
				if (parent.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor && parent.PW_Password.IsEmpty)
				{
					parent.PW_PasswordInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0086_1aMessage);
				}

				if (!parent.IsComprehensiveGuaranteeOrIndividualGuaranteeByGuarantor && !parent.PW_Password.IsEmpty)
				{
					parent.PW_PasswordInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0086_1bMessage);
				}
			}
		}

		void CheckRuleGuaranteeTR0096()
		{
			if (ValidationRuleConfiguration != null)
			{
				var error = ValidationRuleConfiguration.Messages.TR0096Message;
				Parent.ClearRowNotificationsContaining(error);
				if (Parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider
							&& departureValidationDecider.IsRuleTR0096Active && !Parent.PW_Override)
				{
					var hasMissingDuty = Parent.NctsHeader.Bills.SelectMany(bill => bill.GoodsItems).Any(goodsItem => !goodsItem.Fees.Any(fee => fee.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.Duty));

					if (hasMissingDuty)
					{
						Parent.AddRowMessageError(error);
					}
				}
			}
		}
	}
}
