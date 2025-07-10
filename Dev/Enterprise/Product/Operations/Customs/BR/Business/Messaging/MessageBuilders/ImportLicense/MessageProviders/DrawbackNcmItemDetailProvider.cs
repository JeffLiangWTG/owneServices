using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportLicense.Outgoing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business.ImportLicense
{
	public class DrawbackNcmItemDetailProvider : IDrawbackNcmItemDetail
	{
		public DrawbackNcmItemDetailProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		public static DrawbackNcmItemDetailProvider New(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new DrawbackNcmItemDetailProvider(invoiceLine);

		readonly JobComInvoiceLine invoiceLine;

		public string LineNumber => invoiceLine.JI_LineNo.ToString();

		public decimal NetWeight => invoiceLine.JI_NetWeight;

		public decimal InvoiceQty => invoiceLine.JI_InvoiceQuantity;

		public string InvoiceQuantityUQDescription => invoiceLine.InvoiceUQDescInPortugueseBrazil;

		public decimal CustomsQty => invoiceLine.JI_CustomsQuantity;

		public decimal UnitValue
		{
			get
			{
				var result = decimal.Zero;
				if (invoiceLine.JI_InvoiceQuantity > 0)
				{
					result = Utilities.Round(invoiceLine.JI_Calc_InvAmount / invoiceLine.JI_InvoiceQuantity, 7);
				}
				return result;
			}
		}

		public string GoodsDescription => invoiceLine.FullGoodsDescription;

		public decimal FobValue => invoiceLine.JI_Calc_FOB;

		public string DrawbackAcItem => invoiceLine.DrawbackItemNumber.IsEmpty ? string.Empty : invoiceLine.DrawbackItemNumber.ToString();

		public string Brand => invoiceLine.JI_BrandName;

		public string Model => invoiceLine.JI_Model;

		public string SerialNumber => invoiceLine.JI_UsedMaterialSerialNumber;

		public string ManufactureYear => invoiceLine.JI_UsedMaterialManufactureYear;
	}
}
