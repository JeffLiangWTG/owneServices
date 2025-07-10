using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineUserControl))]
sealed class ExportInvoiceLineUserControlTest : CommonInvoiceLineUserControlTest<ExportInvoiceLineUserControlForTest>
{
	public void TestNullEntryInstructionOnInvoiceLineThrowsNoException()
	{
		using (var form = new ZForm(declaration))
		using (var userControl = new ExportInvoiceLineUserControlForTest())
		{
			userControl.JobDeclaration = declaration;
			form.Controls.Add(userControl);
			form.Show();

			AssertNoExceptionThrown("No exception expected", () => invoiceLine.JI_CEI = ZGuid.Empty);
		}
	}

	public void TestOriginStateColumnIsVisible()
	{
		using (var form = new ZForm(declaration))
		using (var userControl = new ExportInvoiceLineUserControlForTest())
		{
			form.Controls.Add(userControl);
			InitControlGrid(userControl);

			var customsInvoiceLinesBoundGrid = userControl.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");

			var originStateColumnStyle = customsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin);

			AssertNotNull("JI_StateOrRegionOfOrigin", originStateColumnStyle);
			AssertEquals("JI_StateOrRegionOfOrigin.IsVisible", true, originStateColumnStyle.IsVisible);
		}
	}

	public void TestGetDefaultColumnsForGrid()
	{
		using (var form = new ZForm(declaration))
		using (var userControl = new ExportInvoiceLineUserControlForTest())
		{
			userControl.JobDeclaration = declaration;
			form.Controls.Add(userControl);
			form.Show();

			var defaultColumnsForGridExposed = userControl.GetDefaultColumnsForGridExposed().ToList();
			AssertCollectionContains("GetDefaultColumnsForGrid result", JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin, defaultColumnsForGridExposed);

			var countryOfOriginColumnIndex = defaultColumnsForGridExposed.IndexOf(JobComInvoiceLine.Schema.JI_CountryOfOrigin);
			AssertNotEquals("[PRE-REQUISITE] Country of origin Column index", -1, countryOfOriginColumnIndex);

			var originStateColumnIndex = defaultColumnsForGridExposed.IndexOf(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin);
			AssertEquals("OriginState column should be next to CountryOfOrigin column, OriginState column index", ++countryOfOriginColumnIndex, originStateColumnIndex);
		}
	}

	public void TestSupportingDocumentsUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new ExportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			control.Controls.Find("SupportingDocumentsTabPage", true).First().Show();
			var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(InvoiceLineLayoutSupportingDocumentsUserControl), supportingDocument.UserControlType);
		}
	}

	public void Test_PrincipalsRepresentativeGroupBox_Visibility()
	{
		using (var form = new ZForm())
		using (var control = new ExportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var principalsRepresentativeGroupBox = control.FindSingle<ZGroupBox>("PrincipalsRepresentativeGroupBox");
			AssertEquals("For ExportInvoiceLineUserControl, PrincipalsRepresentativeGroupBox visibility", false, principalsRepresentativeGroupBox.Visible);
		}
	}

	public void TestJI_CustomsQuantityCalcDropEditDecimalPlaces()
	{
		using (var control = new ExportInvoiceLineUserControl())
		{
			var customsQuantityCalcDropEdit = control.FindSingle<ZCalcDropEdit>("JI_CustomsQuantityCalcDropEdit");
			AssertEquals("JI_CustomsQuantityCalcDropEdit Decimal places for IT", 5, customsQuantityCalcDropEdit.Decimals);
		}
	}

	public void TestRemarksTabPage_Visible()
	{
		using (var control = GetNewInvoiceLineUserControl())
		{
			control.SetDataBinding(declaration, "");
			control.JobDeclaration = declaration;

			declaration.JE_MessageType = "EXP";
			declaration.MessageVersion = "TXT";
			var remarksTabPage = GetRemarksTabPage();
			AssertEquals("RemarksTabPage, Visibility for EXP UCC6=false", true, remarksTabPage.TabVisible);

			declaration.MessageVersion = "XML";
			remarksTabPage = GetRemarksTabPage();
			AssertNull("RemarksTabPage, for EXP UCC6=true", remarksTabPage);

			ZTabPage GetRemarksTabPage() => control.FindSingleOrDefault<ZTabPage>("RemarksTabPage");
		}
	}

	public void TestRemarksTabPageTitleLabel()
	{
		using (var control = GetNewInvoiceLineUserControl())
		{
			var remarksTabPage = control.FindSingleOrDefault<ZTabPage>("RemarksTabPage");
			AssertNotNull("RemarksTabPage should be visible in IT.", remarksTabPage);
			AssertEquals("Remarks Tab caption", "[44] Remarks", remarksTabPage.CaptionResourceString.Caption);
		}
	}

	public void TestRemarksTabpageIndex()
	{
		using (var userControl = GetNewInvoiceLineUserControl())
		{
			var remarksTabPage = userControl.FindSingleOrDefault<ZTabPage>("RemarksTabPage");
			AssertNotNull(remarksTabPage);

			var additionalInfosTabPage = userControl.FindSingleOrDefault<ZTabPage>("AdditionalInfosTabPage");
			AssertNotNull(additionalInfosTabPage);

			var lineDetailTabControl = userControl.FindSingleOrDefault<ZTemplateTabControl>("LineDetailTabControl");
			AssertNotNull(lineDetailTabControl);

			int indexOfAdditionalInfoTabPage = lineDetailTabControl.TabPages.IndexOf(additionalInfosTabPage);

			AssertEquals("Remarks tab position should be after AdditionalInfos page in Details Tab control", indexOfAdditionalInfoTabPage + 1, lineDetailTabControl.TabPages.IndexOf(remarksTabPage));
		}
	}

	public void TestAdditionalInfoTabIndex()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			control.SetDataBinding(declaration, null);
			form.Controls.Add(control);
			form.Show();
			var (additionalInfoTabPageIndex, previousDocumentTabPageIndex) = GetTagPageIndexes(control);
			AssertEquals("[UCC6] Tab Index", previousDocumentTabPageIndex + 1, additionalInfoTabPageIndex);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			control.SetDataBinding(declaration, null);
			form.Controls.Add(control);
			form.Show();

			var (additionalInfoTabPageIndex, previousDocumentTabPageIndex) = GetTagPageIndexes(control);
			Assert("[Non-UCC6] AdditionalInfoTab is before PrevDocuments Tab", previousDocumentTabPageIndex > additionalInfoTabPageIndex);
		}

		(int, int) GetTagPageIndexes(ExportInvoiceLineUserControlForTest control)
		{
			var lineDetailTabControl = control.FindSingleOrDefault<ZTemplateTabControl>("LineDetailTabControl");
			AssertNotNull(lineDetailTabControl);

			var additionalInfosTabPage = control.FindSingleOrDefault<ZTabPage>("AdditionalInfosTabPage");
			AssertNotNull(additionalInfosTabPage);

			var previousDocumentsTabPage = control.FindSingleOrDefault<ZTabPage>("PreviousDocumentsTabPage");
			AssertNotNull(previousDocumentsTabPage);

			var additionalInfoTabPageIndex = lineDetailTabControl.TabPages.IndexOf(additionalInfosTabPage);
			var previousDocumentTabPageIndex = lineDetailTabControl.TabPages.IndexOf(previousDocumentsTabPage);
			return (additionalInfoTabPageIndex, previousDocumentTabPageIndex);
		}
	}

	public void TestAdditionalInfosTabPageCaptionAndUserControlType()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(Ucc6ExportInvLineAddInfoUserControlWithGrid));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControl));
		}
	}

	public void TestPreviousDocumentsTabPageCaptionAndUserControlType()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(declaration, "PreviousDocumentsTabPage", "PreviousDocumentsUserControl", "Previous Documents", typeof(LayoutUcc6ExportPreviousDocumentsUserControl));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(declaration, "PreviousDocumentsTabPage", "PreviousDocumentsUserControl", "[40] Previous Documents", typeof(PreviousDocumentsUserControl));
		}
	}

	public void TestProductCodeControlTextLength()
	{
		using (var control = new ExportInvoiceLineUserControl())
		{
			var lineDetailTabControl = control.FindSingleOrDefault<ZTemplateTabControl>("LineDetailTabControl");
			var box = lineDetailTabControl.FindSingle<ZCodeFindBox>("PartNoCodeFindBox");
			AssertEquals(35, box.PreBoundMaxLength);
		}
	}

	public void TestColumn_JI_RN_NKCountryOfExport() => ValidateColumnForUCC6("JI_RN_NKCountryOfExport");

	public void TestColumn_ZG_PortTaxRate() => ValidateColumnForUCC6("ZG_PortTaxRate");

	public void TestColumn_JI_CustomsThirdQuantity() => ValidateColumnForUCC6("JI_CustomsThirdQuantity", (colInfo) =>
	{
		AssertEquals("Group name caption", "Third Qty", colInfo.GroupName.Caption);
	});

	public void TestColumn_JI_CustomsThirdUnitQty() => ValidateColumnForUCC6("JI_CustomsThirdUnitQty", (colInfo) =>
	{
		AssertEquals("Group name caption", "Third Qty", colInfo.GroupName.Caption);
	});

	public void TestCustomsInvoiceLinesBoundGridOrderForUCC6()
	{
		var testCases = new List<string>
		{
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.JI_CEI,
			JobComInvoiceLine.Schema.EntryInstructionDescription,
			JobComInvoiceLine.Schema.JI_FormattedProcedure,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
			JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
			AddInfoJobComInvoiceLine.Schema.ZG_CountryOfDestination,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_FormattedTariff,
			JobComInvoiceLine.Schema.JI_Description,
			JobComInvoiceLine.Schema.JI_SupplementaryCode1,
			JobComInvoiceLine.Schema.JI_SupplementaryCode2,
			AddInfoJobComInvoiceLine.Schema.ZG_PortTaxRate,
			AddInfoJobComInvoiceLine.Schema.ZG_CusNumber,
			JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
			JobComInvoiceLine.Schema.JI_LinePrice,
			JobComInvoiceLine.Schema.JI_Weight,
			JobComInvoiceLine.Schema.JI_WeightUQ,
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
			JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
			JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
			JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
			JobComInvoiceLine.Schema.JI_Volume,
			JobComInvoiceLine.Schema.JI_VolumeUQ,
			JobComInvoiceLine.Schema.JI_BondedWhsQuantity,
			JobComInvoiceLine.Schema.JI_BondedWhsUnitQty,
			JobComInvoiceLine.Schema.JI_PreviousEntryNumber,
			JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
			JobComInvoiceLine.Schema.EntryReferenceNumber,
			JobComInvoiceLine.Schema.MergedLineNumber,
		};

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			InitControlGrid(control);

			var invLinesGrid = control.CustomsInvoiceLinesBoundGrid;
			AssertNotNull(nameof(invLinesGrid), invLinesGrid);

			var columnStyleList = invLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
			var actualColumnOrder = string.Join(", ", columnStyleList.Where(x => x.IsVisible).Select(x => x.ColumnName));
			var expectedColumnOrder = string.Join(", ", testCases);
			AssertEquals("Column Order", expectedColumnOrder, actualColumnOrder);
		}
	}

	void InitControlGrid(ExportInvoiceLineUserControlForTest control)
	{
		control.JobDeclaration = declaration;
		control.InitializeGridLayout();
		control.Show();
	}

	void ValidateColumnForUCC6(string columnName, Action<ZGridColumnInfo> assertWhenUCC6IsOn = null)
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			InitControlGrid(control);

			CombineAssertions($"UCC6 on for {columnName}", () =>
			{
				var invLinesGrid = control.CustomsInvoiceLinesBoundGrid;
				AssertNotNull(nameof(invLinesGrid), invLinesGrid);

				var colInfo = invLinesGrid.GetColumnStyle(columnName);
				AssertNotNull(columnName, colInfo);

				if (assertWhenUCC6IsOn is not null)
				{
					assertWhenUCC6IsOn(colInfo);
				}
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			InitControlGrid(control);

			CombineAssertions($"UCC6 off for {columnName}", () =>
			{
				var invLinesGrid = control.CustomsInvoiceLinesBoundGrid;
				AssertNotNull(nameof(invLinesGrid), invLinesGrid);
				AssertNull(columnName, invLinesGrid.GetColumnStyle(columnName));
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarantType = EUJobMessageTypeList.Codes.Export;
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
	}

	protected override Type InvoiceLineDetailsPanelLayoutType => typeof(ExportInvoiceLineDetailsLayout);

	protected override ExportInvoiceLineUserControlForTest GetNewInvoiceLineUserControl() => new ExportInvoiceLineUserControlForTest();

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryInstruction entryInstruction;
}

sealed class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl, IHasTaxTabPageExposedForTest, IHasPanelLayoutMembersExposedForTest
{
	public ExportInvoiceLineUserControlForTest()
	{
	}

	public ZTabPage TaxTabPageExposed => TaxTabPage;
	public ZTabPage PreviousDocumentsTabPageExposed => PreviousDocumentsTabPage;
	public string[] GetDefaultColumnsForGridExposed() => base.GetDefaultColumnsForGrid();

	ZBool IHasPanelLayoutMembersExposedForTest.DynamicLayoutAppliedExposed => DynamicLayoutApplied;
	ZBool IHasPanelLayoutMembersExposedForTest.HasDifferentPanelLayoutExposed => HasDifferentPanelLayout;
	IPanelLayoutProvider IHasPanelLayoutMembersExposedForTest.GetNewInvoiceLineDetailsPanelLayoutExposed() => GetNewInvoiceLineDetailsPanelLayout();
}
