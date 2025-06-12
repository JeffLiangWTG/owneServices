
namespace eServices.eHubPortal.Services
{
	public interface IAccessGroupService
	{
		Task<bool> IsAuthorized(string policyName);
		Task<bool> IsAuthorizedForRead(string? ownerId);
		Task<bool> IsAuthorizedForWrite(string? ownerId);
	}
}