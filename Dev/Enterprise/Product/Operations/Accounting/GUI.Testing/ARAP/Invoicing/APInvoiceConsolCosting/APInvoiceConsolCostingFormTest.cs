using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceConsolCostingForm))]
	public class APInvoiceConsolCostingFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			APInvoice invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			return new APInvoiceConsolCostingForm(invoice.ConsolCosting);
		}

		protected override bool ExpectedExecuteAllFetchHintsBeforeValidateAll
		{
			get { return false; }
		}

		public void TestConsolCostingScreenButtonVisibility()
		{
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				Assert(form.BulkConsolCostImportButton_ForTestOnly.Visible);
				Assert(form.ApportionButton_ForTestOnly.Visible);
				Assert(!form.CloseButton_ForTestOnly.Visible);
				AssertEquals(APInvoiceConsolCostingFormResult.Cancel, form.FormResult);

				form.Close();
				AssertEquals("Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Show();
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.OnShown_ForTestOnly(EventArgs.Empty);

				Assert(!form.BulkConsolCostImportButton_ForTestOnly.Visible);
				Assert(!form.ApportionButton_ForTestOnly.Visible);
				Assert(form.CloseButton_ForTestOnly.Visible);
				AssertEquals(APInvoiceConsolCostingFormResult.Close, form.FormResult);

				form.Close();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnConsolChangedEventHookedAndUnHookedCorrectly()
		{
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				APInvoiceConsolCosting consolCosting = form.ApportionmentList_ForTestOnly;
				Assert("Should be hooked", consolCosting.ConsolCosts.IsConsolCostChangedHooked);
				form.OnClosing_ForTestOnly(null);
				Assert("Should not be unhooked", consolCosting.ConsolCosts.IsConsolCostChangedHooked);
				form.OnClosed_ForTestOnly(null);
				Assert("Should be unhooked", !consolCosting.ConsolCosts.IsConsolCostChangedHooked);
			}
		}

		public void TestTaxAmountColumnName()
		{
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				ZCalcEditColumnStyle taxAmountColumn = (ZCalcEditColumnStyle)form.ApportionmentChargesGrid_ForTestOnly.Columns["JR_OSCostGSTAmt_Calc"].ColumnStyle;
				AssertEquals("Tax Amount", taxAmountColumn.CaptionResourceString.Caption);
			}
		}

		public void TestExtraTaxAmountColumnIsReadonly()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);

			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Precondition:", false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSExtraTaxAmount").IsUnavailable);

				ZCalcEditColumnStyle extraTaxAmountColumn = (ZCalcEditColumnStyle)form.ApportionmentsGrid_ForTestOnly.Columns["E6_OSExtraTaxAmount"].ColumnStyle;
				Assert("Tax Amount", extraTaxAmountColumn.ReadOnly);
			}
		}

		public void TestColumnsModified()
		{
			string[] apportionmentsGridColumns = new string[] { "E6_AC_ChargeCode", "E6_ParentID", "E6_OSCostAmount", "E6_AT_TaxRate", "E6_A9_VATClass", "E6_OSGSTAmount_Calc", "GSTInclusiveAmount", "E6_PPDCLT", "UnApportionedAmount", "E6_ApportionmentMethod", "IsFinal", "E6_LocalCostAmount", "E6_Calc_LocalGSTAmount", "E6_Calc_LocalTotalAmount", "E6_ApportionToRelatedShipments", "CostExchangeRate+Currency", "CostExchangeRate+Rate", "E6_MasterBillNumber", "E6_ConsolCostAccrual", "E6_ConsolTotalAccrual", "E6_TaxDate" };
			string[] apportionmentChargesGridColumns = new string[] { "JR_IsUsedForApportionment", "JR_JobNumber", "JR_HouseBill", "JR_PrepaidCollect", "JR_GB", "JR_GE", "JR_Chargeable", "JR_OSCostAmt", "JR_OSCostGSTAmt_Calc", "JR_Calc_OSCostAmtWithGST", "JR_ShipmentNumberOfColoadMaster", "JR_ActualWeight", "JR_ActualWeightUnit", "IsFinal", "JR_JH_InternalJob", "JR_GB_InternalBranch", "JR_GE_InternalDept", "JR_LocalCostAmt", "JR_Calc_LocalCostAmtWithGST", "JR_Cost_LocalGSTAmount", "JR_RL_NKOrigin", "JR_RL_NKDestination" };

			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				int i = 0;
				string[] formApportionmentsGridColumns = new string[form.ApportionmentsGrid_ForTestOnly.Columns.Count];

				foreach (ZGridColumn column in form.ApportionmentsGrid_ForTestOnly.Columns)
				{
					formApportionmentsGridColumns[i] = column.ColumnName;
					i++;
				}
				AssertContainsExactElementsInAnyOrder("[ApportionmentsGrid] Columns have been changed. Consider adding / removing changed columns for incomplete invoice serialization.", apportionmentsGridColumns, formApportionmentsGridColumns);
			}

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				int i = 0;
				string[] formApportionmentChargesGridColumns = new string[form.ApportionmentChargesGrid_ForTestOnly.Columns.Count];

				foreach (ZGridColumn column in form.ApportionmentChargesGrid_ForTestOnly.Columns)
				{
					formApportionmentChargesGridColumns[i] = column.ColumnName;
					i++;
				}
				AssertContainsExactElementsInAnyOrder("[ApportionmentChargesGrid] Columns have been changed. Consider adding / removing changed columns for incomplete invoice serialization.", apportionmentChargesGridColumns, formApportionmentChargesGridColumns);
			}

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			apportionmentChargesGridColumns = new string[] { "JR_IsUsedForApportionment", "JR_JobNumber", "JR_HouseBill", "JR_PrepaidCollect", "JR_GB", "JR_GE", "JR_Chargeable", "JR_OSCostAmt", "JR_OSCostGSTAmt_Calc", "JR_Calc_OSCostAmtWithGST", "JR_ShipmentNumberOfColoadMaster", "JR_ActualWeight", "JR_ActualWeightUnit", "IsFinal", "JR_JH_InternalJob", "JR_GB_InternalBranch", "JR_GE_InternalDept", "JR_LocalCostAmt", "JR_Calc_LocalCostAmtWithGST", "JR_Cost_LocalGSTAmount", "JR_RL_NKOrigin", "JR_RL_NKDestination" };

			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				int i = 0;
				string[] formApportionmentChargesGridColumns = new string[form.ApportionmentChargesGrid_ForTestOnly.Columns.Count];

				foreach (ZGridColumn column in form.ApportionmentChargesGrid_ForTestOnly.Columns)
				{
					formApportionmentChargesGridColumns[i] = column.ColumnName;
					i++;
				}
				AssertContainsExactElementsInAnyOrder("[ApportionmentChargesGrid] Columns have been changed. Consider adding / removing changed columns for incomplete invoice serialization.", apportionmentChargesGridColumns, formApportionmentChargesGridColumns);
			}
		}

		public void TestGovtChargeCodeColumnsAreInAvailableColumnList()
		{
			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var job = TestObjectCreator.Job1;
					using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();

						AssertEquals(!enableGovtChargeCode, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_CostGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_SellGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_CostGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_SellGovtChargeCode).IsUnavailable);
					}
				}
			}
		}

		public void TestHideColumnsForNonGstRegisteredCompany()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("GSTInclusiveAmount").IsUnavailable);
				AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_Calc_LocalGSTAmount").IsUnavailable);
				AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_Calc_LocalTotalAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTRealAmount").IsUnavailable);
				AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTAmount_Calc").IsUnavailable);
				AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_AT_TaxRate").IsUnavailable);
				AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_TaxDate").IsUnavailable);
				AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_A9_VATClass").IsUnavailable);

				AssertEquals(false, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle("JR_OSCostGSTAmt_Calc").IsUnavailable);
				AssertEquals(false, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle("JR_Calc_OSCostAmtWithGST").IsUnavailable);

				AssertEquals(false, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("InvoiceCurrencyTotalAmountWithTax").IsUnavailable);
				AssertEquals(false, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("InvoiceCurrencyTotalTaxAmount").IsUnavailable);
				AssertEquals(false, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("LocalTotalAmountWithTax").IsUnavailable);
				AssertEquals(false, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("LocalTotalTaxAmount").IsUnavailable);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("GSTInclusiveAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_Calc_LocalGSTAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_Calc_LocalTotalAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTRealAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTAmount_Calc").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_AT_TaxRate").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_TaxDate").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_A9_VATClass").IsUnavailable);

				AssertEquals(true, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle("JR_OSCostGSTAmt_Calc").IsUnavailable);
				AssertEquals(true, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle("JR_Calc_OSCostAmtWithGST").IsUnavailable);

				AssertEquals(true, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("InvoiceCurrencyTotalAmountWithTax").IsUnavailable);
				AssertEquals(true, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("InvoiceCurrencyTotalTaxAmount").IsUnavailable);
				AssertEquals(true, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("LocalTotalAmountWithTax").IsUnavailable);
				AssertEquals(true, form.ConsolSummaryGrid_ForTestOnly.GetColumnStyle("LocalTotalTaxAmount").IsUnavailable);
			}
		}

		public void TestControlsHidingForCanada()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				AssertEquals("Company should be in Canada", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
				form.Show();
				Application.DoEvents();
				AssertColumnStyle(form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount), false, null, "QST Amount", "QST Amt");
				AssertColumnStyle(form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount), false, null, "GST Amount", "GST Amt");
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				AssertEquals("Company should be in Canada", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSExtraTaxAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTRealAmount").IsUnavailable);
			}
		}

		public void TestControlsHidingForIndia()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				AssertNotEquals("Not India company", Core.Constants.CountryCodes.India, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSExtraTaxAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTRealAmount").IsUnavailable);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);
			try
			{
				using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
				{
					form.Show();
					Application.DoEvents();
					AssertColumnStyle(form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount), false, null, "SGST Amount", "SGST Amt");
					AssertColumnStyle(form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount), false, null, "CGST/IGST Amount", "GST Amount");
				}

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
				{
					form.Show();
					Application.DoEvents();
					AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSExtraTaxAmount").IsUnavailable);
					AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTRealAmount").IsUnavailable);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestPlaceOfSupplyDropEditVisibility()
		{
			foreach (var regValue in new[] { true, false })
			{
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();

						AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_PlaceOfSupply").IsUnavailable);
						AssertEquals(false, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle("JR_CostPlaceOfSupply").IsUnavailable);
					}
				}
			}

			foreach (var regValue in new[] { true, false })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					using (var form = (APInvoiceConsolCostingForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();

						AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_PlaceOfSupply").IsUnavailable);
						AssertEquals(true, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle("JR_CostPlaceOfSupply").IsUnavailable);
					}
				}
			}
		}

		public void TestSupplyTypeColumnVisibility()
		{
			AssertSupplyTypeColumnVisibility(true);
			AssertSupplyTypeColumnVisibility(false);

			void AssertSupplyTypeColumnVisibility(bool isAvailable)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isAvailable))
				{
					using (var form = (APInvoiceConsolCostingForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();

						AssertEquals(!isAvailable, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCostSchema.Constants.E6_SupplyType).IsUnavailable);
						if (isAvailable)
						{
							AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCostSchema.Constants.E6_SupplyType).IsVisible);
							AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCostSchema.Constants.E6_SupplyType).IsReadOnly);
						}
					}
				}
			}
		}

		public void TestTaxBranchColumnsVisibility()
		{
			AssertTaxBranchColumnsVisibility(true, true);
			AssertTaxBranchColumnsVisibility(true, false);
			AssertTaxBranchColumnsVisibility(false, true);
			AssertTaxBranchColumnsVisibility(false, false);

			void AssertTaxBranchColumnsVisibility(bool isEnaleRegistry, bool isGSTRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isEnaleRegistry))
				using (var form = (APInvoiceConsolCostingForm)GetFormToBashCore())
				{
					form.Show();
					Application.DoEvents();

					var expectedVisible = isEnaleRegistry && isGSTRegistered;

					AssertEquals(!expectedVisible, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCostSchema.Constants.E6_GB_CostTaxBranch).IsUnavailable);
					AssertEquals(!expectedVisible, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.CostTaxBranchName).IsUnavailable);
					AssertEquals(!expectedVisible, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_GB_CostTaxBranch).IsUnavailable);
					if (expectedVisible)
					{
						AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCostSchema.Constants.E6_GB_CostTaxBranch).IsVisible);
						AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCostSchema.Constants.E6_GB_CostTaxBranch).IsReadOnly);
						AssertEquals(false, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.CostTaxBranchName).IsVisible);
						AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.CostTaxBranchName).IsReadOnly);

						AssertEquals(true, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_GB_CostTaxBranch).IsVisible);
						AssertEquals(true, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_GB_CostTaxBranch).IsReadOnly);
					}
				}
			}
		}

		public void TestCostSupplyTypeColumnVisibility()
		{
			AssertCostSupplyTypeColumnVisibility(true);
			AssertCostSupplyTypeColumnVisibility(false);

			void AssertCostSupplyTypeColumnVisibility(bool isAvailable)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isAvailable))
				{
					using (var form = (APInvoiceConsolCostingForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();

						AssertEquals(!isAvailable, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_CostSupplyType).IsUnavailable);
						if (isAvailable)
						{
							AssertEquals(true, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_CostSupplyType).IsVisible);
							AssertEquals(true, form.ApportionmentChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_CostSupplyType).IsReadOnly);
						}
					}
				}
			}
		}

		public void TestControlsHidingForMexico()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				AssertNotEquals("Not Mexico company", Core.Constants.CountryCodes.Mexico, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSExtraTaxAmount").IsUnavailable);
				AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTRealAmount").IsUnavailable);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico);
			try
			{
				using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
				{
					form.Show();
					Application.DoEvents();
					AssertColumnStyle(form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount), false, null, "RET Amount", "RET Amt");
					AssertColumnStyle(form.ApportionmentsGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount), false, null, "IVA Amount", "IVA Amt");
				}

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
				{
					form.Show();
					Application.DoEvents();
					AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSExtraTaxAmount").IsUnavailable);
					AssertEquals(true, form.ApportionmentsGrid_ForTestOnly.GetColumnStyle("E6_OSGSTRealAmount").IsUnavailable);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestBulkConsolCostImportButtonVisibleOnAPCreditNote()
		{
			APCreditNote note = Factory.New<APCreditNote>();
			using (APInvoiceConsolCostingForm form = new APInvoiceConsolCostingForm(note.ConsolCosting))
			{
				form.Show();
				Assert(form.BulkConsolCostImportButton_ForTestOnly.Visible);
			}
		}

		public void TestShowMessageBoxOnMutexError()
		{
			var mutexFactory = new BusinessObjectFactory();
			var consol = mutexFactory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S01399727";

			mutexFactory.Save();

			string expectedMessage =
@"You have created the job S01399727 on another form, but haven't saved it yet.
Please close or save other forms that use job S01399727 to continue.";

			var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex();
			var inv = Factory.New<APInvoice>();
			inv.AH_OH = TestObjectCreator.AALSHI.PK;
			using (InvoiceForm invForm = new InvoiceForm(inv))
			{
				invForm.Show();
				invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				AssertEquals("should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
				var cost = inv.ConsolCosting.ConsolCosts.AddNew();
				cost.E6_ParentID = consol.PK;
				cost.E6_ParentTableCode = "JK";

				AssertEquals("Mutex error should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				((ZForm)ZFormModaliser.ActiveForm).Dispose();
			}

			var shipmentInCurrentFactory = Factory.Load<ForwardingShipment>(shipment.PK);
			using (new Job.Loader(shipmentInCurrentFactory).TryCreateWithMutex())
			{
				var invoiceInCurrentFactory = Factory.New<APInvoice>();
				invoiceInCurrentFactory.AH_OH = TestObjectCreator.AALSHI.PK;
				using (var invForm = new InvoiceForm(invoiceInCurrentFactory))
				{
					invForm.Show();
					invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var consolCostForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					consolCostForm.OnClosing_ForTestOnly(null);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var cost = invoiceInCurrentFactory.ConsolCosting.ConsolCosts.AddNew();
					cost.E6_ParentID = consol.PK;
					cost.E6_ParentTableCode = "JK";

					AssertEquals("Mutex error should be shown and mutex event must be unhooked on form closed, not closing as closing can be cenceled.",
						expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					((ZForm)ZFormModaliser.ActiveForm).Dispose();
				}
			}

			shipmentJob.Dispose();
		}

		public void TestOnlyApplyMessageBoxIsShownToUserWhileApplyingOrClosingApportionToConsolsForm()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Shipments.AddNew();

			factory.Save();

			APInvoice inv = null;

			try
			{
				foreach (ODisplayMode displayMode in Enum.GetValues(typeof(ODisplayMode)))
				{
					inv = factory.NewWithValidTestData<APInvoice>();
					inv.AH_OH = TestObjectCreator.AALSHI.PK;

					using (InvoiceForm invForm = new InvoiceForm(inv))
					{
						invForm.Show();
						invForm.DisplayMode = displayMode;

						invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
						AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
						var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
						var cost = inv.ConsolCosting.ConsolCosts.AddNew();
						cost.E6_ParentID = consol.PK;
						cost.E6_ParentTableCode = "JK";
						cost.E6_AC_ChargeCode = creator.CC12.PK;
						cost.E6_OSCostAmount = 100M;
						Assert(!cost.HasErrors);

						Assert("Messge list contains an empty message", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(null));

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						Assert("Messge list contains an empty message", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(null));

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						((ZForm)ZFormModaliser.ActiveForm).Close();
						AssertEquals("Should be still showing the apportionment form", false, apportionmentForm.IsDisposed);
						AssertEquals("Messages should include only one message", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
						Assert("User should be asked", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"Do you want to apply changes?"));
						Assert("Messge list contains an empty message", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(null));

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						Assert("Messge list contains an empty message", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(null));

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						apportionmentForm.Close();
						AssertEquals("Messages should include only one message", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
						Assert("User should be asked", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"Do you want to apply changes?"));
						Assert("Messge list contains an empty message", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(null));
						Application.DoEvents();

						AssertEquals("The apportionment form should be closed", true, apportionmentForm.IsDisposed);
						AssertEquals("Should be one consol cost imported", 1, inv.ConsolCosting.ConsolCosts.Count);
					}
				}
			}
			finally
			{
				inv.ClearApportionmentJobMutexes();
			}
		}

		public void TestShowMessageBoxOnFormCloseWithoutApply()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;

			factory.Save();

			var inv = factory.NewWithValidTestData<APInvoice>();
			inv.AH_OH = TestObjectCreator.AALSHI.PK;

			try
			{
				using (var invForm = new InvoiceForm(inv))
				{
					invForm.Show();
					invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost = inv.ConsolCosting.ConsolCosts.AddNew();
					cost.E6_ParentID = consol.PK;
					cost.E6_ParentTableCode = "JK";
					cost.E6_AC_ChargeCode = creator.CC12.PK;
					cost.E6_OSCostAmount = 100M;
					Assert(!cost.HasErrors);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					((ZForm)ZFormModaliser.ActiveForm).Close();
					AssertEquals("LastMessage.Text", "Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should be still showing the apportionment form", false, apportionmentForm.IsDisposed);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					apportionmentForm.Close();
					AssertEquals("LastMessage.Text", "Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					Application.DoEvents();

					AssertEquals("The apportionment form should be closed", true, apportionmentForm.IsDisposed);
					AssertEquals("Should be one consol cost imported", 1, inv.ConsolCosting.ConsolCosts.Count);
				}
			}
			finally
			{
				inv.ClearApportionmentJobMutexes();
			}
		}

		public void TestShowMessageBoxOnJobMutexError_APCreditNote()
		{
			var factoryInAnotherCWInstance = new BusinessObjectFactory() { RefreshEnabled = false };
			var creator = new TestObjectCreator(factoryInAnotherCWInstance);
			var consol = factoryInAnotherCWInstance.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S01399727";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = creator.AALSHI.PK;
			factoryInAnotherCWInstance.Save();

			using (var job = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var creditNote = Factory.New<APCreditNote>();
				creditNote.AH_OH = TestObjectCreator.AALSHI.PK;

				AssertNotEquals("Simulate creating job header in another form/factory.", creditNote.Factory._Instance, job.Factory._Instance);

				using (var creditNoteForm = new CreditNoteForm(creditNote))
				{
					creditNoteForm.Show();
					creditNoteForm.InvoiceDetails.ApportionChargesButton.PerformClick();

					AssertEquals("Should show the apportionment form.", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());

					var consolCost = creditNote.ConsolCosting.ConsolCosts.AddNew();
					consolCost.E6_ParentID = consol.PK;
					consolCost.E6_ParentTableCode = JobConsolSchema.Constants.Prefix;

					var expectedMessage = @"You have created the job S01399727 on another form, but haven't saved it yet.
Please close or save other forms that use job S01399727 to continue.";

					AssertEquals("Should show proper error message.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					((ZForm)ZFormModaliser.ActiveForm).Dispose();
				}
			}
		}

		public void TestIsDataVersionLogsMenuItemVisible()
		{
			using (APInvoiceConsolCostingForm form = (APInvoiceConsolCostingForm)GetFormToBashCore())
			{
				AssertEquals("DataVersionLogsMenuItem should be visible on ApportionmentChargesGrid, because here we use copy of original charge and can't see actual charge log so.",
					false, form.ApportionmentChargesGrid_ForTestOnly.IsDataVersionLogsMenuItemVisible);
			}
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirement_ConsolCosting()
		{
			var factory = new BusinessObjectFactory();
			GlbDepartment.GetCurrentDepartment(factory).GE_Misc = false;
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
					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost1 = invoice.ConsolCosting.ConsolCosts.AddNew();
					cost1.E6_ParentID = consol.PK;
					cost1.E6_ParentTableCode = "JK";
					cost1.E6_AC_ChargeCode = creator.CC1.PK;
					cost1.E6_OSCostAmount = 70M;
					Assert(!cost1.HasErrors);

					var cost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
					cost2.E6_ParentID = consol.PK;
					cost2.E6_ParentTableCode = "JK";
					cost2.E6_AC_ChargeCode = creator.CC2.PK;
					cost2.E6_OSCostAmount = 170M;
					Assert(!cost2.HasErrors);

					//Job & Charge Code
					TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode, false, invoice);

					cost1.ApportionmentCharges[0].JR_OSCostAmt = 30M;
					cost1.ApportionmentCharges[1].JR_OSCostAmt = 40M;
					cost2.ApportionmentCharges[0].JR_OSCostAmt = 50M;
					cost2.ApportionmentCharges[1].JR_OSCostAmt = 120M;

					AssertEquals("The cost variance requires None approval, 30 is NOT above 100.", ZBool.True, cost1.ApportionmentCharges[0].IsFinal);
					AssertEquals("The cost variance requires None approval, 40 is NOT above 100.", ZBool.True, cost1.ApportionmentCharges[1].IsFinal);

					AssertEquals("The cost variance requires None approval, 50 is NOT above 100.", ZBool.True, cost2.ApportionmentCharges[0].IsFinal);
					AssertEquals("The cost variance requires 1st level approval, 120 is above 100.", ZBool.False, cost2.ApportionmentCharges[1].IsFinal);

					var expectError = @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag";

					Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;
					cost2.ApportionmentCharges[0].IsFinal = ZBool.False;
					AssertHasError("Shouldn't untick the flag after calculation when you have no security right.", cost2.ApportionmentCharges[0].IsFinalInfo, expectError);
					cost2.ApportionmentCharges[0].IsFinal = ZBool.True;
					AssertNoErrors(cost2.ApportionmentCharges[0].IsFinalInfo);

					Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;
					cost2.ApportionmentCharges[0].IsFinal = ZBool.False;
					AssertNoErrors("We have the security right to untick the flag.", cost2.ApportionmentCharges[0].IsFinalInfo);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					apportionmentForm.Close();
					AssertEquals("LastMessage.Text", "Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					Application.DoEvents();

					AssertEquals("There should be 4 lines in the AP Invoice.", 4, invoice.Lines.Count);
					var cc1Job1Line = invoice.Lines[0];
					var cc1Job2Line = invoice.Lines[1];
					var cc2Job1Line = invoice.Lines[2];
					var cc2Job2Line = invoice.Lines[3];
					AssertEquals("Imported from the apportion split charge.", ZBool.True, cc1Job1Line.AL_IsFinalCharge);
					AssertEquals("Imported from the apportion split charge.", ZBool.True, cc1Job2Line.AL_IsFinalCharge);
					AssertEquals("The value has been changed manually.", ZBool.False, cc2Job1Line.AL_IsFinalCharge);
					AssertEquals("Imported from the apportion split charge.", ZBool.False, cc2Job2Line.AL_IsFinalCharge);

					//Job
					TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.Job, false, invoice);

					var cc3Job1Line = creator.CreateAPInvoiceLine(invoice, job1, creator.CC3, creator.AUD, 1m, "line1", 0m);

					cc3Job1Line.AL_LocalExTaxAmount = 30M;
					AssertEquals("The cost varianc requires 1st level approval, 30 + 50 + 30 = 110 is above 100.", ZBool.False, cc1Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires 1st level approval, 30 + 50 + 30 = 110 is above 100.", ZBool.False, cc2Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires 1st level approval, 30 + 50 + 30 = 110 is above 100.", ZBool.False, cc3Job1Line.AL_IsFinalCharge);
					AssertEquals("Shouldn't be affected, because they are not in the same job.", ZBool.True, cc1Job2Line.AL_IsFinalCharge);
					AssertEquals("Shouldn't be affected, because they are not in the same job.", ZBool.False, cc2Job2Line.AL_IsFinalCharge);

					cc3Job1Line.AL_LocalExTaxAmount = 10M;
					AssertEquals("The cost variance requires None approval, 30 + 50 + 10 = 90 is NOT above 100.", ZBool.True, cc1Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires None approval, 30 + 50 + 10 = 90 is NOT above 100.", ZBool.True, cc2Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires None approval, 30 + 50 + 10 = 90 is NOT above 100.", ZBool.True, cc3Job1Line.AL_IsFinalCharge);
					AssertEquals("Shouldn't be affected, because they are not in the same job.", ZBool.True, cc1Job2Line.AL_IsFinalCharge);
					AssertEquals("Shouldn't be affected, because they are not in the same job.", ZBool.False, cc2Job2Line.AL_IsFinalCharge);

					//Monitor Total Invoice Variance
					TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.Job, true, invoice);

					cc3Job1Line.AL_LocalExTaxAmount = 5M;
					AssertEquals("The cost variance requires 2nd level approval, 30 + 40 + 50 + 120 + 5 = 245 is above 200.", ZBool.False, cc1Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires 2nd level approval, 30 + 40 + 50 + 120 + 5 = 245 is above 200.", ZBool.False, cc2Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires 2nd level approval, 30 + 40 + 50 + 120 + 5 = 245 is above 200.", ZBool.False, cc3Job1Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires 2nd level approval, 30 + 40 + 50 + 120 + 5 = 245 is above 200.", ZBool.False, cc1Job2Line.AL_IsFinalCharge);
					AssertEquals("The cost variance requires 2nd level approval, 30 + 40 + 50 + 120 + 5 = 245 is above 200.", ZBool.False, cc2Job2Line.AL_IsFinalCharge);

					//Manually change the flag
					cc3Job1Line.AL_IsFinalCharge = ZBool.True;

					AssertEquals("The value changes because another charge related to the same job has been changed manually.", ZBool.True, cc1Job1Line.AL_IsFinalCharge);
					AssertEquals("The value changes because another charge related to the same job has been changed manually.", ZBool.True, cc2Job1Line.AL_IsFinalCharge);
					AssertEquals("The value has been changed manually.", ZBool.True, cc3Job1Line.AL_IsFinalCharge);
					AssertEquals("Imported from the apportion split charge.", ZBool.False, cc1Job2Line.AL_IsFinalCharge);
					AssertEquals("Imported from the apportion split charge.", ZBool.False, cc2Job2Line.AL_IsFinalCharge);

					//Reopen the Consol Costing Form
					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

					AssertEquals("Imported from the transaction line.", ZBool.True, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].IsFinal);
					AssertEquals("Imported from the transaction line.", ZBool.False, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[1].IsFinal);
					AssertEquals("Imported from the transaction line.", ZBool.True, invoice.ConsolCosting.ConsolCosts[1].ApportionmentCharges[0].IsFinal);
					AssertEquals("Imported from the transaction line.", ZBool.False, invoice.ConsolCosting.ConsolCosts[1].ApportionmentCharges[1].IsFinal);

					invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[1].IsFinal = ZBool.True;

					AssertEquals(ZBool.True, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].IsFinal);
					AssertEquals("The value has been changed manually.", ZBool.True, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[1].IsFinal);
					AssertEquals(ZBool.True, invoice.ConsolCosting.ConsolCosts[1].ApportionmentCharges[0].IsFinal);
					AssertEquals("The value changes because another charge related to the same job has been changed manually.", ZBool.True, invoice.ConsolCosting.ConsolCosts[1].ApportionmentCharges[1].IsFinal);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					apportionmentForm.Close();
					Application.DoEvents();

					foreach (APInvoiceLine line in invoice.Lines)
					{
						AssertEquals(ZBool.True, line.AL_IsFinalCharge);
					}
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestAutoTickFinalFlag_ApportionSplitCharge_SecurityRight()
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

					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost1 = invoice.ConsolCosting.ConsolCosts.AddNew();
					cost1.E6_ParentID = consol.PK;
					cost1.E6_ParentTableCode = "JK";
					cost1.E6_AC_ChargeCode = creator.CC12.PK;
					cost1.E6_OSCostAmount = 70M;
					Assert(!cost1.HasErrors);

					TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode, false, invoice);

					var charge1 = cost1.ApportionmentCharges[0];
					var charge2 = cost1.ApportionmentCharges[1];
					charge1.JR_OSCostAmt = 30M;
					charge2.JR_OSCostAmt = 130M;

					AssertEquals("The cost variance requires None approval, 30 is NOT above 100.", ZBool.True, charge1.IsFinal);
					AssertEquals("The cost variance requires 1st level approval, 130 is above 100.", ZBool.False, charge2.IsFinal);

					var expectErrorUntick = @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag";

					Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;
					charge1.IsFinal = ZBool.False;
					AssertHasError("Should have error.", charge1.IsFinalInfo, expectErrorUntick);

					Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;
					var validation1 = charge1.Validation as ApportionSplitChargeValidation;
					validation1.ValidateIsFinal();
					AssertNoErrors("Should have no errors.", charge1.IsFinalInfo);

					var expectErrorTick = @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice";

					Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
					charge2.IsFinal = ZBool.True;
					AssertHasError("Should have error.", charge2.IsFinalInfo, expectErrorTick);

					Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
					var validation2 = charge2.Validation as ApportionSplitChargeValidation;
					validation2.ValidateIsFinal();
					AssertNoErrors("Should have no errors.", charge2.IsFinalInfo);
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		ForwardingConsol newConsol(string name, bool isLinked, string port1, string port2)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			//Factory.Save();
			consol.JK_UniqueConsignRef = name;
			consol.JK_RL_NKLoadPort = port1;
			consol.JK_RL_NKDischargePort = port2;

			Transport lastTransport = consol.Transports[0];
			lastTransport.JW_RL_NKLoadPort = port1;
			lastTransport.JW_RL_NKDiscPort = port2;
			lastTransport.JW_IsLinked = isLinked;

			return consol;
		}

		void setupConsols(out ZGuid consolPK, out ZGuid consol2PK, out ZString voyageFlight, out ZString shipmentNumber, out ZString masterBillNumber, out ZString houseBillNumber, out ZString containerNumber, out ZString coLoadMasterBillNumber, out ZString bookingReference, out ForwardingConsol consol1, out ForwardingConsol consol2)
		{
			consol1 = newConsol("C000001", false, "AUMEL", "SGSIN");
			consolPK = consol1.PK;
			masterBillNumber = "08112345675";
			consol1.JK_MasterBillNum = masterBillNumber;
			coLoadMasterBillNumber = "1234567890";
			bookingReference = coLoadMasterBillNumber;
			consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = coLoadMasterBillNumber;

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			Transport transport1 = consol1.Transports[0];
			transport1.JW_Vessel = vessel.RV_FK;
			voyageFlight = "1111";
			transport1.JW_VoyageFlight = voyageFlight;

			consol2 = newConsol("C000002", false, "SGSIN", "NZAKL");
			consol2PK = consol2.PK;
			consol2.JK_MasterBillNum = "081";
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "1234509876";

			RefVessel vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_LloydsNumber = "1234567";

			Transport transport2 = consol2.Transports[0];
			transport2.JW_Vessel = vessel2.RV_FK;
			transport2.JW_VoyageFlight = "1122";

			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipmentNumber = "S10001000";
			shipment1.JS_UniqueConsignRef = shipmentNumber;
			houseBillNumber = "TEST1000";
			shipment1.JS_HouseBill = houseBillNumber;
			Job shipmentJob1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			shipmentJob1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S10002000";
			shipment2.JS_HouseBill = "TEST2000";
			Job shipmentJob2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			shipmentJob2.JH_GE = GlbDepartment.CurrentDepartment.PK;

			ForwardingContainer container1 = consol1.Containers.AddNew();
			containerNumber = "AAA";
			container1.JC_ContainerNum = containerNumber;
			ForwardingContainer container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "AAB";
			Factory.Save();
		}

		public void TestPrefixWithCodeToLookupConsolId()
		{
			ZString voyageFlight;
			ZGuid consolPK;
			ZGuid consol2PK;
			ZString shipmentNumber;
			ZString masterBillNumber;
			ZString houseBillNumber;
			ZString containerNumber;
			ZString coLoadMasterBillNumber;
			ZString bookingReference;
			ForwardingConsol consol1;
			ForwardingConsol consol2;

			setupConsols(out consolPK, out consol2PK, out voyageFlight, out shipmentNumber, out masterBillNumber, out houseBillNumber, out containerNumber, out coLoadMasterBillNumber, out bookingReference, out consol1, out consol2);

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			using (InvoiceForm invoiceForm = new InvoiceForm(apInvoice))
			{
				invoiceForm.Show();

				apInvoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

				invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				AssertEquals("should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
				APInvoiceConsolCostingForm consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

				JobConsolCost cost = apInvoice.ConsolCosting.ConsolCosts.AddNew();

				AssertConsol(consolCostingForm, apInvoice, ref cost, consolPK, "V", voyageFlight, voyageFlight.Substring(0, 2));

				AssertConsol(consolCostingForm, apInvoice, ref cost, consolPK, "S", shipmentNumber, shipmentNumber.Substring(0, 5));

				AssertConsol(consolCostingForm, apInvoice, ref cost, consolPK, "M", masterBillNumber, masterBillNumber.Substring(0, 3));

				AssertConsol(consolCostingForm, apInvoice, ref cost, consolPK, "H", houseBillNumber, houseBillNumber.Substring(0, 4));

				AssertConsol(consolCostingForm, apInvoice, ref cost, consolPK, "T", containerNumber, containerNumber.Substring(0, 2));

				AssertConsol(consolCostingForm, apInvoice, ref cost, consolPK, "L", coLoadMasterBillNumber, coLoadMasterBillNumber.Substring(0, 5));

				apInvoice.ConsolCosting.ConsolCosts.RemoveAndDeleteAll();

				consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
				consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
				consol1.JK_BookingReference = "1234567890";
				consol2.JK_BookingReference = "1234509876";
				Factory.Save();

				cost = apInvoice.ConsolCosting.ConsolCosts.AddNew();

				AssertConsol(consolCostingForm, apInvoice, ref cost, consolPK, "B", bookingReference, bookingReference.Substring(0, 5));
			}
		}

		public void TestAPInvoiceConsolCostingFormSuspendsReportingDeletedApportionmentCharges()
		{
			APInvoice invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);

			Assert(!invoice.IsReportingDeletedApportionmentChargesSuspended);
			using (APInvoiceConsolCostingForm form = new APInvoiceConsolCostingForm(invoice.ConsolCosting))
			{
				form.Show();
				Assert(invoice.IsReportingDeletedApportionmentChargesSuspended);
				form.Close();
				Assert(!invoice.IsReportingDeletedApportionmentChargesSuspended);
			}
		}

		void AssertConsol(APInvoiceConsolCostingForm consolCostingForm, APInvoice apInvoice, ref JobConsolCost cost, ZGuid consolPK, ZString prefix, ZString code1, ZString code2)
		{
			ZGrid grid = consolCostingForm.ApportionmentsGrid_ForTestOnly;
			AssertEquals("Grid BindTo", "ConsolCosts", grid.BindTo);

			EditGrid(grid, prefix, code1, cost);

			AssertEquals("Should contain 1 JobConsolCost", 1, apInvoice.ConsolCosting.ConsolCosts.Count);

			AssertEquals("JobConsolCost.E6_ParentID should equal", consolPK, cost.E6_ParentID);

			apInvoice.ConsolCosting.ConsolCosts.RemoveAndDeleteAll();
			cost = apInvoice.ConsolCosting.ConsolCosts.AddNew();

			EditGrid(grid, prefix, code2, cost);

			AssertEquals("JobConsolCost.E6_ParentID should equal", ZGuid.Invalid, cost.E6_ParentID);
			AssertHasError(cost.E6_ParentIDInfo, "There are multiple possible ViewGenericConsols. Select the Consol cell and press F4 to choose one.");

			apInvoice.ConsolCosting.ConsolCosts.RemoveAndDeleteAll();
			cost = apInvoice.ConsolCosting.ConsolCosts.AddNew();

			EditGrid(grid, prefix, "XYZXYZ", cost);

			AssertEquals("JobConsolCost.E6_ParentID should equal", ZGuid.Invalid, cost.E6_ParentID);
			AssertHasError(cost.E6_ParentIDInfo, "Enter a valid Consol.");

			apInvoice.ConsolCosting.ConsolCosts.RemoveAndDeleteAll();
			cost = apInvoice.ConsolCosting.ConsolCosts.AddNew();
		}

		void AssertColumnStyle(ZGridColumnInfo zGridColumnInfo, bool isUnavailable, string caption, string captionResourceStringCaption, string captionResourceStringShortCaption)
		{
			AssertEquals(isUnavailable, zGridColumnInfo.IsUnavailable);
			AssertEquals(caption, zGridColumnInfo.Caption);
			AssertEquals(captionResourceStringCaption, zGridColumnInfo.CaptionResourceString.Caption);
			AssertEquals(captionResourceStringShortCaption, zGridColumnInfo.CaptionResourceString.ShortCaption);
		}

		void EditGrid(ZGrid grid, ZString prefix, ZString code, JobConsolCost cost)
		{
			if (cost != null)
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(ZGuid.Empty, ZString.Empty);
			}
			InitGtid(grid);
			AssertNotNull("LastFocusedColumn should be set when Grid gets Focus", grid.LastFocusedColumn);
			AssertEquals("Should be Editing Column 1", 1, grid.CurrentCell.ColumnNumber);
			AssertEquals("Should be Editing Row 0", 0, grid.CurrentCell.RowNumber);

			PostCodeToGrid(grid, prefix + ":" + code);

			PostKeyToGrid(grid, Keys.Tab);
			AssertEquals("Should be Editing Column 2", 2, grid.CurrentCell.ColumnNumber);
			AssertEquals("Should be Editing Row 0", 0, grid.CurrentCell.RowNumber);
		}

		void InitGtid(ZGrid grid)
		{
			grid.Focus();
			Application.DoEvents();

			grid.CurrentCell = new DataGridCell(0, 1);
			Application.DoEvents();
			PostKeyToGrid(grid, Keys.F2);
		}

		void PostKeyToGrid(ZGrid grid, Keys key, bool doEvents = true)
		{
			ZArchitecture.GUI.Internal.ZFindBoxUserControl control = (ZArchitecture.GUI.Internal.ZGridFindBox)grid.LastFocusedColumn.EditControl;
			KeySender.PostKeyDown(control.CodeBox, control.CodeBox.Handle, key);
			if (doEvents)
			{
				Application.DoEvents();
			}
		}

		void PostCodeToGrid(ZGrid grid, ZString code)
		{
			ZArchitecture.GUI.Internal.ZFindBoxUserControl control = (ZArchitecture.GUI.Internal.ZGridFindBox)grid.LastFocusedColumn.EditControl;
			foreach (char c in code)
			{
				KeySender.SendKeyPress(control.CodeBox, c);
			}
			Application.DoEvents();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_innerValue ?? (TestObjectCreator_innerValue = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_innerValue;
	}
}
