using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceHeaderDeepCopyStrategy : Customs.Business.JobComInvoiceHeaderDeepCopyStrategy
	{
		public JobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, CloneType cloneType)
			: this(invoiceToClone, cloneType, null, invoiceToClone.Factory, null)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: this(invoiceToClone, cloneType, clonedDeclaration, clonedDeclaration.Factory, pkPairsDictionaryCollection)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, CloneType cloneType, BaseJobDeclaration clonedDeclaration, BusinessObjectFactory alternativeFactoryToInstantiateInWhichToClone, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceToClone, cloneType, clonedDeclaration, alternativeFactoryToInstantiateInWhichToClone, pkPairsDictionaryCollection)
		{
			this.clonedDeclaration = clonedDeclaration;
		}

		new JobComInvoiceHeader InvoiceToClone
		{
			get { return (JobComInvoiceHeader)base.InvoiceToClone; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			JobComInvoiceHeader clonedByBase = (JobComInvoiceHeader)base.CloneInternal(args);

			clonedByBase.PreviousDocuments.AddCloneFrom(InvoiceToClone.PreviousDocuments, args);
			clonedByBase.AdditionalInfos.AddCloneFrom(InvoiceToClone.AdditionalInfos, args);
			clonedByBase.SupportingDocuments.AddCloneFrom(InvoiceToClone.SupportingDocuments, args);
			return clonedByBase;
		}

		protected override Customs.Business.JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection);
		}

		protected override void LinkInvoiceToDeclaration(BaseJobComInvoiceHeader clonedInvoice)
		{
			var invoice = (JobComInvoiceHeader)clonedInvoice;
			using (invoice.SuspendAddingSupportingDocumentAutomatically())
			{
				base.LinkInvoiceToDeclaration(clonedInvoice);
			}
		}
	}
}
