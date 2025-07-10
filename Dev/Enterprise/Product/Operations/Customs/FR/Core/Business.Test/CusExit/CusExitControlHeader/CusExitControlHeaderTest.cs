using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusExit.Testing
{
	[TestedType(typeof(CusExitControlHeader))]
	class CusExitControlHeaderTest : EU.Business.Testing.CusExitControlHeaderTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CusExitControlHeader>();
		}

		public void TestTypeSafe()
		{
			var exitHeader = GetNewBusinessObject() as CusExitControlHeader;
			AssertType<CusExitDetailCollection>(exitHeader.CusExitDetails);
		}
	}
}
