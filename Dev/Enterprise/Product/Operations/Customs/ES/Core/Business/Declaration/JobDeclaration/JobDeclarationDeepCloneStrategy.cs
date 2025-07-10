using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using EUDeclaration = Enterprise.Customs.EU.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class JobDeclarationDeepCloneStrategy : EU.Business.Declaration.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType)
			: base(declarationToClone, cloneType, declarationToClone.Factory)
		{
		}

		protected override void CopyGuarantees(EUDeclaration oldDec, EUDeclaration newDec, BusinessObjectCloneArgs args)
		{
			base.CopyGuarantees(oldDec, newDec, args);
			newDec.Guarantees.ForEach(x => ((GuaranteeForDeclaration)x).PW_BondAmount = 0);
		}
	}
}
