namespace Enterprise.Customs.IT.Business;

public interface IUCC6AndTransitionPeriodProvider
{
	bool IsUCC6AndIsExport { get; }
	bool IsTransitionPeriodAES30 { get; }
	bool IsImport { get; }
}
