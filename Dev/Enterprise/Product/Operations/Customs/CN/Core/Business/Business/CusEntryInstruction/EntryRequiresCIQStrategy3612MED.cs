using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategy3612MED : IEntryRequiresCIQStrategy
{
	internal static string Procedure3612MEDRequireCIQMessage => Res.GetString("8833891B-55DF-49E4-8D50-A0E53E030702", "procedure code is 3612 and some goods are medical supplies");

	public ZString Validate(CusEntryInstruction instruction)
	{
		var invoiceLines = instruction.InvoiceLines.Cast<JobComInvoiceLine>();
		if (instruction.CEI_Style == CNRefCusProcedure.Codes._3612 && invoiceLines.Any(l => l.UniversalTariff?.HasCommodityTypeMED() ?? false))
		{
			return Procedure3612MEDRequireCIQMessage;
		}
		return ZString.Empty;
	}
}
