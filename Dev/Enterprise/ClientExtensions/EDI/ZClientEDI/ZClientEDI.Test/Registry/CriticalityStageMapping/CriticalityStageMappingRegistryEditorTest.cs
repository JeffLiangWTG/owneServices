using System;
using Enterprise.Integration;
using Enterprise.ProcessManagement.GUI.Test;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(CriticalityStageMappingRegistryEditor))]
	public class CriticalityStageMappingRegistryEditorTest : CodeDescriptionBoolTreeRegistryEditorTest
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CriticalityStageMappingRegistryEditor(RegistryItem.DataType, RegistryItem.EditorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CriticalityStageMappingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var editorInfo = new CriticalityStageMappingRegistryEditorInfo(new MultilingualString[] { (NoResString)"1", (NoResString)"1" }, (NoResString)"B", null, new bool[] { true, true }, new bool[] { true, true }, false);
			return new CriticalityStageMappingRegistryItem("", null, null, null, RegistryStorageFlags.System, editorInfo, new CriticalityStageMappingCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var result = new CriticalityStageMappingCollection();
			return new object[] { result };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((CriticalityStageMappingControl)editorPane).ReadOnly;
		}
	}
}
