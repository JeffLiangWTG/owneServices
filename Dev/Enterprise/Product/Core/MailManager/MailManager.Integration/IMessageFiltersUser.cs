namespace Enterprise.MailManager.Integration
{
	public interface IMessageFiltersUser
	{
		bool NeedFactory();
		void SetFactory(IMessageProcessorFactory factory);
	}
}
