using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public delegate void SavingEventHandler<T>(T bizO)
		where T : BusinessObject;

	public interface ISavingProvider<T>
		where T : BusinessObject
	{
		void FireOnSavingEvent();
		event SavingEventHandler<T> Saving;
	}
}
