using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RateValidityRegistryItemEditor))]
	sealed class RateValidityRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new RateValidityRegistryItemEditor(null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RateValidityControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RateValidityControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			IntRegistryItem result = new IntRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new RateValidityRegistryEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { 1, -1234 };
		}

		#endregion
	}
}
