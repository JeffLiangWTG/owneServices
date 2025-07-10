using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(SourceModuleFinderForm))]
	sealed class SourceModuleFinderFormTest : ZFormBasherTest
	{
		public void TestModuleCodeColumnCaption()
		{
			var moduleFinder = new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
			using (var form = new SourceModuleFinderForm(moduleFinder))
			{
				form.Show();
				var column = form.SourceModuleGrid.GetColumnStyle("ModuleCode");
				AssertNotNull(column);
				AssertEquals("Menu Section Code", column.Caption);
			}
		}

		public void TestModuleDescriptionColumnCaption()
		{
			var moduleFinder = new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
			using (var form = new SourceModuleFinderForm(moduleFinder))
			{
				form.Show();
				var column = form.SourceModuleGrid.GetColumnStyle("ModuleDescription");
				AssertNotNull(column);
				AssertEquals("Menu Section Description", column.Caption);
			}
		}

		public void TestShowCloseButtonOnly()
		{
			var moduleFinder = new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
			using (var form = new SourceModuleFinderForm(moduleFinder) { ShowCloseButtonOnly = true })
			{
				form.Show();
				var okButton = form.Controls.Find("OkButton", true).Single() as ZButton;
				var cancelZButton = form.Controls.Find("CancelZButton", true).Single() as ZButton;
				AssertEquals(false, okButton.Visible);
				AssertEquals(true, cancelZButton.Visible);
				AssertEquals("Close", cancelZButton.CaptionResourceString.Caption);
			}
		}

		public void TestShowDescriptionFilterOnly()
		{
			var moduleFinder = new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
			using (var form = new SourceModuleFinderForm(moduleFinder) { ShowDescriptionFilterOnly = true })
			{
				form.Show();
				var productAreaDropEdit = form.Controls.Find("productAreaDropEdit", true).Single() as ZDropEdit;
				var menuSectionDropEdit = form.Controls.Find("menuSectionDropEdit", true).Single() as ZDropEdit;
				var descriptionTextBox = form.Controls.Find("DescriptionTextBox", true).Single() as ZTextBox;
				AssertEquals(false, productAreaDropEdit.Visible);
				AssertEquals(false, menuSectionDropEdit.Visible);
				AssertEquals("Filter by Description", descriptionTextBox.CaptionResourceString.Caption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new SourceModuleFinderForm(new SourceModuleFinder(ProductTypes.Codes.Enterprise, ModuleListType.MenuSection, "", Factory));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "DescriptionTextBox";
		}
	}
}
