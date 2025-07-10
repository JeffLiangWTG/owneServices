using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class CusTempStorageRegPremisesValidation : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesValidation
{
	public CusTempStorageRegPremisesValidation(CusTempStorageRegPremises parent) : base(parent)
	{
	}

	protected override void CheckSRP_CustomsLocation()
	{
		base.CheckSRP_CustomsLocation();
		CheckIfDuplicatePremiseExists();
	}

	protected void CheckIfDuplicatePremiseExists()
	{
		var query = new ZDBOnlyQuery(typeof(CusTempStorageRegPremises));
		_ = query.AddToFilter(CusTempStorageRegPremisesSchema.SRP_Type, Parent.SRP_Type);
		_ = query.AddToFilter(CusTempStorageRegPremisesSchema.SRP_CustomsLocation, Parent.SRP_CustomsLocation);

		if (Parent.Factory.ExistsInDatabase(CusTempStorageRegPremisesSchema.Constants.TableName, query))
		{
			Parent.SRP_CustomsLocationInfo.AddError(ResString.GetMultilingualString("0B8F79C4-5781-4F2E-AEAD-90C54C9330C4",
														"Duplicate premises: Already exists a Premises record with the same Type ({0}) and Location ({1})",
														Parent.SRP_Type, Parent.SRP_CustomsLocation));
		}
	}
}
