using System;
using System.Net.Http;

namespace Enterprise.Dash.Integration.Services
{
	public interface IDashGlowService
	{
		HttpResponseMessage MatchOrganisations(Guid docPk);
		HttpResponseMessage MatchProductCodes(Guid docPk);
	}
}
