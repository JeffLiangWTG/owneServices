using System.Web;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Web.TestWebApplication.Common
{
	public class WebEnvProvider : EnvProvider
	{
		protected override IDbEnvironment GetDbEnvironmentInstance() => new WebDbEnvironment();

		public override BaseEnvironment Instance
		{
			get
			{
				WebEnvironment environment;

				if (HttpContext.Current != null && HttpContext.Current.Application != null)
				{
					environment = WebEnvironmentFromHttpCurrentSession ?? WebEnvironmentFromHttpCurrentContext;
				}
				else
				{
					environment = new WebEnvironment();
				}

				return environment;
			}
		}

		static WebEnvironment WebEnvironmentFromHttpCurrentSession
		{
			get
			{
				var currentSession = HttpContext.Current.Session;
				if (currentSession == null)
				{
					return null;
				}

				var cachedEnvironment = (WebEnvironment)currentSession[EnvironmentIdentifier];
				if (cachedEnvironment != null)
				{
					return cachedEnvironment;
				}

				lock (currentSession.SyncRoot)
				{
					cachedEnvironment = (WebEnvironment)currentSession[EnvironmentIdentifier];
					if (cachedEnvironment != null)
					{
						return cachedEnvironment;
					}

					cachedEnvironment = new WebEnvironment();
					currentSession[EnvironmentIdentifier] = cachedEnvironment;
				}

				return cachedEnvironment;
			}
		}

		static WebEnvironment WebEnvironmentFromHttpCurrentContext
		{
			get
			{
				if (HttpContext.Current.Items[EnvironmentIdentifier] == null)
				{
					HttpContext.Current.Items[EnvironmentIdentifier] = new WebEnvironment();
				}

				return (WebEnvironment)HttpContext.Current.Items[EnvironmentIdentifier];
			}
		}

		const string EnvironmentIdentifier = "Enterprise.WebTestEnvironment";
	}
}
