using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(HSExtensionCodeCollection))]
	sealed class HSExtensionCodeCollectionTest : CusCodeDataCollectionTest<HSExtensionCode>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new HSExtensionCodeCollection(invoiceLine);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}

		protected override CusCodeDataCollection<HSExtensionCode> GetCusCodeDataCollection()
		{
			var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new HSExtensionCodeCollection(invoiceLine);
		}
	}
}
