using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Customs.CA.DIF.GUI;
using Enterprise.Customs.Module.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Module.Testing
{
	[TestedType(typeof(DIFController))]
	sealed class DIFControllerTest : DISControllerBaseTest
	{
		public override void TestSecurity()
		{
			var controller = new DIFController();
			AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.CACustomsDIFEdit, controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.CACustomsDIFEdit, controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.CACustomsDIFView, controller.GetCheckPointForView(null));
		}

		public void TestControllerForLPCO()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var pgaHeader = invoiceLine.CFIAPGAHeader;
			var lpcoView = pgaHeader.LPCOViews.AddNew();
			Factory.Save();

			Controller.ShowEditForm(lpcoView.LPCO);
			AssertEquals(typeof(DIFForm), Controller.LastShownForm.GetType());
		}

		public override Type ControllerToBashType => typeof(DIFController);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase() => JobDeclaration;

		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
		BusinessObject jobDeclaration;
	}
}
