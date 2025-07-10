using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusGoodsLocationAddressLookups : EU.Business.CusGoodsLocationAddressLookups
{
	public CusGoodsLocationAddressLookups(CusGoodsLocationAddress parent) : base(parent)
	{
	}

	public override ICollection AuthorisationNumberList
	{
		get
		{
			var parent = Parent;
			var numbers = GetCusAuthorisationHeaderCollection();
			numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", new ZGuid(parent.IdentificationHolderPK), false));
			numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber, "Property", parent.E2_GovRegNum));
			numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", parent.E2_GovRegNumType, false));
			return numbers;
		}
	}

	public CusAuthorisationHeaderCollection GetCusAuthorisationHeaderCollection()
	{
		var cusAuthorisationHeaderCollection = new CusAuthorisationHeaderCollectionFiltered(Factory, Parent.E2_GovRegNumType, Parent.IdentificationHolderPK);
		cusAuthorisationHeaderCollection.AdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, ZBool.True);
		return cusAuthorisationHeaderCollection;
	}
}
