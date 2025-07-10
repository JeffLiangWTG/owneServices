using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class AdditionalInfosUserControlWithGridTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				AssertNotNull("AdditionalInfosGrid", control.FindSingle<ZGrid>("AdditionalInfosGrid"));
				AssertNotNull("GridAndDetailsSplitContainer", control.FindSingle<KSplitContainer>("GridAndDetailsSplitContainer"));
			}
		}

		public void TestSupportingInfoUserControls_GridBindingMember()
		{
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				AssertEquals("FilteredInvoiceLines", control.GridBindingMember);
			}
		}

		public void TestSupportingInfoUserControls_Grid()
		{
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				AssertSame(control.Grid, control.FindSingle<ZGrid>("AdditionalInfosGrid"));
			}
		}

		public void TestAdditionalInfosGridDefaultBinding()
		{
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				AssertEquals("FilteredInvoiceLines.AdditionalInfos", control.FindSingle<ZGrid>("AdditionalInfosGrid").GetBindingMember());
			}
		}

		public void TestDetailsLayoutControlDefaultBinding()
		{
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				AssertEquals("FilteredInvoiceLines.AdditionalInfos", control.FindSingle<AdditionalInformationDetailsLayoutControl>("DetailsLayoutControl").GetBindingMember());
			}
		}

		public void TestGridColumnNames_InvoiceLine()
		{
			AssertGridColumnNames(new InvoiceLineTestData());
		}

		public void TestGridColumnNames_CusClassPartPivot()
		{
			AssertGridColumnNames(new CusClassPartPivotTestData());
		}

		public void TestGridColumnNames_InvoiceHeader()
		{
			AssertGridColumnNames(new InvoiceHeaderTestData());
		}

		[RequiresSTA]
		public void TestGridColumnNames_ExitSummary()
		{
			AssertGridColumnNames(new ExitSummaryTestData());
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder_InvoiceLine()
		{
			AssertGridColumnOrder(new InvoiceLineTestData());
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder_CusClassPartPivot()
		{
			AssertGridColumnOrder(new CusClassPartPivotTestData());
		}

		public void TestGridDefaultColumnOrder_InvoiceHeader()
		{
			AssertGridColumnOrder(new InvoiceHeaderTestData());
		}

		public void TestGridDefaultColumnOrder_ExitSummary()
		{
			AssertGridColumnOrder(new ExitSummaryTestData());
		}

		public void TestGridColumnWidths_InvoiceLine()
		{
			AssertGridColumnWidths(new InvoiceLineTestData());
		}

		public void TestGridColumnWidths_CusClassPartPivot()
		{
			AssertGridColumnWidths(new CusClassPartPivotTestData());
		}

		public void TestGridColumnWidths_InvoiceHeader()
		{
			AssertGridColumnWidths(new InvoiceHeaderTestData());
		}

		public void TestGridColumnWidths_ExitSummary()
		{
			AssertGridColumnWidths(new ExitSummaryTestData());
		}

		public void TestCreateNewInvoiceHeaderAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<InvoiceHeaderAdditionalInformationDetailsLayout>(control.CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewInvoiceLineAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<InvoiceLineAdditionalInformationDetailsLayout>(control.CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewExitSummaryAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<ExitSummaryAdditionalInformationDetailsLayout>(control.CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewDeclarationAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<DeclarationAdditionalInformationDetailsLayout>(control.CreateNewDeclarationAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewClassPartPivotAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<InvoiceLineAdditionalInformationDetailsLayout>("Use InvoiceLineLayout for ClassPartPivot by default", control.CreateNewClassPartPivotAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestColumnCasingAndDecimalPlaces()
		{
			using (var frm = new ZForm(Factory.NewWithValidTestData<JobDeclaration>()))
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				frm.Controls.Add(control);
				frm.Show();

				var referenceNumberColumnStyleInfo = control.Grid.GetColumnStyle(nameof(AdditionalInfo.CSI_ReferenceNumber)) as ZTextBoxColumnStyleInfo;
				var descriptionColumnStyleInfo = control.Grid.GetColumnStyle(nameof(AdditionalInfo.CSI_Description)) as ZTextBoxColumnStyleInfo;
				var amountColumnStyleInfo = control.Grid.GetColumnStyle(nameof(AdditionalInfo.CSI_Value)) as ZCalcEditColumnStyleInfo;

				CombineAssertions(() =>
				{
					AssertEquals("CSI_ReferenceNumber casing", CharacterCasing.Normal, referenceNumberColumnStyleInfo.CharacterCasing);
					AssertEquals("CSI_Description casing", CharacterCasing.Normal, descriptionColumnStyleInfo.CharacterCasing);
					AssertEquals("CSI_Value decimals", 2, amountColumnStyleInfo.Decimals);
				});
			}
		}

		public void TestSupportMultipleResourceStringData()
		{
			using var control = new AdditionalInfosUserControlWithGrid();
			var supportMultipleResourceStringDataSupporter = control as ISupportMultipleResourceStringDataSupporter;
			AssertNotNull("Control as ISupportMultipleResourceStringDataSupporter", supportMultipleResourceStringDataSupporter);
			AssertSequencesEqual("MultipleKeysToUse", new string[] { JobDeclaration.CaptionKeySAD }, control.MultipleKeysToUse);
		}

		void AssertGridColumnNames(TestData testData)
		{
			using (var frm = new ZForm(testData.GetBindingParent(Factory)))
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "FilteredInvoiceLines", testData.BindingString);
				frm.Controls.Add(control);
				frm.Show();
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _) in testData.OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		void AssertGridColumnOrder(TestData testData)
		{
			using (var frm = new ZForm(testData.GetBindingParent(Factory)))
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "FilteredInvoiceLines", testData.BindingString);
				frm.Controls.Add(control);
				frm.Show();
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				AssertSequencesEqual(testData.OrderedColumnDetails.Select(x => x.ColumnName), grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		void AssertGridColumnWidths(TestData testData)
		{
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "FilteredInvoiceLines", testData.BindingString);
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth) in testData.OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		abstract class TestData
		{
			public abstract string BindingString { get; }

			public abstract BusinessObject GetBindingParent(BusinessObjectFactory factory);

			public abstract (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails { get; }
		}

		class InvoiceLineTestData : TestData
		{
			public override string BindingString => "FilteredInvoiceLines";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<JobDeclaration>();

			public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
			{
				(CusSupportingInfo.Schema.CSI_SubType, "Type", 80),
				(CusSupportingInfo.Schema.CSI_Code, "Code", 80),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
				(CusSupportingInfo.Schema.CSI_Description, "Description", 530),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber2, "Detail", 86),
				(CusSupportingInfo.Schema.CSI_RX_NKCurrency, "Currency", 66),
				(CusSupportingInfo.Schema.CSI_Value, "Amount", 147)
			};
		}

		class CusClassPartPivotTestData : TestData
		{
			public override string BindingString => "PivotsForBinding";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<OrgSupplierPart>();

			public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
			{
				(CusSupportingInfo.Schema.CSI_SubType, "Type", 80),
				(CusSupportingInfo.Schema.CSI_Code, "Code", 80),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
				(CusSupportingInfo.Schema.CSI_Description, "Description", 530),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber2, "Detail", 86),
				(CusSupportingInfo.Schema.CSI_RX_NKCurrency, "Currency", 66),
				(CusSupportingInfo.Schema.CSI_Value, "Amount", 147)
			};
		}

		class InvoiceHeaderTestData : TestData
		{
			public override string BindingString => "Invoices";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<JobDeclaration>();

			public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
			{
				(CusSupportingInfo.Schema.CSI_SubType, "Type", 80),
				(CusSupportingInfo.Schema.CSI_Code, "Code", 80),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
				(CusSupportingInfo.Schema.CSI_Description, "Description", 530)
			};
		}

		class ExitSummaryTestData : TestData
		{
			public override string BindingString => "CusExitDetails";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<CusExitControlHeader>();

			public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
			{
				(CusSupportingInfo.Schema.CSI_SubType, "Type", 80),
				(CusSupportingInfo.Schema.CSI_Code, "Code", 80),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
				(CusSupportingInfo.Schema.CSI_Description, "Description", 530),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber2, "Detail", 86),
				(CusSupportingInfo.Schema.CSI_RX_NKCurrency, "Currency", 66),
				(CusSupportingInfo.Schema.CSI_Value, "Amount", 147)
			};
		}

		class AdditionalInfosUserControlWithGridForTest : AdditionalInfosUserControlWithGrid
		{
			public IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceHeaderAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceLineAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed() => base.CreateNewExitSummaryAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewDeclarationAdditionalInformationDetailsLayoutExposed() => base.CreateNewDeclarationAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewClassPartPivotAdditionalInformationDetailsLayoutExposed() => base.CreateNewClassPartPivotAdditionalInformationDetailsLayout();
		}
	}
}
