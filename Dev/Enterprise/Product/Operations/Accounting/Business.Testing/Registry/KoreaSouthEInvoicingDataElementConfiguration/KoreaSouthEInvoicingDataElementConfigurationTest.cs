using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDataElementConfiguration))]
	public class KoreaSouthEInvoicingDataElementConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestInvoiceType()
		{
			Assert(BizObj.InvoiceTypeInfo.ReadOnly);
		}

		public void TestDataElement()
		{
			Assert(BizObj.DataElementInfo.ReadOnly);
		}

		public void TestConfiguration()
		{
			var bizObj = BizObj;

			AssertEquals("Configuration should be editable.", false, bizObj.ConfigurationInfo.ReadOnly);

			bizObj.Configuration = "AAA";
			AssertEquals("AAA", bizObj.Configuration);

			bizObj.Configuration = "BBB";
			AssertEquals("BBB", bizObj.Configuration);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new KoreaSouthEInvoicingDataElementConfiguration();

			result.InvoiceType = EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice;
			result.DataElement = EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1;
			result.Configuration = $"<{nameof(KoreaSouthEInvoicingDataElementProvider.AmendStatusCodeDescriptionInKorean)}>";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new KoreaSouthEInvoicingDataElementConfiguration BizObj
		{
			get { return (KoreaSouthEInvoicingDataElementConfiguration)base.BizObj; }
		}
	}
}
