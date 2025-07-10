using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToCopy, Customs.Business.CloneType cloneType, BusinessObjectFactory factory)
			: base(declarationToCopy, cloneType, factory)
		{
		}

		protected override Customs.Business.JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(Customs.Business.BaseJobComInvoiceHeader invoiceToClone, Customs.Business.BaseJobDeclaration clonedDeclaration)
		{
			return new JobComInvoiceHeaderDeepCopyStrategy((JobComInvoiceHeader)invoiceToClone, cloneType, (JobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
		}
	}
}
//tested in JobComInvoiceHeaderDeepCopyStrategyTest
