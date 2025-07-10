using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class LayoutPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestEntryInstruction() => AssertPreviousDocumentsUserControl(new EntryInstructionTestData());

		void AssertPreviousDocumentsUserControl(TestData testData) => AssertPreviousDocumentsUserControl(testData, testData.GetBindingParent(Factory));

		void AssertPreviousDocumentsUserControl(TestData testData, BusinessObject declaration)
		{
			using (var form = new ZForm(declaration))
			using (var control = new LayoutPreviousDocumentsUserControl())
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

		abstract class TestData
		{
			public abstract string BindingString { get; }

			public virtual BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<JobDeclaration>();

			public virtual string[] OrderedColumnDetails { get; }

			public virtual PanelLayout LayoutPanel => new PreviousDocumentFieldsLayout().Layout;
		}

		sealed class EntryInstructionTestData : TestData
		{
			public override string BindingString => nameof(JobDeclaration.CustomsEntryInstructions);

			public override string[] OrderedColumnDetails => new[]
			{
				nameof(PreviousDocument.CSI_Code),
				nameof(PreviousDocument.CSI_ReferenceNumber),
			};
		}
	}
}
