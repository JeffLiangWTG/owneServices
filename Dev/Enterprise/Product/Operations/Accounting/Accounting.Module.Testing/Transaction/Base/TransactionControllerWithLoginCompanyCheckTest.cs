using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Transaction.Base.Testing
{
	public abstract class TransactionControllerWithLoginCompanyCheckTest : ZControllerBasherTest
	{
		public void TestShowViewFormWithLoginCompanyMatch()
		{
			TestShowForm_TransactionMustMatchCurrentLoginCompany((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditFormWithLoginCompanyMatch()
		{
			TestShowForm_TransactionMustMatchCurrentLoginCompany((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		protected virtual void TestShowForm_TransactionMustMatchCurrentLoginCompany(Action<TransactionControllerWithLoginCompanyCheck, BusinessObject> showFormDelegate)
		{
			var controller = ZControllerFactory.Create(GetControllerID()) as TransactionControllerWithLoginCompanyCheck;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "GC2";
			otherCompany.GC_Name = "company description";
			otherCompany.GC_RN_NKCountryCode = "AU";

			var transactionHeaderForSameCompany = ParentTransactionHeaderRow as AccTransactionHeader;
			transactionHeaderForSameCompany.AH_GC = Env.CurrentCompany.PK;
			AssertShowFormResult(showFormDelegate, controller, transactionHeaderForSameCompany, true);

			var transactionHeaderForDifferentCompany = ParentTransactionHeaderRow as AccTransactionHeader;
			transactionHeaderForDifferentCompany.AH_GC = otherCompany.PK;
			AssertShowFormResult(showFormDelegate, controller, transactionHeaderForDifferentCompany, !controller.ShouldCheckLoginCompanyMatch_ForTestOnly);

			var transactionHeaderWithOutCompany = ParentTransactionHeaderRow as AccTransactionHeader;
			transactionHeaderForDifferentCompany.AH_GC = ZGuid.Empty;
			AssertShowFormResult(showFormDelegate, controller, transactionHeaderWithOutCompany, true);
		}

		public void TestNonMatchingControllerIDsShouldReturnNullWhenEditingForm()
		{
			AssertNonMatchingControllerIDsShouldReturnNullWhenEditingOrCopyingForm((controller, invoice, copyOfBusinessObject) => controller.ShowEditForm(invoice));
		}

		public void TestNonMatchingControllerIDsShouldReturnNullWhenCopyingForm()
		{
			AssertNonMatchingControllerIDsShouldReturnNullWhenEditingOrCopyingForm((controller, invoice, copyOfBusinessObject) => controller.ShowCopyForm_ForTestOnly(invoice, copyOfBusinessObject));
		}

		void AssertNonMatchingControllerIDsShouldReturnNullWhenEditingOrCopyingForm(Func<TransactionControllerWithLoginCompanyCheck, InvoicingBase, Func<IBusiness, IBusiness>, IZForm> action)
		{
			var controller = ZControllerFactory.Create(GetControllerID()) as TransactionControllerWithLoginCompanyCheck;
			AssertEquals(controller.ID, GetControllerID());

			var invoiceChangedState = ChangeStateOfParentTransactionHeaderRow();
			if (invoiceChangedState != null)
			{
				AssertNull(controller.GetLoadedBusinessEntityInLocalFactory_ForTestOnly(invoiceChangedState));
				using (var form = action(controller, invoiceChangedState, delegate(IBusiness loadedSourceEntity)
				{
					return ((ITemplateCopyable)loadedSourceEntity).TemplateCopy();
				}))
				{
					AssertNull(form);
				}
				AssertEquals(MessageToShowWhenControllerMismatchForBusinessEntity, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertNotEquals("Controllers that implement the INavigationControllerIDProvider must be tested by overriding ChangeStateOfParentTransactionHeaderRow()", typeof(INavigationControllerIDProvider), controller.GetType());
			}
		}

		protected virtual string MessageToShowWhenControllerMismatchForBusinessEntity => null;

		void AssertShowFormResult(Action<TransactionControllerWithLoginCompanyCheck, BusinessObject> showFormDelegate,
			TransactionControllerWithLoginCompanyCheck controller,
			BusinessObject sourceEntity,
			bool expectedIsAllowed)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var expectedLastMessage = "This Transaction is posted into the ledgers of another company on this database. You must login to the following company to view this transaction: GC2 - company description.";
			var expectedLastCaption = "Access Denied: Incorrect login company";

			showFormDelegate(controller, sourceEntity);
			using (var lastShownForm = controller.LastShownForm)
			{
				if (expectedIsAllowed)
				{
					CombineAssertions(() =>
					{
						AssertNotNull("LastShownForm", lastShownForm);
						AssertNotContains("LastMessage.Text", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNotContains("LastMessage.Caption", expectedLastCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					});
				}
				else
				{
					CombineAssertions(() =>
					{
						AssertNull("LastShownForm", lastShownForm);
						AssertEquals("LastMessage.Text", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("LastMessage.Caption", expectedLastCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					});
				}
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			SetupTransactionHeaderRows();
		}

		protected virtual InvoicingBase ChangeStateOfParentTransactionHeaderRow() => null;
		protected virtual void SetupTransactionHeaderRows() { }
		protected abstract BusinessObject ParentTransactionHeaderRow { get; }

		#endregion
	}
}
