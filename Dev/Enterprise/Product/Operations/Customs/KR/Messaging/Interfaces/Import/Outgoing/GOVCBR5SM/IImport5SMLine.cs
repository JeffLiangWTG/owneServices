using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5SMLine
	{
		ZInt EntryLineNo { get; }
		ZString HSCode { get; }
		ZString HSDescription { get; }
		ZString InvoiceDescription { get; }
		ZString BrandName { get; }
		ZString ItemDescription { get; }
		ZString Ingredient { get; }
	}
}
