using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyProcedureCode4561 : IEntryRequiresCIQStrategy
{
	internal static string Procedure4561RequireCIQMessage => Res.GetString("AD3FB93B-8024-41A7-9699-29F600A8A957", "procedure code is 4561");

	public ZString Validate(CusEntryInstruction instruction)
	{
		if (instruction.CEI_Style == CNRefCusProcedure.Codes._4561)
		{
			return Procedure4561RequireCIQMessage;
		}
		return ZString.Empty;
	}
}
