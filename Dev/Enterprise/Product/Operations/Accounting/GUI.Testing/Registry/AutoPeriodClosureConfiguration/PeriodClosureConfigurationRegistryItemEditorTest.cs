using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(PeriodClosureConfigurationRegistryItemEditor))]
	public class PeriodClosureConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new PeriodClosureConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PeriodClosureConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PeriodClosureConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PeriodClosureConfigurationRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes));
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes) };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
