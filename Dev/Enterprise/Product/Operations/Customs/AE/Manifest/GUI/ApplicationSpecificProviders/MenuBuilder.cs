using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI;

public sealed class MenuBuilder : ASYCUDA.GUI.MenuBuilder
{
	public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
	{
	}

	public override ResourceString MenuCaption => ResString.GetMultilingualString("AEManifest|MenuBuilder|MenuCaption", "AE Manifest");

	public override ZMenuItem[] BuildMenu()
	{
		if (!IsValidForMessage())
		{
			return new[] { GetInvalidMessageMenuItem() };
		}

		var menuItems = new List<ZMenuItem>();
		MenuBuilderHelper.AddBillLevelMenuItem(mainForm, menuItems, Header, CreateMessageFromSelectedItems, Res.GetString("D1A52596-B331-49D0-9FC7-F8E5714623CC", "Manifest"));
		return menuItems.ToArray();
	}

	void CreateMessageFromSelectedItems(AsycudaManifestHeader header, string messageSubType, IList<IMessageParent> messageParents, MessageChooser chooser)
	{
		var successfulMessageCount = new Business.CUSCARMessageSender().SendBillMessage(header, chooser);
		if (successfulMessageCount > 0)
		{
			Save(header, successfulMessageCount);
		}
	}

	void Save(AsycudaManifestHeader header, int successfulMessageCount)
	{
		var caption = Res.GetString("FCEBB78B-FC88-40D9-8CC3-CCF8A4D7BD6C", "Send Manifest Message");
		try
		{
			header.Factory.Save();
			header.Messages.Reload(false);
			Globals.Message.ShowInformation(Res.GetString("10B05C08-D5F7-4EF7-B2EA-115CA74A8C51", "{0} CUSCAR Manifest Message(s) Created.", successfulMessageCount), caption);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}
}
