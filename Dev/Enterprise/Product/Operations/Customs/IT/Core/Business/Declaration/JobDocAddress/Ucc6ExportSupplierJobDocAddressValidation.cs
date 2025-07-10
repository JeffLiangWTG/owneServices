using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class Ucc6ExportSupplierJobDocAddressValidation : Ucc6TraderJobDocAddressValidation
{
	public Ucc6ExportSupplierJobDocAddressValidation(JobDocAddress addressToValidate, JobDeclaration declaration)
		: base(addressToValidate, GetSupplierHumanReadableName(), declaration, isMandatory: false)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		var consignorsOnInvoiceLine = declaration
			.InvoiceLines
			.Cast<JobComInvoiceLine>()
			.Select(l => l.JI_OA_ExporterAddress)
			.ToArray();

		if (!consignorsOnInvoiceLine.Any())
		{
			return;
		}

		var parent = Parent;
		var supplierInfo = parent.OrganisationPKInfo;
		var orgAddressPk = parent.E2_OA_Address;

		CheckIfAllConsignorsAreEmpty(supplierInfo, orgAddressPk, consignorsOnInvoiceLine);
		ValidateConsignorAndSupplierIdMatch(supplierInfo, orgAddressPk, consignorsOnInvoiceLine);
	}

	void ValidateConsignorAndSupplierIdMatch(ZPropertyInfo supplierInfo, ZGuid supplierAddressId, ZGuid[] consignorsOnInvoiceLine)
	{
		new HeaderOrLineValueValidator<ZGuid>(
			headerValueProvider: () => supplierAddressId,
			lineValuesProvider: () => consignorsOnInvoiceLine)
		{
			IsEmptyFunc = x => x.IsEmpty,
			IgnoredHeaderWithLinesValuesMessageProvider = () => ValidationCaptions.UCC6TraderJobDocAddressValidation.SupplierAtDeclarationLevelWillBeIgnored
		}
		.ValidateHeader(supplierInfo);
	}

	void CheckIfAllConsignorsAreEmpty(ZPropertyInfo supplierInfo, ZGuid supplierAddressId, ZGuid[] consignorsOnInvoiceLine)
	{
		new HeaderOrLineValueValidator<ZGuid>(
			headerValueProvider: () => supplierAddressId,
			lineValuesProvider: () => consignorsOnInvoiceLine)
		{
			IsEmptyFunc = x => x.IsEmpty,
			EmptyHeaderAndLinesMessageProvider = () => ValidationCaptions.UCC6TraderJobDocAddressValidation.SupplierOrConsignorsMustBeFilled
		}
		.ValidateLine(supplierInfo, supplierAddressId);
	}

	static string GetSupplierHumanReadableName() => Res.GetString("275f98b9-e2f5-447f-a550-c163d23e9b2c", "Supplier");
}
