using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class WarehouseWrapper : IWarehouse
	{
		WarehouseWrapper(CusAuthorisationHeader permit)
		{
			this.permit = Argument.NotNull(permit, nameof(permit));
		}

		readonly CusAuthorisationHeader permit;

		public static WarehouseWrapper New(CusAuthorisationHeader permit) => permit == null ? null : new WarehouseWrapper(permit);

		//Mapping not done yet
		public string CcQualifier => ZString.Empty;

		public string Identifier => identifier ?? (identifier = permit.CPH_Number);
		string identifier;

		public string Type => type ?? (type = permit.CusAuthorisationRules?.FirstOrDefault(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.USE)?.CPR_ValueFrom);
		string type;
	}
}
