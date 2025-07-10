using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class JobComInvoiceHeaderCleanUpStrategy : ICleanUpStrategy
{
	public JobComInvoiceHeaderCleanUpStrategy(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		declaration = Argument.NotNull(invoiceHeader.JobDeclaration, nameof(invoiceHeader.JobDeclaration));
	}

	readonly JobComInvoiceHeader invoiceHeader;
	readonly JobDeclaration declaration;

	void ICleanUpStrategy.CleanUp()
	{
		if (!declaration.IsUCC6)
		{
			CleanUpNonUcc6InCompatibleData();
			return;
		}

		CleanUpUcc6InCompatibleData();
	}

	void CleanUpUcc6InCompatibleData()
	{
		invoiceHeader.PreviousDocuments.RemoveAndDeleteAll();
	}

	void CleanUpNonUcc6InCompatibleData()
	{
		invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
		invoiceHeader.JZ_AdditionalTerms = ZString.Empty;

		if (declaration.IsExport)
		{
			invoiceHeader.AdditionalInfos.RemoveAndDeleteAll();
		}
	}
}
