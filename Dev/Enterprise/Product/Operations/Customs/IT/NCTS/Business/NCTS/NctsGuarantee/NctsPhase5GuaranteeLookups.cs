using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsPhase5GuaranteeLookups : NctsGuaranteeLookups
{
	public NctsPhase5GuaranteeLookups(EU.NCTS.Business.NctsGuarantee parent) : base(parent)
	{
	}

	protected override CustomsOfficeCodeCollection OfficeCodeListCore
		=> EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(Factory, (ZString)CusPermitHeaderApplicationCodeList.Codes.Guarantee);
}
