namespace Enterprise.ZArchitecture.Business.Business.EventManagement.Interfaces
{
	public interface ICrudEventProvider
	{
		Event AddEvent { get; }
		Event DeleteEvent { get; }
		Event ModifiedEvent { get; }
		Event ReadEvent { get; }
	}
}
