using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EventsVisibilityOverrideRegistryControl))]
	sealed class EventsVisibilityOverrideRegistryControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestParentGridReadOnly()
		{
			using (ZForm form = new ZForm())
			{
				using (EventsVisibilityOverrideRegistryControl control = new EventsVisibilityOverrideRegistryControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					Assert("ParentGrid.ReadOnly", control.ParentGrid.ReadOnly);

					control.ReadOnly = false;
					var codeColumnName = "Code";
					var descriptionColumnName = "EnglishDescription";
					var columnInfo = control.ParentGrid.ColumnStyles.Cast<ZGridColumnInfo>();

					Assert("!ParentGrid.ReadOnly", !control.ParentGrid.ReadOnly);
					Assert("ParentGrid, Code column !IsReadOnly", !columnInfo.Single(x => x.ColumnName == codeColumnName).IsReadOnly);
					Assert("ParentGrid, Description IsReadOnly", columnInfo.Single(x => x.ColumnName == descriptionColumnName).IsReadOnly);
				}
			}
		}

		[GuiTest]
		public void TestTranslateButtonIsNotVisible()
		{
			using (var form = new RegistryFormTest.RegistryFormForTest())
			{
				form.Show();
				var control = new EventVisibilityOverrideRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new EventVisibilityOverrideCollection());
				var item2 = new RegistryItemTag(control);
				var itemNode2 = new TreeNode("Item2") { Tag = item2 };
				var translateButton = (ZButton)form.Controls.Find("translateButton", true)[0];
				form.GetRegistriesTreeView().Nodes.Add(itemNode2);
				form.GetRegistriesTreeView().SelectedNode = itemNode2;
				Assert("Translate Button should not be visible", !translateButton.Visible);
			}
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new EventsVisibilityOverrideRegistryControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EventsVisibilityOverrideRegistryControl)control).ChildGrid.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			EventVisibilityOverrideCollection collection = new EventVisibilityOverrideCollection();
			EventVisibilityOverride categorisedTaskTypes = collection.AddNew();
			return collection;
		}
	}
}
