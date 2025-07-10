using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToCopy, Customs.Business.CloneType cloneType, BusinessObjectFactory factory)
			: base(declarationToCopy, cloneType, factory)
		{
		}

		protected override HashSet<string> ExcludeCopySystemDefinedValues => new HashSet<string> { JobDeclaration.BGMReferenceCounterString };
	}
}
