using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsDepartureMovementHeaderValidation : NctsCommonMovementHeaderValidation
	{
		public NctsDepartureMovementHeaderValidation(NctsDepartureMovementHeader parent)
			: base(parent)
		{
		}

		protected new INctsDepartureMovementHeaderValidationDecider ValidationDecider => (INctsDepartureMovementHeaderValidationDecider)base.ValidationDecider;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTirCarnetNumber();
			ValidateTirCarnetExpiryDate();
			ValidateGoodsLocationDescription();
			ValidateFromWarehouseOrgPK();
		}

		public void ValidateFromWarehouseOrgPK()
		{
			ValidateCalculatedProperty(Parent.FromWarehouseOrgPKInfo);
		}

		protected virtual void CheckFromWarehouseOrgPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.FromWarehouseOrgPKInfo);
		}

		protected override void CheckBM_OA_WarehouseAddress()
		{
			base.CheckBM_OA_WarehouseAddress();

			var parent = Parent;
			if (!parent.FromWarehouseOrgPK.IsEmpty && parent.BM_OA_WarehouseAddress.IsEmpty)
			{
				parent.BM_OA_WarehouseAddressInfo.AddWarning(Res.GetString("5a285cc4-5ad5-4217-ac79-87d344119827", "Warehouse will not be saved because no address is selected."));
			}
		}

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			var parent = Parent;
			var inBondEntryTypeInfo = parent.BM_InBondEntryTypeInfo;
			MandatoryValidation.MessageErrorIfNotEntered(inBondEntryTypeInfo);
			if (NctsHeader is NctsHeader nctsHeader)
			{
				CheckCustomsOffices(nctsHeader, inBondEntryTypeInfo);
				CheckMandatoryGuaranteeIfNeeded(nctsHeader, inBondEntryTypeInfo);
				CheckMandatoryGoodsItem(nctsHeader, inBondEntryTypeInfo);
				CheckConditionC035(nctsHeader, inBondEntryTypeInfo);
				CheckConditionC547(nctsHeader, inBondEntryTypeInfo);
				nctsHeader.CheckConditionC904(inBondEntryTypeInfo);
				CheckConditionR902(nctsHeader, inBondEntryTypeInfo);
				CheckConditionR903(nctsHeader, inBondEntryTypeInfo);
				nctsHeader.CheckConditionR909(inBondEntryTypeInfo);
				CheckConditionR911(nctsHeader, inBondEntryTypeInfo);
			}
		}

		void CheckConditionR902(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if ((ValidationDecider?.IsRuleR902Active ?? false)
				&& Parent.IsTIRDeclaration
				&& (nctsHeader.IsPhase5 ? Parent.HasTransitOffice() : nctsHeader.HasTransitOffice()))
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.R902, Res.GetString("1198A884-311B-45CE-99F2-04C8FF59049B", "Offices of Transit cannot be used.")));
			}
		}

		void CheckConditionR903(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if ((ValidationDecider?.IsRuleR903Active ?? false)
				&& Parent.IsTIRDeclaration
				&& Parent.IsSimplifiedNctsProcedure)
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.R903, Res.GetString("484AB684-8BB8-44E5-BC55-9DB0435F599F", "TIR declaration and simplified procedure cannot be used together.")));
			}
		}

		void CheckConditionR911(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if ((ValidationDecider?.IsRuleR911Active ?? false)
				&& nctsHeader.DepartureCustomsOfficeCodeCountry == Core.Constants.CountryCodes.SanMarino
				&& NctsHeaderValidationHelper.IsEuMemberState(nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture)
				&& Parent.BM_InBondEntryType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
				&& Parent.BM_InBondEntryType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories)
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.R911, Res.GetString("4146EAB9-3EAF-4678-8555-8D6C00A1A101", "Declaration Type can only be 'T2' or 'T2F'.")));
			}
		}

		void CheckConditionC035(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var departureCustomsOfficeCodeCountry = nctsHeader.IsPhase5 ? Parent.DepartureCustomsOfficeCodeCountry : nctsHeader.DepartureCustomsOfficeCodeCountry;

			if ((ValidationDecider?.IsRuleC035Active ?? false)
				&& NctsHeaderValidationHelper.IsInternalCommunityTransitProcedureType(Parent.BM_InBondEntryType)
				&& (!departureCustomsOfficeCodeCountry.IsEmpty && NctsHeaderValidationHelper.IsNonEuCommonTransitCountry(departureCustomsOfficeCodeCountry))
				&& !HasPreviousDoc())
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C035, Res.GetString("E404191E-C7CF-4596-9B43-8082B7AA6144", "Previous Administrative References are required for internal Community transit movements.")));
			}
		}

		bool HasPreviousDoc() => Parent.Header.DepartureGoodsItems.SelectMany(i => i.PreviousDocuments).Any();

		void CheckCustomsOffices(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var customsOfficeRequirementHelper = nctsHeader.IsPhase5 ? Parent.CustomsOfficeRequirementHelper : nctsHeader.CustomsOfficeRequirementHelper;
			var errors = customsOfficeRequirementHelper.Validate().Distinct();
			errors.ForEach(e => info.AddMessageError(e));
		}

		protected override void CheckBM_RL_NKForeignDestPort()
		{
			base.CheckBM_RL_NKForeignDestPort();
			var info = Parent.BM_RL_NKForeignDestPortInfo;
			if (ValidationDecider?.IsRuleC191Active ?? false)
			{
				CheckConditionC191(Parent.Header, info);
			}
			ListValidation.MessageErrorIfInvalidCode(info);
		}

		protected void CheckConditionC191(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.BH_FTZMove && Parent.PlaceOfLoading.IsEmpty)
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C191, Res.GetString("EF8637AD-A201-4930-8B89-6678CEC55D6A", "Place of Loading is required for a Safety and Security movement.")));
			}
		}

		protected override void CheckBM_InlandTransportMode()
		{
			base.CheckBM_InlandTransportMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_InlandTransportModeInfo);
		}

		protected override void CheckBM_ExportTransportMode()
		{
			base.CheckBM_ExportTransportMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_ExportTransportModeInfo);
			CheckConditionC599(NctsHeader, Parent.BM_ExportTransportModeInfo);
		}

		void CheckConditionC599(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if ((ValidationDecider?.IsRuleC599Active ?? false)
				&& nctsHeader.BH_FTZMove
				&& !NctsHeaderValidationHelper.IsEuMemberState(nctsHeader.DepartureCustomsOfficeCodeCountry)
				&& Parent.BM_ExportTransportMode.IsEmpty)
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C599, Res.GetString("0B2EF8B1-4A87-4510-8FD0-99C9E238629F", "Identity of Means of Transport at Departure is required.")));
			}
		}

		protected override void CheckBM_RN_NKTransportAtDepartureCountry()
		{
			base.CheckBM_RN_NKTransportAtDepartureCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_RN_NKTransportAtDepartureCountryInfo);
		}

		protected override void CheckBM_RN_NKTransportAtDepartureTrailer1Nationality()
		{
			base.CheckBM_RN_NKTransportAtDepartureTrailer1Nationality();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo);
		}

		protected override void CheckBM_RN_NKTransportAtDepartureTrailer2Nationality()
		{
			base.CheckBM_RN_NKTransportAtDepartureTrailer2Nationality();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo);
		}

		protected override void CheckBM_RN_NKTOLCarrierNationality()
		{
			base.CheckBM_RN_NKTOLCarrierNationality();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_RN_NKTOLCarrierNationalityInfo);
		}

		protected override void CheckBM_TOLCarrierCode()
		{
			base.CheckBM_TOLCarrierCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_TOLCarrierCodeInfo);
		}

		protected override void CheckBM_BTAIndicator()
		{
			base.CheckBM_BTAIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_BTAIndicatorInfo);
			NctsHeader.CheckConditionC587(Parent.BM_BTAIndicatorInfo);
		}

		protected override void CheckBM_MethodOfPayment()
		{
			base.CheckBM_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_MethodOfPaymentInfo);
		}

		protected override void CheckBM_GS_NKCusAgent()
		{
			base.CheckBM_GS_NKCusAgent();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_GS_NKCusAgentInfo);
		}

		protected override void CheckBM_ExportDate()
		{
			base.CheckBM_ExportDate();
			if (IsExportDateMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ExportDateInfo, messagePrefix: ExportDateMandatoryMessagePrefix);
			}
		}

		protected virtual ZBool IsExportDateMandatory => Parent.IsSimplifiedNctsProcedure;

		protected virtual ZString ExportDateMandatoryMessagePrefix => ZString.Empty;

		protected override void CheckBM_PlaceOfUnloading()
		{
			base.CheckBM_PlaceOfUnloading();
			CheckBM_PlaceOfUnloadingMandatory();
		}

		protected virtual void CheckBM_PlaceOfUnloadingMandatory()
		{
			CheckConditionC589(NctsHeader, Parent.BM_PlaceOfUnloadingInfo);
		}

		void CheckConditionC589(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if ((ValidationDecider?.IsRuleC589Active ?? false)
				&& nctsHeader.BH_FTZMove
				&& nctsHeader.PlaceOfUnloadingCode.IsEmpty)
			{
				var specificCircumstanceIndicator = Parent.BM_BTAIndicator;
				if (specificCircumstanceIndicator != SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies
					&& specificCircumstanceIndicator != SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators)
				{
					info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C589, Res.GetString("F47981D8-708C-4B52-B599-A901A1213D01", "Place of Unloading is required for a Safety and Security movement.")));
				}
			}
		}

		protected override void CheckBM_RL_NKDestinationPort()
		{
			base.CheckBM_RL_NKDestinationPort();
			var parent = Parent;
			var info = parent.BM_RL_NKDestinationPortInfo;
			ListValidation.MessageErrorIfInvalidCode(info);
			if (!parent.BM_RL_NKDestinationPort.IsEmpty)
			{
				if (parent.HasGoodsItemsWithCountryOfDestination)
				{
					info.AddMessageError(Res.GetString("EF976D56-6E00-4BB5-9DC4-5A48F4DBAD2E", "Destination Country must be filled either in Declaration tab or Goods tab but not both."));
				}
			}
		}

		protected override void CheckBM_GrossWeightUQ()
		{
			base.CheckBM_GrossWeightUQ();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.BM_GrossWeightUQInfo);
		}

		protected override void CheckBM_CustomsOfficeAtBorder()
		{
			base.CheckBM_CustomsOfficeAtBorder();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_CustomsOfficeAtBorderInfo);
		}

		public void ValidateTirCarnetNumber()
		{
			ValidateCalculatedProperty(Parent.TirCarnetNumberInfo);
		}

		protected void CheckTirCarnetNumber()
		{
			if (Parent.IsTIRDeclaration)
			{
				CheckTirCarnetNumberCore();
			}
		}

		protected virtual void CheckTirCarnetNumberCore()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TirCarnetNumberInfo);
		}

		public void ValidateTirCarnetExpiryDate()
		{
			ValidateCalculatedProperty(Parent.TirCarnetExpiryDateInfo);
		}

		protected void CheckTirCarnetExpiryDate()
		{
			CheckTirCarnetExpiryDateMandatory();
		}

		protected virtual void CheckTirCarnetExpiryDateMandatory()
		{
			if (Parent.IsTIRDeclaration)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TirCarnetExpiryDateInfo);
			}
		}

		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		protected virtual void CheckMandatoryGuaranteeIfNeeded(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.GetEffectiveGuarantees().Count == 0)
			{
				info.AddMessageError(Res.GetString("2DB02FF3-5733-4ECE-B850-448979A71CB7", "You need to supply a valid guarantee."));
			}

			if (ShouldCheckGuaranteeForTIRDeclaration && !nctsHeader.IsPhase5)
			{
				CheckGuaranteeForTIRDeclaration(nctsHeader, info, GuaranteeForTIRDeclarationRuleCode);
			}
		}

		void CheckGuaranteeForTIRDeclaration(NctsHeader nctsHeader, ZPropertyInfo info, string ruleCode)
		{
			if (Parent.IsTIRDeclaration
				&& !HasTirGuaranteeType(nctsHeader))
			{
				info.AddMessageError(Res.GetString("D26FC7D9-1BAE-4BD0-BDBC-8F747A83DA5E", "A TIR declaration requires a guarantee of type B (TIR). ({0})", ruleCode));
			}
		}

		bool HasTirGuaranteeType(NctsHeader nctsHeader) => nctsHeader.GetEffectiveGuarantees().Cast<NctsGuarantee>().Any(g => g.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention);

		protected virtual bool ShouldCheckGuaranteeForTIRDeclaration => true;

		protected string GuaranteeForTIRDeclarationRuleCode => Rules_C_Conditions.Codes.C900;

		void CheckMandatoryGoodsItem(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (ShouldCheckMandatoryGoodsItem && nctsHeader.TotalNumberOfItems == 0)
			{
				info.AddMessageError(MandatoryGoodsItemMessage);
			}
		}

		protected virtual bool ShouldCheckMandatoryGoodsItem => true;

		protected virtual string MandatoryGoodsItemMessage => Res.GetString("51F1DE65-5F14-4C1E-83BF-D7AB8C38DC09", "You need to supply at least one goods item.");

		protected virtual void CheckConditionC547(NctsHeader nctsHeader, ZPropertyInfo inBondEntryTypeInfo)
		{
			if ((ValidationDecider?.IsRuleC547Active ?? false) && nctsHeader.BH_FTZMove)
			{
				var specificCircumstanceIndicator = Parent.BM_BTAIndicator;
				var goodsItems = Parent.Header.DepartureGoodsItems;
				if ((specificCircumstanceIndicator.IsEmpty || specificCircumstanceIndicator != SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments)
					&& !CommercialReferenceNumberPresentAtAnyLevel()
					&& goodsItems.Count > 0
					&& goodsItems[0].SupportingDocuments.Count == 0)
				{
					inBondEntryTypeInfo.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C547, Res.GetString("EFC1FEB2-B11E-4270-8C00-5E2C0E586238", "Transport document must be present on the first goods item.")));
				}
			}
		}

		bool CommercialReferenceNumberPresentAtAnyLevel()
		{
			return Parent.GoodsItems.Any(i => !i.BY_CommercialReferenceNumber.IsEmpty) || !Parent.BM_AdditionalText.IsEmpty;
		}

		public void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		protected virtual void CheckGoodsLocationDescription()
		{
		}
	}
}
