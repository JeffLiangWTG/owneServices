using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLineFeeCollection : EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
{
	public CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory) : base(entryLine, factory)
	{
	}

	public void ApplyCustomsCompliantSort() => Sort(EntryLineFeeComparer);

	public ZDecimal GetTotalAmount(ZString methodOfPayment) => AllLineFees.Where(x => x.CF_MethodOfPayment == methodOfPayment).Sum(x => x.CF_ChargeAmount);

	protected override bool AllowSort => false;

	protected override bool AllowNewCore
	{
		get
		{
			var parentEntryLine = Master;
			return !parentEntryLine.IsDeleted
					&& parentEntryLine is CusEntryLine itCusEntryLine
					&& (itCusEntryLine.Header?.EntryInstruction?.IsDutiesAndFeeCalculationAllowed() ?? true);
		}
	}

	protected override void OnLoaded()
	{
		base.OnLoaded();

		ApplyCustomsCompliantSort();
	}

	public bool HasExcludeActionForGivenFeeType(ZString feeType)
	{
		return Elements.Cast<CusEntryLineFee>().Any(f => f.CF_ChargeType == feeType && f.IsActionExclude);
	}

	CusEntryLineFeeComparer EntryLineFeeComparer => entryLineFeeComparer ?? (entryLineFeeComparer = new CusEntryLineFeeComparer());
	CusEntryLineFeeComparer entryLineFeeComparer;
}
