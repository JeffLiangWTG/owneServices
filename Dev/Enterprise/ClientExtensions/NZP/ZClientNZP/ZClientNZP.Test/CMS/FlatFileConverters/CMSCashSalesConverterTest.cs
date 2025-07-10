using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSCashSalesConverterTest : CMSFlatFileConverterTestCase
	{
		public void TestExportAndTransactionCode()
		{
			ReadOnlyCodeDescriptionPairList pairList = NZPDataRegistry.Instance.InvoiceTermsInTFile;
			AssertEquals("PRE: Invoice Terms Count", 2, pairList.Count);
			Assert("PRE: COD is in registry", pairList.ContainsCode(Core.Constants.InvoiceTerms.CashOnDelivery));
			Assert("PRE: PIA is in registry", pairList.ContainsCode(Core.Constants.InvoiceTerms.PaymentInAdvance));
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.TxnLines.AddNew();
			AssertNoRowsExported(header, Xsd.TxnType.CRD, Core.Constants.InvoiceTerms.FromMonthEnd);
			AssertNoRowsExported(header, Xsd.TxnType.ADJ, Core.Constants.InvoiceTerms.FromMonthEnd);
			AssertNoRowsExported(header, Xsd.TxnType.CRD, Core.Constants.InvoiceTerms.CashOnDelivery);
			AssertRowsExportAndTrasnactionCode(header, Xsd.TxnType.ADJ, Core.Constants.InvoiceTerms.CashOnDelivery, Constants.CashTransactionCode);
			AssertRowsExportAndTrasnactionCode(header, Xsd.TxnType.ADJ, Core.Constants.InvoiceTerms.PaymentInAdvance, Constants.CashTransactionCode);
		}

#region Implementation
		protected override ICMSConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CMSCashSalesConverterTestClass(new NotificationBuffer(), Factory);
				}

				return fConverter;
			}
		}

		CMSCashSalesConverterTestClass fConverter;
		class CMSCashSalesConverterTestClass : CMSCashSalesConverter, ICMSConverter
		{
			public CMSCashSalesConverterTestClass(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
#endregion
	}
}
