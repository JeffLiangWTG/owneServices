using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(QuoteValidityRegistryItemEditor))]
	sealed class QuoteValidityRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new QuoteValidityRegistryItemEditor(null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((QuoteValidityControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(QuoteValidityControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			IntRegistryItem result = new IntRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new QuoteValidityRegistryEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { 0, -1234 };
		}

		#endregion
	}
}
