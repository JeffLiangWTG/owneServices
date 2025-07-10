using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
{
	public EntryCreationStrategy(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine baseInvoiceLine)
	{
		var mergeKey = base.GetKeyForHeaderCore(baseInvoiceLine);
		var invoice = (JobComInvoiceHeader)baseInvoiceLine.InvoiceHeader;

		mergeKey.Add(invoice.JZ_IncoTerm);
		mergeKey.Add(invoice.JZ_IncoTermPlace);
		mergeKey.Add(invoice.ZG_AgreedPlaceCode);

		if (!IsUCC6AndIsExport)
		{
			mergeKey.Add(invoice.JZ_ValuationCode);
		}

		mergeKey.Add(invoice.JZ_RX_NKInvoice_Currency);
		mergeKey.Add(invoice.JZ_AdditionalTerms);
		return mergeKey;
	}

	public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
	{
		var mergeKey = base.GetKeyForLine(baseInvoiceLine);
		var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
		var invoice = invoiceLine.InvoiceHeader;

		mergeKey.Add(invoiceLine.ZG_SteelType);
		mergeKey.Add(GetPackageType(invoiceLine));
		mergeKey.Add(invoiceLine.ZG_PortTaxRate);
		mergeKey.Add(invoiceLine.JI_StateOrRegionOfOrigin);

		if (IsUCC6AndIsExport)
		{
			mergeKey.Add(invoice.JZ_ValuationCode);
		}

		return mergeKey;
	}

	ZString GetPackageType(JobComInvoiceLine invoiceLine)
	{
		var packageType = ZString.Empty;
		var packagePivot = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
		if (packagePivot != null)
		{
			packageType = Declaration.Packages.Cast<BasePackage>().SingleOrDefault(x => x.PK == packagePivot.CHC_CW)?.CW_PackType ?? ZString.Empty;
		}
		return packageType;
	}

	protected override string[] GetAdditionalInfoKeysCore() => System.Array.Empty<string>();

	protected override string[] GetTaxKeysCore() => System.Array.Empty<string>();

	protected override string[] GetSupportingDocumentKeysCore()
	{
		return new string[]
		{
			SupportingDocument.Schema.CSI_Code,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_Status,
			SupportingDocument.Schema.CSI_UnitOfQuantity,
			SupportingDocument.Schema.CSI_YearOfIssue,
			SupportingDocument.Schema.CSI_RN_NKCountryCode,
		};
	}

	protected override string[] GetCusSupplyChainActorReferenceKeysCore()
	{
		return new string[]
		{
			CusSupplyChainActorReference.Schema.CFR_Code,
			CusSupplyChainActorReference.Schema.CFR_Reference,
		};
	}
}
