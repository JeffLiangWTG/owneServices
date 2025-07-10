using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobDeclarationDeepCloneStrategy))]
sealed class JobDeclarationDeepCloneStrategyTest : Customs.Business.Testing.JobDeclarationDeepCloneStrategyAbstractTest<JobDeclaration>
{
	protected override Customs.Business.JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(JobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
	{
		return new JobDeclarationDeepCloneStrategy(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
	}
}
