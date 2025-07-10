using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeader.Loader))]
	internal class CusGuaranteeHeaderLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusGuaranteeHeader.Loader(Factory);
		}

		[ExpectNoExceptions]
		public void TestLoadCusGuaranteeHeaderListFromPermitHolder()
		{
			var factory = new BusinessObjectFactory();
			var cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(2);
			var transaction = cusGuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.FillWithValidTestData();
			Factory.Save();
			var possibleCusGuarantees = CusGuaranteeHeader.Loader.LoadCusGuaranteeHeaderListFromPermitHolder(factory, cusGuaranteeHeader.CPH_OH_PermitHolder, cusGuaranteeHeader.Country.Code);
			NUnit.Framework.Assert.That(possibleCusGuarantees.Length, NUnit.Framework.Is.EqualTo(1));
			cusGuaranteeHeader.CPH_EndDate = new ZDate(2008, 09, 13);
			Factory.Save();
			possibleCusGuarantees = CusGuaranteeHeader.Loader.LoadCusGuaranteeHeaderListFromPermitHolder(factory, cusGuaranteeHeader.CPH_OH_PermitHolder, cusGuaranteeHeader.Country.Code);
			NUnit.Framework.Assert.That(possibleCusGuarantees.Length, NUnit.Framework.Is.EqualTo(0));
		}
	}
}
