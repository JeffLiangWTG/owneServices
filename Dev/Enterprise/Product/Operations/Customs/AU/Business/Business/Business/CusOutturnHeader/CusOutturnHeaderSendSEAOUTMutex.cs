using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderSendSEAOUTMutex : MutexID
	{
		public static readonly CusOutturnHeaderSendSEAOUTMutex Instance = new CusOutturnHeaderSendSEAOUTMutex();

		protected CusOutturnHeaderSendSEAOUTMutex()
			: base("CusOutturnHeaderSendSEAOUTMutex", "CusOutturnHeader SEAOUT Message Sending Lock.")
		{
		}
	}
}
