using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZAddressRegistryItemEditor))]
	sealed class ZAddressRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[RequiresSTA]
		public void TestSelectedAddress()
		{
			ZAddressRegistryItemEditorForTest newEditor = new ZAddressRegistryItemEditorForTest(null);

			using (ZAddressControl editorPane = (ZAddressControl)Editor.NewWinFormsEditorPane())
			{
				ZGuid newGuid = ZGuid.NewZGuid();
				newEditor.SetValueZGuid(editorPane, newGuid);
				AssertEquals("GetValueFromEditorPane()", newGuid.ToGuid(), (Guid)Editor.GetValueFromEditorPane(editorPane));

				newEditor.SetValueZGuid(editorPane, ZGuid.Empty);
				AssertEquals("GetValueFromEditorPane()", Guid.Empty, (Guid)Editor.GetValueFromEditorPane(editorPane));

				newEditor.SetValueZGuid(editorPane, ZGuid.Invalid);
				AssertEquals("GetValueFromEditorPane()", Guid.Empty, (Guid)Editor.GetValueFromEditorPane(editorPane));
			}
		}

		public void TestGetCustomValidation()
		{
			using (ZAddressControl editorPane = (ZAddressControl)Editor.NewWinFormsEditorPane())
			{
				Editor.EnableEditorPane(editorPane, true);
				Editor.SetValueFromEditorPane(editorPane, Guid.Empty);
				AssertEquals("ErrorMessage", Res.GetString("739ed02f-98de-4efa-9db4-630401e839d6", "Please select a valid address from an organization."), Editor.GetCustomValidation(editorPane));
				Editor.SetValueFromEditorPane(editorPane, Guid.NewGuid());
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ZAddressRegistryItemEditor(null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new ZAddressRegistryItemEditorForTest(null).GetEditorPaneType();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			GuidRegistryItem result = new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new ZAddressRegistryEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { Guid.NewGuid(), Guid.Empty };
		}

		#endregion
	}
}
