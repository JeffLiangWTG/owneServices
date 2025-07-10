using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba
{
	class ITAFileGeneratorProvider : ICollectionBatchFileGeneratorProvider
	{
		public ITAFileGeneratorProvider(AccCollectionBatch accCollectionBatch, IEnvironment environment)
		{
			collectionBatch = Argument.NotNull(accCollectionBatch, nameof(accCollectionBatch));
			company = Argument.NotNull((GlbCompany)environment.CurrentCompany, nameof(environment.CurrentCompany));
			isProductionSystem = environment.IsProductionSystem;
			dateTime = environment.Time.CurrentLocalDateTime;
		}
		protected readonly AccCollectionBatch collectionBatch;
		protected readonly GlbCompany company;
		protected readonly bool isProductionSystem;
		protected readonly ZDateTime dateTime;

		ICollectionBatchValidation ICollectionBatchFileGeneratorProvider.GetValidation()
		{
			return new FileGeneratorITAFileFormatDataValidation(collectionBatch, company, isProductionSystem, dateTime);
		}

		ICollectionBatchFileGenerator ICollectionBatchFileGeneratorProvider.GetHelper()
		{
			return new FileGeneratorITAFileFormatDataHelper(collectionBatch, company, isProductionSystem, dateTime);
		}
	}
}
