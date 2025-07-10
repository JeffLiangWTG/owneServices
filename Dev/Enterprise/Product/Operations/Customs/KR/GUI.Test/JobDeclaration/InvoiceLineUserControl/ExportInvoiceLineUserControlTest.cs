using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestUseUniversalTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var formattedTariffColumn = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.JI_FormattedTariff));
				AssertNotNull("The Tariff column exists", formattedTariffColumn);
				AssertEquals(typeof(TariffColumnStyleInfo), formattedTariffColumn.GetType());

				var columnAsUniversalTariff = (TariffColumnStyleInfo)formattedTariffColumn;
				AssertEquals(Core.Constants.CountryCodes.KoreaSouth, columnAsUniversalTariff.GetDataGrouping());
				AssertEquals(Universal.Constants.TariffTypes.HarmonizedSystem, columnAsUniversalTariff.GetTariffType());
			}
		}

		public void TestIngredientColumn()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					var invoiceLinesGrid = control.CustomsInvoiceLinesBoundGrid;
					var ingredientColumnStyleInfo = (ZMultiLineTextBoxColumnInfo)invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.JI_Ingredient));
					AssertNotNull("IngredientColumnStyleInfo exists", ingredientColumnStyleInfo);
				}
			}
		}

		public void TestInvoiceLines_InvoiceChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.LineChargesTabPage;
					var grid = control.InvoiceLineCharges.ChargesGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ChargeType);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.ChargeCodeDescription);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Amount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_RX_NKCurrency);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ExchangeRate);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Percentage);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsDutiable);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount);
					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsIncludedInITOT).IsVisible);
					AssertNull(grid.GetColumnStyle("J7_IsGSTApplicable"));
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption, "Add to FOB?");
				}
			}
		}

		public void TestInvoiceLines_ApportionedChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.LineChargesTabPage;
					var grid = control.InvoiceLineCharges.ApportionedChargesGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ChargeType);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.ChargeCodeDescription);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Amount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_RX_NKCurrency);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ExchangeRate);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsDutiable);

					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsIncludedInITOT).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_FullOrPartialApportionment).IsVisible);
					AssertNull(grid.GetColumnStyle("J7_IsGSTApplicable"));
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption, "Add to FOB?");
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Caption, "Included In Invoice");
				}
			}
		}

		public void TestInvLines_LineDetailsGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					var grid = control.CustomsInvoiceLinesBoundGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_LineNo);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Calc_Invoice);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PartNo);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_FormattedTariff);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_InvoiceQuantity);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_InvoiceUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.UnitPrice);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_LinePrice);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CustomsQuantity);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Description);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NetWeight);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NetWeightUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NoOfPacks);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PackType);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CountryOfOrigin);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.CertificateOfOriginIssueStatus));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.CriteriaForDeterminingCountryOfOrigin));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_COOLabelLocation));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PrimaryPreference);
					AssertEquals("FTA Type", grid.GetColumnCaption(JobComInvoiceLine.Schema.JI_PrimaryPreference));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Model);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_BrandName);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_Ingredient));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.CusEntryLine) + "+" + nameof(Business.CusEntryLine.CL_LineNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_SequenceNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_PreviousEntryNumber));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CEI);

					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_LotNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.PRA_ReferenceNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.PRA_DateOfIssue)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.PRA_DateOfExpiry)).IsVisible);

					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_SkipManifestReport)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Weight).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_WeightUQ).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Volume).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_VolumeUQ).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_OrderNumber).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.InvoiceHeader) + "+" + JobComInvoiceHeader.Schema.JZ_InvoiceDisplaySequence).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomAttrib1).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomAttrib2).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomAttrib3).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomAttrib4).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomAttrib5).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomAttrib6).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomTextBlob1).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartAttrib1).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartAttrib2).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartAttrib3).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.CustomsUnitPrice)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty).IsVisible);

					AssertNull(grid.GetColumnStyle("JI_ContainerMode"));
					AssertNull(grid.GetColumnStyle("JI_CC"));
				}
			}
		}

		public void TestInvLines_LineDetailsLineCalculations()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = (ExportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					AssertEquals(control.JI_Calc_DutyConvertToLocalCurrencyControl.Visible, false);
					AssertEquals(control.JI_Calc_GSTConvertToLocalCurrencyControl.Visible, false);

					AssertEquals(control.CL_LineNumberCalcEdit.Visible, true);
					AssertEquals(control.KR_SequenceNoCalcEdit.Visible, true);
				}
			}
		}

		public void TestInvLines_OtherDetailsGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				var line = declaration.InvoiceLines.AddNew();
				var gaApproval = line.GAApprovalDataCollection.AddNew();
				gaApproval.CSI_Procedure = ZString.Empty;
				gaApproval.CSI_SubType = ZString.Empty;
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = (ExportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.OtherDetailsTabPage;
					var grid = control.ApprovalDocumentGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_LineNo);
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_SubType);
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_Code);
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_Procedure);
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_Description);
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_ReferenceNumber);
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_DateOfIssue);
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_ReferenceNumber2);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(GAApproval.NonGAReasonType));
					AssertEquals(grid.Columns[index++].ColumnName, CusSupportingInfo.Schema.CSI_AdditionalDescription);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(GAApproval.ExportNonGAMandatoryDocument));

					AssertEquals(true, grid.GetColumnStyle(nameof(GAApproval.ExportNonGAMandatoryDocument)).IsReadOnly);
				}
			}
		}

		public void TestReorderTabPages()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;

				using (var control = (ExportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					var tabControl = control.LineDetailTabControl;
					var index = 0;
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["LineDetailsTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["OtherDetailsTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["LineChargesTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["CustomFieldsTabPage"]);
				}
			}
		}

		public void TestInvLines_OtherDetailsControlBagNotIncluded()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.FindSingle<ZTabPage>("OtherDetailsTabPage");

					AssertEquals(true, control.FindSingle<ZDropEdit>("SkipManifestReportDropEdit").Visible);

					var grid = control.FindSingle<ZGrid>("SecondHandVehiclesBoundGrid");
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, VehicleNumber.Schema.CY_Order);
					AssertEquals(grid.Columns[index++].ColumnName, VehicleNumber.Schema.CY_Data);
				}
			}
		}

		[TestDate(2023, 10, 19)]
		public void TestTariffFindBox()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.EntryNumber = "4500122000001X";
				entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2022, 06, 06);
				var entryLine = entry.MergedLines.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					var lineDetailsTab = (ZTabPage)control.LineDetailTabControl.TabPages["LineDetailsTabPage"];
					control.LineDetailTabControl.SelectedTab = lineDetailsTab;
					var tariffFindBox = lineDetailsTab.FindSingle<TariffFindBox>("TariffFindBox");

					AssertNotNull(tariffFindBox.GetTariffType);
					AssertNotNull(tariffFindBox.GetDataGrouping);
					AssertNotNull(tariffFindBox.GetEffectiveDate);

					AssertEquals("HSN", tariffFindBox.GetTariffType());
					AssertEquals("KR", tariffFindBox.GetDataGrouping());
					AssertEquals("2022-06-06", tariffFindBox.GetEffectiveDate().ToString("yyyy-MM-dd"));
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
	}
}
