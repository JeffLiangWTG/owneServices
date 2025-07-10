using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusEntryLine : EU.Business.Declaration.CusEntryLine
	, Integration.Customs.BE.ICusEntryLine
{
	public CusEntryLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
	{
		return new CusEntryLineFeeCollection(this, Factory);
	}

	public new CusEntryLineFeeCollection Fees => (CusEntryLineFeeCollection)base.Fees;
}
