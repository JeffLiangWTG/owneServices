using System.Drawing;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;

namespace Enterprise.Client.MFI.DocWrappers
{
	public class DocMFIARInvoice : DocARInvoice
	{
		#region Constructors and Type Overriding

		protected DocMFIARInvoice(InvoicingBase invoicingBase, BusinessObjectFactory factory)
			: base(invoicingBase, factory)
		{
		}

		public new static DocMFIARInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return (invoicingBase != null) ? new DocMFIARInvoice(invoicingBase, factory) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocARInvoice OverriddenNewMethod(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return DocMFIARInvoice.New(invoicingBase, factory);
		}

		#endregion

		#region Properties

		public ZString GoodsValueFormattedWithCurrencyCode
		{
			get
			{
				ZString result = ZString.Empty;

				if (Shipment != null && Shipment.GoodsCurr != null)
				{
					NumberFormatInfo noCurrencySymbolFormat = Env.CurrentCompany.Country.Culture.NumberFormat;
					noCurrencySymbolFormat.CurrencySymbol = "";
					noCurrencySymbolFormat.CurrencyDecimalDigits = Shipment.GoodsCurr.Decimals;
					result = string.Format("{0} {1}", Shipment.GoodsValue.ToString("C", noCurrencySymbolFormat).TrimEnd(), Shipment.GoodsCurr.Code);
				}
				return result;
			}
		}

		#region Barcode

		public ZString BarcodeForFontPlaceholder
		{
			get
			{
				ZString result = ZString.Empty;
				if (!BarcodeForPlaceholder.IsEmpty)
				{
					TextBarcode barcode = new TextBarcode(BarcodeForPlaceholder, true);
					result = barcode.TextAs128sFontString;
				}
				return result;
			}
		}

		public ZString BarcodeForPlaceholder
		{
			get
			{
				ZString result = ZString.Empty;
				if (Shipment != null)
				{
					result = string.Format("{0} | {1} | {2}", AccountCode, JobInvoiceNumber, OSTotalFormatted);
				}
				return result;
			}
		}

		#endregion

		public override Image InvoiceLogo
		{
			get
			{
				Image fImage;

				if (MFIConstants.NZ.ClientSpecificCondition && DocumentDeliveryMode == nameof(Enterprise.ZArchitecture.Core.PrintCopyType.PRN))
				{
					fImage = MFIDataRegistry.Instance.InvoiceLetterhead;
				}
				else
				{
					fImage = base.InvoiceLogo;
				}

				return fImage;
			}
		}

		#endregion
	}
}
