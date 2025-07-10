
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoOutturnMutexID : MutexID
	{
		public static SeaCargoOutturnMutexID Instance { get; } = new SeaCargoOutturnMutexID();

		protected SeaCargoOutturnMutexID()
			: base("SeaCargoOutturnMutex", "Sea Cargo Outturn Record Lock.")
		{
		}
	}
}
