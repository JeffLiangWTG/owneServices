using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Common.GUI.Testing
{
	public class EntryStatusFilterGUIProviderTest : TestCaseWithFactory
	{
		public void TestProvidedControls()
		{
			using (var strip = new ZFilterStrip())
			{
				var source = new ZBindingSource(strip, typeof(EntryStatusFilter));

				var controls = EntryStatusFilterGUIProvider.GetEntryStatusFilterControls(strip, source, useComparisonOperator: true, useFilterType: true);

				var operatorDropEdit = controls.Single(x => x.Name == "operatorDropEdit") as ZDropEdit;
				AssertEquals("BindingMember", "ComparisonOperator", operatorDropEdit.GetBindingMember());
				AssertEquals("CharacterCasing", CharacterCasing.Lower, operatorDropEdit.CharacterCasing);

				var typeDropEdit = controls.Single(x => x.Name == "typeDropEdit") as ZDropEdit;
				AssertEquals("BindingMember", nameof(EntryStatusFilter.FilterType), typeDropEdit.GetBindingMember());
				AssertEquals("ShowDescriptionBox", false, typeDropEdit.ShowDescriptionBox);
				AssertEquals("Visible", true, typeDropEdit.Visible);

				var typeLabel = controls.Single(x => x.Name == "typeLabel") as ZLabel;
				AssertEquals("Label Text", "Type", typeLabel.Text);
				AssertEquals("Visible", true, typeLabel.Visible);

				var entryStatusDropEdit = controls.Single(x => x.Name == "entryStatusDropEdit") as ZDropEdit;
				AssertEquals("BindingMember", "Property", entryStatusDropEdit.GetBindingMember());
				AssertEquals("ShowDescriptionBox", true, entryStatusDropEdit.ShowDescriptionBox);
				AssertEquals("Width", 200, entryStatusDropEdit.Width);

				foreach (var control in controls)
				{
					control.Dispose();
				}
			}
		}

		public void TestTypeControls_OnlyEntryStatusVisible()
		{
			using (var strip = new ZFilterStrip())
			{
				var source = new ZBindingSource(strip, typeof(EntryStatusFilter));

				var controls = EntryStatusFilterGUIProvider.GetEntryStatusFilterControls(strip, source, useComparisonOperator: false, useFilterType: false);

				var operatorDropEdit = controls.Single(x => x.Name == "operatorDropEdit") as ZDropEdit;
				AssertEquals("Visible", false, operatorDropEdit.Visible);

				var typeDropEdit = controls.Single(x => x.Name == "typeDropEdit") as ZDropEdit;
				AssertEquals("Visible", false, typeDropEdit.Visible);

				var typeLabel = controls.Single(x => x.Name == "typeLabel") as ZLabel;
				AssertEquals("Visible", false, typeLabel.Visible);

				var entryStatusDropEdit = controls.Single(x => x.Name == "entryStatusDropEdit") as ZDropEdit;
				AssertEquals("Visible", true, entryStatusDropEdit.ShowDescriptionBox);
				AssertEquals("Width - Expands to end of filter strip when FilterType dropdown is hidden", 300, entryStatusDropEdit.Width);

				foreach (var control in controls)
				{
					control.Dispose();
				}
			}
		}
	}
}
