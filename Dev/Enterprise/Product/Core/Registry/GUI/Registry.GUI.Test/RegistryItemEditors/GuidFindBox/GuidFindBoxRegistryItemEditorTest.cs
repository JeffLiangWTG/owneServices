using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GuidFindBoxRegistryItemEditor))]
	sealed class GuidFindBoxRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestModuleID()
		{
			using (ZGuidFindBox editorPane = (ZGuidFindBox)Editor.NewWinFormsEditorPane())
			{
				AssertEquals("ModuleID", ModuleIDs.AccGLHeader, editorPane.ModuleID);
			}
		}

		public void TestIsPrimaryKeyFromCodeRequired()
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode);
			GuidFindBoxRegistryItemEditorForTest newEditor = new GuidFindBoxRegistryItemEditorForTest(null, editorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			newEditor.IsPrimaryKeyFromCodeRequired = true;

			using (ZGuidFindBox editorPane = (ZGuidFindBox)newEditor.NewWinFormsEditorPane())
			{
				AssertEquals("IsPrimaryKeyFromCodeRequired", true, editorPane.IsPrimaryKeyFromCodeRequired);
			}
		}

		[RequiresSTA]
		public void TestListIsNotLoaded()
		{
			using (ZForm testForm = new ZForm())
			using (ZGuidFindBox editorPane = (ZGuidFindBox)Editor.NewWinFormsEditorPane())
			{
				testForm.Controls.Add(editorPane);
				testForm.Show();
				Assert("BusinessObjectCollection should not be loaded as it is bound to a FindBox", !((IBusinessObjectCollection)editorPane.List).IsLoaded);
			}
		}

		public void TestSelectedPK()
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader);
			GuidFindBoxRegistryItemEditorForTest newEditor = new GuidFindBoxRegistryItemEditorForTest(null, editorInfo, null);

			using (ZGuidFindBox editorPane = (ZGuidFindBox)newEditor.NewWinFormsEditorPane())
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

		public void TestGetCustomValidationWithCustomMessage()
		{
			var info = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, default, ResString.GetMultilingualString("7B2DDF28-B9A7-443A-8402-1E1245DB21A3", "Errors should never pass silently."));
			var editor = new GuidFindBoxRegistryItemEditor(null, info, null);

			using (ZGuidFindBox editorPane = (ZGuidFindBox)editor.NewWinFormsEditorPane())
			{
				(((IDataBoundControl)editorPane).DataSource as GuidFindBoxBusinessObject).SelectedPK = ZGuid.Invalid;
				AssertEquals("ErrorMessage", "Errors should never pass silently.", editor.GetCustomValidation(editorPane));
			}
		}

		public void TestGetCustomValidation()
		{
			using (ZGuidFindBox editorPane = (ZGuidFindBox)Editor.NewWinFormsEditorPane())
			{
				Editor.SetValueFromEditorPane(editorPane, Guid.Empty);
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				Editor.SetValueFromEditorPane(editorPane, Guid.NewGuid());
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				AssertEquals("ErrorMessage", "Please select a valid selection.", Editor.GetCustomValidation(new Button()));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new GuidFindBoxRegistryItemEditor(null, new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new GuidFindBoxRegistryItemEditorForTest(null, null, null).GetEditorPaneType();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			GuidRegistryItem result = new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { Guid.NewGuid(), Guid.Empty };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		#region GuidFindBoxRegistryItemEditorForTest

		class GuidFindBoxRegistryItemEditorForTest : GuidFindBoxRegistryItemEditor
		{
			public GuidFindBoxRegistryItemEditorForTest(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallback)
				: base(dataType, editorInfo, fallback)
			{
			}

			public Type GetEditorPaneType()
			{
				return typeof(MyGuidFindBox);
			}

			public void SetValueZGuid(Control editorPane, ZGuid value)
			{
				((MyGuidFindBox)editorPane).DataSource.SelectedPK = value;
			}
		}

		#endregion

		#endregion
	}
}
