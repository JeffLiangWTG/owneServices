using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLinePackagePivotCollection : Customs.Business.InvoiceLinePackagePivotCollection
{
	public InvoiceLinePackagePivotCollection(BaseJobComInvoiceLine line)
		: base(line)
	{
	}

	public new InvoiceLinePackagePivot AddNew()
	{
		return (InvoiceLinePackagePivot)base.AddNew();
	}

	public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(InvoiceLinePackagePivot);

	public new InvoiceLinePackagePivot this[int index]
	{
		get { return (InvoiceLinePackagePivot)Elements[index]; }
	}
}
