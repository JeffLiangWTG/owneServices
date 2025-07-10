using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ByPackageAndMarkLineComparer : BaseJobComInvoiceLine.LineComparer
{
	protected override int CompareCore(IBaseInvoiceLine lineX, IBaseInvoiceLine lineY)
	{
		var invoiceLineX = (JobComInvoiceLine)lineX;
		var invoiceLineY = (JobComInvoiceLine)lineY;

		var targetlineX = GetTargetLine(invoiceLineX);
		var targetlineY = GetTargetLine(invoiceLineY);

		if (object.ReferenceEquals(targetlineX, targetlineY))
		{
			var packX = (InvoiceLinePackagePivot)invoiceLineX.PackagesPivot.First();
			var packY = (InvoiceLinePackagePivot)invoiceLineY.PackagesPivot.First();

			return packY.CHC_NumberOfPacks.CompareTo(packX.CHC_NumberOfPacks);
		}

		return base.CompareCore(targetlineX, targetlineY);
	}

	JobComInvoiceLine GetTargetLine(JobComInvoiceLine invoiceLine)
	{
		var targetLine = invoiceLine;
		var packagePivot = invoiceLine.PackagesPivot.FirstOrDefault() as InvoiceLinePackagePivot;
		if (packagePivot != null && packagePivot.CHC_NumberOfPacks == 0)
		{
			var packXType = packagePivot.Package?.CW_PackType ?? ZString.Empty;
			var packXMarks = packagePivot.Package?.CW_MarksAndNos ?? ZString.Empty;

			var parentLine = invoiceLine.CusEntryLine.Header.InvoiceLines.LastOrDefault(x => HasSamePackage(x, packXType, packXMarks));

			if (parentLine != null)
			{
				targetLine = parentLine as JobComInvoiceLine;
			}
		}

		return targetLine;
	}

	bool HasSamePackage(BaseJobComInvoiceLine line, ZString packType, ZString packMarksAndNos)
	{
		return line.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(pivot => (pivot.Package?.CW_PackType ?? ZString.Empty) == packType && (pivot.Package?.CW_MarksAndNos ?? ZString.Empty) == packMarksAndNos && pivot.CHC_NumberOfPacks > 0);
	}
}
