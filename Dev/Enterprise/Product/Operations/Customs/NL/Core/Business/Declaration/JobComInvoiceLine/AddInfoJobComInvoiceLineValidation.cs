using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
{
	public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent) : base(parent)
	{
	}

	protected new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

	protected override void CheckZG_TransNature()
	{
		base.CheckZG_TransNature();
		ListValidation.MessageErrorIfInvalidCode(Parent.ZG_TransNatureInfo);

		var invoiceHeader = Parent.Parent.InvoiceHeader;
		var entryInstruction = Parent.Parent.EntryInstruction;

		if (entryInstruction != null && Parent.ZG_TransNature.IsEmpty && invoiceHeader.JZ_ValuationCode.IsEmpty && entryInstruction.HasEmptyTransactionNatureForMessage)
		{
			Parent.ZG_TransNatureInfo.AddMessageError(Res.GetString("1972AF01-2AD6-4CEE-87D1-99B1094B8B53", "Transaction Nature is required on either Invoice header OR Invoice Item level OR Entry Instruction Level"));
		}
	}

	protected override void CheckZG_CountryOfDestination()
	{
		base.CheckZG_CountryOfDestination();

		ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CountryOfDestinationInfo, Parent.Parent.AddInfoLookups.CountryOfDestinationList);
		if (Parent.ZG_CountryOfDestination.IsEmpty && Parent.Parent.Declaration.JE_GoodsDestination.IsEmpty)
		{
			Parent.ZG_CountryOfDestinationInfo.AddMessageError(Res.GetString("DEA8EE3A-FDCD-45F4-8E55-5DF8059D090D", "Destination is required on either Invoice header OR Invoice Item level"));
		}
	}
}
