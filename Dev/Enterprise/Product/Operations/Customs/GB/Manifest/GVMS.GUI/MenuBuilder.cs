using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GVMS.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("C6243E16-C747-4EFD-8A84-5677795780C9", "GVMS");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();
			var header = Header as AsycudaManifestHeader;

			if (IsValidForMessage())
			{
				if (header.RegistrationNumber.IsEmpty)
				{
					AddSendManifestMenuItem(mainForm, menuItems, header);
				}
				else if (header.AMA_CustomsStatus != GVMSCustomsStatus.Codes.Cancelled)
				{
					AddSendManifestAmendmentMenuItem(mainForm, menuItems, header);
					AddDeleteManifestMenuItem(mainForm, menuItems, header);
				}
				else
				{
					AddReSendManifestMenuItem(mainForm, menuItems, header);
				}
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			if (header.Consol != null)
			{
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, ResString.GetMultilingualString("GVMS|PopulateFromConsol", "Populate from consol"), header, () => GVMSExtensions.PopulateReferenceFromConsol(header, ""), false);
			}

			return menuItems.ToArray();
		}

		public void AddSendManifestMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header)
		{
			var captionSendManifest = ResString.GetMultilingualString("GVMS|SendNewManifest", "Send &Manifest");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, captionSendManifest, header, () => CreateManifestLevelMessage(header, Constants.GVMSMessageSubTypes.NEW), true);
		}

		public void AddSendManifestAmendmentMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header)
		{
			var captionSendManifest = ResString.GetMultilingualString("GVMS|SendNewManifest", "Send &Manifest");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, captionSendManifest, header, () => CreateManifestLevelMessage(header, Constants.GVMSMessageSubTypes.AMEND), true);
		}

		public void AddReSendManifestMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header)
		{
			var captionSendManifest = ResString.GetMultilingualString("GVMS|ReSendManifest", "Re-send &Manifest");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, captionSendManifest, header, () => ReSendManifest(header), true);
		}

		public void AddDeleteManifestMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header)
		{
			var captionSendManifest = ResString.GetMultilingualString("GVMS|DeleteManifest", "Delete &Manifest");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, captionSendManifest, header, () => CreateManifestLevelMessage(header, Constants.GVMSMessageSubTypes.CANCEL), false);
		}

		void ReSendManifest(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			if (Globals.Message.ShowConfirmation("This GMR is in status 'cancelled'.  Submitting again will cause the GMR ID to be replaced with a new one if accepted.  Proceed?", "Confirm Send", "Type 'yes' to continue", "yes", MessageBoxIcon.Information) == DialogResult.OK)
			{
				CreateManifestLevelMessage(header, Constants.GVMSMessageSubTypes.NEW);
			}
		}

		void CreateManifestLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			if (header is AsycudaManifestHeader gvmsHeader)
			{
				try
				{
					var wrappedManifestHeader = new GvmsManifestSenderWrapper(gvmsHeader);
					var message = gvmsHeader.Messages.AddNew(typeof(GVMSEDIMessage));
					message.EM_LinkedObject = gvmsHeader; // some plumbing is missing which requires this
					message.EM_MessageText = GVMSEDIMessage.Serialize(wrappedManifestHeader, indentedNicely: true);
					message.EM_MessageSubType = messageSubType;
					message.EM_ReceiveTransmit = GVMSEDIMessage.Direction.Transmit;
					header.Factory.Save();
					if (message.EM_MessageText.Length > 0)
					{
						Globals.Message.Show("Message queued for delivery");
					}
				}
				catch (ZSaveException ex)
				{
					Globals.Message.Show(Res.GetString("4AD5417E-DFCA-4C95-B10D-D4AE5EB692A1", "The following error was encountered while saving the changes:") + ex.Message);
				}
			}
		}
	}
}
