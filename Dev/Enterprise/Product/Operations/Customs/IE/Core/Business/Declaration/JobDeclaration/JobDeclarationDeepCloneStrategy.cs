using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobDeclarationDeepCloneStrategy : EU.Business.Declaration.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}
	}
}
