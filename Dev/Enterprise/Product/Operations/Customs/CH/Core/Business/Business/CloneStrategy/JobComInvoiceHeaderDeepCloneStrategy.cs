using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class JobComInvoiceHeaderDeepCloneStrategy : JobComInvoiceHeaderDeepCopyStrategy
{
	public JobComInvoiceHeaderDeepCloneStrategy(JobComInvoiceHeader invoice, CloneType cloneType)
		: base(invoice, cloneType)
	{
	}

	public JobComInvoiceHeaderDeepCloneStrategy(JobComInvoiceHeader invoice, CloneType cloneType, JobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
		: base(invoice, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
	{
	}

	protected override Customs.Business.JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
	{
		return new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone as JobComInvoiceLine, cloneType, clonedInvoice, pkPairsDictionaryCollection);
	}

	public override BusinessObject Clone()
	{
		var clonedObject = (JobComInvoiceHeader)base.Clone();

		clonedObject.SpecialMentions = ((JobComInvoiceHeader)bizObjToClone).SpecialMentions;

		return clonedObject;
	}
}
