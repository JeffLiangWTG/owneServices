using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Registry.Testing
{
	[TestedType(typeof(LogDocumentRendererRegistryControl))]
	sealed class LogDocumentRendererRegistryControlTestCase : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new LogDocumentRendererRegistry();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly || businessEntity.IsReadOnly;
		}
	}
}
