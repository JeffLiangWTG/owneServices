using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(JobChargesPopupForm))]
	public class JobChargesPopupFormTest : ZFormBasherTest
	{
		public void TestRelatedJobNumber()
		{
			var creator = new TestObjectCreator(Factory);
			var gatewayConsol = creator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = creator.CreateShipment("S00011", gatewayConsol);
			Factory.Save();

			var gatewayJob = creator.CreateJob(gatewayConsol, createWithMutex: false);
			var gatewayCharge = creator.CreateCharge(gatewayJob, creator.CC1, 10m, 10m);
			gatewayCharge.JR_Calc_RelatedJobNumber = shipment.JS_UniqueConsignRef;
			Factory.Save();

			var invoiceLine = Factory.New<APInvoiceLine>();
			invoiceLine.AL_AC = creator.CC1.PK;
			invoiceLine.AL_JH = gatewayJob.PK;
			var importer = new JobChargesImporter(invoiceLine);
			AssertEquals(1, importer.JobChargesCollection.Count);

			using (var form = new JobChargesPopupForm(importer))
			{
				form.Show();
				var jobChargesGrid = form.Controls.Find("JobChargesGrid", true).FirstOrDefault();
				AssertNotNull(jobChargesGrid);

				var relatedJobNumberColumn = ((ZDisplayGrid)jobChargesGrid).Columns.FirstOrDefault(c => c.ColumnName == "JR_Calc_RelatedJobNumber");
				AssertNotNull("Related Job Number column should exist.", relatedJobNumberColumn);
				Assert("Related Job Number is not available by default.", !relatedJobNumberColumn.IsVisible);
				Assert("Related Job Number is read only.", relatedJobNumberColumn.ColumnStyle.ReadOnly);

				((ZDisplayGrid)jobChargesGrid).Select(0);
				AssertEquals(shipment.JS_UniqueConsignRef, ((Charge)((ZDisplayGrid)jobChargesGrid).GetFirstSelectedRow()).JR_Calc_RelatedJobNumber);
			}
		}

		public void TestTaxBranchVisibility()
		{
			AssertTaxBranchVisibility(true, true);
			AssertTaxBranchVisibility(true, false);
			AssertTaxBranchVisibility(false, true);
			AssertTaxBranchVisibility(false, false);

			void AssertTaxBranchVisibility(bool isEnableRegistry, bool isGSTRegistered)
			{
				var line = Factory.NewWithValidTestData<APInvoiceLine>();
				var businessEntity = new JobChargesImporter(line);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableRegistry))
				using (var form = new JobChargesPopupForm(businessEntity))
				{
					form.Show();

					var expectedVisible = isEnableRegistry && isGSTRegistered;
					AssertEquals(expectedVisible, form.JobChargesGrid.Columns.Contains(AutoJobCharge.Schema.JR_GB_CostTaxBranch));
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			InvoicingLineBase apLine = (InvoicingLineBase)Factory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			apLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			return new JobChargesPopupForm(new JobChargesImporter(apLine));
		}

		public void TestFormCaption()
		{
			using (JobChargesPopupForm form = (JobChargesPopupForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Caption must be 'Import Charges'", "Import Charges", form.FormCaption);
			}
		}

		public void TestFormVerb()
		{
			using (JobChargesPopupForm form = (JobChargesPopupForm)GetFormToBashCore())
			{
				AssertEquals("Caption must be empty", string.Empty, form.FormVerb);
			}
		}
	}
}
