using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport008Line
	{
		ZString ItemCategory { get; }
		ZString ItemCode { get; }
		ZString InvoiceDescription { get; }
		ZString BrandName { get; }
		ZInt MonthOfUse { get; }
		ZDecimal Quantity { get; }
		ZDecimal Price { get; }
		ZString Model { get; }
	}
}
