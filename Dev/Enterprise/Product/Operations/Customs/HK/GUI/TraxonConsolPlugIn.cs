using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Customs.HK.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using ResString = Enterprise.Customs.HK.GUI.ResString;

namespace Enterprise.Customs.HK.PlugIn
{
	public class TraxonConsolPlugIn : ZPlugIn
	{
		public TraxonConsolPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			consol = hostEntity as ForwardingConsol;
			if (consol != null)
			{
				MessageSender = new TraxonMessageSender(consol);
				consol.JK_TransportModeInfo.ValueChanged += Consol_JK_TransportModeChanged;
				ChangeTheVisibility();
			}
		}

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (consol != null)
				{
					consol.JK_TransportModeInfo.ValueChanged -= Consol_JK_TransportModeChanged;
				}
			}

			base.Dispose(disposing);
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (mainMenuItem == null)
			{
				mainMenuItem = new ZMenuItem("ISAC");
				SetupTopLevelMenu();
			}
			return mainMenuItem;
		}

		protected override Control GetNewUserControl()
		{
			Control result = new TraxonMessageUserControl();
			result.Dock = DockStyle.Fill;
			return result;
		}

		protected override ZBool HasUserControl => true;

		public override string Name => "Traxon";

		protected override IBusiness GetBusinessEntityForPlugIn() => MessageSender.Status;

		protected void MenuItemSendConsol_Click(object sender, EventArgs e)
		{
			SendMessage(false);
		}

		protected void MenuItemWithdrawalConsol_Click(object sender, EventArgs e)
		{
			SendMessage(true);
		}

		protected ForwardingConsol consol;
		protected MenuItem mainMenuItem;

		TraxonMessageSender MessageSender
		{
			get;
			set;
		}

		void SendMessage(bool cancel)
		{
			if (!consol.HasChanges || (mainMenuItem?.GetMainMenu()?.GetForm() is ZForm parentForm && parentForm.FireSaveButton() == ContinueWithSave.Yes))
			{
				if (!TraxonForwardingConsolValidation.IsProcessingISACMessageValid(MasterFiles.Business.GlbBranch.CurrentBranch))
				{
					Globals.Message.Show(ResString.GetMultilingualString("B136AF1D-988C-4B83-8A02-5E586D2144C6", "ISAC Message sending configuration should be completed in the registry before sending messages"), "ISAC Message Sending Configuration Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					var oldCursor = Cursor.Current;
					try
					{
						Cursor.Current = Cursors.WaitCursor;
						var status = MessageSender.SendMessage(cancel, false,
							message => Globals.Message.ShowInformation(message),
							message =>
							{
								var messageToShown = $"Error(s) detected:\r\n\r\n{message}\r\n\r\nWould you like to continue?";
								if (Env.Security.ISACHKSendWithMessageErrors.IsAllowed)
								{
									return Globals.Message.Show(messageToShown, ResString.GetMultilingualString("E87CDA58-BCF4-40D4-9EEF-D11C95F32EF5", "Continue?"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
								}

								Globals.Message.ShowInformation(messageToShown);
								return false;
							});
						if (status == TraxonMessageSender.SuccessfullyResult)
						{
							Env.Licence.Manifest.Login(this);
						}
						try
						{
							Factory.Save();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
					finally
					{
						Cursor.Current = oldCursor;
					}
				}
			}
		}

		protected void SetupTopLevelMenu()
		{
			var menuItemSendConsol = new ZMenuItem(ResString.GetMultilingualString("Customs.HK.TraxonConsolPlugIn.Send", "&Send"));
			menuItemSendConsol.Click += MenuItemSendConsol_Click;
			mainMenuItem.MenuItems.Add(menuItemSendConsol);

			var menuItemWithdrawalConsol = new ZMenuItem(ResString.GetMultilingualString("Customs.HK.TraxonConsolPlugIn.Withdrawal", "&Withdrawal"));
			menuItemWithdrawalConsol.Click += MenuItemWithdrawalConsol_Click;
			mainMenuItem.MenuItems.Add(menuItemWithdrawalConsol);
		}

		protected void ChangeTheVisibility()
		{
			Enabled = consol != null && consol.JK_TransportMode == Enterprise.Core.Constants.TransportModes.Air;
		}

		protected void Consol_JK_TransportModeChanged(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}
	}
}
