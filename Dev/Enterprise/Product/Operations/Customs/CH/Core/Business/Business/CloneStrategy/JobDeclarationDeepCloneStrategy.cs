using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
{
	public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToClone, CloneType cloneType)
		: base(declarationToClone, cloneType)
	{
	}

	public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
	{
	}

	protected override JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
	{
		return new JobComInvoiceHeaderDeepCloneStrategy((JobComInvoiceHeader)invoiceToClone, cloneType, (JobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
	}
}
