using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public interface ICASendsMessagesToCustoms : ISendsMessagesToCustoms
	{
		ActionPurpose FormActionPurpose { get; set; }
	}
}
