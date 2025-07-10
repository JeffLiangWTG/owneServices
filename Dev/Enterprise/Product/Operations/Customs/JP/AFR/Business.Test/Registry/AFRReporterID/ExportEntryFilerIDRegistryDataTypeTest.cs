using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(AFRReporterIDRegistryDataType))]
	sealed class ExportEntryFilerIDRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AFRReporterIDRegistryDataType>
	{
		protected override string ExpectedEditorName => "AFRReporterIDRegistryItemEditor";

		protected override AFRReporterIDRegistryDataType GetNewDataType()
		{
			return new AFRReporterIDRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var filer1 = new AFRReporterID
			{
				ReporterID = "12345",
				Password = "2345"
			};

			var filer2 = new AFRReporterID
			{
				ReporterID = "23456",
				Password = "1111"
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(filer1, new AFRReporterIDRegistryDataType().Serialise(filer1)),
				new ValidSampleAndBinaryValueInDB(filer2, new AFRReporterIDRegistryDataType().Serialise(filer2)),
			};
		}
	}
}
