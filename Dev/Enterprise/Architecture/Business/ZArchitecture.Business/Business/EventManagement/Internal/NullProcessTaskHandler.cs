namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public class NullProcessTaskHandler : IProcessTaskHandler
	{
		void IProcessTaskHandler.Fire()
		{
		}

		void IProcessTaskHandler.Withdraw()
		{
		}
	}
}
