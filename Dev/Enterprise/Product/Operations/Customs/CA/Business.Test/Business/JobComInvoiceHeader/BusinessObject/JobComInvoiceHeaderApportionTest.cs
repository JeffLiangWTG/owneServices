using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		public override void TestInvoiceChargeSetBeforeInvoiceCurrency()
		{
			var helper = new DeclarationTestHelper(Factory, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CA_RX_DeclaredCurr = ZGuid.Empty;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var lCH = invoice.Charges.AddNew(CAChargeTypeList.Codes.LandingCharges, 100);
			Assert("Landing Charges Currency Is not set", lCH.J7_RX_NKCurrency.IsEmpty);

			invoice.JZ_RX_NKInvoice_Currency = helper.AUD.RX_Code;
			AssertEquals("Landing Charges currency is set now", helper.AUD.RX_Code, lCH.J7_RX_NKCurrency);
		}

		#region Implementation
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}
}
