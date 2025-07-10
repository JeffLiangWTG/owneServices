using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDataElementConfigurationCollection))]
	public class KoreaSouthEInvoicingDataElementConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<KoreaSouthEInvoicingDataElementConfigurationCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool SupportsAddNew => false;

		protected override KoreaSouthEInvoicingDataElementConfigurationCollection GetCollectionToTest()
		{
			return new KoreaSouthEInvoicingDataElementConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new KoreaSouthEInvoicingDataElementConfiguration();
		}

		public void TestAllowRemove()
		{
			AssertEquals("PreCondition", false, GetCollectionToTest().ReadOnly);
			AssertEquals(false, GetCollectionToTest().AllowRemove);
		}
	}
}
