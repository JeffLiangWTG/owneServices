using System.Threading.Tasks;

namespace Enterprise.TrustedMessaging.Intergration
{
	public interface ISystemUserAccountCollectionTermChecker
	{
		Task<bool> CheckTermAcknowledged();
	}
}