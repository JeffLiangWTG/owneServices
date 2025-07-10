using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ImportSupplierHeaderUserControl))]
	sealed class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestGridId()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutLFsI2M1sSfWMZ12NGcJeKw==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestHeaderDescriptionsTabPageNotVisible()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new ImportSupplierHeaderUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				Application.DoEvents();

				var headerDescriptionsTabPage = (ZTabPage)control.Controls.Find("HeaderDescriptionsTabPage", true).FirstOrDefault();
				AssertNull("HeaderDescriptionsTabPage should NOT be visible in DE. HeaderDescriptionsTabPage", headerDescriptionsTabPage);
			}
		}

		public void TestSupportingDocumentsUserControlType()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new ImportSupplierHeaderUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				Application.DoEvents();

				var tabPage = (ZTabPage)control.Controls.Find("SupportingDocumentsTabPage", true).First();
				tabPage.Show();

				var userControl = (ZDynamicControlCreationUserControl)control.Controls.Find("SupportingDocumentsUserControl", true).First();
				AssertEquals(typeof(ImportSupplierHeaderSupportingDocumentsUserControl), userControl.HostedControl.GetType());
			}
		}

		public void TestTabIndex()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("IsJZ_ExchangeRateUserEnterableCheckBox", 5, control.FindSingle<ZCheckBox>("IsJZ_ExchangeRateUserEnterableCheckBox").TabIndex);
					AssertEquals("IncoTermDescriptionTextBox", 9, control.FindSingle<ZTextBox>("IncoTermDescriptionTextBox").TabIndex);
					AssertEquals("AgreedPlaceCodeDropEdit", 10, control.FindSingle<ZDropEdit>("AgreedPlaceCodeDropEdit").TabIndex);
					AssertEquals("JZ_ValuationCodeDropEdit", 11, control.FindSingle<ZDropEdit>("JZ_ValuationCodeDropEdit").TabIndex);
					AssertEquals("GrossWeightCalcDropEdit", 12, control.FindSingle<ZCalcDropEdit>("GrossWeightCalcDropEdit").TabIndex);
					AssertEquals("NetWeightCalcDropEdit", 13, control.FindSingle<ZCalcDropEdit>("NetWeightCalcDropEdit").TabIndex);
					AssertEquals("JZ_InvoiceCurrLandedCostExRateCalcEdit", 14, control.FindSingle<ZCalcEdit>("JZ_InvoiceCurrLandedCostExRateCalcEdit").TabIndex);
					AssertEquals("NoOfPacksCalcDropEdit", 15, control.FindSingle<ZCalcDropEdit>("NoOfPacksCalcDropEdit").TabIndex);
				});
			}
		}

		public void TestSupportingInfoTabsVisible()
		{
			using (var frm = new ZForm(declaration))
			using (var userControl = new ImportSupplierHeaderUserControl())
			{
				frm.Controls.Add(userControl);
				frm.Show();

				var invoiceTabControl = userControl.Controls.Find("InvoiceTabControl", true).Single() as ZTabControl;
				if (invoiceTabControl != null)
				{
					AssertEquals("Should display additional info tab", false, invoiceTabControl.TabPages.ContainsKey("AdditionalInfoTabPage"));
					AssertEquals("Should display supporting document tab", true, invoiceTabControl.TabPages.ContainsKey("SupportingDocumentsTabPage"));
					AssertEquals("Should display previous document tab", false, invoiceTabControl.TabPages.ContainsKey("PreviousDocumentsTabPage"));
				}
			}
		}

		public void TestTabPagesVisible_IPR_AVABR()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new ImportSupplierHeaderUserControl())
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;

				control.JobDeclaration = declaration;
				frm.Controls.Add(control);
				frm.Show();

				var invoiceTabControl = control.FindSingle<ZTabControl>("InvoiceTabControl");
				var supportingDocumentsTabPage = (ZTabPage)invoiceTabControl.AllTabPages.Single(x => x.Name == "SupportingDocumentsTabPage");
				AssertEquals("SupportingDocumentsTabPage is not visible", false, supportingDocumentsTabPage.TabVisible);

				var customFieldsTabPage = (ZTabPage)invoiceTabControl.AllTabPages.Single(x => x.Name == "CustomFieldsTabPage");
				AssertEquals("CustomFieldsTabPage is not visible", false, customFieldsTabPage.TabVisible);
			}
		}

		public void TestIncoTermPlaceControlsCharacterCasing()
		{
			using (var userControl = new ImportSupplierHeaderUserControl())
			{
				var incoTermPlaceControl = userControl.FindSingle<ZTextBox>("JZ_IncoTermPlaceTextBox");
				AssertEquals("JZ_IncoTermPlaceTextBox: CharacterCasing", CharacterCasing.Normal, incoTermPlaceControl.CharacterCasing);
			}
		}

		public void TestInvoiceChargesFixedRateColumnDetails()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var invoiceChargesGrid = control.InvoiceChargesGrid;
				var fixedRateColumnStyle = invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.IsJ7_ExchangeRateIATA);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 47, fixedRateColumnStyle.Width);
					AssertEquals("Mandatory", true, fixedRateColumnStyle.IsMandatory);
				});
			}
		}

		public void TestInvoiceChargesExchangeRateDateColumn()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var invoiceChargesGrid = control.InvoiceChargesGrid;
				var exchangeRateColumnStyle = (ZDateEditColumnStyleInfo)invoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRateDate);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 121, exchangeRateColumnStyle.Width);
					AssertEquals("Mandatory", true, exchangeRateColumnStyle.IsMandatory);
					AssertEquals("Format", ZArchitecture.Core.ZDateTimePickerFormat.Short, exchangeRateColumnStyle.DateTimeFormat);
				});
			}
		}

		public void TestInvoiceChargesGridDefaultColumns()
		{
			string[] defaultColumnsOrder =
			{
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				InvoiceCharge.Schema.J7_IsGSTApplicable,
				InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceCharge.Schema.J7_DistributeBy,
				InvoiceCharge.Schema.IsJ7_ExchangeRateIATA,
				InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_ExchangeRateDate,
				InvoiceCharge.Schema.J7_IsIncludedInITOT
			};

			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var invoiceChargesGrid = control.InvoiceChargesGrid;
				invoiceChargesGrid.SetDataBinding(declaration, "");
				AssertSequencesEqual(defaultColumnsOrder, invoiceChargesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestApportionedChargesFixedRateColumnDetails()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				var fixedRateColumnStyle = apportionedChargesGrid.GetColumnStyle(InvoiceApportionCharge.Schema.IsJ7_ExchangeRateIATA);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 47, fixedRateColumnStyle.Width);
					AssertEquals("Mandatory", true, fixedRateColumnStyle.IsMandatory);
				});
			}
		}

		public void TestApportionedChargesExchangeRateDateColumn()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				var exchangeRateColumnStyle = (ZDateEditColumnStyleInfo)apportionedChargesGrid.GetColumnStyle(InvoiceApportionCharge.Schema.J7_ExchangeRateDate);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 121, exchangeRateColumnStyle.Width);
					AssertEquals("Mandatory", true, exchangeRateColumnStyle.IsMandatory);
					AssertEquals("Format", ZArchitecture.Core.ZDateTimePickerFormat.Short, exchangeRateColumnStyle.DateTimeFormat);
				});
			}
		}

		public void TestApportionedChargesGridDefaultColumns()
		{
			string[] defaultColumnsOrder =
			{
				InvoiceApportionCharge.Schema.J7_ChargeType,
				InvoiceApportionCharge.Schema.J7_Amount,
				InvoiceApportionCharge.Schema.J7_RX_NKCurrency,
				InvoiceApportionCharge.Schema.J7_IsDutiable,
				InvoiceApportionCharge.Schema.J7_IsStatisticalValueApplicable,
				InvoiceApportionCharge.Schema.J7_IsGSTApplicable,
				InvoiceApportionCharge.Schema.J7_IsIncludedInITOT,
				InvoiceApportionCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceApportionCharge.Schema.IsJ7_ExchangeRateIATA,
				InvoiceApportionCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				InvoiceApportionCharge.Schema.J7_ExchangeRate,
				InvoiceApportionCharge.Schema.J7_ExchangeRateDate
			};

			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				apportionedChargesGrid.SetDataBinding(declaration, "");
				AssertSequencesEqual(defaultColumnsOrder, apportionedChargesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestGroupChargesFixedRateColumnDetails()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var groupChargesGrid = control.BaseGroupChargesGrid;
				var fixedRateColumnStyle = groupChargesGrid.GetColumnStyle(GroupInvoiceCharge.Schema.IsJ7_ExchangeRateIATA);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 47, fixedRateColumnStyle.Width);
					AssertEquals("Mandatory", true, fixedRateColumnStyle.IsMandatory);
				});
			}
		}

		public void TestGroupChargesExchangeRateDateColumn()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var groupChargesGrid = control.BaseGroupChargesGrid;
				var exchangeRateColumnStyle = (ZDateEditColumnStyleInfo)groupChargesGrid.GetColumnStyle(GroupInvoiceCharge.Schema.J7_ExchangeRateDate);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 121, exchangeRateColumnStyle.Width);
					AssertEquals("Mandatory", true, exchangeRateColumnStyle.IsMandatory);
					AssertEquals("Format", ZArchitecture.Core.ZDateTimePickerFormat.Short, exchangeRateColumnStyle.DateTimeFormat);
				});
			}
		}

		public void TestGroupChargesGridDefaultColumns()
		{
			string[] defaultColumnsOrder =
			{
				GroupInvoiceCharge.Schema.J7_ChargeType,
				GroupInvoiceCharge.Schema.J7_Amount,
				GroupInvoiceCharge.Schema.J7_RX_NKCurrency,
				GroupInvoiceCharge.Schema.J7_IsDutiable,
				GroupInvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				GroupInvoiceCharge.Schema.J7_IsGSTApplicable,
				GroupInvoiceCharge.Schema.J7_Percentage,
				GroupInvoiceCharge.Schema.J7_DistributeBy,
				GroupInvoiceCharge.Schema.J7_FullOrPartialApportionment,
				BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT,
				GroupInvoiceCharge.Schema.IsJ7_ExchangeRateIATA,
				GroupInvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				GroupInvoiceCharge.Schema.J7_ExchangeRate,
				GroupInvoiceCharge.Schema.J7_ExchangeRateDate
			};

			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var groupChargesGrid = control.BaseGroupChargesGrid;
				groupChargesGrid.SetDataBinding(declaration, "");
				AssertSequencesEqual(defaultColumnsOrder, groupChargesGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestGetCalculateFreightForm()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (var form = new ZForm())
			using (var userControl = new ImportSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesCalculateFreightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");
				var groupChargesCalculateFreightButton = userControl.FindSingle<ZButton>("GroupChargesCalculateFreightButton");

				CombineAssertions(() =>
				{
					invoiceChargesCalculateFreightButton.PerformClick();
					AssertType<CalculateFreightForm>("InvoiceChargesCalculateFreightButton", ZFormModaliser.ActiveForm);

					ZFormModaliser.ActiveForm.Close();
					groupChargesCalculateFreightButton.PerformClick();
					AssertType<CalculateFreightForm>("GroupChargesCalculateFreightButton", ZFormModaliser.ActiveForm);
				});
			}
		}

		public void TestSellerColumnsAvailable()
		{
			using var frm = new ZForm(declaration);
			using var control = new ImportSupplierHeaderUserControl();
			control.JobDeclaration = declaration;

			control.InitializeGridLayout();
			frm.Controls.Add(control);
			frm.Show();

			CombineAssertions(() =>
			{
				AssertColumnIsAvailable(control, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OA_SellerAddress);
				AssertColumnIsAvailable(control, BaseJobComInvoiceHeader.Schema.SellerOrgPK);
			});
		}

		public void TestBuyerColumnsAvailable()
		{
			using var frm = new ZForm(declaration);
			using var control = new ImportSupplierHeaderUserControl();
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			frm.Controls.Add(control);
			frm.Show();

			CombineAssertions(() =>
			{
				AssertColumnIsAvailable(control, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OA_BuyerAddress);
				AssertColumnIsAvailable(control, BaseJobComInvoiceHeader.Schema.BuyerOrgPK);
			});
		}

		public void TestCalculateInsuranceFormBusinessEntity_ShouldBeGermanCalculateInsuranceBizObj()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			using var form = new ZForm();
			using var userControl = new ImportSupplierHeaderUserControl();
			userControl.SetDataBinding(declaration, "");
			userControl.JobDeclaration = declaration;
			form.Controls.Add(userControl);
			form.Show();
			var invoiceChargesCalculateInsuranceButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateInsuranceButton");
			invoiceChargesCalculateInsuranceButton.PerformClick();

			AssertType<CalculateInsuranceForm>("Precondition", ZFormModaliser.ActiveForm);
			var calculateInsuranceForm = (CalculateInsuranceForm)ZFormModaliser.ActiveForm;
			AssertType<CalculateInsuranceBizObj>(calculateInsuranceForm.BusinessEntity);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
		}
		
		JobDeclaration declaration;

		static void AssertColumnIsAvailable(ImportSupplierHeaderUserControl control, string columnName)
		{
			Assert($"{columnName} must not be removed", control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(x => x.ColumnName == columnName && !x.IsUnavailable));
		}
	}
}
