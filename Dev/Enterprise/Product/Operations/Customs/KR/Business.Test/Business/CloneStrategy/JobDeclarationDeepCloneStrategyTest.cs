using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business.Testing
{
	class JobDeclarationDeepCloneStrategyTest : Customs.Business.Testing.JobDeclarationDeepCloneStrategyAbstractTest<JobDeclaration>
	{
		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(JobDeclaration declarationToClone, Customs.Business.CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			return new JobDeclarationDeepCloneStrategy(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		protected override ZString AddInfoTestData => "CustomsDivision=10";
	}
}
