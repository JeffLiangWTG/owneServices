using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer.Testing
{
	[TestedType(typeof(LogDocumentRendererRegistryDataType))]
	class LogDocumentRendererRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<LogDocumentRendererRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "LogDocumentRendererRegistryItemEditor"; }
		}

		protected override LogDocumentRendererRegistryDataType GetNewDataType()
		{
			return new LogDocumentRendererRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(new LogDocumentRendererRegistry(), System.Array.Empty<byte>()) };
		}
	}
}
