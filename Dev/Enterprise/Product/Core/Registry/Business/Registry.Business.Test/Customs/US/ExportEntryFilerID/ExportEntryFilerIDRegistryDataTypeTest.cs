using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(ExportEntryFilerIDRegistryDataType))]
	sealed class ExportEntryFilerIDRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ExportEntryFilerIDRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "ExportEntryFilerIDRegistryItemEditor"; }
		}

		protected override ExportEntryFilerIDRegistryDataType GetNewDataType()
		{
			return new ExportEntryFilerIDRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "12-1234560";
			filer.EntryFilerIDType = AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(filer, new ExportEntryFilerIDRegistryDataType().Serialise(filer))
			};
		}
	}
}
