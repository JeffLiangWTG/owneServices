using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class Ucc6ExportJobComInvoiceLineConsignorValidator
{
	public Ucc6ExportJobComInvoiceLineConsignorValidator(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	readonly JobComInvoiceLine invoiceLine;

	internal void Validate()
	{
		var entryInstruction = invoiceLine.EntryInstruction;
		var supplierID = invoiceLine.Declaration?.JE_OH_Supplier ?? ZGuid.Empty;

		var targetPropertyInfo = invoiceLine.JI_OA_ExporterAddressInfo;

		CheckConsignorAddressFieldsMaximumLengths(targetPropertyInfo);

		if (entryInstruction == null || !supplierID.IsEmpty)
		{
			return;
		}

		var allInvoiceLines = entryInstruction.InvoiceLines
			.Cast<JobComInvoiceLine>()
			.ToArray();

		CheckConsignorIsPresentInInvoiceLines(supplierID, allInvoiceLines, targetPropertyInfo);

		CheckExporterAddressIsEmptyAndConsignorIsPresentInInvoiceLines(allInvoiceLines, targetPropertyInfo);
	}

	void CheckConsignorIsPresentInInvoiceLines(ZGuid supplierID, JobComInvoiceLine[] allInvoiceLines, ZPropertyInfo targetPropertyInfo)
	{
		new HeaderOrLineValueValidator<ZGuid>(
			headerValueProvider: () => supplierID,
			lineValuesProvider: () => allInvoiceLines.Select(x => x.JI_OA_ExporterAddress))
		{
			IsEmptyFunc = x => x.IsEmpty,
			EmptyHeaderAndLinesMessageProvider = () => ValidationCaptions.UCC6TraderJobDocAddressValidation.SupplierOrConsignorsMustBeFilled
		}
		.ValidateHeader(targetPropertyInfo);
	}

	void CheckConsignorAddressFieldsMaximumLengths(ZPropertyInfo targetPropertyInfo)
	{
		var consignorAddress = invoiceLine.Factory.Load<OrgAddress>(invoiceLine.JI_OA_ExporterAddress);

		if (consignorAddress != null && invoiceLine.Declaration != null)
		{
			new CustomsAddressValidator(consignorAddress, HumanReadableTargetPropertyName, invoiceLine.Declaration, shouldApplyTransitionPeriod: false)
				.ValidateMaximumLengthCustomsFields(targetPropertyInfo);
		}
	}

	void CheckExporterAddressIsEmptyAndConsignorIsPresentInInvoiceLines(JobComInvoiceLine[] invoiceLines, ZPropertyInfo targetPropertyInfo)
	{
		var invoiceLinesExceptCurrentLine = invoiceLines.Where(i => i.PK != invoiceLine.PK).ToArray();
		if (invoiceLine.JI_OA_ExporterAddress.IsEmpty && IsConsignorSpecifiedOnAnyInvoiceLine(invoiceLinesExceptCurrentLine))
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo, HumanReadableTargetPropertyName);
		}
	}

	bool IsConsignorSpecifiedOnAnyInvoiceLine(JobComInvoiceLine[] invoiceLines) => invoiceLines.Any(i => !i.JI_OA_ExporterAddress.IsEmpty);

	string HumanReadableTargetPropertyName => Res.GetString("de69abab-7be5-4833-b112-b9efcbc07675", "Consignor");
}
