using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetailCollection))]
	sealed public class CusLineTariffDetailCollectionTest : Customs.Business.Testing.CusLineTariffDetailCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => CusLineTariffDetailCollection;

		CusLineTariffDetailCollection cusLineTariffDetailCollection;

		CusLineTariffDetailCollection CusLineTariffDetailCollection => cusLineTariffDetailCollection ?? (cusLineTariffDetailCollection = new CusLineTariffDetailCollection(Factory.New<JobComInvoiceLine>()));

		public void TestAddNewMaxCount()
		{
			const int maxRowCount = 6;
			var testCollection = GetCollectionToTest();
			for (var i = 0; i < maxRowCount - 1; i++)
			{
				testCollection.AddNew();
			}
			Assert($"Currently, collection has {testCollection.Count} elements", testCollection.AllowNew);
			testCollection.AddNew();
			Assert($"Currently, collection has {testCollection.Count} elements", !testCollection.AllowNew);
		}
	}
}
