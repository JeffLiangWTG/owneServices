using CargoWise.Types;

namespace Enterprise.Accounting.Business.Riba
{
	public interface ICollectionBatchValidation
	{
		ZString GetPreSaveValidationMessage();
	}
}
