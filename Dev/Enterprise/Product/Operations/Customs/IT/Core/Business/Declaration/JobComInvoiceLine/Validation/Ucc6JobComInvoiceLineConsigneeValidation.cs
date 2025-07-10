using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class Ucc6JobComInvoiceLineConsigneeValidation
{
	public Ucc6JobComInvoiceLineConsigneeValidation(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	readonly JobComInvoiceLine invoiceLine;

	public void ValidateConsignee()
	{
		var consigneeAddressPk = invoiceLine.JI_OA_ConsigneeAddress;
		var consigneeAddressInfo = invoiceLine.JI_OA_ConsigneeAddressInfo;
		var consigneeHumanReadableName = consigneeAddressInfo.HumanReadableName;

		if (consigneeAddressPk.IsEmpty)
		{
			DoMandatoryValidation(consigneeAddressInfo, consigneeAddressPk);
		}
		else
		{
			new CustomsAddressValidator(invoiceLine.ConsigneeAddress, consigneeHumanReadableName, Declaration)
				.ValidateMaximumLengthCustomsFields(consigneeAddressInfo);
		}
	}

	#region Implementation

	void DoMandatoryValidation(ZPropertyInfo consigneeAddressInfo, ZGuid consigneeAddressPk)
	{
		if (!ConsigneeAtHeaderLevelIsEmpty())
		{
			return;
		}

		new HeaderOrLineValueValidator<ZGuid>(
			headerValueProvider: () => consigneeAddressPk,
			lineValuesProvider: () => GetInvoiceLinesRelatedToSameInstruction().Select(x => x.JI_OA_ConsigneeAddress))
		{
			IsEmptyFunc = x => x.IsEmpty
		}
		.ValidateLine(consigneeAddressInfo, consigneeAddressPk);
	}

	bool ConsigneeAtHeaderLevelIsEmpty()
	{
		return Declaration?.ImporterDocumentaryAddress.OrganisationPK.IsEmpty ?? false;
	}

	JobDeclaration Declaration => invoiceLine.Declaration;

	IEnumerable<JobComInvoiceLine> GetInvoiceLinesRelatedToSameInstruction()
	{
		return invoiceLine.EntryInstruction
			?.InvoiceLines
			.Cast<JobComInvoiceLine>() ?? Enumerable.Empty<JobComInvoiceLine>();
	}

	#endregion
}
