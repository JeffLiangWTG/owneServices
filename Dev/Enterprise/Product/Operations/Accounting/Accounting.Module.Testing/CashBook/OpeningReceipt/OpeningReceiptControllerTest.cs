using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(OpeningReceiptController))]
	class OpeningReceiptControllerTest : AccountingTransactionControllerTest
	{
		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookOpeningReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewCashBookOpeningReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewCashBookOpeningReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewCashBookOpeningReceipt; }
		}

		public void TestGetForm()
		{
			using (OpeningReceiptForm orcForm = Controller.GetForm_ForTestOnly(OpeningReceipt) as OpeningReceiptForm)
			{
				AssertNotNull("Form type should be OpeningReceiptForm", orcForm);
			}
		}

		public void TestGetFormSetsSubmittedFromForm()
		{
			Controller.Reversing_ForTestOnly = null;
			OpeningReceipt.SubmittedFromForm = false;
			using (OpeningReceiptForm orcForm = (OpeningReceiptForm)Controller.GetForm_ForTestOnly(OpeningReceipt))
			{
				Assert("SubmittedFromForm should be true", OpeningReceipt.SubmittedFromForm);
			}

			OpeningReceipt.SubmittedFromForm = false;
			Controller.Reversing_ForTestOnly = new ReversingFactory().NewReversing(OpeningReceipt);
			Controller.Reversing_ForTestOnly.Reverse();
			using (OpeningReceiptForm orcForm = (OpeningReceiptForm)Controller.GetForm_ForTestOnly(OpeningReceipt))
			{
				Assert("SubmittedFromForm should be false", !OpeningReceipt.SubmittedFromForm);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OpeningReceipt;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return OpeningReceipt; }
		}

		protected new OpeningReceiptController Controller
		{
			get { return base.Controller as OpeningReceiptController; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			OpeningReceipt = Factory.NewWithValidTestData<OpeningReceipt>();
			Factory.Save();
		}

		OpeningReceipt OpeningReceipt;
	}
}
