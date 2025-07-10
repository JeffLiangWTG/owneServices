using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusOutturnCustomsStatusCalculator))]
	sealed class CusOutturnCustomsStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalculator()
		{
			CusOutturn outturn = Factory.New<CusOutturn>();
			CusOutturnCustomsStatusCalculator calculator = new CusOutturnCustomsStatusCalculator(outturn);
			AssertEquals(outturn.C5_CustomsStatusInfo, calculator.StatusInfo);
			calculator.DeriveStatusNow();
			AssertEquals(ZString.Empty, outturn.C5_CustomsStatus);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusOutturnCustomsStatusCalculator(Factory.New<CusOutturn>());
	}
}
