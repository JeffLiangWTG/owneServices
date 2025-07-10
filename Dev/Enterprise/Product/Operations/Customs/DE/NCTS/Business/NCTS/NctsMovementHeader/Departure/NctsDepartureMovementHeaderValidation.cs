using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDepartureMovementHeaderValidation : NctsDepartureMovementHeaderPhase5Validation
	{
		public NctsDepartureMovementHeaderValidation(NctsDepartureMovementHeader parent)
			: base(parent)
		{
		}

		protected override void CheckBM_BTAIndicator()
		{
			base.CheckBM_BTAIndicator();

			var indicator = Parent.BM_BTAIndicator;
			if (indicator == SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators
				&& (!HasAEOOrgCusCode(Parent.Header.Principal.Organisation)
					|| !HasAEOOrgCusCode(Parent.Header.SecurityConsignor.Organisation)))
			{
				Parent.BM_BTAIndicatorInfo.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C587, Res.GetString("c760a561-7856-4e37-b777-92de0acac99f", "'E - Authorized economic operators' is only allowed if Principal and Security Consignor are Authorized Economic Operators with AEO-S or AEO-F Certificate.")));
			}

			bool HasAEOOrgCusCode(OrgHeader org)
			{
				var certificatePrefix = new HashSet<string>() { "AEOS", "AEOF" };
				return org != null && org.CustomsCodes.Cast<OrgCusCode>().Any(c => c.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator && certificatePrefix.Contains(c.OK_CustomsRegNo.SubstringSafe(0, 4)));
			}
		}

		protected override void CheckBM_PlaceOfUnloadingMandatory()
		{
			base.CheckBM_PlaceOfUnloadingMandatory();
			var parent = Parent;
			var header = parent.Header;
			if (header != null)
			{
				if (header.BH_FTZMove
					&& header.PlaceOfUnloadingCode.IsEmpty
					&& parent.BM_BTAIndicator != SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators)
				{
					parent.BM_PlaceOfUnloadingInfo.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C589, Res.GetString("19cae9a7-1864-422a-b19c-6221b271294b", "Place of Unloading is required for a Safety and Security movement.")));
				}
			}
		}

		protected override void CheckBM_PaperlessInbondNum()
		{
			base.CheckBM_PaperlessInbondNum();
			var targetInfo = Parent.BM_PaperlessInbondNumInfo;
			if (!targetInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			var parent = Parent;
			if (parent.BM_RL_NKDestinationPort == Core.Constants.CountryCodes.SanMarino
				&& !parent.BM_InBondEntryType.In(new ZString[] { NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure,
					NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories }))
			{
				parent.BM_InBondEntryTypeInfo.AddMessageError(Res.GetString("842E4825-C9B3-4732-9C8D-F5DA71851E8D", "Country/Region of Destination San Marino requires Declaration Type 'T2' or 'T2F'."));
			}
		}
	}
}
