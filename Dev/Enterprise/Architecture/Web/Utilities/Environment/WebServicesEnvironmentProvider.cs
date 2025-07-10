using System.Web;
using CargoWise.Data;
using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	public class WebServicesEnvironmentProvider : EnvProvider
	{
		const string EnvironmentIdentifier = "EnterpriseWebEnvironment";

		public override BaseEnvironment Instance => CreateInstance();

		protected override IDbEnvironment GetDbEnvironmentInstance()
		{
			return new WebDbEnvironment();
		}

		BaseEnvironment CreateInstance()
		{
			WebServicesEnvironment environment;

			if (HttpContext.Current != null && HttpContext.Current.Application != null)
			{
				var currentSession = HttpContext.Current.Session;

				if (currentSession != null)
				{
					var cachedEnvironment = (WebServicesEnvironment)currentSession[EnvironmentIdentifier];
					if (cachedEnvironment == null)
					{
						lock (currentSession.SyncRoot)
						{
							cachedEnvironment = (WebServicesEnvironment)currentSession[EnvironmentIdentifier];
							if (cachedEnvironment == null)
							{
								cachedEnvironment = new WebServicesEnvironment();
								currentSession[EnvironmentIdentifier] = cachedEnvironment;
							}
						}
					}
					environment = cachedEnvironment;
				}
				else
				{
					if (HttpContext.Current.Items[EnvironmentIdentifier] == null)
					{
						HttpContext.Current.Items[EnvironmentIdentifier] = new WebServicesEnvironment();
					}

					environment = (WebServicesEnvironment)HttpContext.Current.Items[EnvironmentIdentifier];
				}
			}
			else
			{
				environment = new WebServicesEnvironment();
			}

			return environment;
		}
	}
}
