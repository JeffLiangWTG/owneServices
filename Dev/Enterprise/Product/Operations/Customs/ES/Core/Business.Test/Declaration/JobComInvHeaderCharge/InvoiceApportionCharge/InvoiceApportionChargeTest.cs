using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	public class InvoiceApportionChargeTest : EU.Business.Declaration.Testing.InvoiceApportionChargeTest
	{
		protected override void PrepareCharge(JobComInvCharge charge)
		{
			base.PrepareCharge(charge);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsNotIncludedInInvoice = true;
		}

		protected override (BaseJobDeclaration, string) GetDeclarationAndChargeCodeForTest()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			return (testDec, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge);
		}
	}
}
