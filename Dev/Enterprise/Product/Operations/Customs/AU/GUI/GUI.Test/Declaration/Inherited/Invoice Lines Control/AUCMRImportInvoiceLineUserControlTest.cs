using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCMRImportInvoiceLineUserControlTest : AUImportInvoiceLineUserControlTest
	{
		[ExpectNoExceptions]
		public override void TestInstantiation()
		{
			using (var control = new AUCMRImportInvoiceLineUserControl())
			{
			}
		}

		public void TestPremisesIdColumnModuleID()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new AUCMRImportInvoiceLineUserControl())
			{
				var premisesIdColumn = control.AQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is off", Enterprise.ZArchitecture.Modules.ModuleIDs.Premises, premisesIdColumn.ModuleID);
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new AUCMRImportInvoiceLineUserControl())
			{
				var premisesIdColumn = control.AQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is on", Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, premisesIdColumn.ModuleID);
			}
		}

		public void TestInvoiceLineGridContext()
		{
			using (AUCMRImportInvoiceLineUserControl control = new AUCMRImportInvoiceLineUserControl())
			{
				AssertEquals("Context is set", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestAddInfoCMRMode()
		{
			using (var testForm = new AUCustomsDeclarationFormForTest(Factory.New<JobDeclaration>()))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AUCMRImportInvoiceLineUserControl invoiceLines = testForm.InvoiceUserControl as AUCMRImportInvoiceLineUserControl;
				AssertEquals("Should show CMR mode", true, invoiceLines.JI_AddInfoBoundAddInfoControl.ShowCMRAddInfo);
			}
		}

		public void TestAQISContainersTabControl()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				AUCMRImportInvoiceLineUserControl invoiceLines = testForm.InvoiceUserControl as AUCMRImportInvoiceLineUserControl;
				ZTabPage containerTabPage = invoiceLines.ContainersTabPage;
				AssertEquals("Text on Tab Page", "Containers", containerTabPage.Text);
				AssertEquals("Text on group box", "Containers", invoiceLines.FindSingle<ZGroupBox>("ContainersGroupBox").Text);
				AssertEquals("Label is shown", false, invoiceLines.ContainerTabHiddenLabel.Visible);
			}
		}

		public void TestAQISContainersTabControlForExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = "SEA";
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AUCMRImportInvoiceLineUserControl invoiceLines = testForm.InvoiceUserControl as AUCMRImportInvoiceLineUserControl;
				invoiceLines.LineDetailTabControl.SelectedTab = invoiceLines.ContainersTabPage;
				AssertEquals("Label is shown", true, invoiceLines.ContainerTabHiddenLabel.Visible);
			}
		}

		public void TestWHSFieldsAreShownForExWarehouse()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
			AssertEquals(false, declaration.IsWHSUniversalXMLActive);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			var whsFields = new[] { AUAddInfo.Schema.ZA_WRN, AUAddInfo.Schema.ZA_WRL, AUAddInfo.Schema.ZA_WRQ, AUAddInfo.Schema.ZA_WRU };
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;
				foreach (var columnName in whsFields)
				{
					AssertEquals(columnName + " should not be visible", false, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(AddInfoPrefix + columnName).IsVisible);
				}
			}

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;
				foreach (var columnName in whsFields)
				{
					AssertEquals(columnName + " should be visible", true, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(AddInfoPrefix + columnName).IsVisible);
				}
			}

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;
				foreach (var columnName in whsFields)
				{
					AssertEquals(columnName + " should not be visible", false, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(AddInfoPrefix + columnName).IsVisible);
				}
			}
		}

		public void TestWHSAddressOnlyShowsOnOldAndExWarehouseJobs()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			AssertEquals(false, declaration.IsWHSUniversalXMLActive);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;
				foreach (var columnName in new[] { AUAddInfo.Schema.WarehouseOrgFK, AUAddInfo.Schema.ZA_OA_WarehouseAddress_Hidden })
				{
					AssertEquals(columnName + " should be available", false, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(AddInfoPrefix + columnName).IsUnavailable);
				}
			}

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;
				foreach (var columnName in new[] { AUAddInfo.Schema.WarehouseOrgFK, AUAddInfo.Schema.ZA_OA_WarehouseAddress_Hidden })
				{
					AssertEquals(columnName + " should not be available", true, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(AddInfoPrefix + columnName).IsUnavailable);
				}
			}

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;
				foreach (var columnName in new[] { AUAddInfo.Schema.WarehouseOrgFK, AUAddInfo.Schema.ZA_OA_WarehouseAddress_Hidden })
				{
					AssertEquals(columnName + " should be available", false, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(AddInfoPrefix + columnName).IsUnavailable);
				}
			}
		}

		public void TestSupplierLookupVisibleForExWarehouse()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
			AssertEquals(false, declaration.IsWHSUniversalXMLActive);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;

				AssertEquals("Supplier should be visible with ExWarehouse", true, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_OH_Supplier).IsVisible);
				AssertEquals("Supplier should be org finder", typeof(MasterFiles.GUI.ZOrganisationFindBoxColumnStyle), invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_OH_Supplier).ColumnStyleType);
			}

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (AUCMRImportInvoiceLineUserControl)form.InvoiceUserControl;
				AssertEquals("Supplier should not be visible with Import", false, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_OH_Supplier).IsVisible);
			}
		}

		public void TestWRNException_CS00146164()
		{
			var warehousing1 = GetNewWarehousingCreator();
			warehousing1.Declaration.DoMerge();
			Factory.Save();

			var n30Factory = new BusinessObjectFactory();
			var n30Declaration = JobDeclaration.New(n30Factory);
			n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
			n30Declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.ExWarehouse;
			var n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
			n30Line1.AddInfo.ZA_WRN = "EEE1";
			n30Line1.AddInfo.ZA_WRL = 1;
			n30Line1.JI_InvoiceQuantity = 55;
			n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

			using (var form = new ZForm(n30Declaration))
			{
				var grid = new ZGrid();
				var codeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();

				codeFindBoxColumnStyleInfo.BindToList = "AddInfo+Lookups+EntryKeys";
				codeFindBoxColumnStyleInfo.Caption = "WRN";
				codeFindBoxColumnStyleInfo.ColumnName = "AddInfo+ZA_WRN";
				codeFindBoxColumnStyleInfo.GroupName = NoResourceStringData.GetData("WRN/WRL");
				codeFindBoxColumnStyleInfo.ModuleID = ZArchitecture.Modules.ModuleIDs.WhsEntryLine;
				grid.ColumnStyles.Add(codeFindBoxColumnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(n30Declaration.InvoiceLines, "");
				grid.BeginEdit(grid.Columns[0].ColumnStyle, 0);
				((IFindBoxUserControl)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).SelectFromPopupForm();
				try
				{
					EmbeddedModulePopup popup = ZFormModaliser.ActiveForm as EmbeddedModulePopup;
					AssertNotNull(popup);
				}
				finally
				{
					IDisposable disposable = ZFormModaliser.ActiveForm;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
		}

		public void TestICSPermitTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var invoiceLineUsercontrol = new AUCMRImportInvoiceLineUserControl())
			{
				invoiceLineUsercontrol.JobDeclaration = declaration;
				var icsPermitPage = (ZTabPage)invoiceLineUsercontrol.LineDetailTabControl.TabPages["ICSPermitTabPage"];
				AssertEquals(true, icsPermitPage.TabVisible);
			}
		}

		public void TestSetCantCreateInvoiceLinesLabel_JobDeclarationRowDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			{
				var control = new AUCMRImportInvoiceLineUserControlForTest();
				control.JobDeclaration = declaration;
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				control.InitializeGridLayout();
				control.SetDataBinding(declaration, "");
				declaration.Delete();
				control.SetCantCreateInvoiceLinesLabelInternal();
				AssertEquals(false, control.CantCreateInvoiceLinesLabelInternal.Visible);
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_TransportMode = "AIR";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (var testForm = new ZForm(declaration))
			{
				var userControl = new AUCMRImportInvoiceLineUserControlForTest();
				userControl.JobDeclaration = declaration;
				testForm.Controls.Add(userControl);
				testForm.Show();
				userControl.LineDetailTabControl.SelectedTab = userControl.LineDetailsTabPageInternal;
				AssertEquals("PackToBond visible", true, userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_IsPackToBondForLine].IsVisible);
				AssertNull("UseBondedWarehouseAutomation not available", userControl.CustomsInvoiceLinesBoundGrid.Columns[AUImportInvoiceLineUserControl.AddInfoPrefix + AUAddInfo.Schema.UseBondedWarehouseAutomation]);
				AssertEquals("Customs Value column caption", "Price", userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice).Caption);
				AssertEquals("Valuation Basis Visibility", true, userControl.ValuationBasisBoundDropEdit.Visible);
				AssertEquals("Header Valuation Basis Visibility", true, userControl.HeaderValuationBasisTextBox.Visible);
				AssertEquals("Transaction Basis Visibility", true, userControl.TransactionBasisZDropEdit.Visible);
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_TransportMode = "AIR";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (var testForm = new ZForm(declaration))
			{
				var userControl = new AUCMRImportInvoiceLineUserControlForTest();
				userControl.JobDeclaration = declaration;
				testForm.Controls.Add(userControl);
				testForm.Show();
				userControl.LineDetailTabControl.SelectedTab = userControl.LineDetailsTabPageInternal;
				AssertNull("UseBondedWarehouseAutomation not available", userControl.CustomsInvoiceLinesBoundGrid.Columns[AUImportInvoiceLineUserControl.AddInfoPrefix + AUAddInfo.Schema.UseBondedWarehouseAutomation]);
				AssertNull("PackToBond not available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_IsPackToBondForLine]);
				AssertEquals("Customs Value column caption", "Customs Value", userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice).Caption);
				AssertEquals("Valuation Basis Visibility", false, userControl.ValuationBasisBoundDropEdit.Visible);
				AssertEquals("Header Valuation Basis Visibility", false, userControl.HeaderValuationBasisTextBox.Visible);
				AssertEquals("Transaction Basis Visibility", false, userControl.TransactionBasisZDropEdit.Visible);
			}

			declaration.SetSupportsBondedWarehousingForTesting(true);
			using (var testForm = new ZForm(declaration))
			{
				var userControl = new AUCMRImportInvoiceLineUserControlForTest();
				userControl.JobDeclaration = declaration;
				testForm.Controls.Add(userControl);
				testForm.Show();
				AssertEquals("UseBondedWarehouseAutomation visible", true, userControl.CustomsInvoiceLinesBoundGrid.Columns[AUImportInvoiceLineUserControl.AddInfoPrefix + AUAddInfo.Schema.UseBondedWarehouseAutomation].IsVisible);
				AssertNull("PackToBond not available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_IsPackToBondForLine]);
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
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AUCMRImportInvoiceLineUserControl invoiceLines = testForm.InvoiceUserControl as AUCMRImportInvoiceLineUserControl;
				invoiceLines.LineDetailTabControl.SelectedTab = invoiceLines.AQISTabPage;
				AssertEquals("Label is not shown", false, invoiceLines.AQISTabHiddenLabel.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLines = testForm.InvoiceUserControl as AUCMRImportInvoiceLineUserControl;
				invoiceLines.LineDetailTabControl.SelectedTab = invoiceLines.AQISTabPage;
				AssertEquals("Label is not shown", true, invoiceLines.AQISTabHiddenLabel.Visible);
			}
		}

		public void TestCantCreateInvoiceLinesTextChangesWhenSAC()
		{
			string expectedTextWhenSAC = "Invoice Lines are not applicable to a SAC without lines." + System.Environment.NewLine + "If Invoice Lines are required you should select SWL in the Style field on the Declaration Tab.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SAC";
			declaration.JE_ApplicationCode = "CMR";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				var userControl = new AUCMRImportInvoiceLineUserControlForTest();
				userControl.JobDeclaration = declaration;
				testForm.Controls.Add(userControl);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertNotNull("Invoice User Control", userControl);
				AssertEquals(expectedTextWhenSAC, userControl.CantCreateInvoiceLinesTextInternal);
				AssertEquals(true, userControl.CantCreateInvoiceLinesLabelInternal.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageSubType = "FRM";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals(true, userControl.CantCreateInvoiceLinesTextInternal != expectedTextWhenSAC);
				AssertEquals(false, userControl.CantCreateInvoiceLinesLabelInternal.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AUCustomsImportsMessagingMode = "CMR";
		}

		MergedDeclarationCreator2Line GetNewWarehousingCreator()
		{
			var creator = new MergedDeclarationCreator2Line(Factory);
			creator.Declaration.JE_OH_Supplier = ZGuid.Empty;
			creator.InvoiceLine1.InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			creator.InvoiceLine2.InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			creator.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			creator.Declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			creator.InvoiceLine1.JI_IsPackToBondForLine = true;
			creator.InvoiceLine2.JI_IsPackToBondForLine = true;
			return creator;
		}

		string AddInfoPrefix => AUImportInvoiceLineUserControl.AddInfoPrefix;

		sealed class AUCMRImportInvoiceLineUserControlForTest : AUCMRImportInvoiceLineUserControl
		{
			internal ZString CantCreateInvoiceLinesTextInternal => CantCreateInvoiceLinesText;

			internal void SetCantCreateInvoiceLinesLabelInternal() => SetCantCreateInvoiceLinesLabel();

			internal ZLabel CantCreateInvoiceLinesLabelInternal => CantCreateInvoiceLinesLabel;

			internal ZTabPage LineDetailsTabPageInternal => LineDetailsTabPage;
		}
	}
}
