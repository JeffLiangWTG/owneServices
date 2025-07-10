namespace Enterprise.Integration.ZArchitecture
{
#if DEBUG
	public interface IBackgroundAppDomainWorkItemLeakListener { }
	public interface IUserIdleWorkerLeakListener { }
	public interface IColumnLayoutCacheTestListener { }
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IRecentItemManagerTestListener { }
	public interface IZFormActivityLoggerTestListener { }
#endif

	public interface IModuleFilterCollection { }
	public interface IFilterControl { }
	public interface IGridControl { }

	public interface IEnterpriseCodeRetriever
	{
		string EnterpriseCodeFromRegistry { get; }
	}
}
