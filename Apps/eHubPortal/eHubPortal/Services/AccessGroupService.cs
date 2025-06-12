using eServices.eHubPortal.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace eServices.eHubPortal.Services;

public class AccessGroupService(IAuthorizationService authorizationService, AuthenticationStateProvider authenticationStateProvider) : IAccessGroupService
{
	public async Task<bool> IsAuthorized(string policyName)
	{
		var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
		if (user is null or { Identity.IsAuthenticated: false })
			return false;
		return (await authorizationService.AuthorizeAsync(user, policyName)).Succeeded;
	}

	public async Task<bool> IsAuthorizedForRead(string? ownerId)
	{
		var readAccessGroup = ownerId switch
		{
			OwnerId.Customs => AccessGroups.CustomsReadOnly,
			OwnerId.Air => AccessGroups.AirReadOnly,
			OwnerId.Ocean => AccessGroups.OceanReadOnly,
			_ => AccessGroups.GlobalReadOnly
		};
		return await IsAuthorized(readAccessGroup);
	}

	public async Task<bool> IsAuthorizedForWrite(string? ownerId)
	{
		var writeAccessGroup = ownerId switch
		{
			OwnerId.Customs => AccessGroups.CustomsReadWrite,
			OwnerId.Air => AccessGroups.AirReadWrite,
			OwnerId.Ocean => AccessGroups.OceanReadWrite,
			_ => AccessGroups.Admin
		};
		return await IsAuthorized(writeAccessGroup);
	}
}
