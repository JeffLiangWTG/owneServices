using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class JobComInvoiceLineDeepCopyStrategy : Customs.Business.JobComInvoiceLineDeepCloneStrategy
	{
		public JobComInvoiceLineDeepCopyStrategy(JobComInvoiceLine invoiceLineToClone, Customs.Business.CloneType cloneType, JobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		new JobComInvoiceHeader clonedInvoice
		{
			get { return (JobComInvoiceHeader)base.clonedInvoice; }
		}

		new JobComInvoiceLine bizObjToClone
		{
			get { return (JobComInvoiceLine)base.bizObjToClone; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobComInvoiceLine)base.CloneInternal(args);

			if (IsTemplateCopy && clonedInvoice.JobDeclaration != null && clonedInvoice.JobDeclaration.IsQuarantine)
			{
				_ = bizObjToClone.QuarantineExDocLine.Clone(result, FreightUtilities.GetValueOrDefault(pkPairsDictionaryCollection, JobDeclarationDeepCloneStrategy.JobDocAddressPKPairsKey));	// Using FreightUtilities directly to avoid confusion with System.Collections.Generic.GetValueOrDefault
				result.Declaration.IsAQISCertificateRequest = bizObjToClone.Declaration.IsAQISCertificateRequest;
				bizObjToClone.RFPNumbers.Clone(result);
			}

			return result;
		}
	}
}
