using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer.Testing
{
	[TestedType(typeof(LogDocumentRendererRegistry))]
	class LogDocumentRendererRegistryTest : RegistryBusinessObjectTemplateTestCase<LogDocumentRendererRegistry>
	{
		protected override LogDocumentRendererRegistry GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override LogDocumentRendererRegistry GetBusinessObjectToSerialise()
		{
			var result = (LogDocumentRendererRegistry)GetNewBusinessObject();

			result.LogFilePath = "log.txt";
			result.GenerateCallStacks = ZBool.True;

			return result;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
