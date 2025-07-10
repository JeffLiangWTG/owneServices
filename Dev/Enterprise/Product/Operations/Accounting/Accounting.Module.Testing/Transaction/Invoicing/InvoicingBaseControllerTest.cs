using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class InvoicingBaseControllerTest : AccountingTransactionControllerTest
	{
		public void TestShowImportedDataForm()
		{
			InvoicingBase transaction = (InvoicingBase)Factory.New(GetExpectedBusinessObjectType());
			IZForm form1 = ((InvoicingBaseController)Controller).ShowImportedDataForm(transaction);
			AssertNotNull("Form should not be null", form1);
			AssertEquals("Form Type", form1.GetType(), GetExpectedFormType());
			AssertEquals("IsImportedFromFile", true, transaction.IsImportedFromFile);

			if (Controller != null && Controller.LastShownForm != null)
			{
				BaseInvoicingForm form = Controller.LastShownForm as BaseInvoicingForm;
				if (form != null)
				{
					Application.DoEvents();
					bool expected = transaction.AH_Ledger != LedgerTypes.AccountsReceivable;
					AssertEquals("InvoiceTotalValidationCheckbBox should be visible", expected, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
					AssertEquals("ValidInvoiceTotalCalcEdit should be visible", expected, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
				}
				Controller.LastShownForm.Dispose();
			}
		}

		protected void TestTemplateCopyFormIsInEditModeCore()
		{
			InvoicingBase transaction = (InvoicingBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			Factory.Save();
			IZForm form = ((InvoicingBaseController)Controller).ShowTemplateCopyForm(transaction);
			AssertNotNull("Form should not be null", form);
			AssertEquals("Form's Display mode should be Edit", ODisplayMode.Edit, form.DisplayMode);
			if (form != null)
			{
				form.Dispose();
			}
		}

		public void TestShowDeleteForm()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			InvoicingBase transaction = (InvoicingBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
			transaction.AH_ExchangeRate = 0.99m;
			Factory.Save();

			using (ZForm form = Controller.ShowDeleteForm(transaction) as ZForm)
			{
				Assert("Exchange rate should be readonly", (form.BusinessEntity as TransactionHeader).AH_ExchangeRateInfo.ReadOnly);
			}
		}

		public void TestShowNewFormDefaultsCurrency()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			org.CompanyData.OB_RX_NKARDDefltCurrency = uSD.RX_Code;
			org.CompanyData.OB_RX_NKAPDefltCurrency = uSD.RX_Code;
			org.CompanyData.OB_AC_APDefaultChargeCode = chargeCode.PK;
			Factory.Save();

			var collection = new ITransactionCollection<InvoicingBase>(Factory);
			collection.OrganizationGuid = org.PK;
			Controller.SetCollectionForDefaultsAndValidation(collection);
			using (ZForm form = (ZForm)Controller.ShowNewForm())
			{
				InvoicingBase invBase = (InvoicingBase)form.BusinessEntity;
				Assert("SubmittedFromForm should be true", invBase.SubmittedFromInvoicingForm);
				AssertEquals("Currency should be USD", uSD.RX_Code, invBase.AH_RX_NKTransactionCurrency);
				if (invBase is APInvoice)
				{
					AssertEquals("There should be one line", 1, invBase.Lines.Count);
					AssertEquals("Chargecode should be the test charge code", chargeCode.PK, invBase.Lines[0].AL_AC);
				}
			}
		}

		public void TestShowImportedNoSecurityRights()
		{
			InvoicingBase transaction = (InvoicingBase)Factory.New(GetExpectedBusinessObjectType());
			InvoicingLineBase line = (InvoicingLineBase)transaction.Lines.AddNew();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			if (transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				Job job = new Job.Loader(shipment).TryCreateWithMutex();
				line.AL_JH = job.PK;
			}

			((InvoicingBaseController)Controller).CheckPointForNew_ForTestOnly.IsAllowed = false;
			AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
			{
				IZForm form = ((InvoicingBaseController)Controller).ShowImportedDataForm(transaction);
				AssertNull("Form should be null", form);
				AssertContains("Security right error message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			if (transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				using (Job job = new Job.Loader(shipment).TryCreateWithMutex())
				{
					AssertNotNull("Job should have been created because mutex should have been released.", job);
				}
			}

			AssertEquals("IsImportedFromFile", false, transaction.IsImportedFromFile);
		}

		public virtual void TestGetFormCoreThroughProcessTask_NoException()
		{
			try
			{
				using (var processTasks = new ProcessTasksModule())
				{
					((IWorkflowProvider)TestTransaction).WorkflowItems.Tasks.AddNew();
					processTasks.ModuleDecisionProvider.HandleDefaultAction(((IWorkflowProvider)TestTransaction).WorkflowItems.ToArray());
					var form = ZApplication.GetOpenForms().FirstOrDefault(x => x.GetType() == GetExpectedFormType());
					AssertNotNull(form);
				}
			}
			finally
			{
				ZApplication.GetOpenForms().First(x => x.GetType() == GetExpectedFormType()).Dispose();
			}
		}

		protected abstract Type GetExpectedBusinessObjectType();

		protected abstract Type GetExpectedFormType();

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestTransaction; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestTransaction = (InvoicingBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			Factory.Save();
		}

		protected InvoicingBase TestTransaction;
	}
}
