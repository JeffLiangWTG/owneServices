using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class WarehouseWrapper : IWarehouse
	{
		WarehouseWrapper(EU.Business.CusAuthorizationUsage authorisationUsage)
		{
			this.authorisationUsage = Argument.NotNull(authorisationUsage, nameof(authorisationUsage));
		}
		readonly EU.Business.CusAuthorizationUsage authorisationUsage;

		public string Identifier => identifier ?? (identifier = authorisationUsage.AGC_Number);
		string identifier;

		public string Type => type ?? (type = authorisationUsage.AGC_Code);
		string type;

		public static WarehouseWrapper New(EU.Business.CusAuthorizationUsage authorisationUsage) => authorisationUsage == null ? null : new WarehouseWrapper(authorisationUsage);
	}
}
