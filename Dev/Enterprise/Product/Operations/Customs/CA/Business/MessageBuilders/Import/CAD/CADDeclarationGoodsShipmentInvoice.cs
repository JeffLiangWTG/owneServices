using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentInvoice : ICADMessageDeclarationGoodsShipmentInvoice
{
	public CADDeclarationGoodsShipmentInvoice(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = invoiceHeader;
	}

	readonly JobComInvoiceHeader invoiceHeader;

	ICADMessageMoney ICADMessageDeclarationGoodsShipmentInvoice.Amount
	{
		get
		{
			if (invoiceHeader != null)
			{
				return new CADMessageMoney(invoiceHeader.JZ_InvoiceAmount, invoiceHeader.Invoice_Currency);
			}
			else
			{
				return null;
			}
		}
	}

	string ICADMessageDeclarationGoodsShipmentInvoice.ID => invoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty;

	string ICADMessageDeclarationGoodsShipmentInvoice.TypeCode
	{
		get
		{
			return "380";
		}
	}
}
