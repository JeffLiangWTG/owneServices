using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HBLPackLinesDisplayOrderRegistryItemEditor))]
	sealed class HBLPackLinesDisplayOrderRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "EDF");
			list.AddPair("XYZ", "OPQ");

			var listProvider = new CodeDescriptionPairListProvider(() => list);
			var editorInfo = new HBLPackLinesDisplayOrderRegistryEditorInfo(listProvider);

			return new HBLPackLinesDisplayOrderRegistryItemEditor(new HBLPackLinesDisplayOrderRegistryDataType(listProvider), editorInfo);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(HBLPackLinesDisplayOrderControl);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "ABC", "XYZ" };
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("ABC", "EDF");
				list.AddPair("XYZ", "OPQ");
				return list;
			});

			return new CodePairRegistryItem(new RegistryItemImpl("", (NoResString)"", (NoResString)"", (NoResString)"",
				new HBLPackLinesDisplayOrderRegistryDataType(listProvider), RegistryStorageFlags.System, "ABC"));
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
