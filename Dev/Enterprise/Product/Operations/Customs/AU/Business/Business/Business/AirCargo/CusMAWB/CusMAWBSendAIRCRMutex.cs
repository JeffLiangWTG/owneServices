
namespace Enterprise.Customs.AU.Declaration.Business
{
	using Enterprise.ZArchitecture.Modules;

	public class CusMAWBSendAIRCRMutex : MutexID
	{
		public static CusMAWBSendAIRCRMutex Instance { get; } = new CusMAWBSendAIRCRMutex();

		protected CusMAWBSendAIRCRMutex()
			: base("CusMAWBSendAIRCRMutex", "CusMAWB AIRCR Message Sending Lock.")
		{
		}
	}
}
