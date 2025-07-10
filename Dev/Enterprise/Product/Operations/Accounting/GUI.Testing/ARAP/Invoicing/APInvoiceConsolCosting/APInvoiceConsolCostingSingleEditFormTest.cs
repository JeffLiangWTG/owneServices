using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceConsolCostingSingleEditForm))]
	public class APInvoiceConsolCostingSingleEditFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			APInvoice inv = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			JobConsolCost cost = inv.ConsolCosting.ConsolCosts.AddNew();
			return new APInvoiceConsolCostingSingleEditForm(cost);
		}

		public void TestApplyChangesDialogAppearsWhenFormIsClosed()
		{
			APInvoice inv = Factory.New<APInvoice>();
			JobConsolCost cost = inv.ConsolCosting.ConsolCosts.AddNew();
			using (APInvoiceConsolCostingSingleEditForm form = new APInvoiceConsolCostingSingleEditForm(cost))
			{
				form.Show();
				form.Close();
				Assert("User should be asked", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"Do you want to apply changes?"));
			}
		}

		public void TestFormIsNotClosedWhenNoIsSelectedOnDialog()
		{
			APInvoice inv = Factory.New<APInvoice>();
			JobConsolCost cost = inv.ConsolCosting.ConsolCosts.AddNew();
			using (APInvoiceConsolCostingSingleEditForm form = new APInvoiceConsolCostingSingleEditForm(cost))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.Close();
				Assert("User should be asked", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"Do you want to apply changes?"));
				AssertEquals("Should be still showing the single edit form", false, form.IsDisposed);
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
					var inv = Factory.New<APInvoice>();
					var cost = inv.ConsolCosting.ConsolCosts.AddNew();
					using (var form = new APInvoiceConsolCostingSingleEditForm(cost))
					{
						form.Show();
						AssertEquals(true, form.FixedPlaceOfSupplyDropEdit_ForTestOnly.Visible);
						AssertEquals(true, !form.ApportionedChargesGrid_ForTestOnly.GetColumnStyle("JR_CostPlaceOfSupply").IsUnavailable);
						form.BusinessEntity.Factory.RemoveContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice);
					}
				}
			}

			foreach (var regValue in new[] { true, false })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var inv = Factory.New<APInvoice>();
					var cost = inv.ConsolCosting.ConsolCosts.AddNew();
					using (var form = new APInvoiceConsolCostingSingleEditForm(cost))
					{
						form.Show();
						AssertEquals(false, form.FixedPlaceOfSupplyDropEdit_ForTestOnly.Visible);
						AssertEquals(false, !form.ApportionedChargesGrid_ForTestOnly.GetColumnStyle("JR_CostPlaceOfSupply").IsUnavailable);
						form.BusinessEntity.Factory.RemoveContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice);
					}
				}
			}
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirement_ConsolCosting()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			var job1 = creator.CreateJob(shipment1);
			var shipment2 = consol.Shipments.AddNew();
			var job2 = creator.CreateJob(shipment2);

			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creator.AALSHI.PK;

			try
			{
				using (var invoiceForm = new InvoiceForm(invoice))
				{
					invoiceForm.Show();

					//Set up the test data
					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost1 = invoice.ConsolCosting.ConsolCosts.AddNew();
					cost1.E6_ParentID = consol.PK;
					cost1.E6_ParentTableCode = "JK";
					cost1.E6_AC_ChargeCode = creator.CC12.PK;
					cost1.E6_OSCostAmount = 70M;
					Assert(!cost1.HasErrors);

					var cost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
					cost2.E6_ParentID = consol.PK;
					cost2.E6_ParentTableCode = "JK";
					cost2.E6_AC_ChargeCode = creator.CC13.PK;
					cost2.E6_OSCostAmount = 170M;
					Assert(!cost2.HasErrors);

					TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode, false, invoice);

					cost1.ApportionmentCharges[0].JR_OSCostAmt = 30M;
					cost1.ApportionmentCharges[1].JR_OSCostAmt = 40M;
					cost2.ApportionmentCharges[0].JR_OSCostAmt = 50M;
					cost2.ApportionmentCharges[1].JR_OSCostAmt = 120M;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					consolCostingForm.Close();
					AssertEquals("LastMessage.Text", "Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					Application.DoEvents();

					AssertEquals("There should be 4 lines in the AP Invoice.", 4, invoice.Lines.Count);
					var cc1Job1Line = invoice.Lines[0];
					var cc1Job2Line = invoice.Lines[1];
					var cc2Job1Line = invoice.Lines[2];
					var cc2Job2Line = invoice.Lines[3];
					AssertEquals("The cost variance requires None approval, 30 is NOT above 100.", ZBool.True, cc1Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires None approval, 40 is NOT above 100.", ZBool.True, cc1Job2Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires None approval, 50 is NOT above 100.", ZBool.True, cc2Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires 1st level approval, 120 is above 100.", ZBool.False, cc2Job2Line.AL_IsFinalCharge);

					//Operate in the form
					invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
					var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
					AssertNotNull(menuItem);
					menuItem.PerformClick();
					AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingSingleEditForm), ZFormModaliser.ActiveForm.GetType());
					var singleConsolCostingEditForm = (APInvoiceConsolCostingSingleEditForm)ZFormModaliser.ActiveForm;
					AssertEquals(typeof(JobConsolCost), singleConsolCostingEditForm.BusinessEntity.GetType());
					var jobConsolCost = (JobConsolCost)singleConsolCostingEditForm.BusinessEntity;
					AssertEquals(2, singleConsolCostingEditForm.ApportionedChargesGrid_ForTestOnly.ListManager.Count);
					AssertEquals(typeof(ApportionSplitCharge), singleConsolCostingEditForm.ApportionedChargesGrid_ForTestOnly.ListManager.List[0].GetType());
					var charge1 = (ApportionSplitCharge)singleConsolCostingEditForm.ApportionedChargesGrid_ForTestOnly.ListManager.List[0];
					var charge2 = (ApportionSplitCharge)singleConsolCostingEditForm.ApportionedChargesGrid_ForTestOnly.ListManager.List[1];
					AssertEquals(creator.CC12.PK, charge1.JR_AC);
					AssertEquals(creator.CC12.PK, charge2.JR_AC);
					AssertEquals(ZBool.True, charge1.IsFinal);
					AssertEquals(ZBool.True, charge2.IsFinal);

					var expectError = @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag";

					Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;
					charge1.IsFinal = ZBool.False;
					AssertHasError("Should have error.", charge1.IsFinalInfo, expectError);

					Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;
					var validation = charge1.Validation as ApportionSplitChargeValidation;
					validation.ValidateIsFinal();
					AssertNoErrors("Should have no errors.", charge1.IsFinalInfo);

					jobConsolCost.E6_OSCostAmount = 180m;
					charge1.JR_OSCostAmt = 30m;
					charge2.JR_OSCostAmt = 150m;
					AssertEquals("The cost variance requires None approval, 30 is NOT above 100.", ZBool.True, charge1.IsFinal);
					AssertEquals("The cost variance requires 1st level approval, 150 is above 100.", ZBool.False, charge2.IsFinal);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					singleConsolCostingEditForm.Close();
					AssertEquals("LastMessage.Text", "Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					Application.DoEvents();

					cc1Job1Line = invoice.Lines[0];
					cc1Job2Line = invoice.Lines[1];
					cc2Job1Line = invoice.Lines[2];
					cc2Job2Line = invoice.Lines[3];
					AssertEquals("Imported from the apportion split charge.", ZBool.True, cc1Job1Line.AL_IsFinalCharge);
					AssertEquals("Imported from the apportion split charge.", ZBool.False, cc1Job2Line.AL_IsFinalCharge);
					AssertEquals("Shouldn't be changed.", ZBool.True, cc2Job1Line.AL_IsFinalCharge);
					AssertEquals("Shouldn't be changed.", ZBool.False, cc2Job2Line.AL_IsFinalCharge);
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestUpdateTaxAmountWithIndianStateTaxRate()
		{
			AssertUpdateTaxAmount(true, false);
		}

		public void TestUpdateTaxAmountWithUseLocalExTaxAmountToCalculateLocalTaxRegistry()
		{
			AssertUpdateTaxAmount(false, true);
		}

		public void TestUpdateTaxAmountWithoutUseLocalExTaxAmountToCalculateLocalTaxRegistry()
		{
			AssertUpdateTaxAmount(false, false);
		}

		public void AssertUpdateTaxAmount(bool isIndiaStateTax, bool isUseLocalExTaxAmountToCalculateLocalTaxRegistry)
		{
			if (isUseLocalExTaxAmountToCalculateLocalTaxRegistry)
			{
				AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			}

			var countryCode = isIndiaStateTax ? Constants.CountryCodes.India : Constants.CountryCodes.Australia;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var factory = new BusinessObjectFactory();
				var creator = new TestObjectCreator(factory);

				var consol = factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "C001";
				var shipment1 = consol.Shipments.AddNew();
				creator.CreateJob(shipment1);

				factory.Save();

				var currency = isIndiaStateTax ? factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "INR")) : creator.AUD;
				var taxRatePK = isIndiaStateTax ? creator.STAGST.PK : creator.GST1.PK;
				var invoice = creator.CreateInvoice(typeof(APInvoice), "inv1", currency, 1.0m, creator.Creditor1);

				try
				{
					using (var invoiceForm = new InvoiceForm(invoice))
					{
						invoiceForm.Show();

						Assert("No APInvoiceApportionToConsol BusinessContext outside the APInvoiceConsolCostingForm", !factory.HasContext(BusinessContext.APInvoiceApportionToConsol));
						invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
						Assert("APInvoiceApportionToConsol BusinessContext is ON inside the APInvoiceConsolCostingForm", factory.HasContext(BusinessContext.APInvoiceApportionToConsol));
						var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

						var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
						cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
						cost.E6_GC = GlbCompany.CurrentCompany.PK;
						cost.E6_AC_ChargeCode = creator.FRT.PK;
						cost.E6_AT_TaxRate = taxRatePK;
						cost.E6_RX_NKCurrency = currency.RX_Code;
						cost.E6_OSCostAmount = 665.37M;
						cost.E6_CostGovtChargeCode = "HELLO";
						cost.E6_SellGovtChargeCode = "HELLO";
						cost.E6_AH_APInvoice = invoice.PK;
						cost.ApportionmentCharges[0].JR_GE = creator.FESDepartment.PK;
						cost.ApportionmentCharges[0].JR_OSCostAmt = 665.37M;

						if (isIndiaStateTax)
						{
							AssertEquals("local tax amount should be updated with the new tax amount value", 119.76M, cost.E6_Calc_LocalGSTAmount);
							AssertEquals("665.37 + 119.76", 785.13M, cost.GSTInclusiveAmount);
							AssertEquals("apportioned charges should be also updated with the new tax amount value", 119.76M, cost.ApportionmentCharges[0].JR_Cost_LocalGSTAmount);
							AssertEquals("apportioned charges should be also updated with the new tax amount value", 785.13M, cost.ApportionmentCharges[0].JR_Calc_LocalCostAmtWithGST);
						}
						else
						{
							AssertEquals("local tax amount should be updated with the new tax amount value", 66.54M, cost.E6_Calc_LocalGSTAmount);
							AssertEquals("665.37 + 66.54", 731.91M, cost.GSTInclusiveAmount);
							AssertEquals("apportioned charges should be also updated with the new tax amount value", 66.54M, cost.ApportionmentCharges[0].JR_Cost_LocalGSTAmount);
							AssertEquals("apportioned charges should be also updated with the new tax amount value", 731.91M, cost.ApportionmentCharges[0].JR_Calc_LocalCostAmtWithGST);
						}

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						consolCostingForm.Close();
						Application.DoEvents();

						var invoiceLine = invoice.Lines[0];
						AssertLineAmounts(invoiceLine);

						Assert("APInvoiceApportionToConsol BusinessContext is removed when APInvoiceConsolCostingForm is closed", !factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice));

						invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
						var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
						menuItem.PerformClick();
						Assert("APInvoiceApportionToConsol BusinessContext is ON inside the APInvoiceConsolCostingSingleEditForm", factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice));
						using (var singleConsolCostingEditForm = (APInvoiceConsolCostingSingleEditForm)ZFormModaliser.ActiveForm)
						{
							var jobConsolCost = (JobConsolCost)singleConsolCostingEditForm.BusinessEntity;
							var charge = (ApportionSplitCharge)singleConsolCostingEditForm.ApportionedChargesGrid_ForTestOnly.ListManager.List[0];

							var originalOsTaxAmount = jobConsolCost.E6_OSGSTAmount_Calc;
							jobConsolCost.E6_OSGSTAmount_Calc = 120M;

							Assert("E6_OSGSTAmount is modified in APInvoiceConsolCostingSingleEditForm hence will get OSTaxAmountModifiedFromCalculatedAmountForConsolCost context",
								factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));

							jobConsolCost.E6_OSGSTAmount_Calc = originalOsTaxAmount;

							Assert("OSTaxAmountModifiedFromCalculatedAmountForConsolCost context should not be there anymore as the original amount is restored",
								!factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));

							jobConsolCost.E6_OSGSTAmount_Calc = 120M;

							Assert("E6_OSGSTAmount is modified in APInvoiceConsolCostingSingleEditForm hence will get OSTaxAmountModifiedFromCalculatedAmountForConsolCost context",
								factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));

							if (isIndiaStateTax)
							{
								Assert("tax rate is India State", cost.TaxRate.IsIndiaStateTax);
							}

							AssertEquals("local tax amount should be updated with the new tax amount value", 120M, jobConsolCost.E6_Calc_LocalGSTAmount);
							AssertEquals("665.37 + 120", 785.37M, jobConsolCost.GSTInclusiveAmount);
							AssertEquals("apportioned charges should be also updated with the new tax amount value", 120M, charge.JR_Cost_LocalGSTAmount);
							AssertEquals("apportioned charges should be also updated with the new tax amount value", 785.37M, charge.JR_Calc_LocalCostAmtWithGST);

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
							singleConsolCostingEditForm.Close();
							Application.DoEvents();
						}

						invoiceLine = invoice.Lines[0];
						AssertEquals("local tax amount should be updated with the new OS tax amount value", 120M, invoiceLine.AL_LocalTaxAmount);
						if (isIndiaStateTax)
						{
							AssertEquals("local GST amount should be updated with the new OS tax amount value", 60M, invoiceLine.AL_LocalGSTAmount);
							AssertEquals("local Extra tax amount should be updated with the new OS tax amount value", 60M, invoiceLine.AL_LocalExtraTaxAmount);

							AssertEquals("OS GST amount should be updated with the new OS tax amount value", 60M, invoiceLine.AL_OSGSTAmount);
							AssertEquals("OS Extra tax amount should be updated with the new OS tax amount value", 60M, invoiceLine.AL_OSExtraTaxAmount);
						}

						AssertEquals("665.37 + 120", 785.37M, invoiceLine.AL_OverseasTotal);

						//Add new line with GST tax to show that the Factory context is not impacting the calculation where it should not impact
						var line2 = creator.CreateInvoiceLine(invoice, 665.37M, currency, 1M);
						line2.AL_AT = taxRatePK;

						AssertLineAmounts(line2);

						Assert("ModifyingConsolCostAmountFromAPInvoice BusinessContext is removed when APInvoiceConsolCostingSingleEditForm is closed", !factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice));
						Assert("OSTaxAmountModifiedFromCalculatedAmountForConsolCost BusinessContext is removed when APInvoiceConsolCostingSingleEditForm is closed", !factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost));
					}
				}
				finally
				{
					invoice.ClearApportionmentJobMutexes();
				}
			}

			void AssertLineAmounts(InvoicingLineBase line)
			{
				if (isIndiaStateTax)
				{
					AssertEquals("665.37 + 119.76", 785.13M, line.AL_OverseasTotal);
					AssertEquals("665.37 + 119.76", 785.13M, line.AL_LocalTotalAmount);
					AssertEquals("OS main tax amount", 59.88M, line.AL_OSGSTAmount);
					AssertEquals("local main tax amount", 59.88M, line.AL_LocalGSTAmount);
					AssertEquals("OS extra tax amount", 59.88M, line.AL_OSExtraTaxAmount);
					AssertEquals("local main tax amount", 59.88M, line.AL_LocalExtraTaxAmount);
				}
				else
				{
					AssertEquals("665.37 + 66.54", 731.91M, line.AL_OverseasTotal);
					AssertEquals("665.37 + 66.54", 731.91M, line.AL_LocalTotalAmount);
					AssertEquals("tax amount should be updated with the new value", 66.54M, line.AL_OSGSTAmount);
					AssertEquals("local tax amount should be updated with the new tax amount value", 66.54M, line.AL_LocalGSTAmount);
				}
			}
		}

		public void TestDoublePopupNotTriggerDeveloperException()
		{
			ExceptionReporterTestListener.Instance.Clear();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			TestObjectCreator.CreateJob(shipment1);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "inv1", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Creditor1);

			using (var invoiceForm = new InvoiceForm(invoice))
			{
				invoiceForm.Show();

				Assert("No APInvoiceApportionToConsol BusinessContext outside the APInvoiceConsolCostingForm", !Factory.HasContext(BusinessContext.APInvoiceApportionToConsol));
				invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				Assert("APInvoiceApportionToConsol BusinessContext is ON inside the APInvoiceConsolCostingForm", Factory.HasContext(BusinessContext.APInvoiceApportionToConsol));
				var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

				var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
				cost.E6_GC = GlbCompany.CurrentCompany.PK;
				cost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
				cost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				cost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
				cost.E6_OSCostAmount = 665.37M;
				cost.E6_CostGovtChargeCode = "HELLO";
				cost.E6_SellGovtChargeCode = "HELLO";
				cost.E6_AH_APInvoice = invoice.PK;
				cost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FESDepartment.PK;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 665.37M;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consolCostingForm.Close();
				Application.DoEvents();

				invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
				var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
				menuItem.PerformClick();

				AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingSingleEditForm), ZFormModaliser.ActiveForm.GetType());
				var singleConsolCostingEditForm = (APInvoiceConsolCostingSingleEditForm)ZFormModaliser.ActiveForm;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				invoice.Lines[0].AL_LocalExTaxAmount = 500m;
				var singleConsolCostingEditForm2 = (APInvoiceConsolCostingSingleEditForm)ZFormModaliser.ActiveForm;
				AssertNotEquals("A new single edit form is opened", singleConsolCostingEditForm, singleConsolCostingEditForm2);

				AssertNullOrEmpty("No Developer exception 'ModifyingConsolCostDetailsFromAPInvoice context is set unexpectedly' raised.", ErrorReporter.LastMessageReported);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestCanNotSaveInvoiceWhenAPInvoiceConsolCostingSingleEditFormNotClosedByUsingButtionClick()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.ActiveOrAllAddresses[0].PK;
			Factory.Save();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, 100m, TestObjectCreator.Creditor1);
			consolCost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FESDepartment.PK;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "inv1", TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);

			using (var invoiceForm = new InvoiceForm(invoice))
			{
				invoiceForm.Show();
				invoiceForm.ControllerID = ControllerIDs.APInvoice;
				invoice.SubmittedFromInvoicingForm = true;

				invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

				var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				InvoicingBaseConsolCostImporter importer = new InvoicingBaseConsolCostImporter(new BusinessObjectFactory(), originatingCost, invoice);
				importer.ImportCostsIntoCosting(new BusinessObject[1] { consolCost });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				consolCostingForm.Close();
				Application.DoEvents();

				Assert("Pre-condition: Inovie didn't save to database.", !invoice.IsInDatabase);
				Assert("Factory should not context IllegalSaveOperations before APInvoiceConsolCostingSingleEditForm show.", !Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice));

				invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
				var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
				menuItem.PerformClick();

				var apInvoiceConsolCostingSingleEditForm = ZFormModaliser.ActiveForm;
				AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingSingleEditForm), apInvoiceConsolCostingSingleEditForm.GetType());
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				Assert("Factory should contain context IllegalSaveOperations after APInvoiceConsolCostingSingleEditForm show.", Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice));

				var result = invoiceForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Should return 'ContinueWithSave.No' for invoiceForm.ValidateAndSave when APInvoiceConsolCostingSingleEditForm didn't close", ContinueWithSave.No, result);

				apInvoiceConsolCostingSingleEditForm.Close();

				result = invoiceForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Should return 'ContinueWithSave.Yes' for invoiceForm.ValidateAndSave when APInvoiceConsolCostingSingleEditForm closed", ContinueWithSave.Yes, result);
				Assert("Invoice should saved in databse.", invoice.IsInDatabase);
			}
		}

		public void TestCanNotSaveInvoiceWhenAPInvoiceConsolCostingSingleEditFormNotClosedByUsingHotKey()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.ActiveOrAllAddresses[0].PK;
			Factory.Save();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, 100m, TestObjectCreator.Creditor1);
			consolCost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FESDepartment.PK;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "inv1", TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);

			using (var invoiceForm = new InvoiceForm(invoice))
			{
				invoiceForm.Show();
				invoiceForm.ControllerID = ControllerIDs.APInvoice;
				invoice.SubmittedFromInvoicingForm = true;

				invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

				var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				InvoicingBaseConsolCostImporter importer = new InvoicingBaseConsolCostImporter(new BusinessObjectFactory(), originatingCost, invoice);
				importer.ImportCostsIntoCosting(new BusinessObject[1] { consolCost });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				consolCostingForm.Close();
				Application.DoEvents();

				Assert("Pre-condition: Inovie didn't save to database.", !invoice.IsInDatabase);
				Assert("Factory should not context IllegalSaveOperations before APInvoiceConsolCostingSingleEditForm show.", !Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice));

				invoiceForm.InvoiceDetails.TransactionLinesGrid.SelectAllElements();
				invoiceForm.InvoiceDetails.TransactionLinesGrid.CurrentCell = new DataGridCell(0, 10);
				invoiceForm.InvoiceDetails.TransactionLinesGrid.BeginEdit(invoiceForm.InvoiceDetails.TransactionLinesGrid.Columns[10].ColumnStyle, 0);
				invoiceForm.InvoiceDetails.TransactionLinesGrid.LastFocusedColumn.EditControl.Text = "18-Jan-21";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				KeySender.SendKeyDownToProcessCmdKey(invoiceForm, Keys.S | Keys.Control);
				Application.DoEvents();

				Assert("Invoice should not saved in databse bafore APInvoiceConsolCostingSingleEditForm close.", !invoice.IsInDatabase);

				var apInvoiceConsolCostingSingleEditForm = ZFormModaliser.ActiveForm;
				AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingSingleEditForm), apInvoiceConsolCostingSingleEditForm.GetType());
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				Assert("Factory should contain context IllegalSaveOperations after APInvoiceConsolCostingSingleEditForm show.", Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice));

				apInvoiceConsolCostingSingleEditForm.Close();

				var result = invoiceForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Should return 'ContinueWithSave.Yes' for invoiceForm.ValidateAndSave when APInvoiceConsolCostingSingleEditForm closed", ContinueWithSave.Yes, result);
				Assert("Invoice should saved in databse.", invoice.IsInDatabase);
			}
		}

		public void TestSetting_OSTaxAmountModifiedFromCalculatedAmountForConsolCost_ContextIncorrectlyTriggersDeveloperException()
		{
			SetupAndAssertDeveloperExceptionWhenIncorrectContextIsSet(() => Factory.SetContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost), "OSTaxAmountModifiedFromCalculatedAmountForConsolCost context is set before ModifyingConsolCostDetailsFromAPInvoice context is set.");
		}

		void SetupAndAssertDeveloperExceptionWhenIncorrectContextIsSet(Action setIncorrectContext, string expectedMessage)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			TestObjectCreator.CreateJob(shipment1);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "inv1", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Creditor1);

			setIncorrectContext();

			try
			{
				using (var invoiceForm = new InvoiceForm(invoice))
				{
					invoiceForm.Show();

					Assert("No APInvoiceApportionToConsol BusinessContext outside the APInvoiceConsolCostingForm", !Factory.HasContext(BusinessContext.APInvoiceApportionToConsol));
					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					Assert("APInvoiceApportionToConsol BusinessContext is ON inside the APInvoiceConsolCostingForm", Factory.HasContext(BusinessContext.APInvoiceApportionToConsol));
					var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

					var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
					cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
					cost.E6_GC = GlbCompany.CurrentCompany.PK;
					cost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
					cost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
					cost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
					cost.E6_OSCostAmount = 665.37M;
					cost.E6_CostGovtChargeCode = "HELLO";
					cost.E6_SellGovtChargeCode = "HELLO";
					cost.E6_AH_APInvoice = invoice.PK;
					cost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FESDepartment.PK;
					cost.ApportionmentCharges[0].JR_OSCostAmt = 665.37M;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					consolCostingForm.Close();
					Application.DoEvents();

					invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
					var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
					menuItem.PerformClick();

					AssertEquals("Developer exception must be raised.", expectedMessage, ErrorReporter.LastMessageReported);
					ExceptionReporterTestListener.Instance.Clear();
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestGovernmentChargeCodeChange()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			creator.CC12.AC_GovtChargeCode = "HELLO";

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			creator.CreateJob(shipment1);
			var shipment2 = consol.Shipments.AddNew();
			creator.CreateJob(shipment2);

			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creator.AALSHI.PK;

			try
			{
				using (var invoiceForm = new InvoiceForm(invoice))
				{
					invoiceForm.Show();

					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
					cost.E6_ParentID = consol.PK;
					cost.E6_ParentTableCode = "JK";
					cost.E6_AC_ChargeCode = creator.CC12.PK;
					cost.E6_OSCostAmount = 70M;
					cost.E6_CostGovtChargeCode = "HELLO";
					cost.ApportionmentCharges[0].JR_OSCostAmt = 30M;
					cost.ApportionmentCharges[1].JR_OSCostAmt = 40M;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					consolCostingForm.Close();
					Application.DoEvents();

					invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
					var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
					menuItem.PerformClick();
					var singleConsolCostingEditForm = (APInvoiceConsolCostingSingleEditForm)ZFormModaliser.ActiveForm;
					var jobConsolCost = (JobConsolCost)singleConsolCostingEditForm.BusinessEntity;
					var charge1 = (ApportionSplitCharge)singleConsolCostingEditForm.ApportionedChargesGrid_ForTestOnly.ListManager.List[0];
					var charge2 = (ApportionSplitCharge)singleConsolCostingEditForm.ApportionedChargesGrid_ForTestOnly.ListManager.List[1];

					singleConsolCostingEditForm.GovtChargeCodeTextBox_ForTestOnly.Text = "WORLD";
					singleConsolCostingEditForm.GovtChargeCodeTextBox_ForTestOnly.DataBindings["Text"].WriteValue();

					AssertEquals("WORLD", jobConsolCost.E6_CostGovtChargeCode);
					AssertEquals(jobConsolCost.E6_CostGovtChargeCode, charge1.JR_CostGovtChargeCode);
					AssertEquals(jobConsolCost.E6_CostGovtChargeCode, charge2.JR_CostGovtChargeCode);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					singleConsolCostingEditForm.Close();
					Application.DoEvents();

					AssertEquals(jobConsolCost.E6_CostGovtChargeCode, invoice.Lines[0].AL_GovtChargeCode);
					AssertEquals(jobConsolCost.E6_CostGovtChargeCode, invoice.Lines[1].AL_GovtChargeCode);
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestAvailableForCostGovtChargeCodeTextBoxAndColumn()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = consol.Shipments.AddNew();
			TestObjectCreator.CreateJob(shipment1);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "inv1", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Creditor1);
			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			cost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			cost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			cost.E6_OSCostAmount = 665.37M;
			cost.E6_CostGovtChargeCode = "HELLO";
			cost.E6_SellGovtChargeCode = "HELLO";
			cost.E6_AH_APInvoice = invoice.PK;
			cost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FESDepartment.PK;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 665.37M;

			using (var form = new APInvoiceConsolCostingSingleEditForm(cost))
			{
				form.Show();

				Assert("JR_CostGovtChargeCode column should be availble.", !form.ApportionedChargesGrid_ForTestOnly.Columns["JR_CostGovtChargeCode"].IsUnavailable);
				Assert("JR_CostGovtChargeCode column should be visible", form.ApportionedChargesGrid_ForTestOnly.Columns["JR_CostGovtChargeCode"].IsVisible);

				var govtChargeCodeTextBox = form.CostDetails_ForTestOnly.Controls.OfType<Control>().FirstOrDefault(x => x.Name == "govtChargeCodeTextBox");

				Assert("govtChargeCodeTextBox should be enabled.", govtChargeCodeTextBox.Enabled);
				Assert("govtChargeCodeTextBox should be visible.", govtChargeCodeTextBox.Visible);

				form.Close();
			}

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			using (var form = new APInvoiceConsolCostingSingleEditForm(cost))
			{
				form.Show();

				AssertNull("Can not find JR_CostGovtChargeCode column in apportionedChargesGrid.", form.ApportionedChargesGrid_ForTestOnly.Columns.ToList().Find(x => x.ColumnName == "JR_CostGovtChargeCode"));
				AssertNull("Can not find govtChargeCodeTextBox in cost details panel.",form.CostDetails_ForTestOnly.Controls.OfType<Control>().FirstOrDefault(x => x.Name == "govtChargeCodeTextBox"));

				form.Close();
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
