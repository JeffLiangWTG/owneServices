using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkConsolCostImportForm))]
	public class APInvoiceBulkConsolCostImportFormTest : ZFormBasherTest
	{
		public void TestSelectDeSelectAllButtons()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var consol1 = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = testObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol1);
			testObjectCreator.CreateJob(shipment, false);
			testObjectCreator.CreateConsolCost(consol1, testObjectCreator.CC1, testObjectCreator.USD, 1.5m, 150m, testObjectCreator.AALSHI);

			var consol2 = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00002");
			var shipment2 = testObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol2);
			testObjectCreator.CreateJob(shipment2, false);
			testObjectCreator.CreateConsolCost(consol2, testObjectCreator.CC1, testObjectCreator.USD, 1.5m, 200m, testObjectCreator.AALSHI);

			Factory.Save();

			using (InvoicingBaseBulkConsolCostImportForm testForm = (InvoicingBaseBulkConsolCostImportForm)GetFormToBash())
			{
				var importer = testForm.BusinessEntity as InvoicingBaseBulkConsolCostImporter;
				AssertNotNull(importer);

				importer.LoadConsolsCollection();
				AssertEquals(2, importer.Consols.Count);

				testForm.Show();

				testForm.DeselectAllButton_ForTestOnly.PerformClick();
				AssertEquals(false, importer.Consols[0].IsSelectedForImport);
				AssertEquals(false, importer.Consols[1].IsSelectedForImport);

				testForm.SelectAllButton_ForTestOnly.PerformClick();
				AssertEquals(true, importer.Consols[0].IsSelectedForImport);
				AssertEquals(true, importer.Consols[1].IsSelectedForImport);
			}
		}

		[ExpectNoExceptions]
		public void TestSelectDeSelectAllButtonsWhenNoConsolCostNoLongerAvailable()
		{
			var newFactory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);

			var consol1 = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = testObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol1);
			testObjectCreator.CreateJob(shipment, false);
			var consolcost = testObjectCreator.CreateConsolCost(consol1, testObjectCreator.CC1, testObjectCreator.USD, 1.5m, 150m, testObjectCreator.AALSHI);

			var consol2 = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00002");
			var shipment2 = testObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol2);
			testObjectCreator.CreateJob(shipment2, false);
			var consolcost2 = testObjectCreator.CreateConsolCost(consol2, testObjectCreator.CC1, testObjectCreator.USD, 1.5m, 200m, testObjectCreator.AALSHI);

			newFactory.Save();

			using (InvoicingBaseBulkConsolCostImportForm testForm = (InvoicingBaseBulkConsolCostImportForm)GetFormToBash())
			{
				var importer = testForm.BusinessEntity as InvoicingBaseBulkConsolCostImporter;
				AssertNotNull(importer);

				var invoice = importer.Parent.ParentAPInvoice;
				invoice.AH_OH = testObjectCreator.AALSHI.PK;
				invoice.AH_TransactionNum = "001";
				invoice.SubmittedFromInvoicingForm = true;

				importer.LoadConsolsCollection();

				AssertEquals(2, importer.Consols.Count);

				testForm.Show();

				testForm.SelectAllButton_ForTestOnly.PerformClick();

				var newFactory2 = new BusinessObjectFactory();
				var consolCostInNewFactory2 = newFactory2.Load<JobConsolCost>(consolcost.PK);
				consolCostInNewFactory2.ApportionmentCharges.RemoveAndDeleteAll();
				consolCostInNewFactory2.Delete();
				newFactory2.Save();

				AssertNoExceptionThrown(() => testForm.DeselectAllButton_ForTestOnly.PerformClick());
				AssertNoExceptionThrown(() => testForm.SelectAllButton_ForTestOnly.PerformClick());
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
					using (var form = (InvoicingBaseBulkConsolCostImportForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(false, form.ConsolCostsGrid_ForTestOnly.GetColumnStyle("E6_PlaceOfSupply").IsUnavailable);
					}
				}
			}

			foreach (var regValue in new[] { true, false })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					using (var form = (InvoicingBaseBulkConsolCostImportForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();
						AssertEquals(true, form.ConsolCostsGrid_ForTestOnly.GetColumnStyle("E6_PlaceOfSupply").IsUnavailable);
					}
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			APInvoice invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(costing);
			return new InvoicingBaseBulkConsolCostImportForm(importer);
		}
	}
}
