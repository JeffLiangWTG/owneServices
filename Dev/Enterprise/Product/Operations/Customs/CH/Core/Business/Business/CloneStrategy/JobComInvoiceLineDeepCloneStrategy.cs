using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class JobComInvoiceLineDeepCloneStrategy : Customs.Business.JobComInvoiceLineDeepCloneStrategy
{
	public JobComInvoiceLineDeepCloneStrategy(JobComInvoiceLine invoiceLineToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
		: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
	{
	}

	JobComInvoiceLine InvoiceLine => invoiceLine ??= (JobComInvoiceLine)invoiceLineToClone;
	JobComInvoiceLine invoiceLine;

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var result = base.CloneInternal(args);
		var chResult = (JobComInvoiceLine)result;

		using (result.GetValidationSuspender())
		using (result.SuspendSettingHasChanges())
		{
			chResult.AdditionalTaxes.CloneFrom(InvoiceLine.AdditionalTaxes, args);
			chResult.AdditionalFees.CloneFrom(InvoiceLine.AdditionalFees, args);
		}

		return chResult;
	}
}
