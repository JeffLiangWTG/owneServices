using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(MarkUpPercentagesRegistryItemEditor))]
	sealed class MarkUpPercentagesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestMyMarkUpPercentagesControl_NullData()
		{
			using (MyMarkUpPercentagesControlForTest editorPane = new MyMarkUpPercentagesControlForTest())
			{
				AssertEquals("Precondition: Data should be null", null, editorPane.Data);
				AssertEquals("Precondition: FieldValue should be empty", "", editorPane.FieldValue);

				editorPane.FieldValue = "ALL|PFFAA|0|0|0";
				AssertEquals("FieldValue", "ALL|PFFAA|0|0|0", editorPane.FieldValue);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new MarkUpPercentagesRegistryItemEditor(null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MarkUpPercentagesContainer)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MarkUpPercentagesRegistryItemEditor.MyMarkUpPercentagesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new MarkUpPercentagesRegistryEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "ALL|PFFAA|0|0|0" };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#region MyMarkUpPercentagesControlForTest

		class MyMarkUpPercentagesControlForTest : MarkUpPercentagesRegistryItemEditor.MyMarkUpPercentagesControl
		{
			public new MarkUpPercentagesCollectionWrapper Data
			{
				get { return base.Data; }
			}
		}

		#endregion

		#endregion
	}
}
