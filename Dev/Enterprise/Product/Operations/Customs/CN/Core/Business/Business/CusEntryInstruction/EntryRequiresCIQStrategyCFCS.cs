using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyCFCS : IEntryRequiresCIQStrategy
{
	internal static string CFCSRequireCIQMessage => Res.GetString("2A1FA30F-0828-4FDD-9EC5-4BCF259F2E6A", "some goods are compressors using CFCS");

	public ZString Validate(CusEntryInstruction instruction)
	{
		var invoiceLines = instruction.InvoiceLines.Cast<JobComInvoiceLine>();
		if (invoiceLines.Any(l => l.UniversalTariff?.HasCommodityTypeCFCS() ?? false))
		{
			return CFCSRequireCIQMessage;
		}
		return ZString.Empty;
	}
}
