using static Enterprise.Integration.Forwarding;

namespace Enterprise.Integration.Freight
{
	public interface IUniqueConsolProvider
	{
		/// <summary>
		/// Returns the Consol if there is only one. Otherwise null.
		/// </summary>
		IForwardingConsol UniqueConsol { get; }
	}
}
