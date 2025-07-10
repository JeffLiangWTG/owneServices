//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStorageReferenceValidation
//
//    This class should be used for overriding validation in AutoStorageReferenceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentScanning.Business
{
	public class StorageReferenceValidation : AutoStorageReferenceValidation
	{
		public StorageReferenceValidation(AutoStorageReference parent) : base(parent)
		{
		}
	}
}
