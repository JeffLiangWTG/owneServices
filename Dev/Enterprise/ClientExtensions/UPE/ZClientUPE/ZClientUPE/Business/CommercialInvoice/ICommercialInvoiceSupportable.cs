using System.Drawing;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.CommercialInvoice
{
	public interface ICommercialInvoiceSupportable
	{
		Image CommercialInvoiceImage { get; }
		ZBool HasCommercialInvoice { get; }
	}
}
