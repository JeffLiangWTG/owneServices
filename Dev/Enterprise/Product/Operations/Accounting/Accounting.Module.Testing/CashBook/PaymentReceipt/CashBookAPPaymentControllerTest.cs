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
	[TestedType(typeof(CashBookAPPaymentController))]
	class CashBookAPPaymentControllerTest : APPaymentControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CashBookAPPayment;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewCashBookPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewCashBookPayment; }
		}

		protected override void CoreTestForShowNewForm()
		{
			var controller = new CashBookAPPaymentController();

			using (var newForm = controller.ShowNewForm())
			{
				AssertNotNull("New Form", newForm);
				AssertEquals("New Form Type", typeof(APPaymentForm), newForm.GetType());

				var newFormAsPaymentForm = (APPaymentForm)newForm;
				var typeOfBusinessEntity = newFormAsPaymentForm.BusinessEntity.GetType();

				AssertEquals("Type of New BusinessEntity", typeof(APPayment), typeOfBusinessEntity);
				AssertEquals("BusinessEntity in database", false, ((BusinessObject)newFormAsPaymentForm.BusinessEntity).IsInDatabase);

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
