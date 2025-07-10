using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business;

public static class CusGoodsLocationAddressHelper
{
	public static void SetDefaultAuthorisationNumber(EU.Business.CusGoodsLocationAddress goodsLocationAddress)
	{
		var lookups = goodsLocationAddress.Lookups;
		var authorisationNumberList = lookups.AuthorisationNumberList;
		if (authorisationNumberList.Count == 1)
		{
			goodsLocationAddress.AuthorisationNumber = authorisationNumberList
				.Cast<CusAuthorisationHeader>()
				.Single()
				.CPH_Number;
		}
	}

	public static void SetValidationStatusToManIfAddressIsOverride(EU.Business.CusGoodsLocationAddress goodsLocationAddress)
	{
		goodsLocationAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
	}
}
