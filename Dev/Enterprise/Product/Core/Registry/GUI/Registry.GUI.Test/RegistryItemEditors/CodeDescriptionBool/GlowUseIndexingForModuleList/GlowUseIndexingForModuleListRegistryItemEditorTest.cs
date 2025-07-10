using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowUseIndexingForModuleListRegistryItemEditor))]
	sealed class GlowUseIndexingForModuleListRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestValue()
		{
			var editor = GetEditor();
			using (var editorPane = editor.NewWinFormsEditorPane())
			{
				var list = new string[] { "GlbStaff", "JobShipment" };
				editor.SetValueFromEditorPane(editorPane, list);
				var value = editor.GetValueFromEditorPane(editorPane) as string[];
				AssertContainsExactElementsInAnyOrder(list, value);
			}
		}

		public void TestSetValueFromEditorPane()
		{
			var editor = GetEditor();

			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IGlbStaff" });
			}))
			using (var editorPane = editor.NewWinFormsEditorPane())
			{
				var list = new string[] { "GlbStaff", "Dummy" };
				editor.SetValueFromEditorPane(editorPane, list);
				var codeDescriptions = ((IDataBoundControl)editorPane).DataSource as CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection;
				AssertNotNull(codeDescriptions);

				var verifiedModules = codeDescriptions.OfType<CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString>()
					.Where(row => row.String1.IsEmpty).Select(r => r.Code);
				var unverifiedModules = codeDescriptions.OfType<CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString>()
					.Where(row => row.String1 == "✔️").Select(r => r.Code);

				AssertCollectionContains("GlbStaff", verifiedModules);
				AssertCollectionNotContains("Dummy", verifiedModules);

				AssertCollectionNotContains("GlbStaff", unverifiedModules);
				AssertCollectionContains("Dummy", unverifiedModules);
			}
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var editorInfo = new GlowUseIndexingForModuleListEditorInfo();
			return new StringArrayRegistryItem("TEST", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, Array.Empty<string>(), editorInfo);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new GlowUseIndexingForModuleListRegistryItemEditor(new DelimitedStringArrayRegistryDataType());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionBoolWithExtraColumnControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var list = new string[] { "JobShipment", "GlbStaff" };
			return new[] { list };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionBoolWithExtraColumnControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override void TearDown()
		{
			GlowModuleToCW1ModuleConverter.ClearEntityTypesCache();
			base.TearDown();
		}
	}
}
