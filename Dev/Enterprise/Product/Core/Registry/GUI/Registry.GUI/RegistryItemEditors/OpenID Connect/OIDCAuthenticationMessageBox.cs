using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public interface IOIDCAuthenticationMessageBox
	{
		ContainerControl ParentControl { get; }
		Task<string> VerifyOidcConfig(IOIDCConfig oidcConfig, string domainHint);
		Task<LoginAuthenticationInfo> PerformLogin(IOIDCConfig oidcConfig);
	}

	public interface IDisposableOIDCAuthenticationMessageBox : IDisposable, IOIDCAuthenticationMessageBox { }

	public class OIDCAuthenticationMessageBox : ZMessageBox, IDisposableOIDCAuthenticationMessageBox
	{
		public ContainerControl ParentControl { get; }

		protected CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

		public OIDCAuthenticationMessageBox(ContainerControl parentControl) : base(
			Res.GetString("644A0AE4-15AE-42ED-8B96-2B337F8013AB",
				"Verifying OpenID Connect Settings. In order to verify, you need to login as a controller user in the web pop up window, click \"Cancel\" to cancel the verification process."),
			string.Empty,
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning,
			Res.GetString("DF40720E-1E5F-4C81-9C47-1F922EE22AAA", "Cancel"))
		{
			ParentControl = parentControl;
			StartPosition = FormStartPosition.WindowsDefaultLocation;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			CancellationTokenSource.Dispose();
			base.Dispose(isNotFinalizing);
		}

		public async Task<LoginAuthenticationInfo> PerformLogin(IOIDCConfig oidcConfig)
		{
			var authenticationInfo = await PerformAction(
				(cancellationToken) => OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, WebUrlLauncher.Launch, cancellationToken));
			authenticationInfo.TakeThreadOwnershipForUser();
			return authenticationInfo;
		}

		/// <summary>
		/// Verify the OIDC configuration within the domain.
		/// </summary>
		/// <returns><see cref="string.Empty"/> when the config is correct, otherwise a message describing the error.</returns>
		public async Task<string> VerifyOidcConfig(IOIDCConfig oidcConfig, string domainHint)
		{
			return await PerformAction(
				(cancellationToken) => OIDCUserLogin.VerifyOidcConfig(oidcConfig, WebUrlLauncher.Launch, domainHint, cancellationToken));
		}

		protected virtual async Task<T> PerformAction<T>(Func<CancellationToken, T> innerTaskFunc)
		{
			var messageBox = this as ZMessageBox;
			var task = Task.Run(() =>
			{
				var result = innerTaskFunc(CancellationTokenSource.Token);

				ParentControl.Invoke(new Action(() =>
				{
					if (!messageBox.IsDisposed)
					{
						messageBox.Close();
					}
				}));

				return result;
			});

			messageBox.Closed += (_, __) =>
			{
				if (!task.IsCompleted)
				{
					CancellationTokenSource.Cancel();
				}
			};

			if (!messageBox.IsDisposed)
			{
				ZFormModaliser.ShowDialogWithoutDispose(messageBox, ParentControl.ParentForm);
			}

			return await task;
		}
	}
}
