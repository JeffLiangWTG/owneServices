using System.Threading.Tasks;
using Enterprise.TrustedMessaging.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TrustedMessaging.GUI
{
	public class SystemUserAccountCollectionTermChecker : ISystemUserAccountCollectionTermChecker
	{
		public Task<bool> CheckTermAcknowledged() => new TermsAcknowledgementChecker(new SystemUserAccountCollectionTerm(), null).CheckTermAcknowledged();
	}
}