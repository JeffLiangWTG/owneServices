using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using CusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.BE.Business;

public class Totals : ITtotals
{
	public Totals(CusEntryHeader cusEntry)
	{
		Argument.NotNull(cusEntry, CusEntryHeader.Schema.TableName);
		this.cusEntry = cusEntry;
	}

	readonly CusEntryHeader cusEntry;

	public ZDecimal Items { get => cusEntry.MergedLines.Count; }
	public ZBool ItemsSpecified { get => Items.IsValid && !Items.IsEmpty; }
	public ZDecimal Packages
	{
		get
		{
			return cusEntry.MergedLines.Sum(cl => cl.InvoiceLines.Sum(ji => ((JobComInvoiceLine)ji).PackagesPivot.Sum(pivot => ((InvoiceLinePackagePivot)pivot).Package.CW_PackQty)));
		}
	}
	public ZBool PackagesSpecified { get => Packages.IsValid && !Packages.IsEmpty; }
	public ZDecimal TotalGrossmass
	{
		get
		{
			return cusEntry.MergedLines.Sum(cl => cl.InvoiceLines.Sum(ji => new ZWeight(((JobComInvoiceLine)ji).JI_CustomsQuantity, ((JobComInvoiceLine)ji).JI_CustomsUnitQty).InKilogramsSafe));
		}
	}
	public ZBool TotalGrossmassSpecified { get => TotalGrossmass.IsValid && !TotalGrossmass.IsEmpty; }
	public ZDecimal TotalNetmass
	{
		get
		{
			return cusEntry.MergedLines.Sum(cl => cl.EffectiveNetWeight.InKilogramsSafe);
		}
	}
}
