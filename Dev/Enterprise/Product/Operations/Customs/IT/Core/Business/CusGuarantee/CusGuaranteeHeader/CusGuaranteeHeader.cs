using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.IT.ICusGuaranteeHeader
{
	public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new GuaranteeCountrySpecificInstruction CountrySpecificInstruction => (GuaranteeCountrySpecificInstruction)base.CountrySpecificInstruction;
}
