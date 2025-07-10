namespace Enterprise.Accounting.Business.Riba
{
	public interface ICollectionBatchFileGeneratorProvider
	{
		ICollectionBatchValidation GetValidation();
		ICollectionBatchFileGenerator GetHelper();
	}
}
