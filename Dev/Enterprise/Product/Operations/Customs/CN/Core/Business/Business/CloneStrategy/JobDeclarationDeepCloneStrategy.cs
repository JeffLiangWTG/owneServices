using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn) : base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		new JobDeclaration DeclarationToClone => (JobDeclaration)base.DeclarationToClone;

		protected override IEnumerable<JobDocAddress> CountrySpecificJobDocAddresses
		{
			get
			{
				yield return DeclarationToClone.ManufacturerDocumentaryAddress;
				yield return DeclarationToClone.BuyerDocAddress;
			}
		}
	}
}
