using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(IncoTermComboBoxRegistryItemEditor))]
	sealed class IncoTermComboBoxRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[TestDate(2020, 1, 1)]
		public void TestIncotermsWarning()
		{
			var editorInfo = new ComboBoxRegistryEditorInfo(new IncoTermsCodeDescriptionPairListProvider(IncoTermsListType.ActiveIncoTerms));
			var editor = new IncoTermComboBoxRegistryItemEditor(new CodePairRegistryDataType(new CodeDescriptionPairListProvider(() => editorInfo.LookUpList), false, true), editorInfo);
			using (var editorPane = editor.NewWinFormsEditorPane())
			{
				var bizo = ComboBoxRegistryItemEditor.GetBizo(editorPane);
				Assert("Default Incoterm should have no warnings", bizo.ValueInfo.GetWarnings().Count() == 0);
				bizo.Value = Core.Constants.IncoTerms.DeliveredAtTerminal;
				AssertEquals("Incoterm 'DAT' should have warning", true, bizo.ValueInfo.HasWarning("This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules."));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			var editorInfo = new ComboBoxRegistryEditorInfo(new OLookUpEditType());
			return new IncoTermComboBoxRegistryItemEditor(new CodePairRegistryDataType(OLookUpEditType.ShipmentScreenLayout), editorInfo);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new IncoTermComboBoxRegistryItemEditorForTest(null, null).GetEditorPaneType();
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "AUT" };
		}
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodePairRegistryItem("", null, null, null, OLookUpEditType.ShipmentScreenLayout, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		#region ComboBoxRegistryItemEditor

		class IncoTermComboBoxRegistryItemEditorForTest : IncoTermComboBoxRegistryItemEditor
		{
			public IncoTermComboBoxRegistryItemEditorForTest(IRegistryDataType dataType, IRegistryEditorInfo editorInfo) : base(dataType, editorInfo)
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
