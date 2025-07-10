using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor))]
	sealed class CodeDescriptionWithEnabledAndDefaultsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestNewBoundWinFormsEditorPane()
		{
			var editorInfo = new CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo();
			editorInfo.CodeColumnCaption = (NoResString)"Custom Code Caption";
			editorInfo.DescriptionColumnCaption = (NoResString)"Custom Description Caption";
			var editor = new CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor(RegistryItem.DataType, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));

			using (var editorPane = (CodeDescriptionWithEnabledAndDefaultsRegistryControl)editor.NewWinFormsEditorPane())
			{
				AssertEquals("Custom Code Caption", editorPane.Grid.GetColumnCaption("Code"));
				AssertEquals("Custom Description Caption", editorPane.Grid.GetColumnCaption("EnglishDescription"));
			}
		}

		#region Implementation

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			var dataType = new CodeDescriptionWithEnabledAndDefaultsRegistryDataType(new CodeDescriptionWithEnabledAndDefaultCollection(), true);
			var editorInfo = new CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo();
			return new CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor(dataType, editorInfo, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionWithEnabledAndDefaultsRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionWithEnabledAndDefaultsRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionWithEnabledAndDefaultsRegistryItem("", null, null, null, new CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo(), RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionWithEnabledAndDefaultCollection(), true);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection1 = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			collection1.AddNew("tel", (NoResString)"Lync", true, true);
			collection1.AddNew("callto", (NoResString)"Skype", false, true);

			var collection2 = new CodeDescriptionWithEnabledAndDefaultCollection();
			collection2.AddNew("sip", (NoResString)"XXX", true, true);

			return new[] { collection1, collection2 };
		}

		#endregion
	}
}
