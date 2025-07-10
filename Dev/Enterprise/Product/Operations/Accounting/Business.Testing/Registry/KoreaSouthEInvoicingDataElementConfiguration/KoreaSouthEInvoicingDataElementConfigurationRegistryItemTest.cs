using System.Text;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDataElementConfigurationRegistryItem))]
	class KoreaSouthEInvoicingDataElementConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<KoreaSouthEInvoicingDataElementConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<KoreaSouthEInvoicingDataElementConfigurationCollection, KoreaSouthEInvoicingDataElementConfigurationCollection> GetNewRegistryItem()
		{
			return new KoreaSouthEInvoicingDataElementConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new KoreaSouthEInvoicingDataElementConfigurationCollection());
		}
	}

	[TestedType(typeof(KoreaSouthEInvoicingDataElementConfigurationDataType))]
	class KoreaSouthEInvoicingDataElementConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<KoreaSouthEInvoicingDataElementConfigurationDataType>
	{
		#region Implementation

		protected override KoreaSouthEInvoicingDataElementConfigurationDataType GetNewDataType()
		{
			return new KoreaSouthEInvoicingDataElementConfigurationDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "KoreaSouthEInvoicingDataElementConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new KoreaSouthEInvoicingDataElementConfigurationCollection()
			{
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, "ConfigurationValue"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3, "")
			};

			var collection2 = new KoreaSouthEInvoicingDataElementConfigurationCollection()
			{
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, "ConfigurationValue"),
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3, "")
			};

			var expectedXmlValue_collection1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfKoreaSouthEInvoicingDataElementConfiguration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><KoreaSouthEInvoicingDataElementConfiguration><InvoiceType>Original Invoice</InvoiceType><DataElement>Invoice Document Header Description Line 2</DataElement><Configuration>ConfigurationValue</Configuration></KoreaSouthEInvoicingDataElementConfiguration><KoreaSouthEInvoicingDataElementConfiguration><InvoiceType>Original Invoice</InvoiceType><DataElement>Invoice Document Header Description Line 3</DataElement><Configuration></Configuration></KoreaSouthEInvoicingDataElementConfiguration><KoreaSouthEInvoicingDataElementConfiguration><InvoiceType>Amendment</InvoiceType><DataElement>Invoice Document Header Description Line 3</DataElement><Configuration></Configuration></KoreaSouthEInvoicingDataElementConfiguration></ArrayOfKoreaSouthEInvoicingDataElementConfiguration>";

			var expectedXmlValue_collection2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfKoreaSouthEInvoicingDataElementConfiguration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><KoreaSouthEInvoicingDataElementConfiguration><InvoiceType>Original Invoice</InvoiceType><DataElement>Invoice Document Header Description Line 2</DataElement><Configuration>ConfigurationValue</Configuration></KoreaSouthEInvoicingDataElementConfiguration><KoreaSouthEInvoicingDataElementConfiguration><InvoiceType>Amendment</InvoiceType><DataElement>Invoice Document Header Description Line 3</DataElement><Configuration></Configuration></KoreaSouthEInvoicingDataElementConfiguration></ArrayOfKoreaSouthEInvoicingDataElementConfiguration>";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, Encoding.Unicode.GetBytes(expectedXmlValue_collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, Encoding.Unicode.GetBytes(expectedXmlValue_collection2))
			};
		}

		#endregion Implementation
	}
}
