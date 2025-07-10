using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	public class InvoiceApportionChargeTest : Customs.Business.Testing.BaseApportionedChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		protected override (BaseJobDeclaration, string) GetDeclarationAndChargeCodeForTest()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MessageType = BRJobMessageTypeList.Codes.MiscellaneousCustoms;
			return (testDec, Common.CustomsChargeTypeList.Codes.OverseasFreight);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = base.Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.MiscellaneousCustoms;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader.GroupCharges.AddNew();
		}
	}
}
