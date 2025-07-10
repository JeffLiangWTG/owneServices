using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GlobalBase;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Native.Service
{
	public partial class Global : ZEnterpriseGlobal
	{
		// sealed the method to close the door from modifications
		// application is required to be started up quick, light and safe, and not to hit the db
		// as the database might be in the progress of upgrade
		protected sealed override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);
		}

		protected sealed override void Application_BeginRequest(object sender, EventArgs e)
		{
			base.Application_BeginRequest(sender, e);

			SetupUserContext();
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Web Application. Should Set up User Context rather than temporary user context")]
		static void SetupUserContext()
		{
			if (!(Env.CurrentUserContext?.User?.IsWebUser ?? false))
			{
				lock (setupUserContextLock)
				{
					if (!(Env.CurrentUserContext?.User?.IsWebUser ?? false))
					{
						using (Db.DisposableActionForDbConnection())
						{
							var userContext = new UserContext(ZArchitecture.Environment.User.WebUserName, EnvProxy.Instance.Registry.WebBranch, EnvProxy.Instance.Registry.WebDepartment);
							Env.SetUserContext(userContext); // Web Application. Should Set up User Context rather than temporary user context
						}
					}
				}
			}
		}

		[ThreadSafe]
		static readonly object setupUserContextLock = new object();
	}
}

#if DEBUG
#region Test

namespace Enterprise.DataTransfer.Native.Service
{
	public partial class Global
	{
		public UserContext ApplicationBeginRequest_ForTest(object sender, EventArgs e)
		{
			Application_BeginRequest(sender, e);
			return Env.CurrentUserContext;
		}
	}
}

#endregion
#endif
