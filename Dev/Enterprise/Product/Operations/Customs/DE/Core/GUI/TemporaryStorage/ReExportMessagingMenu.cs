using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class ReExportMessagingMenu : ZMenuItem
	{
		public ReExportMessagingMenu()
		{
			this.SetCaptionResourceString();
		}

		public CusTempStorageJobHeader Header
		{
			get => header;
			set
			{
				header = value;
				RefreshMenuItems();
			}
		}
		CusTempStorageJobHeader header;

		void RefreshMenuItems()
		{
			if (Header != null)
			{
				MenuItems.Clear();

				var sendReExportMenuItem = new ZMenuItem(ResString.GetMultilingualString("60021E8A-41EB-460B-BCAB-2693743DC04C", "Send Re-Export"));
				sendReExportMenuItem.Click += SendReExportClick;

				MenuItems.Add(sendReExportMenuItem);
			}
		}

		void SendReExportClick(object sender, EventArgs e)
		{
			if (this.PreSaveMessage(Header))
			{
				try
				{
					var cusTempStorageDec = Header.REXDISCusTempStorageDec;
					if (Header.CanSendMessage(cusTempStorageDec))
					{
						var reExportSender = new ReExportSender(Header.REXDISCusTempStorageDec);
						var (canSend, whyCannotSend) = reExportSender.CanSend;
						if (canSend)
						{
							reExportSender.Send();
							Header.Factory.Save();
							Globals.Message.Show(MessagingMenuExtension.MessageHasBeenSent);
						}
						else
						{
							Globals.Message.Show(whyCannotSend);
						}
					}
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
	}
}
