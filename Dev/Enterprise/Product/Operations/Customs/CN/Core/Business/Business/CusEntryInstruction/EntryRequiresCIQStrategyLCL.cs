using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyLCL : IEntryRequiresCIQStrategy
{
	internal static string LCLRequireCIQMessage => Res.GetString("7CC0B6CB-CC9F-4D07-82A1-F17A5FCAB380", "some contains are LCL");

	public ZString Validate(CusEntryInstruction instruction)
	{
		if (instruction.EntryHeader?.Containers?.Any(c => c.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.LCL) ?? false)
		{
			return LCLRequireCIQMessage;
		}
		return ZString.Empty;
	}
}
