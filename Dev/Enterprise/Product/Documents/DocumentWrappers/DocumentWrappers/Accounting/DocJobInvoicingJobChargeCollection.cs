using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocJobInvoicingJobChargeCollection : DocumentWrapperCollection
	{
		public DocJobInvoicingJobChargeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocJobInvoicingJobCharge this[int index]
		{
			get { return (DocJobInvoicingJobCharge)base[index]; }
		}

		public ZString ChargeSheetOSAmountDisplay
		{
			get
			{
				foreach (DocJobInvoicingJobCharge charge in this)
				{
					if (charge.ShowLocalAmountAndExRateOnInvoice)
					{
						return Res.GetString("ef5b983b-b186-44d8-90c0-06fa7dfe0a40", "OS AMOUNT");
					}
				}
				return ZString.Empty;
			}
		}
	}
}
