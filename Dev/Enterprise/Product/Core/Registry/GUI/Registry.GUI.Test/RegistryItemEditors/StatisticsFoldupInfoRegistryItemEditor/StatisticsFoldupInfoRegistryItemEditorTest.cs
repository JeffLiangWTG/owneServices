using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StatisticsFoldupInfoRegistryItemEditor))]
	sealed class StatisticsFoldupInfoRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((StatisticsFoldupInfoUserControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return Enterprise.Registry.GUI.RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override object[] GetValidRegistryValues()
		{
			return new StatisticsFoldupInfoCollection[] { new StatisticsFoldupInfoCollection() };
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(StatisticsFoldupInfoUserControl);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new StatisticsFoldupInfoRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StatisticsFoldupInfoRegistryItem("", ResString.GetMultilingualString("", ""), ResString.GetMultilingualString("", ""), ResString.GetMultilingualString("", ""), RegistryStorageFlags.System, RegistryOptions.Default, StatisticsFoldupInfoCollection.GetDefaultCollection());
		}
	}
}
