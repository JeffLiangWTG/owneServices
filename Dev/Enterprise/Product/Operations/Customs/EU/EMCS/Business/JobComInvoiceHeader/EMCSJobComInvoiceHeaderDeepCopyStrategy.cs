using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceHeaderDeepCopyStrategy : JobComInvoiceHeaderDeepCopyStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public EMCSJobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceToClone, cloneType, clonedDeclaration, clonedDeclaration.Factory, pkPairsDictionaryCollection)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			return (EMCSJobComInvoiceHeader)base.CloneInternal(args);
		}

		protected override JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new EMCSJobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection);
		}
	}
}
