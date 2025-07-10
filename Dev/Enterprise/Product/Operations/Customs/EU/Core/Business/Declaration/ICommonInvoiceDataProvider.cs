namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ICommonInvoiceDataProvider : Customs.Business.ICommonInvoiceDataProvider
	{
		bool IsUCC6 { get; }
		bool IsUCC6AndIsExport { get; }
	}
}
