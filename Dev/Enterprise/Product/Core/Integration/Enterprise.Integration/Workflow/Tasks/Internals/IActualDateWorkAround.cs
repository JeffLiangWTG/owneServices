using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IActualDateWorkAround
	{
#if DEBUG
		void SetActualDateForTest(IProcessTask task, ZDateTime value);
#endif
		void SetActualDateAndIKnowIShouldNotBeCallingThis(IProcessTask task, ZDateTime value);
		void SetActualDateAndIKnowIShouldNotBeCallingThis(IProcessTask task, ZDateTimeOffset value);
	}
}
