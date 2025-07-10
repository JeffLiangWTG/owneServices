using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	class InvoiceApportionChargeTest : Customs.Business.Testing.BaseApportionedChargeTest
	{
		protected override (BaseJobDeclaration, string) GetDeclarationAndChargeCodeForTest()
		{
			var testDec = Factory.New<JobDeclaration>();
			return (testDec, CustomsChargeTypeList.Codes.OverseasInsurance);
		}
	}
}
