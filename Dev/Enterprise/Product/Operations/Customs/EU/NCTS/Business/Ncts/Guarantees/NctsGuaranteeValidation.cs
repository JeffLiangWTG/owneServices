using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuaranteeValidation : EU.Business.Declaration.CommonGuaranteeValidation
	{
		public NctsGuaranteeValidation(NctsGuarantee cusBondDetail)
			: base(cusBondDetail)
		{
		}

		protected new NctsGuarantee Parent => (NctsGuarantee)base.Parent;

		protected override void CheckPW_BondNumber()
		{
			base.CheckPW_BondNumber();
			RuleC085(Parent.PW_BondNumberInfo);
			RuleTR0301();
			ValidatePW_BondType();
			EnsureCountryIsAuthorised();
			EnsureGuaranteeIsExistingAndOwnedByPrincipal();
		}

		protected void EnsureCountryIsAuthorised()
		{
			var cusGuarantee = Parent.CusGuaranteeWithSubTypeFilter;

			if (cusGuarantee != null
				&& Parent.NctsHeader is NctsHeader header)
			{
				var customoffices = header.IsPhase5 ? header.MovementHeader?.CustomsOffices : header.CustomsOffices;
				if (customoffices != null && customoffices.Count > 0)
				{
					var cusguaranteeRuleInvList = cusGuarantee.CusGuaranteeRules.Where(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.INV);
					var listOfCountryToAddInMessage = new List<ZString>();

					if (cusguaranteeRuleInvList.Any())
					{
						var listCountry = new RefCountryCollection(Parent.Factory);
						foreach (var rule in cusguaranteeRuleInvList)
						{
							var value = rule.CPR_ValueFrom;
							if (!listOfCountryToAddInMessage.Contains(value) && customoffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Data.Substring(0, 2) == value) != null)
							{
								var country = listCountry.Cast<RefCountry>().FirstOrDefault(x => x.Code == value).Description;
								Parent.PW_BondNumberInfo.AddMessageError(Res.GetString("77A88E83-69B3-4FD4-A3E8-8D96761C43B8", "This guarantee is not valid in country {0}", country));
								listOfCountryToAddInMessage.Add(value);
							}
						}
					}
				}
			}
		}

		void EnsureGuaranteeIsExistingAndOwnedByPrincipal()
		{
			if (Parent.CusGuarantee == null && !Parent.PW_BondNumber.IsEmpty)
			{
				var guaranteeMatchedWithoutPermitHolder = Parent.Factory.LoadTop1<CusGuaranteeHeader>(Parent.LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter);

				if (guaranteeMatchedWithoutPermitHolder == null)
				{
					var nctsHeader = Parent.Parent as NctsHeader;
					var movementHeader = nctsHeader?.MovementHeader;
					var isGuaranteeUsedinTIRContext = movementHeader != null && movementHeader.IsTIRDeclaration && Parent.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention;
					if (!isGuaranteeUsedinTIRContext)
					{
						Parent.PW_BondNumberInfo.AddMessageError(Res.GetString("0A6D34C0-5D2A-4660-AD9C-2FCC2F32541B", "This guarantee does not exist, and so transactions will not be managed. It is recommended to create a guarantee record for the best functionality."));
					}
				}
				else
				{
					Parent.PW_BondNumberInfo.AddMessageError(Res.GetString("D5954692-E608-4731-AB8F-757335F8F14A", "This guarantee is not owned by the selected principal."));
				}
			}
		}

		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();
			LiabiltyAmountIsMandatory(Parent.PW_BondAmountInfo);
			CheckRemainingBalanceGreaterThanPW_BondAmount(Parent.PW_BondAmountInfo);
			NctsValidationHelper.CheckPermitRuleNotFound(Parent);
		}

		protected override void CheckPW_BondType()
		{
			var parent = Parent;
			var bondTypeInfo = parent.PW_BondTypeInfo;
			var bondType = parent.PW_BondType;

			base.CheckPW_BondType();
			RuleC085(bondTypeInfo);
			RuleC086(bondTypeInfo);
			ValidatePW_BondNumber();
			ValidatePW_Password();
			ValidatePW_BondAmount();
			ValidateGuaranteeCount();

			if (bondType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(bondTypeInfo);
			}
		}

		void ValidateGuaranteeCount()
		{
			var nctsHeader = Parent.Parent as NctsHeader;

			if (nctsHeader != null)
			{
				var maxGuaranteeCount = nctsHeader.MaximumGuaranteeCount;
				var messageError = string.Format(CultureInfo.CurrentCulture, Res.GetString("43B56CC1-3B54-4F02-B223-B8ADBF1D431E", "The maximum number of guarantees allowed is {0}."), maxGuaranteeCount);
				Parent?.RemoveRowMessageError(messageError);

				if (maxGuaranteeCount > 0 && nctsHeader.Guarantees.Count > maxGuaranteeCount)
				{
					Parent.AddRowMessageError(messageError);
				}
			}
		}

		protected override void CheckPW_Password()
		{
			base.CheckPW_Password();
			RuleC086(Parent.PW_PasswordInfo);
			ValidatePW_BondType();
		}

		protected override void CheckPW_SuretyCode()
		{
			base.CheckPW_SuretyCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_SuretyCodeInfo);
		}

		void RuleC086(ZPropertyInfo zPropertyInfo)
		{
			if (Parent.PW_Password.IsEmpty && ValidationExtendMethods.GuaranteeTypesApplicable.Contains(Parent.PW_BondType) && ApplyRuleC086)
			{
				zPropertyInfo.AddMessageError(Res.GetString("44FAF05E-EC81-4DCC-8D39-279339BE12CB", "Guarantee Access Code (password) is required (C086)."));
			}
		}
		protected virtual bool ApplyRuleC086 => true;

		void RuleC085(ZPropertyInfo zPropertyInfo)
		{
			if (Parent.PW_BondNumber.IsEmpty && ValidationExtendMethods.GuaranteeTypesApplicableForRuleC085.Contains(Parent.PW_BondType))
			{
				zPropertyInfo.AddMessageError(Res.GetString("62F1D34D-FA5F-4934-9F14-3EC5A881457D", "A Guarantee Reference Number is required (C085)."));
			}
		}

		protected virtual bool ApplyRuleTR0301 => true;

		void RuleTR0301()
		{
			if (ApplyRuleTR0301)
			{
				ValidationExtendMethods.RuleTR301(Parent.PW_BondType, Parent.PW_BondNumber, Parent.PW_BondNumberInfo);
			}
		}

		void LiabiltyAmountIsMandatory(ZPropertyInfo zPropertyInfo)
		{
			if (ValidationExtendMethods.GuaranteeTypesApplicable.Contains(Parent.PW_BondType) && Parent.PW_BondAmount == ZDecimal.Zero)
			{
				zPropertyInfo.AddMessageError(Res.GetString("22E4FCB7-C03B-44BC-86D0-1CE03E8166F3", "A Guarantee Liability Amount is required."));
			}
		}

		void CheckRemainingBalanceGreaterThanPW_BondAmount(ZPropertyInfo zPropertyInfo)
		{
			var nctsGuarantee = Parent;
			var cusGuarantee = nctsGuarantee?.CusGuarantee;
			if (cusGuarantee != null)
			{
				var remainingBalance = cusGuarantee.CPH_Calc_TotalBalanceIncludingPending.Amount;
				var bondAmount = nctsGuarantee.PW_BondAmount;
				if (bondAmount > remainingBalance)
				{
					zPropertyInfo.AddMessageError(Res.GetString("7A860318-B575-4FDC-9061-E7DBDEC6B96D", "This guarantee has a remaining balance of {0} but this movement would incur a liability of {1}", remainingBalance, bondAmount));
				}
			}
		}
	}
}
