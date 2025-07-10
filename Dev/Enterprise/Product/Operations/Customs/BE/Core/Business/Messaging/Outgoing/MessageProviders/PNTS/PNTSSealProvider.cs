using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class PNTSSealProvider : IPNTSSeal
{
	public PNTSSealProvider(ZString seal)
	{
		this.seal = seal;
	}
	readonly ZString seal;

	public string Identifier => seal;
}
