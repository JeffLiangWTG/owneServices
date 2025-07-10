using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionWithMandatoryDescriptionRegistryItemEditor))]
	sealed class CodeDescriptionWithMandatoryDescriptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeDescriptionWithMandatoryDescriptionRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionWithMandatoryDescriptionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionWithMandatoryDescriptionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionWithMandatoryDescriptionRegistryItem("", null, null, null, RegistryStorageFlags.System, new CodeDescriptionWithMandatoryDescriptionCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CodeDescriptionWithMandatoryDescriptionCollection()
			{
				{ "AAA", (NoResString)"AAA Description" },
				{ "ZZZ", (NoResString)"ZZZ Description" },
			} };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
