using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public sealed class AdditionalProcedureCode : EU.Business.AdditionalProcedureCode
{
	public AdditionalProcedureCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusCodeDataValidation GetNewValidation()
	{
		if (Parent is JobComInvoiceLine invoiceLine && IsUcc6AndIsExport())
		{
			return new ExportUcc6AdditionalProcedureCodeValidation(this);
		}
		return base.GetNewValidation();

		bool IsUcc6AndIsExport() => invoiceLine.Declaration?.IsUCC6AndIsExport ?? false;
	}
}
