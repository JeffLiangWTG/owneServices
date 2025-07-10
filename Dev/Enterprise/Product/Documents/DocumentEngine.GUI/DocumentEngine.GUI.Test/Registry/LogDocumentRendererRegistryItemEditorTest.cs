using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Registry.Testing
{
	[TestedType(typeof(LogDocumentRendererRegistryItemEditor))]
	sealed class LogDocumentRendererRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new LogDocumentRendererRegistryItemEditor(new LogDocumentRendererRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RegistryZUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LogDocumentRendererRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return DocumentsDataRegistry.Instance.LogDocumentRenderer;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[]
			{
				new LogDocumentRendererRegistry() { LogFilePath = ZString.Empty, GenerateCallStacks = ZBool.False },
				new LogDocumentRendererRegistry() { LogFilePath = "log.txt", GenerateCallStacks = ZBool.False },
				new LogDocumentRendererRegistry() { LogFilePath = "log.txt", GenerateCallStacks = ZBool.True }
			};
		}
	}
}
