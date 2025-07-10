using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Business.Testing
{
	public abstract class FlatFileFormatTestCase : TestCaseWithFactory
	{
		protected abstract FlatFileFormat GetFlatFileFormat();

		public void TestGetClientExtensionReturnsValueForExport()
		{
			FlatFileFormat format = GetFlatFileFormat();
			if (format.FileExtensionForExport == FileExtensionType.ClientSpecific)
			{
				Assert(format.GetType().ToString() + " has an empty client specific extension type. Override GetClientSpecificFileExtension() and return the appropriate string extension", !format.ClientSpecificExtensionForTesting.IsEmpty);
			}
			else
			{
				Assert("No problems here. :)", true);
			}
		}

		public void TestGetClientExtensionReturnsValueForImport()
		{
			FlatFileFormat format = GetFlatFileFormat();
			if (format.FileExtensionForImport == FileExtensionType.ClientSpecific)
			{
				Assert(format.GetType().ToString() + " has an empty client specific extension type. Override GetClientSpecificFileExtension() and return the appropriate string extension", !format.ClientSpecificExtensionForTesting.IsEmpty);
			}
			else
			{
				Assert("No problems here. :)", true);
			}
		}
	}
}
