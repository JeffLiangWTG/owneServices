using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	public interface IOperationalActionLog : IOperationalActionSectionLog
	{
		void SetMasterProgressMax(int max);
		void BumpMasterProgress();
		OperationalActionLogErrorLevel HighestErrorLevelEncountered { get; }
	}
}
