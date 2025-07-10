//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStorageDocsBarcodeValidation
//
//    This class should be used for overriding validation in AutoStorageDocsBarcodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsBarcodeValidation : AutoStorageDocsBarcodeValidation
	{
		public StorageDocsBarcodeValidation(AutoStorageDocsBarcode parent) : base(parent)
		{
		}
	}
}
