using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using DeclarationValueChangedAnnouncer = Enterprise.Customs.CA.Business.DeclarationValueChangedAnnouncer;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;
using PGACodes = Enterprise.Customs.CA.Business.PGACodes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestPackagesPivotTabPageVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();
			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();
			var pack = packageGroup.Packages.AddNew();
			pack.CW_PackQty = 1;
			pack.CW_PackType = "AE";

			var headerPackageCollection = (InvoiceHeaderCusLinkPackageCollection)invoice.PackagesForInvoicesForBindingOnly;

			var headerLinkPackage = Factory.New<InvoiceHeaderPackagePivot>();
			headerLinkPackage.CHZ_JE = declaration.PK;
			headerLinkPackage.CHZ_JZ = invoice.PK;
			headerLinkPackage.CHZ_CW = pack.PK;
			headerLinkPackage.CHZ_NumberOfPacks = 10;
			invoice.PackagesPivot.Add(headerLinkPackage);

			var lineLinkPackage = Factory.New<InvoiceLinePackagePivot>();
			lineLinkPackage.CHC_JE = declaration.PK;
			lineLinkPackage.CHC_JI = invoiceLine.PK;
			lineLinkPackage.CHC_CW = pack.PK;
			lineLinkPackage.CHC_NumberOfPacks = 20;
			invoiceLine.PackagesPivot.Add(lineLinkPackage);

			headerPackageCollection[0].IsLinked = true;

			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var packagesPivotTabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Name == "PackagesPivotTabPage");
				AssertEquals("PackagesPivotTabPage Is Visible", true, packagesPivotTabPage.TabVisible);
			}
		}

		public void TestCFIARegNumbersGrid()
		{
			using (var control = new CAImportInvoiceLineUserControl())
			{
				var numberColumn = control.CFIARegNumbersGrid.GetColumnStyle(CusCodeData.Schema.CY_Data) as ZDropEditColumnStyleInfo;
				AssertNotNull(numberColumn);
			}
		}

		public void TestTariffUserControlForCAGlobalTariff()
		{
			var gridName = "CustomsInvoiceLinesBoundGrid";
			var columnName = JobComInvoiceLine.Schema.JI_FormattedTariff;
			var tariffFindBoxName_TrfCA = "ClassificationNumberFindBox";
			var tariffFindBoxName_SRDb = "ClassificationNumberFromRefDbFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(control, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(control, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(control, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}

		public void TestShouldDeleteLuxuryTaxInvoiceLineEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;

				invoiceLine.CA_ApplyLuxuryTax = true;
				AssertNotNull(invoiceLine.ShouldDeleteLuxuryTaxInvoiceLine);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				invoiceLine.CA_ApplyLuxuryTax = false;
				AssertEquals("Making this change will delete the corresponding luxury tax invoice line. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(invoiceLine.CA_ApplyLuxuryTax);
				AssertNotNull(invoiceLine.LuxuryTaxInvoiceLine);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				invoiceLine.CA_ApplyLuxuryTax = false;
				AssertEquals("Making this change will delete the corresponding luxury tax invoice line. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!invoiceLine.CA_ApplyLuxuryTax);
				AssertNull(invoiceLine.LuxuryTaxInvoiceLine);
			}
		}

		public void TestVisibleOfTabPageWitNotification_AfterDeleteAction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceline1 = invoice.JobComInvoiceLines.AddNew();
			invoiceline1.CA_TCInd = YesNoList.Codes.Yes;
			invoiceline1.CA_HCInd = YesNoList.Codes.No;

			var invoiceline2 = invoice.JobComInvoiceLines.AddNew();
			invoiceline2.CA_TCInd = YesNoList.Codes.No;
			invoiceline2.CA_HCInd = YesNoList.Codes.Yes;

			SetChildProgramTicked(invoiceline1, PGACodes.Codes.TC);
			SetChildProgramTicked(invoiceline2, PGACodes.Codes.HC);

			invoiceline1.JI_OA_ManufacturerAddress = ZGuid.Invalid;
			invoiceline1.CA_ModelYear = "XXXX";

			var pgaHeader = invoiceline1.TCPGAHeader;
			pgaHeader.AddRowError("Make sure there is an error.");

			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var grid = control.CustomsInvoiceLinesBoundGrid;
				grid.SelectSingleElement(invoiceline1);

				var tcTabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.TC);
				var hcTabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.HC);

				AssertNotNull(tcTabPage);
				AssertNull(hcTabPage);

				tcTabPage.Select();
				form.FireValidateAllForTest();

				grid.DeleteMenuItem.PerformClick();

				tcTabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.TC);
				hcTabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.HC);

				AssertNull(tcTabPage);
				AssertNotNull(hcTabPage);
			}
		}

		public void TestGridLayoutForIM2()
		{
			using (var control = new CAImportInvoiceLineUserControl())
			{
				var expectedColumnList =
				new List<string>
					{
						JobComInvoiceLine.Schema.CA_PreviousB3LineNo,
						JobComInvoiceLine.Schema.CA_PreviousB3SubHeaderNo,
						"CusEntryLine+CA_B2SubHeader"
					};

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				declaration.InvoiceLines.AddNew();

				AssertInvoiceLineDefaultColumnsSequenceForIM2(declaration, expectedColumnList);
			}
		}

		public void TestGridLayoutForIMPAndIIDJob()
		{
			using (var control = new CAImportInvoiceLineUserControl())
			{
				var expectedColumnList =
				new List<string>
					{
						JobComInvoiceLine.Schema.CA_CFIAAllProgramInd,
						JobComInvoiceLine.Schema.CA_RN_NKCountryOfSourceCFIA,
						JobComInvoiceLine.Schema.CA_AIRSEndUseCFIA,
						JobComInvoiceLine.Schema.CA_AIRSExtensionCodeCFIA,
						JobComInvoiceLine.Schema.CA_DeliveryLocationCFIA,
						JobComInvoiceLine.Schema.CA_OA_ConsigneeAddressCFIA,
						JobComInvoiceLine.Schema.CA_RW_NKSourceStateCFIA,
						JobComInvoiceLine.Schema.CA_AIRSMiscellaneousCFIA,
						JobComInvoiceLine.Schema.CA_APIProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeAPI,
						JobComInvoiceLine.Schema.CA_CategoryAPI,
						JobComInvoiceLine.Schema.CA_ProductionDateAPI,
						JobComInvoiceLine.Schema.CA_ProductionDateCPR,
						JobComInvoiceLine.Schema.CA_ProductionDateHDR,
						JobComInvoiceLine.Schema.CA_ProductionDateMDE,
						JobComInvoiceLine.Schema.CA_ProductionDateNHP,
						JobComInvoiceLine.Schema.CA_ProductionDatePES,
						JobComInvoiceLine.Schema.CA_ProductionDateVET,
						JobComInvoiceLine.Schema.CA_GTINNumberAPI,
						JobComInvoiceLine.Schema.CA_GTINNumberBBC,
						JobComInvoiceLine.Schema.CA_GTINNumberCTO,
						JobComInvoiceLine.Schema.CA_GTINNumberCPR,
						JobComInvoiceLine.Schema.CA_GTINNumberHDR,
						JobComInvoiceLine.Schema.CA_GTINNumberMDE,
						JobComInvoiceLine.Schema.CA_GTINNumberNHP,
						JobComInvoiceLine.Schema.CA_GTINNumberVET,
						JobComInvoiceLine.Schema.CA_BrandNameAPI,
						JobComInvoiceLine.Schema.CA_BrandNameCPR,
						JobComInvoiceLine.Schema.CA_BrandNameHDR,
						JobComInvoiceLine.Schema.CA_BrandNameOCS,
						JobComInvoiceLine.Schema.CA_BrandNameMDE,
						JobComInvoiceLine.Schema.CA_BrandNamePES,
						JobComInvoiceLine.Schema.CA_BrandNameVET,
						JobComInvoiceLine.Schema.CA_TradeNameCPR,
						JobComInvoiceLine.Schema.CA_TradeNamePES,
						JobComInvoiceLine.Schema.CA_ModelNameMDE,
						JobComInvoiceLine.Schema.CA_ModelNameRED,
						JobComInvoiceLine.Schema.CA_ManufacturerOrgPKCPR,
						JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressCPR,
						JobComInvoiceLine.Schema.CA_BatchLotNumberAPI,
						JobComInvoiceLine.Schema.CA_BatchLotNumberCPR,
						JobComInvoiceLine.Schema.CA_BatchLotNumberHDR,
						JobComInvoiceLine.Schema.CA_BatchLotNumberOCS,
						JobComInvoiceLine.Schema.CA_BatchLotNumberMDE,
						JobComInvoiceLine.Schema.CA_BatchLotNumberNHP,
						JobComInvoiceLine.Schema.CA_BatchLotNumberPES,
						JobComInvoiceLine.Schema.CA_BatchLotNumberVET,
						JobComInvoiceLine.Schema.CA_BBCProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeBBC,
						JobComInvoiceLine.Schema.CA_CategoryBBC,
						JobComInvoiceLine.Schema.CA_ExpiryDateBBC,
						JobComInvoiceLine.Schema.CA_ExpiryDateCTO,
						JobComInvoiceLine.Schema.CA_CTOProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeCTO,
						JobComInvoiceLine.Schema.CA_CategoryCTO,
						JobComInvoiceLine.Schema.CA_CTO_LCO,
						JobComInvoiceLine.Schema.CA_CPRProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeCPR,
						JobComInvoiceLine.Schema.CA_CategoryCPR,
						JobComInvoiceLine.Schema.CA_DSEProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeDSE,
						JobComInvoiceLine.Schema.CA_CategoryDSE,
						JobComInvoiceLine.Schema.CA_ComplianceStatement,
						JobComInvoiceLine.Schema.CA_HDRProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeHDR,
						JobComInvoiceLine.Schema.CA_CategoryHDR,
						JobComInvoiceLine.Schema.CA_OCSProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeOCS,
						JobComInvoiceLine.Schema.CA_CategoryOCS,
						JobComInvoiceLine.Schema.CA_ManufacturerOrgPKOCS,
						JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressOCS,
						JobComInvoiceLine.Schema.CA_MDEProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeMDE,
						JobComInvoiceLine.Schema.CA_CategoryMDE,
						JobComInvoiceLine.Schema.CA_UniqueDeviceIDNumber,
						JobComInvoiceLine.Schema.CA_MDE_LEX,
						JobComInvoiceLine.Schema.CA_NHPProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeNHP,
						JobComInvoiceLine.Schema.CA_CategoryNHP,
						JobComInvoiceLine.Schema.CA_PESProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodePES,
						JobComInvoiceLine.Schema.CA_CategoryPES,
						JobComInvoiceLine.Schema.CA_ManufacturerOrgPKPES,
						JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressPES,
						JobComInvoiceLine.Schema.CA_CASNumber,
						JobComInvoiceLine.Schema.CA_DangerousGoodsDGSubsPES,
						JobComInvoiceLine.Schema.CA_PES_SPCP,
						JobComInvoiceLine.Schema.CA_PES_EPCP,
						JobComInvoiceLine.Schema.CA_REDProgramInd,
						JobComInvoiceLine.Schema.CA_CategoryRED,
						JobComInvoiceLine.Schema.CA_FDANumber,
						JobComInvoiceLine.Schema.CA_VETProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeVET,
						JobComInvoiceLine.Schema.CA_CategoryVET
					};

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				declaration.InvoiceLines.AddNew();

				AssertInvoiceLineDefaultColumnsSequenceForIMPAndIIDJob(declaration, expectedColumnList);
			}
		}

		public void TestImportDataAvailableOnGridLayoutContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals("Import Data available for IMP", false, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.DisableImportDataMenuItem);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (IM2CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals("Import Data available for IM2", true, brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.DisableImportDataMenuItem);
			}
		}

		public void TestGridLayoutContext()
		{
			using (var control = new CAImportInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				control.InitializeGridLayout();
				AssertEquals("Import ColumnLayoutContext", "CAIMP", control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				control.InitializeGridLayout();
				AssertEquals("LVS ColumnLayoutContext", "CALVS", control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvoiceLineColumnsHasAMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var gridColumns = from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles select info;

				var ammvPerUnitColumnInfo = gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_AMMVPerUnit);
				AssertEquals("ammvPerUnitColumnInfo.IsVisible", false, ammvPerUnitColumnInfo.IsVisible);
				AssertEquals("ammvPerUnitColumnInfo.Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78), ammvPerUnitColumnInfo.Width);
				var ammvPercentageColumnInfo = gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_AMMVPercentage);
				AssertEquals("ammvPercentageColumnInfo.IsVisible", false, ammvPercentageColumnInfo.IsVisible);
				AssertEquals("ammvPercentageColumnInfo.Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102), ammvPercentageColumnInfo.Width);
			}
		}

		public void TestInvoiceLineColumnsHasLuxuryTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var gridColumns = from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles select info;

				var luxuryTaxColumnInfo = gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_ApplyLuxuryTax);
				AssertEquals(false, luxuryTaxColumnInfo.IsVisible);
				AssertEquals(60, luxuryTaxColumnInfo.Width);
			}
		}

		public void TestInvoiceLineDefaultColumnsSequenceForImport()
		{
			var expectedColumnList =
				new List<string>
					{
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_Calc_Invoice,
						JobComInvoiceLine.Schema.CA_PageNumber,
						JobComInvoiceLine.Schema.CA_PageRelativeLineNumber,
						JobComInvoiceLine.Schema.JI_B3LineNumber,
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_CC,
						JobComInvoiceLine.Schema.JI_FormattedTariff,
						JobComInvoiceLine.Schema.CA_99TariffCode,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_InvoiceUQ,
						JobComInvoiceLine.Schema.JI_CustomsQuantity,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.JI_LinePrice,
						JobComInvoiceLine.Schema.JI_Description,
						JobComInvoiceLine.Schema.CA_TreatmentCode,
						JobComInvoiceLine.Schema.CA_ValueForDutyCode,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
						JobComInvoiceLine.Schema.JI_PreviousEntryNumber,
						JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
						JobComInvoiceLine.Schema.DangerousGoodsDGSubs,
						JobComInvoiceLine.Schema.JI_MatchingKey,
					};

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.InvoiceLines.AddNew();

			AssertInvoiceLineDefaultColumnsSequence(declaration, expectedColumnList);
		}

		public void TestColumnGroupName()
		{
			using (var control = new CAImportInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				control.InitializeGridLayout();

				AssertEquals("Origin", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin).GroupName.Caption);
				AssertEquals("Origin", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CFIAAllProgramInd).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_RN_NKCountryOfSourceCFIA).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_AIRSEndUseCFIA).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_AIRSExtensionCodeCFIA).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_DeliveryLocationCFIA).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_OA_ConsigneeAddressCFIA).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_RW_NKSourceStateCFIA).GroupName.Caption);
				AssertEquals("PGA CFIA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_AIRSMiscellaneousCFIA).GroupName.Caption);
				AssertEquals("PGA HC API", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_APIProgramInd).GroupName.Caption);
				AssertEquals("PGA HC API", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeAPI).GroupName.Caption);
				AssertEquals("PGA HC API", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryAPI).GroupName.Caption);
				AssertEquals("PGA HC API", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ProductionDateAPI).GroupName.Caption);
				AssertEquals("PGA HC API", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberAPI).GroupName.Caption);
				AssertEquals("PGA HC API", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BrandNameAPI).GroupName.Caption);
				AssertEquals("PGA HC API", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BatchLotNumberAPI).GroupName.Caption);
				AssertEquals("PGA HC BBC", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BBCProgramInd).GroupName.Caption);
				AssertEquals("PGA HC BBC", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeBBC).GroupName.Caption);
				AssertEquals("PGA HC BBC", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryBBC).GroupName.Caption);
				AssertEquals("PGA HC BBC", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberBBC).GroupName.Caption);
				AssertEquals("PGA HC BBC", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ExpiryDateBBC).GroupName.Caption);
				AssertEquals("PGA HC CTO", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeCTO).GroupName.Caption);
				AssertEquals("PGA HC CTO", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryCTO).GroupName.Caption);
				AssertEquals("PGA HC CTO", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ExpiryDateCTO).GroupName.Caption);
				AssertEquals("PGA HC CTO", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberCTO).GroupName.Caption);
				AssertEquals("PGA HC CTO", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTO_LCO).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPRProgramInd).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeCPR).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryCPR).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ManufacturerOrgPKCPR).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressCPR).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ProductionDateCPR).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberCPR).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BrandNameCPR).GroupName.Caption);
				AssertEquals("PGA HC CPR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_TradeNameCPR).GroupName.Caption);
				AssertEquals("PGA HC DSE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_DSEProgramInd).GroupName.Caption);
				AssertEquals("PGA HC DSE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeDSE).GroupName.Caption);
				AssertEquals("PGA HC DSE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryDSE).GroupName.Caption);
				AssertEquals("PGA HC DSE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ComplianceStatement).GroupName.Caption);
				AssertEquals("PGA HC HDR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_HDRProgramInd).GroupName.Caption);
				AssertEquals("PGA HC HDR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeHDR).GroupName.Caption);
				AssertEquals("PGA HC HDR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryHDR).GroupName.Caption);
				AssertEquals("PGA HC HDR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ProductionDateHDR).GroupName.Caption);
				AssertEquals("PGA HC HDR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberHDR).GroupName.Caption);
				AssertEquals("PGA HC HDR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BrandNameHDR).GroupName.Caption);
				AssertEquals("PGA HC HDR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BatchLotNumberHDR).GroupName.Caption);
				AssertEquals("PGA HC OCS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_OCSProgramInd).GroupName.Caption);
				AssertEquals("PGA HC OCS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeOCS).GroupName.Caption);
				AssertEquals("PGA HC OCS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryOCS).GroupName.Caption);
				AssertEquals("PGA HC OCS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ManufacturerOrgPKOCS).GroupName.Caption);
				AssertEquals("PGA HC OCS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressOCS).GroupName.Caption);
				AssertEquals("PGA HC OCS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BrandNameOCS).GroupName.Caption);
				AssertEquals("PGA HC OCS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BatchLotNumberOCS).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_MDEProgramInd).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeMDE).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryMDE).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ProductionDateMDE).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_UniqueDeviceIDNumber).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_MDE_LEX).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberMDE).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BrandNameMDE).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ModelNameMDE).GroupName.Caption);
				AssertEquals("PGA HC MDE", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BatchLotNumberMDE).GroupName.Caption);
				AssertEquals("PGA HC NHP", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_NHPProgramInd).GroupName.Caption);
				AssertEquals("PGA HC NHP", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeNHP).GroupName.Caption);
				AssertEquals("PGA HC NHP", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryNHP).GroupName.Caption);
				AssertEquals("PGA HC NHP", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ProductionDateNHP).GroupName.Caption);
				AssertEquals("PGA HC NHP", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberNHP).GroupName.Caption);
				AssertEquals("PGA HC NHP", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BatchLotNumberNHP).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_PESProgramInd).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodePES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryPES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ManufacturerOrgPKPES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressPES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ProductionDatePES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CASNumber).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_DangerousGoodsDGSubsPES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BrandNamePES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_TradeNamePES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BatchLotNumberPES).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_PES_SPCP).GroupName.Caption);
				AssertEquals("PGA HC PES", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_PES_EPCP).GroupName.Caption);
				AssertEquals("PGA HC RED", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_REDProgramInd).GroupName.Caption);
				AssertEquals("PGA HC RED", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryRED).GroupName.Caption);
				AssertEquals("PGA HC RED", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_FDANumber).GroupName.Caption);
				AssertEquals("PGA HC RED", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ModelNameRED).GroupName.Caption);
				AssertEquals("PGA HC VET", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_VETProgramInd).GroupName.Caption);
				AssertEquals("PGA HC VET", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_IntendedUseCodeVET).GroupName.Caption);
				AssertEquals("PGA HC VET", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CategoryVET).GroupName.Caption);
				AssertEquals("PGA HC VET", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ProductionDateVET).GroupName.Caption);
				AssertEquals("PGA HC VET", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GTINNumberVET).GroupName.Caption);
				AssertEquals("PGA HC VET", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BrandNameVET).GroupName.Caption);
				AssertEquals("PGA HC VET", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_BatchLotNumberVET).GroupName.Caption);
				AssertEquals("Duty & Tax ADD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ADD_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax ADD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ADD_Code).GroupName.Caption);
				AssertEquals("Duty & Tax ADD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ADD_Description).GroupName.Caption);
				AssertEquals("Duty & Tax ADD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ADD_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax ADD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ADD_Override).GroupName.Caption);
				AssertEquals("Duty & Tax ADD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ADD_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax ADD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_ADD_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax CPT", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPT_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax CPT", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPT_Code).GroupName.Caption);
				AssertEquals("Duty & Tax CPT", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPT_Description).GroupName.Caption);
				AssertEquals("Duty & Tax CPT", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPT_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax CPT", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPT_Override).GroupName.Caption);
				AssertEquals("Duty & Tax CPT", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPT_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax CPT", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CPT_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax CTA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTA_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax CTA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTA_Code).GroupName.Caption);
				AssertEquals("Duty & Tax CTA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTA_Description).GroupName.Caption);
				AssertEquals("Duty & Tax CTA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTA_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax CTA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTA_Override).GroupName.Caption);
				AssertEquals("Duty & Tax CTA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTA_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax CTA", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CTA_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax Excise DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXCDTY_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax Excise DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXCDTY_Code).GroupName.Caption);
				AssertEquals("Duty & Tax Excise DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXCDTY_Description).GroupName.Caption);
				AssertEquals("Duty & Tax Excise DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXCDTY_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax Excise DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXCDTY_Override).GroupName.Caption);
				AssertEquals("Duty & Tax Excise DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXCDTY_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax Excise DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXCDTY_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax Classification DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CLSDTY_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax Classification DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CLSDTY_Code).GroupName.Caption);
				AssertEquals("Duty & Tax Classification DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CLSDTY_Description).GroupName.Caption);
				AssertEquals("Duty & Tax Classification DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CLSDTY_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax Classification DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CLSDTY_Override).GroupName.Caption);
				AssertEquals("Duty & Tax Classification DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CLSDTY_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax Classification DTY", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CLSDTY_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax CVD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CVD_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax CVD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CVD_Code).GroupName.Caption);
				AssertEquals("Duty & Tax CVD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CVD_Description).GroupName.Caption);
				AssertEquals("Duty & Tax CVD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CVD_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax CVD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CVD_Override).GroupName.Caption);
				AssertEquals("Duty & Tax CVD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CVD_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax CVD", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_CVD_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax EXS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXS_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax EXS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXS_Code).GroupName.Caption);
				AssertEquals("Duty & Tax EXS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXS_Description).GroupName.Caption);
				AssertEquals("Duty & Tax EXS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXS_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax EXS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXS_Override).GroupName.Caption);
				AssertEquals("Duty & Tax EXS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXS_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax EXS", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_EXS_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax GST", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GST_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax GST", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GST_Code).GroupName.Caption);
				AssertEquals("Duty & Tax GST", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GST_Description).GroupName.Caption);
				AssertEquals("Duty & Tax GST", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GST_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax GST", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GST_Override).GroupName.Caption);
				AssertEquals("Duty & Tax GST", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GST_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax GST", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_GST_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax SAF", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SAF_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax SAF", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SAF_Code).GroupName.Caption);
				AssertEquals("Duty & Tax SAF", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SAF_Description).GroupName.Caption);
				AssertEquals("Duty & Tax SAF", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SAF_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax SAF", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SAF_Override).GroupName.Caption);
				AssertEquals("Duty & Tax SAF", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SAF_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax SAF", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SAF_RateType).GroupName.Caption);
				AssertEquals("Duty & Tax SUR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SUR_Amount).GroupName.Caption);
				AssertEquals("Duty & Tax SUR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SUR_Code).GroupName.Caption);
				AssertEquals("Duty & Tax SUR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SUR_Description).GroupName.Caption);
				AssertEquals("Duty & Tax SUR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SUR_ExemptCode).GroupName.Caption);
				AssertEquals("Duty & Tax SUR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SUR_Override).GroupName.Caption);
				AssertEquals("Duty & Tax SUR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SUR_Rate).GroupName.Caption);
				AssertEquals("Duty & Tax SUR", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.CA_SUR_RateType).GroupName.Caption);
			}
		}

		public void TestHidePGATabsForNoneIIDDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var pgaRequirement = invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.PHAC);

			pgaRequirement.ProgramCodeRequirements[0].Indicator = "Y";

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var tabControl = brokerageControl.InvoiceLinesUserControl.LineDetailTabControl;

				Assert(!tabControl.TabPages.ToList<ZTabPage>().Any(t => t.Text == PGACodes.Codes.PHAC));

				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				Assert("PGA tabs would be only show for IID declaration", tabControl.TabPages.ToList<ZTabPage>().Any(t => t.Text == PGACodes.Codes.PHAC));
			}
		}

		public void TestUnHidePGATabs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).JI_Tariff = "9401611090";
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			using (var form = new JobDeclarationForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var tabControl = brokerageControl.InvoiceLinesUserControl.LineDetailTabControl;
				Assert(tabControl.TabPages.ToList<ZTabPage>().Any(t => t.Text == "PGA Requirements"));
			}
		}

		public void TestHidePGATabsForNotIID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).JI_Tariff = "9401611090";
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			using (var form = new JobDeclarationForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var tabControl = brokerageControl.InvoiceLinesUserControl.LineDetailTabControl;
				Assert(!tabControl.TabPages.ToList<ZTabPage>().Any(t => t.Text == "PGA Requirements"));
			}
		}

		public void TestGridLayoutForWarehouseEntry()
		{
			using (var control = new CAImportInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var helper = new WhsDataTestHelper(Factory);
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				declaration.JE_OH_Importer = helper.Importer.PK;
				declaration.InvoiceLines.AddNew();

				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
					var gridColumns = from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles select info;
					AssertWarehouseColumnsAvailablity(gridColumns, false);

					declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
					AssertWarehouseColumnsAvailablity(gridColumns, false);

					declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
					AssertWarehouseColumnsAvailablity(gridColumns, false);

					declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
					AssertWarehouseColumnsAvailablity(gridColumns, false);

					declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
					AssertWarehouseColumnsAvailablity(gridColumns, true);

					var entry = declaration.ActiveEntryHeaders.AddNew();
					entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

					declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
					AssertWarehouseColumnsAvailablity(gridColumns, true);

					entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

					declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
					AssertWarehouseColumnsAvailablity(gridColumns, true);

					entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

					declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
					AssertWarehouseColumnsAvailablity(gridColumns, true);

					declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
					AssertWarehouseColumnsAvailablity(gridColumns, true);
				}
			}
		}

		public void TestGridLayoutForOGDStatus()
		{
			using (var control = new CAImportInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;

				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
					var gridColumns = from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles select info;
					Assert("CA_OGDStatus", !gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_OGDStatus && x.Caption == "OGD Status").IsUnavailable);
					Assert("CA_OGDStatusDescription", !gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_OGDStatusDescription && x.Caption == "OGD Status Description").IsUnavailable);

					declaration.CA_ServiceOption = ServiceOptions.Codes.IID;
					Assert("CA_OGDStatus", gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_OGDStatus && x.Caption == "OGD Status").IsUnavailable);
					Assert("CA_OGDStatusDescription", gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_OGDStatusDescription && x.Caption == "OGD Status Description").IsUnavailable);

					declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
					Assert("CA_OGDStatus", !gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_OGDStatus && x.Caption == "PGA Status").IsUnavailable);
					Assert("CA_OGDStatusDescription", !gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.CA_OGDStatusDescription && x.Caption == "PGA Status Description").IsUnavailable);
				}
			}
		}

		public void TestSetVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ZString.Empty;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = (CAImportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl;
				var lineDetailTabControl = invoiceLinesUserControl.Controls.Find("LineDetailTabControl", true)[0] as ZTemplateTabControl;
				AssertNull(lineDetailTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(t => t.Text == "Packages"));

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				invoiceLinesUserControl = (CAImportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl;
				lineDetailTabControl = invoiceLinesUserControl.Controls.Find("LineDetailTabControl", true)[0] as ZTemplateTabControl;
				AssertNotNull(lineDetailTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(t => t.Text == "Packages"));
			}
		}

		public void TestSerialNumberColumn()
		{
			using (var testForm = new ZForm())
			{
				var control = new CAImportInvoiceLineUserControl();
				testForm.Controls.Add(control);
				testForm.Show();
				var invoiceLines = testForm.FindSingle<CAImportInvoiceLineUserControl>("CAImportInvoiceLineUserControl");
				var column = invoiceLines.CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_SerialNumber");
				AssertNotNull("Column should exist", column);
				Assert("Column should be visible", column.IsVisible);
				var dropColumn = column as ZDropEditColumnStyleInfo;
				AssertNotNull("Column should be DropEdit", dropColumn);
				AssertEquals("Column should only show code in dropdown", ZDropEdit.ShowInDropDownList.OnlyShowCode, dropColumn.ShowInDropDown);
				AssertEquals("Column should be upper casing", CharacterCasing.Upper, dropColumn.CharacterCasing);
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);

			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);

			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					var subcriberTarget = subcriber.Value.Target;
					if (subcriberTarget is DeclarationValueChangedAnnouncer)
					{
						var onValueChangedField = subcriberTarget.GetType().BaseType.GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
						var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
						var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is CAImportInvoiceLineUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is CAImportInvoiceLineUserControl));
					}
				}
			}
		}

		void SetChildProgramTicked(JobComInvoiceLine invoiceLine, string pgaCode)
		{
			var requirment = invoiceLine.PGARequirements.OfType<PGARequirement>().First(x => x.AgencyCode == pgaCode);
			var programs = requirment.ProgramCodeRequirements.Cast<PGAProgramRequirement>();

			foreach (var program in programs)
			{
				program.DeclareYes = true;
			}
		}

		static void AssertInvoiceLineDefaultColumnsSequenceForIM2(JobDeclaration declaration, IEnumerable<string> expectedColumnList)
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (IM2CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var gridColumns = from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles select info;

				foreach (var expectedColumn in expectedColumnList)
				{
					var previous = gridColumns.First(x => x.ColumnName == expectedColumn);
					AssertNotNull("Has expected column", previous);
				}
			}
		}

		static void AssertInvoiceLineDefaultColumnsSequenceForIMPAndIIDJob(JobDeclaration declaration, IEnumerable<string> expectedColumnList)
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var gridColumns = from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles select info;

				foreach (var expectedColumn in expectedColumnList)
				{
					var previous = gridColumns.First(x => x.ColumnName == expectedColumn);
					AssertNotNull("Has expected column", previous);
				}
			}
		}

		static void AssertInvoiceLineDefaultColumnsSequence(JobDeclaration declaration, IEnumerable<string> expectedColumnList)
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var gridColumns = from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles select info;

				var columnCount = expectedColumnList.Count();
				for (var i = 0; i < columnCount; i++)
				{
					AssertEquals("Name", expectedColumnList.ElementAt(i), gridColumns.ElementAt(i).ColumnName);
					AssertEquals("IsVisible", true, gridColumns.ElementAt(i).IsVisible);
				}
			}
		}

		void AssertWarehouseColumnsAvailablity(IEnumerable<ZGridColumnInfo> gridColumns, bool whsColumnsAvailablity)
		{
			AssertEquals("JI_PreviousEntryNumber", whsColumnsAvailablity, !gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.JI_PreviousEntryNumber).IsUnavailable);
			AssertEquals("JI_PreviousEntryLineNumber", whsColumnsAvailablity, !gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber).IsUnavailable);
		}
	}
}
