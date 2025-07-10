using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Enterprise.RemotePrinting.Client
{
	public static class AuthenticationModulePrioritiser
	{
		class DisposableAction : IDisposable
		{
			public DisposableAction(Action actionOnDisposeOrFinalize)
			{
				this.actionOnDisposeOrFinalize = actionOnDisposeOrFinalize;
			}
			Action actionOnDisposeOrFinalize;

			public void Dispose()
			{
				actionOnDisposeOrFinalize();
				actionOnDisposeOrFinalize = null;
			}

			~DisposableAction()
			{
				if (actionOnDisposeOrFinalize != null)
				{
					actionOnDisposeOrFinalize();
				}
			}
		}

		public static IDisposable MoveToFirstPositionTemporarily(string type)
		{
			var originalOrder = new List<IAuthenticationModule>();
			var item = AuthenticationManager.RegisteredModules;
			while (item.MoveNext())
			{
				originalOrder.Add((IAuthenticationModule)item.Current);
			}

			originalOrder.ForEach(module => AuthenticationManager.Unregister(module));

			var preferredOrder = originalOrder.OrderBy(x => x.AuthenticationType == type ? 0 : 1).ToList();
			preferredOrder.ForEach(x => AuthenticationManager.Register(x));

			return new DisposableAction(() =>
				{
					originalOrder.ForEach(x => AuthenticationManager.Unregister(x));
					originalOrder.ForEach(x => AuthenticationManager.Register(x));
				});
		}
	}
}
