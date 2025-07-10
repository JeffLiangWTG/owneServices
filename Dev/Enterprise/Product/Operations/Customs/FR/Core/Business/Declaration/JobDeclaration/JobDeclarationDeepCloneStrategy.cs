using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobDeclarationDeepCloneStrategy : EU.Business.Declaration.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType)
			: base(declarationToClone, cloneType)
		{
		}

		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { JobDeclarationSchema.Constants.JE_ApplicationCode });
			var newDec = (JobDeclaration)base.CloneInternal(args);
			var oldDec = (JobDeclaration)bizObjToClone;
			newDec.JE_ApplicationCode = oldDec.JE_ApplicationCode;

			return newDec;
		}
	}
}
