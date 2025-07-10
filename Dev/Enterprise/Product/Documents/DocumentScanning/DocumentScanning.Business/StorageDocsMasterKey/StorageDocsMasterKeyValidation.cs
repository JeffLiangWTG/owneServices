//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStorageDocsMasterKeyValidation
//
//    This class should be used for overriding validation in AutoStorageDocsMasterKeyValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsMasterKeyValidation : AutoStorageDocsMasterKeyValidation
	{
		public StorageDocsMasterKeyValidation(AutoStorageDocsMasterKey parent) : base(parent)
		{
		}
	}
}
