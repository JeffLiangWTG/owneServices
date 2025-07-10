using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using EUDeclaration = Enterprise.Customs.EU.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobDeclarationDeepCloneStrategy : EU.Business.Declaration.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override void DeepCopyCountrySpecificDataCore(BaseJobDeclaration clonedResult)
		{
			base.DeepCopyCountrySpecificDataCore(clonedResult);
			((JobDeclaration)clonedResult).ResetApplicationExtender();
		}

		protected override void CopyGuarantees(EUDeclaration oldDec, EUDeclaration newDec, BusinessObjectCloneArgs args)
		{
			var oldDecGB = (JobDeclaration)oldDec;
			var newDecGB = (JobDeclaration)newDec;

			foreach (GBGuarantee guarantee in oldDecGB.Guarantees)
			{
				var newGrantee = (GBGuarantee)guarantee.Clone(args);
				newDecGB.Guarantees.Add(newGrantee);

				if (!guarantee.EntryInstructionID.IsEmpty && pkPairsDictionaryCollection != null)
				{
					if (pkPairsDictionaryCollection.GetValueOrDefault(CusEntryInstructionPKPairsKey).TryGetValue(guarantee.EntryInstructionID, out var guaranteePK))
					{
						newGrantee.EntryInstructionID = guaranteePK;
					}
				}
			}
		}
	}
}
