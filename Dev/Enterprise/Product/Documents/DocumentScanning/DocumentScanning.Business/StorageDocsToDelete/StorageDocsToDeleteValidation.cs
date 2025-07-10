//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStorageDocsToDeleteValidation
//
//    This class should be used for overriding validation in AutoStorageDocsToDeleteValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsToDeleteValidation : AutoStorageDocsToDeleteValidation
	{
		public StorageDocsToDeleteValidation(AutoStorageDocsToDelete parent) : base(parent)
		{
		}
	}
}
