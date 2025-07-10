using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface IEntryInstructionAutoSplitStrategy
	{
		ZString CheckBeforeSplit();
		int EstimatedSplitCount { get; }
		void AutoSplit();
	}
}
