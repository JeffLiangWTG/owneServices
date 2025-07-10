using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToClone, CloneType cloneType)
			: base(declarationToClone, cloneType)
		{
		}

		protected override void DeepCopyCountrySpecificDataCore(BaseJobDeclaration clonedResult)
		{
			var oldDec = ((JobDeclaration)bizObjToClone);
			var newDec = (JobDeclaration)clonedResult;

			if (IsTemplateCopy)
			{
				CloneCountrySpecificData(oldDec, newDec);
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { JobDeclaration.Schema.JE_UCR });
			return base.CloneInternal(args);
		}

		protected void CloneCountrySpecificData(JobDeclaration oldDec, JobDeclaration newDec)
		{
			newDec.FixedJobMessageType = oldDec.FixedJobMessageType;

			if (oldDec != null && newDec != null)
			{
				newDec.CustomsOffices.CloneFrom(oldDec.CustomsOffices);
				newDec.CustomsEnclosures.CloneFrom(oldDec.CustomsEnclosures);
			}
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new JobComInvoiceHeaderDeepCloneStrategy(invoiceToClone as JobComInvoiceHeader, cloneType, clonedDeclaration, pkPairsDictionaryCollection);
		}
	}
}
