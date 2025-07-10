namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public interface IProcessTaskHandler
	{
		void Fire();
		void Withdraw();
	}
}
