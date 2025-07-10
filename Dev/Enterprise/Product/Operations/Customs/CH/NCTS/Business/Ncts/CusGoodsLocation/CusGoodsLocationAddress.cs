using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusGoodsLocationAddress : EU.NCTS.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocationAddressLookups Lookups => (CusGoodsLocationAddressLookups)base.Lookups;

	protected override JobDocAddressLookups GetNewLookups() => new CusGoodsLocationAddressLookups(this);

	protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public override ZGuid IdentificationHolderPK
	{
		get => base.IdentificationHolderPK;
		set
		{
			var oldValue = IdentificationHolderPK;
			base.IdentificationHolderPK = value;
			if (!IsCopying && oldValue != IdentificationHolderPK)
			{
				DefaultAuthorisationNumber();
			}
		}
	}

	void DefaultAuthorisationNumber()
	{
		var query = Lookups.GetCusAuthorisationHeaderCollection().CompleteFilter;
		query.MaximumRows = 2;
		var authorisationNumberList = Factory.Load<CusAuthorisationHeader>(query);

		AuthorisationNumber = authorisationNumberList.Length == 1 ? authorisationNumberList[0].CPH_Number : ZString.Empty;
	}
}
