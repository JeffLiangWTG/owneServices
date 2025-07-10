using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers.Customs.Base;
using CodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.CH.Business;

[AllowNoStaticNew]
public class DocCusEntryLine : DocBaseCusEntryLine
{
	public static DocCusEntryLine New(CusEntryLine entryLine, BusinessObjectFactory factoryToWrap) => entryLine == null ? null : new DocCusEntryLine(entryLine, factoryToWrap);

	protected DocCusEntryLine(CusEntryLine entryLine, BusinessObjectFactory factoryToWrap) : base(entryLine, factoryToWrap)
	{
	}

	public DocJobComInvoiceLine InvoiceLine
	{
		get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
	}

	protected CusEntryLine CusEntryLine => (CusEntryLine)WrappedObject;

	public DocBaseCusEntryHeader EntryHeader
	{
		get => entryHeader ?? (entryHeader = DocBaseCusEntryHeader.New(CusEntryLine.Header, Factory));
		internal set => entryHeader = value;
	}
	DocBaseCusEntryHeader entryHeader;

	public DocDeclaration Declaration
	{
		get => declaration ?? (declaration = DocDeclaration.New(CusEntryLine.Declaration, Factory));
		internal set => declaration = value;
	}
	DocDeclaration declaration;

	#region Overrides

	protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
	{
		return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
	}

	#endregion

	public new ZString Description => CusEntryLine.CL_Description.IsEmpty ? CusEntryLine.RandomLine.JI_Description : CusEntryLine.CL_Description;

	public ZDecimal NetWeight => CusEntryLine.CalcNetWeight;

	public ZDecimal BorderValue => (CusEntryLine.CL_StatisticalValue > 0 && CusEntryLine.CL_StatisticalValue < 1) ? 1m : decimal.Truncate(CusEntryLine.CL_StatisticalValue);

	public ZDecimal GrossWeightInKG => CusEntryLine.CalcGrossWeight;

	public ZBool CommercialGoods => !CusEntryLine.RandomLine.JI_NonTradingGoods;

	public ZDecimal SupplementaryUnits => CusEntryLine.CalcAdditionalQty;

	public ZBool RestrictionObligation => CusEntryLine.RandomLine.Restrictions.Any();

	public ZString ProcedureDescription => CusEntryLine.RandomLine.Lookups.Procedures.GetDescriptionFromCode(CusEntryLine.RandomLine.JI_Procedure);

	public ZString PackagingReferences => GetPackagingReferences();

	string GetPackagingReferences()
	{
		var result = new ZStringBuilder();

		foreach (var packages in CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesPivot.Cast<InvoiceLinePackagePivot>()).GroupBy(x => x.Package.CW_PackType))
		{
			var marksAndNos = string.Join(" ", CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesPivot.Cast<InvoiceLinePackagePivot>().Where(p => p.Package.CW_PackType == packages.Key).Select(pck => pck.Package.CW_MarksAndNos)));

			result.Append($"{packages.Key}, {marksAndNos}, {packages.Sum(x => x.CHC_NumberOfPacks)}");
		}
		return result.ToStringWithDelimiterBetweenAppends("; ");
	}

	public ZString PackageLines => packageLines ??= GetPackageLines();
	string packageLines;

	string GetPackageLines()
	{
		var result = new ZStringBuilder();
		var packingUnitTypesList = CusEntryLine.Declaration.Lookups.PackingUnitTypesList;

		foreach (var packages in CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesPivot.Cast<InvoiceLinePackagePivot>()).GroupBy(x => x.Package.CW_PackType))
		{
			result.Append($"{packingUnitTypesList.GetDescriptionFromCode(packages.Key)} / {packages.Sum(x => x.CHC_NumberOfPacks)}");
		}
		return result.ToStringWithNewLineBetweenAppends();
	}

	public ZString PreviousDocumentLines => previousDocumentLines ??= GetPreviousDocumentLines();
	string previousDocumentLines;

	string GetPreviousDocumentLines()
	{
		var result = new ZStringBuilder();
		var codeType = CusEntryLine.Declaration.IsImport ? CodeListTypes.Codes.PreviousDocumentOfImportDirection : CodeListTypes.Codes.PreviousDocumentOfExportDirection;
		var previousDocumentCodeList = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, codeType, CusEntryLine.RandomLine.EffectiveAssessmentDate);

		foreach (var (code, reference, description) in CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
			.Select(x => x.InvoiceHeader).Distinct().SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>())
			.Select(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)).Distinct())
		{
			result.Append($"{previousDocumentCodeList.GetDescriptionFromCode(code)} / {string.Join(" / ", new[] { reference, description }.Where(x => !x.IsEmpty))}");
		}
		return result.ToStringWithNewLineBetweenAppends();
	}
}
