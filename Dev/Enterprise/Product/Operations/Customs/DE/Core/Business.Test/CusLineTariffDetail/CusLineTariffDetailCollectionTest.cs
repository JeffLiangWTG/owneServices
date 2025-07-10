using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetailCollection))]
	sealed class CusLineTariffDetailCollectionTest : Customs.Business.Testing.CusLineTariffDetailCollectionTest
	{
		public void TestSetDefaultsForNewChild()
		{
			var tariffDetail = CusLineTariffDetailCollection.AddNew();
			AssertEquals("BZ_Type should be defaulted to EXC", Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise, tariffDetail.BZ_Type);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => CusLineTariffDetailCollection;

		CusLineTariffDetailCollection cusLineTariffDetailCollection;
		CusLineTariffDetailCollection CusLineTariffDetailCollection => cusLineTariffDetailCollection ?? (cusLineTariffDetailCollection = new CusLineTariffDetailCollection(Factory.New<JobComInvoiceLine>()));
	}
}
