using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusEntryLineFeeCollection : CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>, IEUCusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
{
	public CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory)
		: base(entryLine, factory)
	{
		this.entryLine = entryLine;
	}

	readonly CusEntryLine entryLine;

	protected override BusinessObject AddNewCore()
	{
		var newObj = (CusEntryLineFee)base.AddNewCore();
		newObj.CF_MethodOfPayment = entryLine?.Declaration?.JE_PaymentMethod ?? ZString.Empty;
		return newObj;
	}
}
