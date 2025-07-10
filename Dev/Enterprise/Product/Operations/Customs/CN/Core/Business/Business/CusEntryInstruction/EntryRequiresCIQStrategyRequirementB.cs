using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyRequirementB : IEntryRequiresCIQStrategy
{
	internal static string DocumentBRequireCIQMessage => Res.GetString("A5BF99F8-1027-4F19-A5FD-D63E06BD07F5", "some tariffs require supporting document B");

	public ZString Validate(CusEntryInstruction instruction)
	{
		var invoiceLines = instruction.InvoiceLines.Cast<JobComInvoiceLine>();
		if (invoiceLines.Any(l => l.UniversalTariff?.HasExportCUSRequirementB() ?? false))
		{
			return DocumentBRequireCIQMessage;
		}
		return ZString.Empty;
	}
}
