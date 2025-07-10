using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyDangerousChemical : IEntryRequiresCIQStrategy
{
	internal static string DangerousChemicalRequireCIQMessage => Res.GetString("E019B1B0-BD6A-48E4-9845-BB5F3C574BD2", "some goods are dangerous chemical");

	public ZString Validate(CusEntryInstruction instruction)
	{
		var invoiceLines = instruction.InvoiceLines.Cast<JobComInvoiceLine>();
		if (invoiceLines.Any(l => l.GoodsIsDangerousChemical || l.HasDangerousGoodsAttribute))
		{
			return DangerousChemicalRequireCIQMessage;
		}
		return ZString.Empty;
	}
}
