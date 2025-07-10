using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class PNTSWarehouseProvider : IWarehouse
{
	public PNTSWarehouseProvider(EU.Business.CusAuthorizationUsage authorizationUsage)
	{
		this.authorizationUsage = Argument.NotNull(authorizationUsage, nameof(authorizationUsage));
	}
	readonly EU.Business.CusAuthorizationUsage authorizationUsage;
	public string Type => "V";

	public string Identifier => authorizationUsage.AGC_Number;
}
