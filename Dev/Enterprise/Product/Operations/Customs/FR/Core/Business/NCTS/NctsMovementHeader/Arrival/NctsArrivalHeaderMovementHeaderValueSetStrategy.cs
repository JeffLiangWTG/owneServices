using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	internal class NctsArrivalHeaderMovementHeaderValueSetStrategy : IValueSetStrategy
	{
		public NctsArrivalHeaderMovementHeaderValueSetStrategy(NctsArrivalMovementHeader arrivalMovementHeader)
		{
			this.arrivalMovementHeader = arrivalMovementHeader;
		}

		readonly NctsArrivalMovementHeader arrivalMovementHeader;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case NctsArrivalMovementHeader.Schema.BM_GONumber:
					ChangeValueInBMLocationOfGoodsCode();
					break;
				case NctsArrivalMovementHeader.Schema.BM_LocationOfGoodsCode:
					ChangeNctsPropertiesWhenBMLocationOfGoodsCodeHasChanged();
					break;
			}
		}

		void ChangeValueInBMLocationOfGoodsCode()
		{
			var bmLocationOfGoodsCode = arrivalMovementHeader.BM_LocationOfGoodsCode;
			if (arrivalMovementHeader.IsSimplifiedNctsProcedure)
			{
				var ncts = arrivalMovementHeader.Header;
				if (ncts != null && ncts.DestinationTrader != null && ncts.Declarant != null)
				{
					var permitHolder = ncts.Declarant.Organisation;
					var appliesTo = ncts.DestinationTrader.E2_OA_Address;

					if (permitHolder != null && !appliesTo.IsEmpty)
					{
						var authorisations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(ncts.Factory, ncts.CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit }, ZDate.Today, new[] { permitHolder.PK }, new[] { appliesTo });
						if (authorisations != null && authorisations.Any())
						{
							bmLocationOfGoodsCode = authorisations.FirstOrDefault().CPH_Number.SubstringSafe(0, 17);
						}
					}
				}
			}
			arrivalMovementHeader.BM_LocationOfGoodsCode = bmLocationOfGoodsCode;
		}

		void ChangeNctsPropertiesWhenBMLocationOfGoodsCodeHasChanged()
		{
			var factory = arrivalMovementHeader.Factory;
			var ncts = arrivalMovementHeader.Header;

			var number = arrivalMovementHeader.BM_LocationOfGoodsCode;
			var type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			var country = Core.Constants.CountryCodes.France;
			var authHeader = CusAuthorisationHeader.Loader
							.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(factory, number, type, country)
							.OrderBy(x => x.CPH_StartDate).FirstOrDefault();

			if (authHeader != null)
			{
				var authRule = authHeader.CusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.OFC);
				if (authRule != null)
				{
					var destinationCustomsOfficeCodeForArrival = authRule.CPR_ValueFrom.Left(ncts.DestinationCustomsOfficeCodeForArrivalInfo.MaxLength);
					if (ncts.IsPhase5)
					{
						arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = destinationCustomsOfficeCodeForArrival;
					}
					else
					{
						ncts.DestinationCustomsOfficeCodeForArrival = destinationCustomsOfficeCodeForArrival;
					}
				}
				ncts.DestinationTrader.OrganisationPK = authHeader.CPH_OA_AppliesTo_ZAddress.OrgPK;
				ncts.Declarant.OrganisationPK = authHeader.CPH_OH_PermitHolder;
			}
		}
	}
}
