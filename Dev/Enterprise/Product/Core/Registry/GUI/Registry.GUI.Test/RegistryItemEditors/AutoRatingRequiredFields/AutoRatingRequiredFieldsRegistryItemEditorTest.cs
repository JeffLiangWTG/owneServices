using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutoRatingRequiredFieldsRegistryItemEditor))]
	sealed class AutoRatingRequiredFieldsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestEditorInfoWithShowIncoterm()
		{
			AutoRatingRequiredFieldsRegistryItemEditor editor = new AutoRatingRequiredFieldsRegistryItemEditor(new AutoRatingRequiredFieldsRegistryEditorInfo(true), RegistryItem.DataType);

			using (Control editorPane = editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.GetType()", typeof(AutoRatingRequiredFieldsWithIncotermControl), editorPane.GetType());
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AutoRatingRequiredFieldsRegistryItemEditor(new AutoRatingRequiredFieldsRegistryEditorInfo(false), RegistryItem.DataType);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutoRatingRequiredFieldsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutoRatingRequiredFieldsRegistryItem("", "", null, null, null, RegistryStorageFlags.System, false);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AutoRatingRequiredFieldsControl)editorPane).ReadOnly;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new AutoRatingRequiredFields() };
		}

		#endregion
	}
}
