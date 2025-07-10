using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CashBookAPReceiptController))]
	class CashBookAPReceiptControllerTest : APReceiptControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CashBookAPReceipt;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewCashBookReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewCashBookReceipt; }
		}

		public void TestShowNewForm()
		{
			var controller = new CashBookAPReceiptController();

			using (var newForm = controller.ShowNewForm())
			{
				AssertNotNull("New Form", newForm);
				AssertEquals("New Form Type", typeof(ReceiptForm), newForm.GetType());

				var form = (ReceiptForm)newForm;
				var typeOfBusinessEntity = form.BusinessEntity.GetType();

				AssertEquals("Type of New BusinessEntity", typeof(APReceipt), typeOfBusinessEntity);
				AssertEquals("BusinessEntity in database", false, ((BusinessObject)form.BusinessEntity).IsInDatabase);

				var tabControl = (newForm as ZForm).FindAll<ZTemplateTabControl>().Single();
				AssertEquals("WorkflowTabPage Not Visible", -1, tabControl.TabPages.IndexOfKey("WorkflowTabPage"));
			}
		}

		public override void TestModuleID()
		{
			AssertNull(Controller.ModuleID);
		}
	}
}
