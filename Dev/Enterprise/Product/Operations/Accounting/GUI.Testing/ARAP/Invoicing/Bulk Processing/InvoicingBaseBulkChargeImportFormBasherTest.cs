using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkChargeImportForm))]
	public class InvoicingBaseBulkChargeImportFormBasherTest : ZFormBasherTest
	{
		public void TestSelectDeSelectAllButtons()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("ABC", "DEF", "C0001");
			consol.JK_MasterBillNum = "12345678901";
			var shipment1 = creator.CreateShipment("S0001", consol);
			var shipment2 = creator.CreateShipment("S0002", consol);
			shipment1.JS_HouseBill = "10987654321";
			shipment2.JS_HouseBill = "10987654322";
			var job1 = creator.CreateJob(shipment1);
			var job2 = creator.CreateJob(shipment2);
			creator.CreateCharge(job1);
			creator.CreateCharge(job2);

			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			var importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();

			using (var form = new InvoicingBaseBulkChargeImportForm(importer))
			{
				AssertNotNull(importer);
				AssertEquals(2, importer.Jobs.Count);

				form.Show();

				form.DeselectAllButton_ForTestOnly.PerformClick();
				AssertEquals(false, importer.Jobs[0].IsSelectedForImport);
				AssertEquals(false, importer.Jobs[1].IsSelectedForImport);

				form.SelectAllButton_ForTestOnly.PerformClick();
				AssertEquals(true, importer.Jobs[0].IsSelectedForImport);
				AssertEquals(true, importer.Jobs[1].IsSelectedForImport);
			}
		}

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

			var invoice = Factory.New<APInvoice>();
			var importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();
			AssertEquals(1, importer.Jobs.Count);

			using (var form = new InvoicingBaseBulkChargeImportForm(importer))
			{
				form.Show();
				var relatedJobNumberColumn = form.ChargesGrid_ForTestOnly.Columns.FirstOrDefault(c => c.ColumnName == "JR_Calc_RelatedJobNumber");
				AssertNotNull("Related Job Number column should exist.", relatedJobNumberColumn);
				Assert("Related Job Number is not available by default.", !relatedJobNumberColumn.IsVisible);
				Assert("Related Job Number is read only.", relatedJobNumberColumn.ColumnStyle.ReadOnly);

				form.JobsGrid_ForTestOnly.SelectSingleElementByPK(gatewayJob.PK);
				form.ChargesGrid_ForTestOnly.Select(0);
				AssertEquals(shipment.JS_UniqueConsignRef, ((Charge)form.ChargesGrid_ForTestOnly.GetFirstSelectedRow()).JR_Calc_RelatedJobNumber);
			}
		}

		public void TestPlaceOfSupplyDropEditVisiblity()
		{
			foreach (var regValue in new[] { true, false })
			{
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					using (var form = (InvoicingBaseBulkChargeImportForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(false, form.ChargesGrid_ForTestOnly.GetColumnStyle("JR_CostPlaceOfSupply").IsUnavailable);
					}
				}
			}

			foreach (var regValue in new[] { true, false })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					using (var form = (InvoicingBaseBulkChargeImportForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(true, form.ChargesGrid_ForTestOnly.GetColumnStyle("JR_CostPlaceOfSupply").IsUnavailable);
					}
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			APInvoice invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
			return new InvoicingBaseBulkChargeImportForm(importer);
		}
	}
}
