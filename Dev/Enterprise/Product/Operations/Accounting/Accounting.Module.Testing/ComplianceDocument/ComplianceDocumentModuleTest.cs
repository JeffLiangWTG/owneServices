using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class ComplianceDocumentModuleTest : ZModuleBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			fComplianceDocumentModule = (ComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID());
		}

		protected override void TearDown()
		{
			if (fComplianceDocumentModule != null)
			{
				fComplianceDocumentModule.Dispose();
			}
			base.TearDown();
		}

		protected ComplianceDocumentModule fComplianceDocumentModule;
		protected ComplianceDocumentModule TestComplianceDocumentModule
		{
			get { return fComplianceDocumentModule; }
		}

		protected ComplianceDocumentController Controller;

		protected abstract SecurityCheckpoint CheckpointForVoid { get; }

		public void TestAllowNew()
		{
			Assert(!TestComplianceDocumentModule.AllowNew);
		}

		public void TestSupportWorkflow()
		{
			Assert(TestComplianceDocumentModule.SupportsWorkflow);
		}

		public void TestVoidComplianceDocumentMenuItem()
		{
			var header = CreateINVComplianceDocumentHeader();
			var voidComplianceDocumentsOldSevurity = CheckpointForVoid.IsAllowed;

			try
			{
				using (var complianceDocumentModule = (ComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var menus = complianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var voidComplianceDocumentMenuItem = menus.FindByText("Void");
					AssertNotNull("Void menu item should exist.", voidComplianceDocumentMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(complianceDocumentModule.EmbeddedControl);
						form.Show();

						voidComplianceDocumentMenuItem.PerformClick();
						AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessages();

						complianceDocumentModule.PerformSearch_ForTest();
						complianceDocumentModule.DisplayGrid.Select(0);
						AssertEquals(1, complianceDocumentModule.DisplayGrid.SelectedElements.Length);

						CheckpointForVoid.IsAllowed = false;
						voidComplianceDocumentMenuItem.PerformClick();
						AssertEquals(CheckpointForVoid.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessages();

						CheckpointForVoid.IsAllowed = true;
						header.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;
						Factory.Save();

						voidComplianceDocumentMenuItem.PerformClick();
						AssertEquals("This compliance document is already finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessages();

						header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						Factory.Save();

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						voidComplianceDocumentMenuItem.PerformClick();
						AssertEquals("This compliance document does not have document number set. Do you want to delete this document record instead?", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessages();

						var lastShownForm = complianceDocumentModule.LastShownComplianceDocumentForm_ForTest_ForTestOnly;
						AssertEquals("System should show the Void Compliance Document form", typeof(ComplianceDocumentForm), lastShownForm.GetType());
						AssertEquals("System should show the Void Compliance Document form", "Void Compliance Document", lastShownForm.Text);
						if (lastShownForm != null)
						{
							lastShownForm.Close();
							if (!lastShownForm.IsDisposed)
							{
								lastShownForm.Dispose();
							}
						}

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						voidComplianceDocumentMenuItem.PerformClick();
						AssertEquals("This compliance document does not have document number set. Do you want to delete this document record instead?", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessages();

						lastShownForm = complianceDocumentModule.LastShownComplianceDocumentForm_ForTest_ForTestOnly;
						AssertEquals("System should show the Delete Compliance Document form", typeof(ComplianceDocumentForm), lastShownForm.GetType());
						AssertEquals("Delete Compliance Document", lastShownForm.Text);
						if (lastShownForm != null)
						{
							lastShownForm.Close();
							if (!lastShownForm.IsDisposed)
							{
								lastShownForm.Dispose();
							}
						}

						UnitTestUserNotification.Instance.ClearMessages();
						header.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
						Factory.Save();

						voidComplianceDocumentMenuItem.PerformClick();
						AssertEquals("This compliance document is already voided.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				CheckpointForVoid.IsAllowed = voidComplianceDocumentsOldSevurity;
			}
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected abstract InvoicingBase CreateInvoice();

		protected abstract AccComplianceDocumentHeader NewComplianceDocumentHeader();

		protected AccComplianceDocumentHeader CreateINVComplianceDocumentHeader()
		{
			var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "", 3);
			vat3.AT_PostingGroupId = 0;

			var ac1 = TestObjectCreator.CreateChargeCode("AC1");
			ac1.AC_AT_GSTRate = vat3.PK;
			ac1.AC_Desc = "desc";

			Invoice invoice = (Invoice)CreateInvoice();
			invoice.AH_OH = TestObjectCreator.Agent.PK;
			var invoiceLine = invoice.Lines.AddNew() as InvoiceLine;
			invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			invoiceLine.AL_AC = ac1.PK;
			invoiceLine.AL_AT = vat3.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = TestObjectCreator.Job1.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = ac1.PK;
			charge.JR_AT_CostGSTRate = invoiceLine.AL_AT;
			charge.JR_AL_APLine = invoice is APInvoice ? invoiceLine.PK : ZGuid.Empty;
			charge.JR_AL_ARLine = invoice is ARInvoice ? invoiceLine.PK : ZGuid.Empty;

			var header = NewComplianceDocumentHeader();

			header.ADH_Ledger = invoice.AH_Ledger;
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.ADH_DocumentNumber = ZString.Empty;
			header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			header.ADH_DocumentDate = new ZDateTime(2019, 02, 01);
			header.ADH_ReportingPeriod = 201902;

			var line = Factory.New<AccComplianceDocumentLine>();
			line.ADL_Sequence = 1;
			line.ADL_ADH = header.PK;
			line.ADL_Description = "desc";

			var pivot = Factory.New<AccComplianceDocumentPivot>();
			pivot.ADP_ADL = line.PK;
			pivot.ADP_AL = invoiceLine.PK;

			Factory.Save();

			return header;
		}
	}
}
