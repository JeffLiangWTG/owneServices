using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing.Apportionment
{
	public class ApportionmentForCommonWorkSheetPluginTest : ApportionmentPlugInBaseTest
	{
		protected override ApportionmentPlugin GetTestApportionmentPluginObject(IBusiness consol)
		{
			return new ApportionmentForCommonWorkSheetPlugin(consol);
		}

		public void TestSecurityWhenPostInvoice()
		{
			using (AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.PayInvoicesAllowFurtherGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestSecurityWhenPreviewOrPostInvoice(JobInvoicingPostingOption.All, false);
			}
		}

		public void TestSecurityWhenPreviewConsolCosts()
		{
			TestSecurityWhenPreviewOrPostInvoice(JobInvoicingPostingOption.ConsolCosts, true);
		}

		public void TestSecurityWhenPreviewAll()
		{
			TestSecurityWhenPreviewOrPostInvoice(JobInvoicingPostingOption.All, true);
		}

		void TestSecurityWhenPreviewOrPostInvoice(JobInvoicingPostingOption postingOption, bool isPreview)
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var legs = new TestCommonWorkSheetHelper(Factory).CreateCartageLegs(cartage, 1);
			workSheet.CartageLegs.AddRange(legs);
			var job = new Job.Loader(cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.ActiveOrAllAddresses[0].PK;

			var listing = workSheet.GetApportionments();
			var cost = listing.CostsCollection.TryAddNew();
			cost.E6_OSCostAmount = 10m;
			cost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			Factory.Save();

			AssertEquals("Precondition:", 1, cost.ApportionmentCharges.Count);

			using (ZForm form = new ZForm(workSheet))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.ApportionmentForCommonWorkSheet);
				form.Show();

				using (var plugIn = (ApportionmentPlugin)form.PlugIns.Instances[0])
				{
					if (isPreview)
					{
						plugIn.PreviewTransactions_ForTestOnly(postingOption);
						var invoicePreviewForm = (InvoicePreviewForm)ZFormModaliser.LastFormShownForTest;

						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNotNull(invoicePreviewForm);

						invoicePreviewForm.InvoicesGrid_ForTestOnly.Select(0);
						invoicePreviewForm.PreviewOnlyButton_ForTestOnly.PerformClick();
						Application.DoEvents();
						Assert("Preview successfully, show XLSPreviewForm", PreviewFormTestHelper.CloseOpenedForms());
					}
					else
					{
						plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
						AssertContains("Do you want to print", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertType<APInvoiceRequisitionForm>("Post successfully, show APInvoiceRequisitionForm", ZFormModaliser.LastFormShownDialogForTest);
					}
				}
			}
		}
	}
}
