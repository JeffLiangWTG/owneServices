using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class CusGoodsLocationAddressLookups(CusGoodsLocationAddress parent) : EU.Business.CusGoodsLocationAddressLookups(parent)
{
	public override ICollection AuthorisationNumberList
	{
		get
		{
			var numbers = new CusAuthorisationHeaderCollectionFiltered(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, parent.IdentificationHolderPK);
			numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", new ZGuid(parent.IdentificationHolderPK), false));
			numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber, "Property", GetAuthorisationNumber()));
			return numbers;
		}
	}
}
