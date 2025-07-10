using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLinePackagePivot : Customs.Business.InvoiceLinePackagePivot
	, IRN22CheckablePackage
	, IR0364CheckablePackage
	, IR0219CheckablePackage
{
	public InvoiceLinePackagePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.CusHouseContPackInvoiceLinePivotValidation GetNewValidation() => new InvoiceLinePackagePivotValidation(this);

	public new Package Package => (Package)base.Package;

	public override ZPropertyInfo CHC_NumberOfPacksInfo
	{
		get
		{
			var numberOfPacksInfo = base.CHC_NumberOfPacksInfo;
			numberOfPacksInfo.HumanReadableName = Res.GetString("65FA6260-790C-48B3-8C70-3D2FB1F04F98", "Invoice Line Pack Quantity");
			return numberOfPacksInfo;
		}
	}

	IEnumerable<IRN22CheckablePackage> IRN22CheckablePackage.GetRelatedEntryPreviousPackages()
	{
		var entryLine = InvoiceLine?.CusEntryLine;
		var entryHeader = entryLine?.Header;

		if (entryLine != null && entryHeader != null)
		{
			return entryHeader
					.MergedLines
					.Where(x => x.CL_LineNumber < entryLine.CL_LineNumber)
					.OrderByDescending(x => x.CL_LineNumber)
					.SelectMany(x => x.InvoiceLines)
					.Cast<JobComInvoiceLine>()
					.SelectMany(x => x.PackagesPivot)
					.Cast<IRN22CheckablePackage>()
					.ToArray();
		}
		return Enumerable.Empty<IRN22CheckablePackage>();
	}

	IEnumerable<IR0364CheckablePackage> IR0364CheckablePackage.GetRelatedEntryOtherPackagesWithSameTypeAndMarks()
	{
		var entryLine = InvoiceLine?.CusEntryLine;
		var entryHeader = entryLine?.Header;

		if (entryLine != null && entryHeader != null)
		{
			return entryHeader
					.MergedLines
					.Where(x => x.CL_LineNumber != entryLine.CL_LineNumber)
					.SelectMany(x => x.InvoiceLines)
					.Cast<JobComInvoiceLine>()
					.SelectMany(x => x.PackagesPivot)
					.Cast<InvoiceLinePackagePivot>()
					.Where(x => x.Package != null && x.Package.CW_PackType == PackType && x.Package.CW_MarksAndNos == MarksAndNos)
					.Cast<IR0364CheckablePackage>()
					.ToArray();
		}
		return Enumerable.Empty<IR0364CheckablePackage>();
	}

	IEnumerable<IR0364CheckablePackage> IR0364CheckablePackage.GetRelatedEntryPackagesWithSameMarksAndPacksGreaterThanZero()
	{
		var entryLine = InvoiceLine?.CusEntryLine;
		var entryHeader = entryLine?.Header;

		if (entryLine != null && entryHeader != null)
		{
			return entryHeader
					.MergedLines
					.SelectMany(x => x.InvoiceLines)
					.Cast<JobComInvoiceLine>()
					.SelectMany(x => x.PackagesPivot)
					.Cast<InvoiceLinePackagePivot>()
					.Where(x => x.Package != null && x.Package.CW_MarksAndNos == MarksAndNos && x.CHC_NumberOfPacks > 0)
					.Cast<IR0364CheckablePackage>()
					.ToArray();
		}
		return Enumerable.Empty<IR0364CheckablePackage>();
	}

	IEnumerable<IR0219CheckablePackage> IR0219CheckablePackage.GetRelatedEntryPackagesWithPacksEqualToZero()
	{
		var entryLine = InvoiceLine?.CusEntryLine;
		var entryHeader = entryLine?.Header;

		if (entryLine != null && entryHeader != null)
		{
			return entryLine
				.InvoiceLines
				.Cast<JobComInvoiceLine>()
				.SelectMany(x => x.PackagesPivot)
				.Cast<InvoiceLinePackagePivot>()
				.GroupBy(x => x.Package)
				.Where(x => x.Sum(pivot => pivot.CHC_NumberOfPacks) == 0)
				.SelectMany(x => x)
				.Cast<IR0219CheckablePackage>()
				.ToArray();
		}
		return Enumerable.Empty<IR0219CheckablePackage>();
	}

	#region ICheckablePackage

	ZString ICheckablePackage.UnitType => PackType;

	ZString ICheckablePackage.MarksAndNumbers => MarksAndNos;

	ZLong ICheckablePackage.UnitCount => new ZLong(CHC_NumberOfPacks);

	ZPropertyInfo ICheckablePackage.UnitCountInfo => CHC_NumberOfPacksInfo;

	#endregion

	ZString MarksAndNos => Package?.CW_MarksAndNos ?? ZString.Empty;

	ZString PackType => Package?.CW_PackType ?? ZString.Empty;
}
