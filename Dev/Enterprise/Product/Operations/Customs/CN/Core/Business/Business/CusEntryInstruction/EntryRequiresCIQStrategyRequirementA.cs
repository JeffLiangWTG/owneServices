using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyRequirementA : IEntryRequiresCIQStrategy
{
	internal static string DocumentARequireCIQMessage => Res.GetString("6DB63B92-A04B-45B1-9C99-5BF7DC53D3B7", "some tariffs requires supporting document A");

	public ZString Validate(CusEntryInstruction instruction)
	{
		var invoiceLines = instruction.InvoiceLines.Cast<JobComInvoiceLine>();
		if (invoiceLines.Any(l => l.UniversalTariff?.HasImportCUSRequirementA() ?? false))
		{
			return DocumentARequireCIQMessage;
		}
		return ZString.Empty;
	}
}
