using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using MessageType = Enterprise.Customs.Common.AU.PRAMessageTypeConstants.MessageType;

namespace Enterprise.Customs.AU.PRA.GUI
{
	[SuppressFormsLocalizedTest]
	public class AUContainerMessagingMenu : KMenuItem
	{
		public AUContainerMessagingMenu()
		{
			this.Text = "&PRA Messaging";
		}

		public IPRAContainerMessaging PRAContainer
		{
			get { return fContainer; }
			set { fContainer = value; }
		}
		IPRAContainerMessaging fContainer;

		#region Implementation

		protected void SubmitContainerMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(MessageType.Submit);
		}

		protected void CancelContainerMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(MessageType.Cancel);
		}

		void ReSubmitContainerMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(MessageType.ReSubmit);
		}

		protected IPRAMessagingData GetDataLayer()
		{
			if (PRAContainer is CusContainer)
			{
				return new CustomsDataLayer((CusContainer)PRAContainer);
			}
			else if (PRAContainer is CFSContainer)
			{
				return new CFSContainerMessagingDataLayer((CFSContainer)PRAContainer);
			}
			else if (PRAContainer is CommonContainer)
			{
				return new FreightDataLayer((CommonContainer)PRAContainer);
			}
			return null;
		}

		protected void SendMessage(MessageType messageType)
		{
			IPRAMessagingData dataLayer = GetDataLayer();
			if (dataLayer != null)
			{
				bool send = true;
				if (messageType == MessageType.ReSubmit)
				{
					send = (Globals.Message.Show("Warning – This message will replace PRA message already sent for this Container, including any already accepted messages. \r\n\r\nDo you want to re-send the PRA message for the selected container?", "Re-Send PRA?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes);
				}

				if (send)
				{
					if (!dataLayer.ContainerIsWaitingForResponse || messageType == MessageType.ReSubmit)
					{
						ZString errors = dataLayer.GetErrorText();
						if (errors.IsEmpty)
						{
							ZString warnings = dataLayer.GetWarningText();
							if (warnings.IsEmpty || UserConfirmWarningsAreNotAProblem(warnings))
							{
								var message = BuildMessageAndPostIt(dataLayer, messageType);
								try
								{
									dataLayer.Save();
									Globals.Message.ShowInformation("PRA Message Sent for Container No: " + dataLayer.ContainerNumber, "Send PRA Message");
								}
								catch (ZSaveException e)
								{
									message.Delete();
									ZExceptionReporting.HandleSaveException(e);
								}
							}
						}
						else
						{
							Globals.Message.ShowError("ERROR: Cannot Send PRA Message for Container No: " + dataLayer.ContainerNumber + "\r\n\r\n" + "The following Mandatory Fields have not been filled in, or are invalid:" + "\r\n\r\n" + errors, "Errors Found Sending PRA Message");
						}
					}
					else
					{
						Globals.Message.ShowError("ERROR: Cannot Send PRA Message for Container No: " + dataLayer.ContainerNumber + "\r\n\r\n" + "Still Waiting For a Response For the Last Message Sent.", "Cannot Send PRA Message");
					}
				}
			}
			else
			{
				Globals.Message.ShowError("Please select the Containers Tab and select a container before trying to send a PRA Message.", "Send PRA Message");
			}
		}

		protected
#if DEBUG
	virtual
#endif
 EDIMessage BuildMessageAndPostIt(IPRAMessagingData messagingData, MessageType messageType)
		{
			MessageBuilder messageBuilder = new MessageBuilder(messagingData, messageType);
			return messageBuilder.PostMessage();
		}

		protected bool UserConfirmWarningsAreNotAProblem(string warnings)
		{
			string warningHeader = "The following Warnings were found when checking the data used to Send a PRA:-";
			string warningFooter = @"
You can still send this message, but this message MAY be rejected by some
of the Terminals or cause problems getting the container accepted by the
Wharf.

The recommended course of action is to amend your data accordingly.

Are you sure you want to send this PRA now?";

			return (Globals.Message.Show(warningHeader + "\r\n\r\n" + warnings + warningFooter, "Send PRA Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes);
		}

		protected bool ContainerSelected
		{
			get { return (PRAContainer != null); }
		}

		protected MenuItem SubmitContainerMenuItem
		{
			get
			{
				if (submitContainerMenuItem == null)
				{
					submitContainerMenuItem = new ZMenuItem("&Submit PRA for Selected Container");
					submitContainerMenuItem.Click += new EventHandler(SubmitContainerMenuItem_Click);
				}

				return submitContainerMenuItem;
			}
		}
		MenuItem submitContainerMenuItem;

		protected MenuItem ReSubmitContainerMenuItem
		{
			get
			{
				if (reSubmitContainerMenuItem == null)
				{
					reSubmitContainerMenuItem = new ZMenuItem("&Re-Send PRA for Selected Container");
					reSubmitContainerMenuItem.Click += new EventHandler(ReSubmitContainerMenuItem_Click);
				}

				return reSubmitContainerMenuItem;
			}
		}
		MenuItem reSubmitContainerMenuItem;

		protected MenuItem CancelContainerMenuItem
		{
			get
			{
				if (cancelContainerMenuItem == null)
				{
					cancelContainerMenuItem = new ZMenuItem("&Cancel PRA for Selected Container");
					cancelContainerMenuItem.Click += new EventHandler(CancelContainerMenuItem_Click);
				}

				return cancelContainerMenuItem;
			}
		}
		MenuItem cancelContainerMenuItem;

		public void ReloadMenuItems(BusinessObject buinessEntity)
		{
			MenuItems.Clear();

			if (buinessEntity != null && (!buinessEntity.IsInDatabase || buinessEntity.HasChanges))
			{
				MenuItems.Add(new ZMenuItem(Declaration.GUI.ResString.GetMultilingualString("6A8D9498-3986-425F-8608-B0CAE1B359F4", "Please save before sending messages")));
				return;
			}

			MenuItems.Add(SubmitContainerMenuItem);
			MenuItems.Add(ReSubmitContainerMenuItem);
			MenuItems.Add(CancelContainerMenuItem);
		}

		#endregion
	}
}
