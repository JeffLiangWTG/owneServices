using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.AU.Declaration.Business.AUJobDeclarationLookups;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUAttachInvoiceCollection))]
	sealed class AUAttachInvoiceCollectionTest : ActiveBusinessObjectCollectionTestCase<AUAttachInvoiceCollection>
	{
		public void TestFilterBusinessObjectDefaults()
		{
			var testDec = Factory.New<JobDeclaration>();
			Assert(testDec.Lookups.InvoicesToAttach.FilterBusinessObjectDefaults.ContainsDefaultFor("Exporter Reference:Property"));
		}

		protected override AUAttachInvoiceCollection GetCollectionToTest()
		{
			var testDec = Factory.New<JobDeclaration>();
			return new AUAttachInvoiceCollection(testDec);
		}
	}
}
