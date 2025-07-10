using System;
using System.Web;
using CargoWise.Data;
using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	/// <summary>
	/// Manager class for web environment
	/// </summary>
	public class WebEnvironmentProvider : EnvProvider
	{
		public WebEnvironmentProvider(Func<IUserContextManager> getContextManager = null)
			: base()
		{
			this.getContextManager = getContextManager ?? (() => default);
		}

#if DEBUG

		public bool IsTest { get; set; }
		WebEnvironment environmentForTesting;
#endif
		readonly Func<IUserContextManager> getContextManager;
		const string environmentSessionKey = "EnterpriseWebEnvironment";

		public override BaseEnvironment Instance
		{
			get
			{
#if DEBUG
				if (IsTest)
				{
					return environmentForTesting ?? (environmentForTesting = GetNewEnvironment());
				}
#endif

				if (HttpContext.Current != null && HttpContext.Current.Application != null)
				{
					// session has been made a local variable because during testing (with IIS) Session became null during the single method call
					// This happened at varying lines of this method
					var session = HttpContext.Current.Session;
					if (session != null)
					{
						var sessionEnvironment = (WebEnvironment)session[environmentSessionKey];
						if (sessionEnvironment == null)
						{
							lock (session.SyncRoot)
							{
								sessionEnvironment = (WebEnvironment)session[environmentSessionKey];
								if (sessionEnvironment == null)
								{
									sessionEnvironment = GetNewEnvironment();
									session[environmentSessionKey] = sessionEnvironment;
								}
							}
						}

						return sessionEnvironment;
					}
					else
					{
						var sharedEnvironment = (WebEnvironment)HttpContext.Current.Items[environmentSessionKey];
						if (sharedEnvironment == null)
						{
							sharedEnvironment = GetNewEnvironment();
							HttpContext.Current.Items[environmentSessionKey] = sharedEnvironment;
						}

						return sharedEnvironment;
					}
				}

				return GetNewEnvironment();
			}
		}

		protected WebEnvironment GetNewEnvironment() => new WebEnvironment(getContextManager());

		protected override IDbEnvironment GetDbEnvironmentInstance()
		{
			return new WebDbEnvironment();
		}
	}

	public class WebServiceEnvironmentProvider : WebEnvironmentProvider
	{
		BaseEnvironment instance;

		public override BaseEnvironment Instance => instance ?? (instance = GetNewEnvironment());
	}
}
