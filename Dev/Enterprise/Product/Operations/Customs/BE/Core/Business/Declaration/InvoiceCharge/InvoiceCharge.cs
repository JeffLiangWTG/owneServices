using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BE.Business.Declaration;

public class InvoiceCharge : EU.Business.Declaration.InvoiceCharge, Integration.Customs.BE.IInvoiceCharge
{
	public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;
}
