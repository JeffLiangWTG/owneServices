using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.Core.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProductRegistration.GUI
{
	public partial class UpdateRegistrationForm : ZChildForm
	{
		public UpdateRegistrationForm()
		{
			InitializeComponent();

			this.refreshButton.ToolTipCaption = ResString.GetMultilingualString("480DC0AE-3343-4185-8F55-ED7E320DA09E", "Refresh");
			originalHelpSize = helpTextBox.ClientSize;
			gapBelowHelp = closeButton.Top - helpTextBox.Bottom;
			ControlDpiScalingHelper.SetHeight(this, this.Height - helpTextBox.Height - gapBelowHelp, false);
			logoBox.Image = BrandingFactory.Instance.ProductLogo;
		}

		ProductRegistrationVerifyResult fullVerifyResult = ProductRegistrationVerifyResult.Unregistered;
		CancellationTokenSource cancelTokenSource;
		Size sizeWithoutHelp;
		Size originalHelpSize;
		readonly int gapBelowHelp;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			sizeWithoutHelp = Size;

			ShowWaitingForVerify();
			DoVerify();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Cancel();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		void Cancel()
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
			if (cancelTokenSource != null)
			{
				cancelTokenSource.Cancel();
			}
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (cancelTokenSource != null)
			{
				cancelTokenSource.Cancel();
			}

			base.OnClosing(e);
		}

		void ShowWaitingForVerify()
		{
			statusBox.Text = Res.GetString("3CBB48BE-279F-456E-9FBE-E621ED59FD8D", "Verifying...");
			helpTextBox.Text = "";
			productKeyBox.ReadOnly = true;
			refreshButton.Enabled = false;
			changeButton.Visible = false;
			waitAnimationBox.Visible = true;
		}

		void refreshButton_Click(object sender, EventArgs e)
		{
			ShowWaitingForVerify();
			DoVerify();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Not working in pixels")]
		void ShowResult(string popupMsg = null, Exception ex = null)
		{
			statusBox.ForeColor = System.Drawing.SystemColors.WindowText;
			var rego = ObjectFactory.Get<IProductRegistration>();

			string statusMsg;
			string keyText = null;
			string helpMsg = null;
			bool keyReadonly = true;
			if (fullVerifyResult == ProductRegistrationVerifyResult.OK)
			{
				statusMsg = Res.GetString("ProductRegistrationStatus|Registered", "Registered");
				var registrationKey = rego.Key;
				keyText = (registrationKey.EnterpriseCode + " " + registrationKey.ServerCode).Trim();
			}
			else if (fullVerifyResult == ProductRegistrationVerifyResult.Unregistered)
			{
				statusMsg = Res.GetString("ProductRegistrationStatus|Unregistered", "Unregistered");
				keyReadonly = false;
				if (productKeyBox.Text.Length == 0)
				{
					var registrationKey = rego.Key;
					keyText = (registrationKey.EnterpriseCode + " " + registrationKey.ServerCode).Trim();
				}
			}
			else if (fullVerifyResult == ProductRegistrationVerifyResult.Fail)
			{
				statusMsg = Res.GetString("ProductRegistrationStatus|Disabled", "Disabled");
				keyReadonly = false;
				keyText = "";
				statusBox.ForeColor = Color.Red;
				var registrationKey = rego.Key;
				var msg1 = Res.GetString("68FB2AB8-FA2E-470D-84E6-21A5393C9717", "This installation is no longer registered.\r\nThe product key provided is locked to another server and database:\r\n       {0} {1}.", registrationKey.ServerName, registrationKey.DatabaseName);
				var msg2 = Res.GetString("5F567FEF-AD19-4B6C-BB11-D14625520978", "Service tasks will be disabled.");
				var msg3 = Res.GetString("C4F069D7-D8C4-49CC-992F-8C709B1B4D84",
@"If the database is being moved then to re-issue the registration for this new location:
    - Login to the old installation and select Help > Unregister Product.
    - Take note of the Product Key before clicking Unregister.
    - Login to the new installation and select Help > Register Product.
    - Enter the Product Key and click Register. 

If the old installation is unavailable, please contact support to reset the registration.");

				helpMsg = msg1 + System.Environment.NewLine + System.Environment.NewLine +
					msg2 + System.Environment.NewLine + System.Environment.NewLine +
					msg3;
			}
			else if (fullVerifyResult == ProductRegistrationVerifyResult.NotFound)
			{
				statusMsg = Res.GetString("ProductRegistrationStatus|Disabled", "Disabled");
				keyReadonly = false;
				keyText = "";
				statusBox.ForeColor = Color.Red;
				var msg1 = Res.GetString("7C7590C0-505A-41FD-9A56-6135D59BBB06", "This installation is no longer registered.\r\nThe product key provided has been deactivated or could not be found.");
				var msg2 = Res.GetString("5F567FEF-AD19-4B6C-BB11-D14625520978", "Service tasks will be disabled.");

				helpMsg = msg1 + System.Environment.NewLine + System.Environment.NewLine +
					msg2;
			}
			else
			{
				statusMsg = Res.GetString("F45E34FB-06F0-47EE-BD34-E097E3D6EF1F", "Unknown");

				if (popupMsg == null)
				{
					if (fullVerifyResult == ProductRegistrationVerifyResult.Timeout)
					{
						popupMsg = ServiceTimeoutMsg;
					}
					else if (ex == null)
					{
						popupMsg = ServiceErrorMsg;
#if DEBUG
						if (!string.IsNullOrEmpty(rego.LastError))
						{
							popupMsg += "\r\n\r\n" + rego.LastError;
						}
#endif
					}
					else
					{
						var inner = ex.GetInnermostException();
						popupMsg = Res.GetString("AE998710-D754-4B40-B15E-3E1D604EED6E", "There was a problem preventing verification.") + "\r\n\r\n" + ex.Message;
						if (inner != ex)
						{
							popupMsg += "\r\n\r\n" + inner.Message;
						}
					}
				}
			}

			if (keyText != null)
			{
				productKeyBox.Text = keyText;
			}

			if (!string.IsNullOrEmpty(helpMsg))
			{
				var helpSize = TextRenderer.MeasureText(helpMsg, helpTextBox.Font) + ControlDpiScalingHelper.NewScaledSize(10, gapBelowHelp / 2, false);
				var delta = helpSize - originalHelpSize;
				delta = ControlDpiScalingHelper.NewScaledSize(Math.Max(delta.Width, 0), Math.Max(delta.Height, 0), false);
				helpSize = originalHelpSize + delta;
				this.Size = sizeWithoutHelp + ControlDpiScalingHelper.NewScaledSize(helpSize.Width - originalHelpSize.Width, helpSize.Height + gapBelowHelp, false);
				helpTextBox.ClientSize = helpSize;
				helpTextBox.Visible = true;
				helpTextBox.Text = helpMsg;
			}
			else
			{
				helpTextBox.Text = "";
				helpTextBox.Visible = false;
				this.Size = sizeWithoutHelp;
				helpTextBox.ClientSize = originalHelpSize;
			}
			productKeyBox.ReadOnly = keyReadonly;
			statusBox.Text = statusMsg;
			refreshButton.Enabled = true;
			changeButton.Visible = CanRegister || CanUnregister;
			UpdateChangeButtonEnabled();
			if (CanRegister)
			{
				changeButton.Text = Res.GetString("33B1DBF6-2210-4CA8-9CF2-E9C93CACFD20", "Register");
			}
			else if (CanUnregister)
			{
				changeButton.Text = Res.GetString("48EDF3B7-AB66-475D-BBF1-E8CFDDA117D1", "Unregister");
			}
			waitAnimationBox.Visible = false;

			if (popupMsg != null)
			{
				Globals.Message.ShowInformation(popupMsg);
			}
		}

		bool CanRegister
		{
			get
			{
				return fullVerifyResult == ProductRegistrationVerifyResult.Unregistered
					|| fullVerifyResult == ProductRegistrationVerifyResult.Fail
					|| fullVerifyResult == ProductRegistrationVerifyResult.NotFound;
			}
		}

		bool CanUnregister
		{
			get { return fullVerifyResult == ProductRegistrationVerifyResult.OK; }
		}

		void UpdateChangeButtonEnabled()
		{
			changeButton.Enabled = (CanRegister && productKeyBox.Text.Length >= 6)
				|| (CanUnregister);
		}

		void productKeyBox_TextChanged(object sender, System.EventArgs e)
		{
			UpdateChangeButtonEnabled();
		}

		void DoVerify()
		{
			if (!Globals.IsTest)
			{
				var context = TaskScheduler.FromCurrentSynchronizationContext();
				cancelTokenSource = new CancellationTokenSource();
				var cancelToken = cancelTokenSource.Token;
				Task.Factory.StartNew<ProductRegistrationVerifyResult>(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var rego = ObjectFactory.Get<IProductRegistration>();
						return rego.FullVerify(cancelToken, 2 * 60 * 1000);
					}
				}, cancelToken)
				.ContinueWith((task) =>
				{
					if (IsDisposed || IsDisposing)
					{
						return;
					}
					else if (task.Status == TaskStatus.RanToCompletion)
					{
						fullVerifyResult = task.Result;
						ShowResult();
					}
					else if (task.Status == TaskStatus.Faulted)
					{
						var ex = task.Exception;
						fullVerifyResult = ProductRegistrationVerifyResult.Error;
						ShowResult(null, ex);
					}
					else if (task.Status == TaskStatus.Canceled)
					{
						DialogResult = DialogResult.Cancel;
						Close();
					}
				}, context);
			}
		}

		void changeButton_Click(object sender, EventArgs e)
		{
			if (CanUnregister)
			{
				var otherUsers = ActiveUserQuery.GetEnterpriseActiveUserSessions(false);
				if (otherUsers.Length > 0)
				{
					Globals.Message.ShowWarning(OtherLoginsMsg);
				}
				else
				{
					var unregisterMsg = Res.GetString("8A0AAA64-DE90-4406-B8AF-B4B05382882A", "Unregister");
					var confirm = Globals.Message.ShowConfirmation(
						Res.GetString("1018B215-62ED-4343-B977-65C15DC902B5", "Unregistering will stop all process controllers and stop service tasks from starting."),
						unregisterMsg, unregisterMsg, MessageBoxIcon.Warning);
					if (confirm == DialogResult.OK)
					{
						ShowWaitingForUnregister();
						DoUnregister();
					}
				}
			}
			else if (CanRegister)
			{
				ShowWaitingForRegister();
				DoRegister();
			}
		}

		void DoRegister()
		{
			var context = TaskScheduler.FromCurrentSynchronizationContext();
			var productKey = productKeyBox.Text.Replace(" ", "");
			cancelTokenSource = new CancellationTokenSource();
			var cancelToken = cancelTokenSource.Token;
			Task.Factory.StartNew<ProductRegistrationRegisterResult>(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var rego = ObjectFactory.Get<IProductRegistration>();
					return rego.Register(productKey, cancelToken);
				}
			}, cancelToken)
				.ContinueWith((task) =>
				{
					cancelTokenSource = null;
					if (IsDisposed || IsDisposing)
					{
						return;
					}
					else if (task.Status == TaskStatus.RanToCompletion)
					{
						var regoResult = task.Result;
						if (regoResult == ProductRegistrationRegisterResult.OK)
						{
							fullVerifyResult = ProductRegistrationVerifyResult.OK;
							ShowResult(Res.GetString("5DE55742-7E75-4368-A36F-BC3DDA216155", "Registration successful!"));
						}
						else
						{
							ShowResult(GetErrorMessage(regoResult));
							if ((regoResult == ProductRegistrationRegisterResult.ProductKeyNotFound || regoResult == ProductRegistrationRegisterResult.ProductKeyUnavailable)
								&& productKeyBox.Enabled && !productKeyBox.ReadOnly)
							{
								productKeyBox.Focus();
							}
						}
					}
					else if (task.Status == TaskStatus.Faulted)
					{
						var ex = task.Exception;
						fullVerifyResult = ProductRegistrationVerifyResult.Error;
						ShowResult(null, ex);
					}
					else if (task.Status == TaskStatus.Canceled)
					{
						DialogResult = DialogResult.Cancel;
						Close();
					}
				}, context);
		}

		string GetErrorMessage(ProductRegistrationRegisterResult regoResult)
		{
			string msg;
			if (regoResult == ProductRegistrationRegisterResult.ProductKeyNotFound)
			{
				msg = Res.GetString("DEC9F6D5-E3D9-4664-B94A-3FF32F59FCF2", "Product key not recognized. Please check the correct key has been entered.");
			}
			else if (regoResult == ProductRegistrationRegisterResult.ProductKeyUnavailable)
			{
				msg = Res.GetString("0EA4FA56-2DE1-4C44-A210-132FB6DABE40", "Product key already in use. Please check the correct key has been entered.");
			}
			else if (regoResult == ProductRegistrationRegisterResult.Timeout)
			{
				msg = ServiceTimeoutMsg;
			}
			else
			{
				msg = ServiceErrorMsg;
			}

			return msg;
		}

		void ShowWaitingForRegister()
		{
			statusBox.Text = "";
			helpTextBox.Text = "";
			refreshButton.Enabled = false;
			changeButton.Enabled = false;
			waitAnimationBox.Visible = true;
			productKeyBox.ReadOnly = true;
		}

		void ShowWaitingForUnregister()
		{
			statusBox.Text = Res.GetString("FFF431F8-6A73-4E4D-A056-3C391C9A3B33", "Unregistering...");
			helpTextBox.Text = "";
			refreshButton.Enabled = false;
			changeButton.Enabled = false;
			waitAnimationBox.Visible = true;
		}

		void DoUnregister()
		{
			var context = TaskScheduler.FromCurrentSynchronizationContext();
			cancelTokenSource = new CancellationTokenSource();
			var cancelToken = cancelTokenSource.Token;

			Task.Factory.StartNew<ProductRegistrationUnregisterResult?>(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var otherUsers = ActiveUserQuery.GetEnterpriseActiveUserSessions(false);
					if (otherUsers.Length > 0)
					{
						return null;
					}

					var rego = ObjectFactory.Get<IProductRegistration>();
					var result = rego.Unregister(cancelToken, 2 * 60 * 1000);
					if (result == ProductRegistrationUnregisterResult.OK)
					{
						ServiceTaskHelper.StopAll();
					}
					return result;
				}
			}, cancelToken)
			.ContinueWith((task) =>
			{
				if (IsDisposed || IsDisposing)
				{
					return;
				}
				else if (task.Status == TaskStatus.RanToCompletion)
				{
					if (task.Result.HasValue)
					{
						var unregisterResult = task.Result.Value;
						if (unregisterResult == ProductRegistrationUnregisterResult.OK ||
							unregisterResult == ProductRegistrationUnregisterResult.NotRegistered)
						{
							fullVerifyResult = ProductRegistrationVerifyResult.Unregistered;
							productKeyBox.Text = "";
							ShowResult();
						}
						else
						{
							ShowResult(Res.GetString("59930612-72A6-4352-8CFE-9BCBED0BF5B5", "The unregistering service was busy or unavailable. Please try again later."));
						}
					}
					else
					{
						ShowResult(OtherLoginsMsg);
					}
				}
				else if (task.Status == TaskStatus.Faulted)
				{
					var ex = task.Exception;
					ShowResult(null, ex);
				}
				else if (task.Status == TaskStatus.Canceled)
				{
					DialogResult = DialogResult.Cancel;
					Close();
				}
			}, context);
		}

		public static string OtherLoginsMsg
		{
			get
			{
				return Res.GetString("A926F4AB-B3EB-416D-89AC-DE7EB34E0A1B", "Other users are logged in. Please ensure all other users are logged out before unregistering.");
			}
		}

		public static string ServiceTimeoutMsg
		{
			get
			{
				return Res.GetString("79C89713-FAD1-452B-8B65-04748E896438", "The online registration service is busy. Please try again later.");
			}
		}

		public static string ServiceErrorMsg
		{
			get
			{
				return Res.GetString("4487DD80-4B65-4214-9A2D-667443253439", "The online registration service is unavailable. If problem persists, please contact support.");
			}
		}
	}
}
