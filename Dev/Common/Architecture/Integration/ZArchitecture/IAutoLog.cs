namespace CargoWise.Integration
{
	public interface IAutoLog
	{
		// Once activated, both Audit Events (ADD/EDT/DEL) and Business Events (INA/ACT) will be logged. (At the same time, it will ignore the `IsAutoLogOnlyEnabledForACT` switch.)
		bool IsAutoLogOnlyEnabledForACT { get; }
		bool IsAutoAdminBusinessObjectLoggerEnabled { get; }
	}
}
