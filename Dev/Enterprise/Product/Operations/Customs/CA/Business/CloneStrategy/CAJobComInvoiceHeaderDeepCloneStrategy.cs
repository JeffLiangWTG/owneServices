using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CAJobComInvoiceHeaderDeepCloneStrategy : JobComInvoiceHeaderDeepCopyStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public CAJobComInvoiceHeaderDeepCloneStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, clonedDeclaration, clonedDeclaration.Factory, pkPairsDictionaryCollection)
		{
		}

		protected new JobComInvoiceHeader InvoiceToClone
		{
			get { return (JobComInvoiceHeader)bizObjToClone; }
		}

		protected override JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new CAJobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection);
		}
	}
}
