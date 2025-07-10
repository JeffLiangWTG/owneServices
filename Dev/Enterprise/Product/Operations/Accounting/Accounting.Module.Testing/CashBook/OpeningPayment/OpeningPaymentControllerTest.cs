using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(OpeningPaymentController))]
	class OpeningPaymentControllerTest : AccountingTransactionControllerTest
	{
		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookOpeningPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewCashBookOpeningPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewCashBookOpeningPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewCashBookOpeningPayment; }
		}

		public void TestGetFormSetsSubmittedFromForm()
		{
			Controller.Reversing_ForTestOnly = null;
			OpeningPayment.SubmittedFromForm = false;
			using (IZForm newForm = Controller.GetForm_ForTestOnly(OpeningPayment))
			{
				Assert("SubmittedFromForm should be true since the payment is not being reversed", OpeningPayment.SubmittedFromForm);
			}

			OpeningPayment.SubmittedFromForm = false;
			Controller.Reversing_ForTestOnly = new ReversingFactory().NewReversing(OpeningPayment);
			Controller.Reversing_ForTestOnly.Reverse();
			using (IZForm reversingForm = Controller.GetForm_ForTestOnly(OpeningPayment))
			{
				Assert("SubmittedFromForm should be false since the payment is being reversed", !OpeningPayment.SubmittedFromForm);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OpeningPayment;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return OpeningPayment; }
		}

		protected new OpeningPaymentController Controller
		{
			get { return base.Controller as OpeningPaymentController; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			OpeningPayment = Factory.NewWithValidTestData<OpeningPayment>();
			Factory.Save();
		}

		OpeningPayment OpeningPayment;
	}
}
