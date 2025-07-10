using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceLineDeepCloneStrategy : Customs.Business.JobComInvoiceLineDeepCloneStrategy
	{
		public JobComInvoiceLineDeepCloneStrategy(JobComInvoiceLine invoiceLineToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		JobComInvoiceLine KRSourceInvoiceLine => krSourceInvoiceLine ?? (krSourceInvoiceLine = (JobComInvoiceLine)invoiceLineToClone);
		JobComInvoiceLine krSourceInvoiceLine;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = base.CloneInternal(args);
			var krResult = (JobComInvoiceLine)result;

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				if (KRSourceInvoiceLine.CertificateOfOriginData != null)
				{
					krResult.CreateCertificateOfOriginDataIfRequired();
					krResult.CertificateOfOriginData.CloneFrom(KRSourceInvoiceLine.CertificateOfOriginData);
				}
				krResult.GAApprovalDataCollection.CloneFrom(KRSourceInvoiceLine.GAApprovalDataCollection, args);
				krResult.PreApprovalCollection.CloneFrom(KRSourceInvoiceLine.PreApprovalCollection, args);
				krResult.VehicleNumbers.CloneFrom(KRSourceInvoiceLine.VehicleNumbers, args);
			}

			return krResult;
		}
	}
}
