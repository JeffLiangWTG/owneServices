using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.PBN.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public override ResourceString MenuCaption => ResString.GetMultilingualString("A1B86845-5793-44CE-8347-474D491DD5CD", "PBN");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			_ = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, ResString.GetMultilingualString("A3276A04-F472-47CD-B36E-F173A187DC5C", "Send PBN"), Header, SendPBN, validateManifest: false);
			_ = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, ResString.GetMultilingualString("8E5E8ECC-046E-48E0-AF24-A07D3A7797F2", "Create PBN"), Header, CreatePBNmenuItem_Click);
			_ = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, ResString.GetMultilingualString("D23AF65F-EDDF-41AD-A28C-277D720339F7", "Update PBN"), Header, UpdatePBNMenuItem_Click);
			_ = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, ResString.GetMultilingualString("A112BCDF-8CC3-4904-A3EE-D2D6971F03BF", "Update PBN Declarations"), Header, UpdatePBNDeclarationsMenuItem_Click);
			_ = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, ResString.GetMultilingualString("CD19EE45-CB5D-4795-AB4C-C6F03ECC88CE", "Lookup PBN"), Header, LookupPBNMenuItem_Click);
			_ = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, ResString.GetMultilingualString("F6E21B1C-C585-4837-9416-246BB69BC13A", "Lookup Channel"), Header, LookupChannelMenuItem_Click);

			return menuItems.ToArray();
		}

		void SendPBN()
		{
			var header = Header;
			if (SaveDataFirst.Confirm(header, mainForm))
			{
				var sendingObjectParent = new PBNMessageSendingObjectParent(header);
				using (var form = new PBNMessageSendingForm(sendingObjectParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						var messageSent = 0;
						foreach (var sendingObject in sendingObjectParent.SendingObjectsCollection.Where(x => x.ShouldSend))
						{
							var sender = sendingObject.CreateSender();
							if (sender.Send())
							{
								messageSent++;
							}
						}

						if (messageSent > 0)
						{
							TrySaveAndShowMessage(Res.GetString("236660E0-2C67-455C-B386-738CE5B392B6", "Message queued for delivery"));
						}
					}
				}
			}
		}

		void CreatePBNmenuItem_Click() => Send(PBNMessageTypes.Codes.CreatePBN);

		void UpdatePBNMenuItem_Click() => Send(PBNMessageTypes.Codes.UpdatePBN);

		void UpdatePBNDeclarationsMenuItem_Click() => Send(PBNMessageTypes.Codes.UpdatePBNDeclarations);

		void LookupPBNMenuItem_Click() => Send(PBNMessageTypes.Codes.LookupPBN);

		void LookupChannelMenuItem_Click() => Send(PBNMessageTypes.Codes.LookupPBNChannel);

		void Send(ZString messageType)
		{
			var sendingObject = new PBNMessageSendingObject(Header);
			sendingObject.MessageType = messageType;
			var sender = sendingObject.CreateSender();
			if (sender.Send())
			{
				TrySaveAndShowMessage(Res.GetString("236660E0-2C67-455C-B386-738CE5B392B6", "Message queued for delivery"));
			}
		}

		void TrySaveAndShowMessage(string message)
		{
			try
			{
				Header.Factory.Save();
				Globals.Message.ShowInformation(message);
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}
	}
}
