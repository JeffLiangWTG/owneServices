using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyCargoAttributes : IEntryRequiresCIQStrategy
{
	internal static string CargoAttributesRequireCIQMessage => Res.GetString("44C45F53-F90F-4224-86D0-13C1A04C5204", "some goods have attribute 21 or 22");

	public ZString Validate(CusEntryInstruction instruction)
	{
		var invoiceLines = instruction.InvoiceLines.Cast<JobComInvoiceLine>();
		if (invoiceLines.Any(l => l.CargoAttributes.ContainsCode(CargoAttributeList.Codes._21) || l.CargoAttributes.ContainsCode(CargoAttributeList.Codes._22)))
		{
			return CargoAttributesRequireCIQMessage;
		}
		return ZString.Empty;
	}
}
