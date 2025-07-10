using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class LayoutPreviousDocumentsUserControlBaseOnlyTest : PreviousDocumentUserControlAbstractTest<LayoutPreviousDocumentsUserControl, JobDeclaration>
	{
		public void TestLineNoColumnStyleMaxValue()
		{
			using (var form = new ZForm(Declaration))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();

				var lineNoColumn = control.PreviousDocumentsGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == "CSI_LineNo");
				AssertEquals("MaxValue", 99999m, lineNoColumn.MaxValue);
			}
		}

		public void TestPackQtyColumnStyleMaxValue()
		{
			var testData = new EntryInstructionUcc6TestData();
			var declaration = testData.GetBindingParent(Factory) as JobDeclaration;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			using (var form = new ZForm(declaration))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();

				var packQtyColumn = control.PreviousDocumentsGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == "CSI_PackQty");
				AssertEquals("MaxValue", 99999999m, packQtyColumn.MaxValue);
			}
		}

		public void TestInvoiceLine() => AssertPreviousDocumentsUserControl(new InvoiceLineTestData());

		public void TestInvoiceHeader() => AssertPreviousDocumentsUserControl(new InvoiceHeaderTestData());

		public void TestJobDeclaration() => AssertPreviousDocumentsUserControl(new JobDeclarationTestData());

		public void TestEntryInstruction_NotUcc6() => AssertPreviousDocumentsUserControl(new EntryInstructionTestData());

		public void TestEntryInstruction_Ucc6()
		{
			var testData = new EntryInstructionUcc6TestData();
			var declaration = testData.GetBindingParent(Factory) as JobDeclaration;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.IsUCC6) + "Core", true))
			{
				AssertPreviousDocumentsUserControl(new EntryInstructionUcc6TestData(), declaration);
			}
		}

		public void TestDetailsLayoutControlDockStyle()
		{
			using (var form = new ZForm(Declaration))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("DetailsLayoutControl should be DockStyle.Fill", System.Windows.Forms.DockStyle.Fill, control.DetailsLayoutControl.Dock);
			}
		}

		void AssertPreviousDocumentsUserControl(TestData testData) => AssertPreviousDocumentsUserControl(testData, testData.GetBindingParent(Factory));

		void AssertPreviousDocumentsUserControl(TestData testData, BusinessObject declaration)
		{
			using (var form = new ZForm(declaration))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();
				form.SetDataBinding(null, null);
				new ControlRebinder().Rebind(control, nameof(JobDeclaration.FilteredInvoiceLines), testData.BindingString);
				form.SetDataBinding(declaration, "");

				CombineAssertions(() =>
				{
					var grid = control.FindSingle<ZGrid>(nameof(LayoutPreviousDocumentsUserControl.PreviousDocumentsGrid));
					AssertGridColumns(testData, grid);

					var detailsLayoutUserControl = control.FindSingle<PreviousDocumentsDetailsLayoutControl>(nameof(LayoutPreviousDocumentsUserControl.DetailsLayoutControl));
					AssertLayoutControls(testData, detailsLayoutUserControl.DetailsPanel);
				});
			}
		}

		void AssertLayoutControls(TestData testData, DynamicLayoutPanel dynamicLayoutPanel)
		{
			var layoutPanel = testData.LayoutPanel;
			AssertEquals("Control count", layoutPanel.IncludedControls.Count, dynamicLayoutPanel.Controls.Count);

			foreach (var controlName in layoutPanel.IncludedControls.Select(x => x.ControlName))
			{
				Assert($"Control {controlName} should exist in layout", dynamicLayoutPanel.Controls.ContainsKey(controlName));
			}
		}

		void AssertGridColumns(TestData testData, ZGrid grid)
		{
			if (testData.OrderedColumnDetails != null)
			{
				AssertSequencesEqual(testData.OrderedColumnDetails, grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
		{
			return new[]
			{
				(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle)),
				(PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyle)),
				(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle)),
				(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
				(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle)),
			};
		}

		abstract class TestData
		{
			public abstract string BindingString { get; }

			public virtual BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<JobDeclaration>();

			public virtual string[] OrderedColumnDetails { get; }

			public virtual PanelLayout LayoutPanel => new PreviousDocumentFieldsLayout().Layout;
		}

		sealed class InvoiceLineTestData : TestData
		{
			public override string BindingString => nameof(JobDeclaration.FilteredInvoiceLines);
		}

		sealed class InvoiceHeaderTestData : TestData
		{
			public override string BindingString => nameof(JobDeclaration.Invoices);
		}

		sealed class JobDeclarationTestData : TestData
		{
			public override string BindingString => string.Empty;
		}

		sealed class EntryInstructionTestData : TestData
		{
			public override string BindingString => nameof(JobDeclaration.CustomsEntryInstructions);

			public override string[] OrderedColumnDetails => new[]
			{
				nameof(PreviousDocument.CSI_Code),
				nameof(PreviousDocument.CSI_SubType),
				nameof(PreviousDocument.CSI_ReferenceNumber),
				nameof(PreviousDocument.CSI_DateOfIssue),
				nameof(PreviousDocument.CSI_LineNo),
			};
		}

		sealed class EntryInstructionUcc6TestData : TestData
		{
			public override string BindingString => nameof(JobDeclaration.CustomsEntryInstructions);

			public override string[] OrderedColumnDetails => new[]
			{
				nameof(PreviousDocument.CSI_Code),
				nameof(PreviousDocument.CSI_CodeDescription),
				nameof(PreviousDocument.CSI_ReferenceNumber),
				nameof(PreviousDocument.CSI_PackQty),
				nameof(PreviousDocument.CSI_PackType),
				nameof(PreviousDocument.CSI_Quantity),
				nameof(PreviousDocument.CSI_UnitOfQuantity),
				nameof(PreviousDocument.CSI_ItemNumber),
			};

			public override PanelLayout LayoutPanel => new UCC6PreviousDocumentFieldsLayout().Layout;
		}
	}
}
