using System;
using System.Web;
using CargoWise.Data;
using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public class WebEnvProvider : EnvProvider
	{
		public WebEnvProvider(Func<IUserContextManager> getContextManager = null)
			: base()
		{
			var contextManagerOrNull = (getContextManager ?? (() => null))();

			lazyWebEnvironment = new Lazy<WebEnvironment>(() => NewWebEnvironment(contextManagerOrNull));
		}

		protected override IDbEnvironment GetDbEnvironmentInstance() => new BaseWebDbEnvironment();

		public override BaseEnvironment Instance
		{
			get
			{
				var webEnvironment = WebEnvironmentCache;
				if (webEnvironment != null)
				{
					return webEnvironment;
				}

				webEnvironment = lazyWebEnvironment.Value;
				WebEnvironmentCache = webEnvironment;

				return webEnvironment;
			}
		}

		static WebEnvironment WebEnvironmentCache
		{
			get
			{
				return
					(WebEnvironment)HttpContext.Current?.Session?[EnvironmentSessionKey]
					?? (WebEnvironment)HttpContext.Current?.Items[EnvironmentSessionKey];
			}
			set
			{
				if (WebEnvironmentCache == value)
				{
					return;
				}

				var currentSession = HttpContext.Current?.Session;
				if (currentSession != null)
				{
					lock (currentSession.SyncRoot)
					{
						currentSession[EnvironmentSessionKey] = value;
					}
				}
				else
				{
					var context = HttpContext.Current;
					if (context != null)
					{
						context.Items[EnvironmentSessionKey] = value;
					}
				}
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (lazyWebEnvironment.IsValueCreated)
			{
				lazyWebEnvironment.Value?.Dispose();
			}

			HttpContext.Current?.Session?.Remove(EnvironmentSessionKey);
			HttpContext.Current?.Items.Remove(EnvironmentSessionKey);

			base.Dispose(isDisposing);
		}

		readonly Lazy<WebEnvironment> lazyWebEnvironment;

		protected virtual WebEnvironment NewWebEnvironment(IUserContextManager userContextManager) => new WebEnvironment(userContextManager);

		public const string EnvironmentSessionKey = nameof(WebEnvironment);
	}
}
