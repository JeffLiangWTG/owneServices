using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceHeaderDeepCloneStrategy : JobComInvoiceHeaderDeepCopyStrategy
	{
		public JobComInvoiceHeaderDeepCloneStrategy(JobComInvoiceHeader invoice, CloneType cloneType)
			: base(invoice, cloneType)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceHeaderDeepCloneStrategy(JobComInvoiceHeader invoice, CloneType cloneType, JobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
		{
		}
		JobComInvoiceHeader KRSourceInvoiceHeader => krSourceInvoiceHeader ?? (krSourceInvoiceHeader = (JobComInvoiceHeader)InvoiceToClone);
		JobComInvoiceHeader krSourceInvoiceHeader;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = base.CloneInternal(args);
			var krResult = (JobComInvoiceHeader)result;

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				krResult.CertificateOfOriginCollection.CloneFrom(KRSourceInvoiceHeader.CertificateOfOriginCollection, args);
			}

			return krResult;
		}

		protected override Customs.Business.JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone as JobComInvoiceLine, cloneType, clonedInvoice, pkPairsDictionaryCollection);
		}
	}
}
