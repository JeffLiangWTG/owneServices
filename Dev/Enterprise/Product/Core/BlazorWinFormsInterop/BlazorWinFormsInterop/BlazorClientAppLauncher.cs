using System;
using System.Web;
using CargoWise.Application;
using CargoWise.Data;
using CargoWiseNext.Infrastructure.Authentication;
using CargoWiseNext.Infrastructure.Installations;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BlazorWinFormsInterop
{
	public interface IBlazorClientAppLauncher
	{
		void Launch(Uri launchUrl, string listenUrl = null);
	}

	public class BlazorClientAppLauncher : IBlazorClientAppLauncher
	{
		/// <summary>
		/// Launches the client application, will apply an auth token to the uri
		/// </summary>
		/// <param name="launchUrl">The uri of the Winzor App Server</param>
		/// <param name="listenUrl">The uri the backchannel should connect to if relevant</param>
		public void Launch(Uri launchUrl, string listenUrl = null)
		{
			var builder = new UriBuilder(launchUrl);
			var queryString = HttpUtility.ParseQueryString(builder.Query);
			queryString.Add(QueryParameters.ClientToken, CreateToken(listenUrl ?? GlbStaff.CurrentUser.GS_LoginName));
			builder.Query = queryString.ToString();

			var protocol = ObjectFactory.Get<IUrlHandlerProvider>().GetUrlHandler();
			ObjectFactory.Get<IProgramLauncher>().Launch($"{protocol}:{builder}", "");
		}

		string CreateToken(string scope)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var accessControl = new TokenizedAccessControl();
				var accessTokenInfo = new AccessTokenInfo(scope, Env.CurrentUserPK, GlbStaffSchema.Constants.Prefix);
				return accessControl.CreateLimitedToken("BLC", accessTokenInfo, TimeSpan.FromMinutes(5), 1);
			}
		}
	}
}
