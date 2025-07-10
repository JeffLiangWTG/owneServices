using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceHeaderDeepCloneStrategy : JobComInvoiceHeaderDeepCopyStrategy
	{
		public JobComInvoiceHeaderDeepCloneStrategy(JobComInvoiceHeader invoice, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: this(invoice, cloneType, clonedDeclaration, clonedDeclaration.Factory, pkPairsDictionaryCollection)
		{
		}

		public JobComInvoiceHeaderDeepCloneStrategy(JobComInvoiceHeader invoice, CloneType cloneType, BaseJobDeclaration clonedDeclaration, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, clonedDeclaration, alternativeFactoryToInstantiateCloneIn, pkPairsDictionaryCollection)
		{
		}

		new JobComInvoiceHeader InvoiceToClone => (JobComInvoiceHeader)base.InvoiceToClone;
		JobDeclaration ClonedDeclaration => (JobDeclaration)base.clonedDeclaration;

		protected override Customs.Business.JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone as JobComInvoiceLine, cloneType, clonedInvoice, pkPairsDictionaryCollection);
		}

		protected override void CloneExtraInvoiceDataInSpecificCountry(BusinessObjectCloneArgs args, BaseJobComInvoiceHeader clonedInvoice)
		{
			base.CloneExtraInvoiceDataInSpecificCountry(args, clonedInvoice);
			if (IsTemplateCopy)
			{
				if (ClonedDeclaration.IsImport)
				{
					var brClonedInvoice = clonedInvoice as JobComInvoiceHeader;
					brClonedInvoice.ExchangeHedgeCollection.CloneFrom(InvoiceToClone.ExchangeHedgeCollection);
				}
			}
		}
	}
}
