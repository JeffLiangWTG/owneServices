using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCMRSupplierHeaderUserControlTest : AUSupplierHeaderUserControlTest
	{
		public void TestPremisesIdColumnModuleID()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new AUCMRSupplierHeaderUserControl())
			{
				var premisesIdColumn = control.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is off", Enterprise.ZArchitecture.Modules.ModuleIDs.Premises, premisesIdColumn.ModuleID);
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new AUCMRSupplierHeaderUserControl())
			{
				var premisesIdColumn = control.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is on", Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, premisesIdColumn.ModuleID);
			}
		}

		public void TestGridId()
		{
			using (var control = new AUCMRSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutgo14qunWIFHdj00dMR9xAw==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestInvoiceHeaderGridContext()
		{
			using (AUCMRSupplierHeaderUserControl control = new AUCMRSupplierHeaderUserControl())
			{
				AssertEquals("Context is set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public override void TestGetColumnOrderForInvoiceHeaderGrid()
		{
			Declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				Assert("Column Order array list should have at least 31 elements", 31 <= supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.Count);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_OA_SupplierAddress);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.SupplierName);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.InvoiceLineTotal);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.ZA_ORG);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_POC");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_PST");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_PRT");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_VALB_Hidden");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.ZA_GSTE);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentNo);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentExRate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_PermitNumbers_Hidden");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_Volume);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_VolumeUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_Weight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_WeightUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_NetWeight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_NetWeightUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_ValuationDateOverride);
				AssertEquals("Inco Term should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm].IsVisible);
				AssertEquals("Agreed Place should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace].IsVisible);
				AssertEquals("InvoiceLineTotal should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency].IsVisible);
				AssertEquals("Invoice Curr Ex Rate should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate].IsVisible);
				AssertEquals("Payment date is not visible", false, supplierHeader.JobComInvoiceHeadersBoundGrid.ColumnStyles.Contains(JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate));
			}
		}

		public void TestAddInfoCMRMode()
		{
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AssertEquals("Should show CMR mode", true, supplierHeader.JZ_AddInfoBoundAddInfoControl.ShowCMRAddInfo);
			}
		}

		public void TestGetColumnOrderForInvoiceHeaderGridForOther()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			Declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				Assert("Column Order array list should have at least 33 elements", 33 <= supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.Count);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_OA_SupplierAddress);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.SupplierName);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.InvoiceLineTotal);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.JZ_Nature10PackCount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.ZA_ORG);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_POC");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_PST");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_PRT");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_VALB_Hidden");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.ZA_GSTE);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentNo);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_PaymentExRate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_PermitNumbers_Hidden");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_Volume);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_VolumeUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_Weight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_WeightUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_NetWeight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeaderSchema.Constants.JZ_NetWeightUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_ValuationDateOverride);
				AssertEquals("Inco Term should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm].IsVisible);
				AssertEquals("Agreed Place should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace].IsVisible);
				AssertEquals("InvoiceLineTotal should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.BaseJobComInvoiceHeader.Schema.InvoiceLineTotal].IsVisible);
				AssertEquals("Invoice Curr Ex Rate should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate].IsVisible);
				AssertEquals("Payment date is not visible", false, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate].IsVisible);
				AssertEquals("Caption on Packages Column", "No. of Packages", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Nature10PackCount).Caption);
			}
		}

		public void TestGetColumnOrderOfSAC()
		{
			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = "CMR";
			Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AssertNotNull("Balance should exist", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString]);
			}

			Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AssertNull("Balance should not exist", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString]);
			}
		}

		public override void TestDeclarationModes()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("JZ_AddInfoBoundTextBox Visible", true, supplierHeader.JZ_AddInfoBoundAddInfoControl.Visible);
				AssertEquals("ZA_VALB_HiddenDropEdit Visible", true, supplierHeader.ZA_VALB_HiddenDropEdit.Visible);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox Visible", true, supplierHeader.zA_GSTECodeFindBox.Visible);
				AssertEquals("Origin Visible", true, supplierHeader.InvoiceOriginCodeFindBox.Visible);
			}
		}

		public override void TestLockingShipmentData()
		{
			JobDeclaration jobDecBizObj = Factory.New<JobDeclaration>();
			jobDecBizObj.JE_ApplicationCode = "CMR";
			jobDecBizObj.SetReadOnlyIncludingChildren(true);
			jobDecBizObj.Invoices.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(jobDecBizObj))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeaderControl = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl as AUDeclarationUserControl;
				testForm.CustomsBrokerageUserControl.SetDeclarationReadOnly(true);
				AssertEquals("JZ_AddInfoBoundTextBox ReadOnly", true, supplierHeaderControl.JZ_AddInfoBoundAddInfoControl.ReadOnly);
				AssertEquals("ZA_GSTECodeFindBox ReadOnly", true, supplierHeaderControl.zA_GSTECodeFindBox.ReadOnly);
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", true, supplierHeaderControl.InvoiceNumberBoundTextBox.ReadOnly);
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", true, supplierHeaderControl.IncoTermBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_ValuationBasisBoundDropDownEdit ReadOnly", true, supplierHeaderControl.ZA_VALB_HiddenDropEdit.ReadOnly);
			}

			jobDecBizObj = Factory.New<JobDeclaration>();
			jobDecBizObj.JE_ApplicationCode = "CMR";
			jobDecBizObj.Invoices.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(jobDecBizObj))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeaderControl = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl as AUDeclarationUserControl;
				AssertEquals("JZ_AddInfoBoundTextBox ReadOnly", false, supplierHeaderControl.JZ_AddInfoBoundAddInfoControl.ReadOnly);
				AssertEquals("ZA_GSTECodeFindBox ReadOnly", false, supplierHeaderControl.zA_GSTECodeFindBox.ReadOnly);
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", false, supplierHeaderControl.InvoiceNumberBoundTextBox.ReadOnly);
				UserIdleWorker.Flush();
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", false, supplierHeaderControl.IncoTermBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_ValuationBasisBoundDropDownEdit ReadOnly", false, supplierHeaderControl.ZA_VALB_HiddenDropEdit.ReadOnly);
			}
		}

		public void TestValuationBasis()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Factory.New<JobDeclaration>()))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AssertEquals("Control Enabled", true, supplierHeader.ZA_VALB_HiddenDropEdit.Visible);
			}
		}

		public void TestAQISTabControl()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				supplierHeader.InvoiceTabControl.SelectedTab = supplierHeader.aQISInfoTabPage;
				AssertEquals("Label is not shown", false, supplierHeader.AQISTabHiddenLabel.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				supplierHeader.InvoiceTabControl.SelectedTab = supplierHeader.aQISInfoTabPage;
				AssertEquals("Label is not shown", true, supplierHeader.AQISTabHiddenLabel.Visible);
			}
		}

		public void TestAUDoesNotHaveBuyerOnInvoiceHeader()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AssertNull("Buyer should not exist", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OH_Buyer]);
			}
		}

		protected override ZInt NumberOfGridColumnsForNature20(bool enabled)
		{
			return enabled ? 47 : 20;
		}

		public override void TestGreyOutFieldsForNature30()
		{
			base.TestGreyOutFieldsForNature30();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUCMRSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AssertEquals("Control Enabled", false, supplierHeader.addInfoPermitNumberBoundTextBox.Visible);
				AssertEquals("Control Enabled", false, supplierHeader.zA_HeaderREL_HiddenDropEdit.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				supplierHeader = testForm.SupplierUserControl as AUCMRSupplierHeaderUserControl;
				AssertEquals("Control Enabled", true, supplierHeader.addInfoPermitNumberBoundTextBox.Visible);
				AssertEquals("Control Enabled", true, supplierHeader.zA_HeaderREL_HiddenDropEdit.Visible);
			}
		}

		protected override JobDeclaration Declaration => declaration;

		protected override ZString AppCode => "CMR";

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
		}
	}
}
