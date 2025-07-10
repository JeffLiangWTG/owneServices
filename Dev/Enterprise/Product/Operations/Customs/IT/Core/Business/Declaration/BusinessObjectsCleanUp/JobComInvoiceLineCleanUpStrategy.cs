using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class JobComInvoiceLineCleanUpStrategy : ICleanUpStrategy
{
	public JobComInvoiceLineCleanUpStrategy(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		declaration = Argument.NotNull(invoiceLine.Declaration, nameof(invoiceLine.Declaration));
	}

	readonly JobComInvoiceLine invoiceLine;
	readonly JobDeclaration declaration;

	void ICleanUpStrategy.CleanUp()
	{
		if (!declaration.IsUCC6)
		{
			invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK = ZGuid.Empty;
			invoiceLine.CusSupplyChainActorReferences.RemoveAndDeleteAll();
			invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			invoiceLine.ZG_CusNumber = ZString.Empty;
		}

		if (!declaration.IsImport)
		{
			invoiceLine.FiscalReferences.RemoveAndDeleteAll();
		}

		if (!declaration.IsUCC6AndIsExport)
		{
			invoiceLine.CusAuthorizationUsages.RemoveAndDeleteAll();
			invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgPK = ZGuid.Empty;
			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
		}
	}
}
