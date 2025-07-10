using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ComboBoxRegistryItemEditor))]
	sealed class ComboBoxRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestCharacterCasing()
		{
			using (var parent = Editor.NewWinFormsEditorPane())
			{
				var editorPane = (ZDropEdit)parent.Controls[0];
				AssertEquals("EditorPane.CharacterCasing", CharacterCasing.Normal, editorPane.CharacterCasing);
			}
		}

		public void TestHasChangesIsSetWhenBizoIsChanged()
		{
			using (var editorPane = Editor.NewWinFormsEditorPane())
			{
				var bizo = ComboBoxRegistryItemEditor.GetBizo(editorPane);
				Assert("When we have done nothing then 'HasChanges' should be false", !bizo.HasChanges);

				bizo.Value = "Foo";
				Assert("When we change the property 'Value' then 'HasChanges' should be true", bizo.HasChanges);
			}
		}

		public void TestShowDescriptionBox()
		{
			using (var parent = Editor.NewWinFormsEditorPane())
			{
				var editorPane = (ZDropEdit)parent.Controls[0];
				AssertEquals("EditorPane.ShowDescriptionBox", true, editorPane.ShowDescriptionBox);
				AssertEquals("EditorPane.ShowDescriptionInDropDown", true, editorPane.ShowDescriptionInDropDown);
			}

			var editorInfo = new ComboBoxRegistryEditorInfo(new OLookUpEditType());
			editorInfo.ShowComboDescription = false;
			var anotherEditor = new ComboBoxRegistryItemEditor(new CodePairRegistryDataType(new OLookUpEditType()), editorInfo);

			using (var parent = anotherEditor.NewWinFormsEditorPane())
			{
				var editorPane = (ZDropEdit)parent.Controls[0];
				AssertEquals("EditorPane.CharacterCasing", CharacterCasing.Normal, editorPane.CharacterCasing);
				AssertEquals("EditorPane.ShowDescriptionBox", false, editorPane.ShowDescriptionBox);
				AssertEquals("EditorPane.ShowDescriptionInDropDown", false, editorPane.ShowDescriptionInDropDown);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			var editorInfo = new ComboBoxRegistryEditorInfo(new OLookUpEditType());
			return new ComboBoxRegistryItemEditor(new CodePairRegistryDataType(OLookUpEditType.ShipmentScreenLayout), editorInfo);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new ComboBoxRegistryItemEditorForTest(null, null).GetEditorPaneType();
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodePairRegistryItem("", null, null, null, OLookUpEditType.ShipmentScreenLayout, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "AUT" };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		#region ComboBoxRegistryItemEditor

		class ComboBoxRegistryItemEditorForTest : ComboBoxRegistryItemEditor
		{
			public ComboBoxRegistryItemEditorForTest(IRegistryDataType dataType, IRegistryEditorInfo editorInfo) : base(dataType, editorInfo)
			{
			}

			public Type GetEditorPaneType()
			{
				return typeof(ZUserControl);
			}
		}

		#endregion

		#endregion
	}
}
