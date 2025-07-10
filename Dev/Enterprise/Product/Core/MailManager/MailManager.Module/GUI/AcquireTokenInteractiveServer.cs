using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Identity.Client;
using Res = MailManager.Module.Res;

namespace Enterprise.MailManager.GUI
{
	public class AcquireTokenInteractiveServer : IAcquireTokenInteractiveServer
	{
		async Task<AuthenticationResult> IAcquireTokenInteractiveServer.AcquireByDeviceCodeAsync(string[] scopes, IPublicClientApplication pca, CancellationToken token)
		{
			var result = await pca.AcquireTokenWithDeviceCode(scopes,
				deviceCodeResult =>
				{
					// This will show a message box which tells the user where to go sign-in using
					// a separate browser and the code to enter once they sign in.
					// The AcquireTokenWithDeviceCode() method will poll the server after firing this
					// device code callback to look for the successful login of the user via that browser.
					// This background polling (whose interval and timeout data is also provided as fields in the
					// deviceCodeCallback class) will occur until:
					// * The user has successfully logged in via browser and entered the proper code
					// * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
					// * The developing application calls the Cancel() method on a CancellationToken sent into the method.
					//   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
					var message = Res.GetString("04554112-58f7-4351-a01d-4474adcf4dd1",
						@"To complete the OAuth 2.0 token cache, click on the Sign into your account button.  You will then be taken to {0} in a web browser and the required code will be copied into memory and can then be pasted into the Code field.  Alternatively you can open a web browser and go to {0} and enter the code {1}. Follow the prompts and once authentication is completed successfully, click OK to close this pop-up window.

Authentication must be completed before the code expires on {2}.",
						deviceCodeResult.VerificationUrl, deviceCodeResult.UserCode, deviceCodeResult.ExpiresOn.ToLocalTime());

					void ShowMessage()
					{
						var messageBox = new AcquireTokenInteractiveMessageBox(message, deviceCodeResult.UserCode, deviceCodeResult.VerificationUrl);
						ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
						messageBox.Dispose();
					}

					if (Form.ActiveForm != null)
					{
						Form.ActiveForm.Invoke(() => { ShowMessage(); });
					}
					else
					{
						ShowMessage();
					}
					return Task.FromResult(0);
				}).ExecuteAsync(token);

			return result;
		}
	}
}
