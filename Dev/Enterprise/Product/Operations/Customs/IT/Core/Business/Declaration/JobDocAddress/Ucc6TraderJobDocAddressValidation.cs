using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

class Ucc6TraderJobDocAddressValidation : TraderJobDocAddressValidation
{
	public Ucc6TraderJobDocAddressValidation(AutoJobDocAddress parent, string traderName, JobDeclaration declaration, bool isMandatory = true) : base(parent, traderName, declaration, isMandatory)
	{
	}

	protected override ZString[] GetRequiredCodeTypeListForEuropeanTrader(OrgHeader organisation)
	{
		return Enumerable.Empty<ZString>().ToArray();
	}
}
