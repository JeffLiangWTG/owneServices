using CargoWise.Types;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestClientSpecificFileExtension()
		{
			FileExtensionFilterBuilder.ClientSpecificFileExtension.Value = ZString.Empty;
			CustomFlatFileFormatForTesting format = new CustomFlatFileFormatForTesting();
			AssertEquals("Client specific extension should have been set in the constructor of the format", "XYZ", FileExtensionFilterBuilder.ClientSpecificFileExtension.Value);
		}

		public void TestClientSpecificFileExtensionDescription()
		{
			FileExtensionFilterBuilder.ClientSpecificFileExtension.Value = ZString.Empty;
			FileExtensionFilterBuilder.ClientSpecificFileExtensionDescription.Value = ZString.Empty;
			CustomFlatFileFormatForTesting format = new CustomFlatFileFormatForTesting();
			AssertEquals("Client specific desc should have been set in the constructor of the format", "My Client's special file extension", FileExtensionFilterBuilder.ClientSpecificFileExtensionDescription.Value);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new CustomFlatFileFormatForTesting();
		}
	}
}
