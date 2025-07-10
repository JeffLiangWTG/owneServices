using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class JobComInvoiceHeaderDeepCopyStrategy : Customs.Business.JobComInvoiceHeaderDeepCopyStrategy
	{
		public JobComInvoiceHeaderDeepCopyStrategy(JobComInvoiceHeader invoiceToClone, Customs.Business.CloneType cloneType, JobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceToClone, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
		{
		}

		new JobDeclaration clonedDeclaration => (JobDeclaration)base.clonedDeclaration;

		new JobComInvoiceHeader InvoiceToClone => (JobComInvoiceHeader)base.InvoiceToClone;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobComInvoiceHeader)base.CloneInternal(args);

			if (clonedDeclaration.IsQuarantine)
			{
				InvoiceToClone.QuarantineExDocHeader.Clone(result.PK);
			}

			return result;
		}

		protected override Customs.Business.JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(Customs.Business.BaseJobComInvoiceLine invoiceLineToClone, Customs.Business.BaseJobComInvoiceHeader clonedInvoice)
		{
			return new JobComInvoiceLineDeepCopyStrategy((JobComInvoiceLine)invoiceLineToClone, cloneType, (JobComInvoiceHeader)clonedInvoice, pkPairsDictionaryCollection);
		}
	}
}
