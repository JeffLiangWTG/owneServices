using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	public class InvoiceApportionChargeTest : Customs.Business.Testing.BaseApportionedChargeTest
	{
		public void TestAmountCorrection()
		{
			Assert("Will be implemented by WI's WI00831078 WI00837660 WI00838181 WI00838254 WI00838273 WI00838919 WI00838938 WI00838971 WI00839025 WI00839039 WI00839057", true);
		}

		public void TestCaptions() => CombineAssertions(() =>
		{
			InvoiceChargesTestHelper.AssertCaptions(Factory.New<InvoiceApportionCharge>());
		});

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		protected override void PrepareCharge(JobComInvCharge charge)
		{
			base.PrepareCharge(charge);
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var euDec = Factory.New<JobDeclaration>();
			euDec.JE_MessageType = MessageTypeList.Codes.Import; // Export is the default but this adds chartegs that are distributed by weight, and all the base tests expect value. So use an import dec. 
			var invoice = euDec.Invoices.AddNew();
			return invoice.GroupCharges.AddNew();
		}
	}
}
