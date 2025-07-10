using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSCreditSalesConverterTest : CMSFlatFileConverterTestCase
	{
		public void TestExportAndTransactionCode()
		{
			ReadOnlyCodeDescriptionPairList pairList = NZPDataRegistry.Instance.InvoiceTermsInTFile;
			AssertEquals("PRE: Invoice Terms Count", 2, pairList.Count);
			Assert("PRE: COD is in registry", pairList.ContainsCode(Core.Constants.InvoiceTerms.CashOnDelivery));
			Assert("PRE: PIA is in registry", pairList.ContainsCode(Core.Constants.InvoiceTerms.PaymentInAdvance));
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.TxnLines.AddNew();
			AssertNoRowsExported(header, Xsd.TxnType.CRD, Core.Constants.InvoiceTerms.CashOnDelivery);
			AssertNoRowsExported(header, Xsd.TxnType.ORC, Core.Constants.InvoiceTerms.CashOnDelivery);
			AssertNoRowsExported(header, Xsd.TxnType.ORC, Core.Constants.InvoiceTerms.PaymentInAdvance);
			AssertNoRowsExported(header, Xsd.TxnType.CRD, Core.Constants.InvoiceTerms.FromShipmentDate);
			AssertRowsExportAndTrasnactionCode(header, Xsd.TxnType.OPY, Core.Constants.InvoiceTerms.FromShipmentDate, Constants.CreditTransactionCode);
		}

#region Implementation
		protected override ICMSConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CMSCreditSalesConverterTestClass(new NotificationBuffer(), Factory);
				}

				return fConverter;
			}
		}

		CMSCreditSalesConverterTestClass fConverter;
		class CMSCreditSalesConverterTestClass : CMSCreditSalesConverter, ICMSConverter
		{
			public CMSCreditSalesConverterTestClass(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
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
