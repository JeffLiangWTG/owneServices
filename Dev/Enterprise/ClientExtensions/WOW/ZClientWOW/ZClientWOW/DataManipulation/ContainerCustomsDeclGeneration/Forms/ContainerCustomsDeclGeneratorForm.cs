using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.Wow
{
	public partial class ContainerCustomsDeclGeneratorForm : ZChildForm, INotifications, INotificationSubscriberQueryUser
	{
		public ContainerCustomsDeclGeneratorForm()
		{
			InitializeComponent();
			fGenerator = new ContainerCustomsDeclGenerator();
			MinimumSize = Size;
			Text = FormCaption;
		}

		#region ZForm Overrides

		public override string FormCaption
		{
			get { return "Container Customs Declaration Generator"; }
		}

		#endregion

		#region Implementation

		protected ContainerCustomsDeclGenerator fGenerator;

		#region Execute

		private void OnExecute_Click(object sender, System.EventArgs e)
		{
			Execute();
		}

		protected void Execute()
		{
			SafeSetAllButtonsEnabled(false);
			ProgressTextBox.Text = "Executing...\r\n\r\n";
			NotificationBuffer buffer = new NotificationBuffer(this);
			try
			{
				if (WowDataRegistry.Instance.DeclarationImporter != Guid.Empty)
				{
					fGenerator.Execute(buffer, true);
				}
				else
				{
					buffer.Notify(new ErrorNotification(ErrorType.Error, RegistryErrorMessage));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(ex);
			}
			finally
			{
				SafeSetAllButtonsEnabled(true);
			}
			ProgressTextBox.AppendText("\r\nDone");
		}

		#endregion

		#region SafeSetAllButtonsEnabled

		protected delegate void SafeSetAllButtonsEnabledDelegate(bool value);
		protected void SafeSetAllButtonsEnabled(bool value)
		{
			if (InvokeRequired)
			{
				Invoke(new SafeSetAllButtonsEnabledDelegate(SafeSetAllButtonsEnabled), new object[] { value });
			}
			else
			{
				this.ExecuteButton.Enabled = value;
				this.CloseButton.Enabled = value;
			}
		}

		#endregion

		#endregion

		#region INotifications Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void INotifications.Add(INotification notification)
		{
			ProgressTextBox.AppendText(notification.Message + "\r\n");
			Application.DoEvents();
		}

		#endregion

		#region INotificationSubscriberQueryUser Members

		public void QueryUser(IQueryUserEventArgs e)
		{
			new NotificationSubscriberGuiHelper().QueryUser(e);
		}

		#endregion

		public const string RegistryErrorMessage = "Please set up Registry 'Woolworths Importer' before Continuing";
	}
}
