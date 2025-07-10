using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyValidationResult
{
	public EntryRequiresCIQStrategyValidationResult()
	{
		Messages = new List<string>() { CusEntryInstructionValidation.GoodsRequireCIQMessage };
	}

	public int ErrorCount { get; set; }

	public List<string> Messages { get; }

	public ZString GetFormattedMessage()
	{
		return string.Join(System.Environment.NewLine + "       ", Messages);
	}

	public bool HasErrors => ErrorCount > 0;
}
