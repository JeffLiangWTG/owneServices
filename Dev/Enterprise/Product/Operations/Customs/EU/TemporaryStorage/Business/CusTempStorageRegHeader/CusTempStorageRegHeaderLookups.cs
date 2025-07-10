using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegHeaderLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderLookups
{
	public CusTempStorageRegHeaderLookups(AutoCusTempStorageRegHeader parent)
		: base(parent)
	{
	}

	public override CustomsOfficeCodeCollection CustomsOfficeList => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory);
}
