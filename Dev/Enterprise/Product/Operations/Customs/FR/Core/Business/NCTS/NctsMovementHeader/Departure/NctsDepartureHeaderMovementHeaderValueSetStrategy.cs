using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	internal class NctsDepartureHeaderMovementHeaderValueSetStrategy : IValueSetStrategy
	{
		public NctsDepartureHeaderMovementHeaderValueSetStrategy(NctsDepartureMovementHeader departureMovementHeader)
		{
			this.departureMovementHeader = departureMovementHeader;
		}

		readonly NctsDepartureMovementHeader departureMovementHeader;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case NctsDepartureMovementHeader.Schema.BM_GONumber:
					ChangeValueInBMLocationOfGoodsCode();
					break;
				case NctsDepartureMovementHeader.Schema.BM_GrossWeight:
					new HarbourFeeDepartureMovementCalculationManager(departureMovementHeader.Factory).Calculate(departureMovementHeader);
					break;
			}
		}

		internal void ChangeValueInBMLocationOfGoodsCode()
		{
			var bmLocationOfGoodsCode = ZString.Empty;
			if (departureMovementHeader.IsSimplifiedNctsProcedure)
			{
				var ncts = departureMovementHeader.Header;
				if (ncts != null && ncts.Consignor != null && ncts.Declarant != null)
				{
					var permitHolder = ncts.Declarant.Organisation;
					var appliesTo = ncts.Consignor.E2_OA_Address;

					if (permitHolder != null && !appliesTo.IsEmpty)
					{
						var authorisations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(ncts.Factory, ncts.CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit }, ZDate.Today, new[] { permitHolder.PK }, new[] { appliesTo });
						if (authorisations != null && authorisations.Any())
						{
							bmLocationOfGoodsCode = authorisations.FirstOrDefault().CPH_Number.SubstringSafe(0, 17);
						}
					}
				}
			}
			departureMovementHeader.BM_LocationOfGoodsCode = bmLocationOfGoodsCode;
		}
	}
}
