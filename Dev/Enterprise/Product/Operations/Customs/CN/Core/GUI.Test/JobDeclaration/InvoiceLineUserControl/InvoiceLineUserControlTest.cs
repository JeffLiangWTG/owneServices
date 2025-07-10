using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.GUI.Testing
{
	public class InvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceLineChargesUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindInvoiceLineUserControl();
				AssertType(typeof(InvoiceLineChargesUserControl), control.InvoiceLineCharges);
			}
		}

		public void TestTariffFindBoxAndRelatedGirdColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindInvoiceLineUserControl();
				var tariffControl = (Universal.GUI.TariffFindBox)control?.Controls.Find("JI_TariffFindBox", true).First();
				AssertNotNull("JI_TariffFindBox", tariffControl);
				AssertEquals("Tariff box country code", Enterprise.Core.Constants.CountryCodes.China, tariffControl.GetCountryCode());
				AssertEquals("Tariff box data grouping", Enterprise.Core.Constants.CountryCodes.China, tariffControl.GetDataGrouping());
				AssertEquals("Tariff box tariff type", Universal.Constants.TariffTypes.HarmonizedSystem, tariffControl.TariffType);
				AssertEquals("Min desc length: ", 2, tariffControl.PartialDescriptionMinLengthForSearch);
				var columnStyle = (Universal.GUI.TariffColumnStyleInfo)control?.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Tariff);
				AssertNotNull("Should have column JI_Tariff", columnStyle);
				AssertEquals("Tariff box country code", Core.Constants.CountryCodes.China, columnStyle.GetCountryCode());
				AssertEquals("Tariff box data grouping", Core.Constants.CountryCodes.China, columnStyle.GetDataGrouping());
				AssertEquals("Tariff box tariff type", Universal.Constants.TariffTypes.HarmonizedSystem, columnStyle.GetTariffType());
				AssertEquals("Min desc length: ", 2, columnStyle.PartialDescriptionMinLengthForSearch);
				Assert("Show Description Filter as Default", tariffControl.ShowDescriptionFilterOnNonNomenclatureTariffModule);
				Assert("Show Description Filter as Default", columnStyle.ShowDescriptionFilterOnNonNomenclatureTariffModule);
			}
		}

		public void TestAdditionalTabOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var control = form.FindInvoiceLineUserControl();
			AssertContainsExactElementsInExactOrder(new[]
			{
					"LineDetailsTabPage",
					"CusSupportingDocumentsTabPage",
					"CIQTabPage",
					"AttachmentsTabPage",
					"CIQProductQualificationsTabPage",
					"LineChargesTabPage",
					"CustomFieldsTabPage"
				}, control.LineDetailTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name).ToList());
		}

		public void TestEntryInstructionColumnVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindInvoiceLineUserControl();
				var columnStyle = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_CEI");
				AssertNotNull("Should have column Entry Instruction", columnStyle);
				AssertEquals("Column Entry Instruction should be available in Interfaced", false, columnStyle.IsUnavailable);
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Column Entry Instruction should be available in Builtin", false, columnStyle.IsUnavailable);
			}
		}

		public void TestSerialNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindInvoiceLineUserControl();
				var columnStyle = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_SerialNumber);
				AssertNotNull("Serial Number column should exist", columnStyle);
				Assert("Serial Number column should not be visible by default", !columnStyle.IsVisible);
			}
		}

		public void TestControlsExistence()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindInvoiceLineUserControl();
				CombineAssertions(() =>
				{
					TestUtility.AssertControlExistance(control, "JI_TariffFindBox", "FilteredInvoiceLines.JI_Tariff");
					TestUtility.AssertControlExistance(control, "DestDistrictCodeFindBox", "FilteredInvoiceLines.JI_DestinationDistrict");
					TestUtility.AssertControlExistance(control, "OriginDestrictCodeFindBox", "FilteredInvoiceLines.JI_OriginDistrict");
					TestUtility.AssertControlExistance(control, "DestRegionCodeFindBox", "FilteredInvoiceLines.JI_DestinationRegion");
					TestUtility.AssertControlExistance(control, "OriginRegionCodeFindBox", "FilteredInvoiceLines.JI_OriginRegion");
					TestUtility.AssertControlExistance(control, "EntryInstructionDropEdit", "FilteredInvoiceLines.JI_CEI");
					TestUtility.AssertControlExistance(control, "ProductCodeFindBox", "FilteredInvoiceLines.JI_PartNo");
					TestUtility.AssertControlExistance(control, "CIQInvoiceLineDetailsUserControl", "FilteredInvoiceLines");
					TestUtility.AssertControlExistance(control, "CIQProductQuantificationsUserControl", "FilteredInvoiceLines");
					TestUtility.AssertControlExistance(control, "FinalDestinationCodeFindBox", "FilteredInvoiceLines.JI_RN_NKCountryOfExport");
					TestUtility.AssertControlExistance(control, "DutyModeDropEdit", "FilteredInvoiceLines.JI_DutyMode");
					TestUtility.AssertControlExistance(control, "PrimaryPreferenceDropEdit", "FilteredInvoiceLines.JI_PrimaryPreference");
					TestUtility.AssertControlExistance(control, "ManualItemNoCalcEdit", "FilteredInvoiceLines.JI_ProductManualNo");
					TestUtility.AssertControlExistance(control, "ProductVersionTextBox", "FilteredInvoiceLines.JI_ProductVersion");
					TestUtility.AssertControlExistance(control, "XC_GoodsSpecModel2TextBox", "FilteredInvoiceLines.XC_GoodsSpecModel2");
					TestUtility.AssertControlExistance(control, "JI_NameOfGoods2TextBox", "FilteredInvoiceLines.JI_NameOfGoods2");
					TestUtility.AssertControlExistance(control, "ChildInstructionManualNoTextBox", "FilteredInvoiceLines.EntryInstruction.ChildInstruction.CEI_ManualNo");
					TestUtility.AssertControlExistance(control, "ManualItemNo2CalcEdit", "FilteredInvoiceLines.JI_ProductManualNo2");
					TestUtility.AssertControlExistance(control, "ChildInstructionDropEdit", "FilteredInvoiceLines.EntryInstruction.ChildInstructionPK");
					TestUtility.AssertControlExistance(control, "XC_GoodsSpecModelTextBox", "FilteredInvoiceLines.XC_GoodsSpecModel");
					TestUtility.AssertControlExistance(control, "JI_NameOfGoodsTextBox", "FilteredInvoiceLines.JI_NameOfGoods");
					TestUtility.AssertControlExistance(control, "EntryInstructionManualNoTextBox", "FilteredInvoiceLines.EntryInstruction.CEI_ManualNo");
					TestUtility.AssertControlExistance(control, "JI_StateOrRegionOfOriginDropEdit", "FilteredInvoiceLines.JI_StateOrRegionOfOrigin");
					TestUtility.AssertControlExistance(control, "JI_CIQOriginStateCodeFindBox", "FilteredInvoiceLines.JI_CIQOriginState");
					TestUtility.AssertControlExistance(control, "CustomsUnitQtyDescTextBox", "FilteredInvoiceLines.JI_CustomsUnitQtyDescription");
					TestUtility.AssertControlExistance(control, "CustomsSecondUnitQtyTextBox", "FilteredInvoiceLines.JI_CustomsSecondUnitQtyDescription");
					TestUtility.AssertControlExistance(control, "CusSupportingDocumentsUserControl", "FilteredInvoiceLines");
					TestUtility.AssertControlExistance(control, "DutyRateTextBox", "FilteredInvoiceLines.UniversalDutyRateFormula");
					TestUtility.AssertControlExistance(control, "CertificateOfOriginCountryCodeFindBox", "FilteredInvoiceLines.CertificateOfOriginCountry");
					TestUtility.AssertControlExistance(control, "CertificateOfOriginTextBox", "FilteredInvoiceLines.CertificateOfOrigin");
					TestUtility.AssertControlExistance(control, "CertificateOfOriginTypeDropEdit", "FilteredInvoiceLines.CertificateOfOriginType");
					TestUtility.AssertControlExistance(control, "TradeAgreementCodeDropEdit", "FilteredInvoiceLines.TradeAgreementCode");
					TestUtility.AssertControlExistance(control, "ItemNoOnCertOfOriginCalcEdit", "FilteredInvoiceLines.ItemNoOnCertOfOrigin");
					TestUtility.AssertControlExistance(control, "JI_TradeUnitQtyDescriptionTextBox", "FilteredInvoiceLines.JI_TradeUnitQtyDescription");
				});
			}
		}

		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				var control = form.FindInvoiceLineUserControl();
				control.LineDetailTabControl.SelectedIndex = 0;
				CombineAssertions(() =>
				{
					var destDistrictCodeFindBox = control.Controls.Find("DestDistrictCodeFindBox", true).First();
					var destRegionCodeFindBox = control.Controls.Find("DestRegionCodeFindBox", true).First();
					var originDestrictCodeFindBox = control.Controls.Find("OriginDestrictCodeFindBox", true).First();
					var originRegionCodeFindBox = control.Controls.Find("OriginRegionCodeFindBox", true).First();
					var originStateCodeFindBox = control.Controls.Find("JI_StateOrRegionOfOriginDropEdit", true).First();
					var ciqOriginStateCodeFindBox = control.Controls.Find("JI_CIQOriginStateCodeFindBox", true).First();
					var primaryPreferenceDropEdit = control.Controls.Find("PrimaryPreferenceDropEdit", true).First();
					Assert("EXP+CUS DestDistrictCodeFindBox", !destDistrictCodeFindBox.Visible);
					Assert("EXP+CUS DestRegionCodeFindBox", !destRegionCodeFindBox.Visible);
					Assert("EXP+CUS OriginDestrictCodeFindBox", originDestrictCodeFindBox.Visible);
					Assert("EXP+CUS OriginRegionCodeFindBox", originRegionCodeFindBox.Visible);
					Assert("EXP+CUS JI_StateOrRegionOfOriginDropEdit", !originStateCodeFindBox.Visible);
					Assert("EXP+CUS JI_CIQOriginStateCodeFindBox", !ciqOriginStateCodeFindBox.Visible);
					Assert("EXP+CUS PrimaryPreferenceDropEdit", !primaryPreferenceDropEdit.Visible);
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
					declaration.JE_MessageSubType = DecTypeList.Codes.Both;
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
					Assert("EXP+BTH DestDistrictCodeFindBox", destDistrictCodeFindBox.Visible);
					Assert("EXP+BTH DestRegionCodeFindBox", destRegionCodeFindBox.Visible);
					Assert("EXP+BTH OriginDestrictCodeFindBox", originDestrictCodeFindBox.Visible);
					Assert("EXP+BTH OriginRegionCodeFindBox", originRegionCodeFindBox.Visible);
					Assert("EXP+BTH JI_StateOrRegionOfOriginDropEdit", originStateCodeFindBox.Visible);
					Assert("EXP+BTH JI_CIQOriginStateCodeFindBox", ciqOriginStateCodeFindBox.Visible);
					Assert("EXP+BTH PrimaryPreferenceDropEdit", !primaryPreferenceDropEdit.Visible);
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
					control = form.FindInvoiceLineUserControl();
					destDistrictCodeFindBox = control.Controls.Find("DestDistrictCodeFindBox", true).First();
					destRegionCodeFindBox = control.Controls.Find("DestRegionCodeFindBox", true).First();
					originDestrictCodeFindBox = control.Controls.Find("OriginDestrictCodeFindBox", true).First();
					originRegionCodeFindBox = control.Controls.Find("OriginRegionCodeFindBox", true).First();
					originStateCodeFindBox = control.Controls.Find("JI_StateOrRegionOfOriginDropEdit", true).First();
					ciqOriginStateCodeFindBox = control.Controls.Find("JI_CIQOriginStateCodeFindBox", true).First();
					primaryPreferenceDropEdit = control.Controls.Find("PrimaryPreferenceDropEdit", true).First();
					Assert("IMP+CUS DestDistrictCodeFindBox", destDistrictCodeFindBox.Visible);
					Assert("IMP+CUS DestRegionCodeFindBox", destRegionCodeFindBox.Visible);
					Assert("IMP+CUS OriginDestrictCodeFindBox", !originDestrictCodeFindBox.Visible);
					Assert("IMP+CUS OriginRegionCodeFindBox", !originRegionCodeFindBox.Visible);
					Assert("IMP+CUS JI_StateOrRegionOfOriginDropEdit", originStateCodeFindBox.Visible);
					Assert("IMP+CUS JI_CIQOriginStateCodeFindBox", ciqOriginStateCodeFindBox.Visible);
					Assert("IMP+CUS PrimaryPreferenceDropEdit", primaryPreferenceDropEdit.Visible);
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
					declaration.JE_MessageSubType = DecTypeList.Codes.Both;
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
					Assert("IMP+BTH DestDistrictCodeFindBox", destDistrictCodeFindBox.Visible);
					Assert("IMP+BTH DestRegionCodeFindBox", destRegionCodeFindBox.Visible);
					Assert("IMP+BTH OriginDestrictCodeFindBox", originDestrictCodeFindBox.Visible);
					Assert("IMP+BTH OriginRegionCodeFindBox", originRegionCodeFindBox.Visible);
					Assert("IMP+BTH JI_StateOrRegionOfOriginDropEdit", originStateCodeFindBox.Visible);
					Assert("IMP+BTH JI_CIQOriginStateCodeFindBox", ciqOriginStateCodeFindBox.Visible);
					Assert("IMP+BTH PrimaryPreferenceDropEdit", primaryPreferenceDropEdit.Visible);
				}

				);
			}

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var fakeDeclaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new JobDeclarationForm(fakeDeclaration))
			{
				form.Show();
				var invoiceLineControls = form.FindInvoiceLineUserControl().Controls;
				var entryInstructionDropEdit = invoiceLineControls.Find("EntryInstructionDropEdit", true).First();
				var entryInstructionManualNoTextBox = invoiceLineControls.Find("EntryInstructionManualNoTextBox", true).First();
				var manualItemNoCalcEdit = invoiceLineControls.Find("ManualItemNoCalcEdit", true).First();
				var childInstructionDropEdit = invoiceLineControls.Find("ChildInstructionDropEdit", true).First();
				var childInstructionManualNoTextBox = invoiceLineControls.Find("ChildInstructionManualNoTextBox", true).First();
				var manualItemNo2CalcEdit = invoiceLineControls.Find("ManualItemNo2CalcEdit", true).First();
				AssertEquals(false, entryInstructionDropEdit.Visible);
				AssertEquals(false, entryInstructionManualNoTextBox.Visible);
				AssertEquals(false, manualItemNoCalcEdit.Visible);
				AssertEquals(false, childInstructionDropEdit.Visible);
				AssertEquals(false, childInstructionManualNoTextBox.Visible);
				AssertEquals(false, manualItemNo2CalcEdit.Visible);
			}
		}

		public void TestCertificateOfOriginControlsVisibility()
		{
			var certificateOfOriginControlNames = new List<string>
			{
				"CertificateOfOriginCountryCodeFindBox",
				"CertificateOfOriginTextBox",
				"CertificateOfOriginTypeDropEdit",
				"TradeAgreementCodeDropEdit",
				"ItemNoOnCertOfOriginCalcEdit"
			};
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var invoiceLineControl = form.FindInvoiceLineUserControl();
				invoiceLineControl.LineDetailTabControl.SelectedIndex = 0;
				foreach (var controlName in certificateOfOriginControlNames)
				{
					var control = invoiceLineControl.Controls.Find(controlName, true).First();
					AssertEquals("IsCertificateOfOriginApplicable false, " + controlName + " should be invisible.", false, control.Visible);
				}

				invLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
				foreach (var controlName in certificateOfOriginControlNames)
				{
					var control = invoiceLineControl.Controls.Find(controlName, true).First();
					AssertEquals("IsCertificateOfOriginApplicable true, " + controlName + " should be visible.", true, control.Visible);
				}
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var invoiceLineControl = form.FindInvoiceLineUserControl();
				invoiceLineControl.LineDetailTabControl.SelectedIndex = 0;
				foreach (var controlName in certificateOfOriginControlNames)
				{
					var control = invoiceLineControl.Controls.Find(controlName, true).First();
					AssertEquals("IsCertificateOfOriginNeeded true(EXP), " + controlName + " should be visible.", true, control.Visible);
				}
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.FindInvoiceLineUserControl();
				var grid = control.CustomsInvoiceLinesBoundGrid;
				Assert(!grid.GetColumnStyle("CusEntryLine+EntryLRNAndEntryLineNo").IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI_StyleDescription).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI_Description).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ProductManualNo).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NameOfGoods).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ProductVersion).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_DutyMode).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PrimaryPreference).IsUnavailable);
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.TradeAgreementCode).IsUnavailable);
				AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.CertificateOfOrigin).IsUnavailable);
				AssertEquals(true, grid.GetColumnStyle(JobComInvoiceLine.Schema.CertificateOfOrigin).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.CertificateOfOriginType).IsUnavailable);
				AssertEquals(true, grid.GetColumnStyle(JobComInvoiceLine.Schema.CertificateOfOriginType).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.CertificateOfOriginCountry).IsUnavailable);
				AssertEquals(true, grid.GetColumnStyle(JobComInvoiceLine.Schema.CertificateOfOriginCountry).IsVisible);
				AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.ItemNoOnCertOfOrigin).IsUnavailable);
				AssertEquals(true, grid.GetColumnStyle(JobComInvoiceLine.Schema.ItemNoOnCertOfOrigin).IsVisible);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Volume).IsUnavailable);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_VolumeUQ).IsUnavailable);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_OrderNumber).IsUnavailable);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine).IsUnavailable);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CC).IsUnavailable);
			}

			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new JobDeclarationForm(fakeDeclaration))
			{
				var control = form.FindInvoiceLineUserControl();
				var grid = control.CustomsInvoiceLinesBoundGrid;
				Assert(!grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ProductManualNo).IsUnavailable);
				Assert(grid.GetColumnStyle("CusEntryLine+EntryLRNAndEntryLineNo").IsUnavailable);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI).IsUnavailable);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI_StyleDescription).IsUnavailable);
				Assert(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI_Description).IsUnavailable);
			}
		}

		public void TestAdditionalTabsVisibilities()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			Factory.Save();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var invoiceLineUserControl = form.FindInvoiceLineUserControl();
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
				invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 1;
				var cusSupportingDocumentsGrid = invoiceLineUserControl.LineDetailTabControl.Controls.Find("CusSupportingDocumentsGrid", true).FirstOrDefault();
				Assert("CusSupportingDocumentsGrid should be visible", cusSupportingDocumentsGrid.Visible);
				invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 2;
				var ciqControl = invoiceLineUserControl.LineDetailTabControl.Controls.Find("CIQInvoiceLineDetailsUserControl", true).FirstOrDefault() as CIQInvoiceLineDetailsUserControl;
				Assert("CIQInvoiceLineDetailsUserControl should be visible", ciqControl.Visible);
				invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 4;
				var productQuantUserControl = invoiceLineUserControl.LineDetailTabControl.Controls.Find("CIQProductQuantificationsUserControl", true).FirstOrDefault() as CIQProductQuantificationsUserControl;
				Assert("CIQProductQuantificationsUserControl should be visible", productQuantUserControl.Visible);
			}
		}

		public void TestInstructionGroupBoxesCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				CombineAssertions(() =>
				{
					var parentEntryInstructionGroupBox = form.CustomsBrokerageUserControl.InvoiceLinesTabPage.Controls.Find("ParentEntryInstructionGroupBox", true)[0] as ZGroupBox;
					var childEntryInstructionGroupBox = form.CustomsBrokerageUserControl.InvoiceLinesTabPage.Controls.Find("ChildEntryInstructionGroupBox", true)[0] as ZGroupBox;
					AssertEquals("IMP+BTH - Parent", "进口报关单", parentEntryInstructionGroupBox.Text);
					AssertEquals("IMP+BTH - Child", "出境备案清单", childEntryInstructionGroupBox.Text);
					Assert("IMP+BTH - Child", childEntryInstructionGroupBox.Visible);
					declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					AssertEquals("IMP+CUS - Parent", "进口报关单", parentEntryInstructionGroupBox.Text);
					Assert("IMP+CUS - Child", !childEntryInstructionGroupBox.Visible);
					declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
					AssertEquals("IMP+CUS - Parent", "进境备案清单", parentEntryInstructionGroupBox.Text);
					Assert("IMP+CUS - Child", !childEntryInstructionGroupBox.Visible);
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_MessageSubType = DecTypeList.Codes.Both;
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					parentEntryInstructionGroupBox = form.CustomsBrokerageUserControl.InvoiceLinesTabPage.Controls.Find("ParentEntryInstructionGroupBox", true)[0] as ZGroupBox;
					childEntryInstructionGroupBox = form.CustomsBrokerageUserControl.InvoiceLinesTabPage.Controls.Find("ChildEntryInstructionGroupBox", true)[0] as ZGroupBox;
					AssertEquals("EXP+BTH - Parent", "进境备案清单", parentEntryInstructionGroupBox.Text);
					AssertEquals("EXP+BTH - Child", "出口报关单", childEntryInstructionGroupBox.Text);
					Assert("EXP+BTH - Child", childEntryInstructionGroupBox.Visible);
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					AssertEquals("EXP+CUS - Parent", "出口报关单", parentEntryInstructionGroupBox.Text);
					Assert("EXP+CUS - Child", !childEntryInstructionGroupBox.Visible);
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					AssertEquals("EXP+CUS - Parent", "出境备案清单", parentEntryInstructionGroupBox.Text);
					Assert("EXP+CUS - Child", !childEntryInstructionGroupBox.Visible);
				}

				);
			}
		}
	}
}
