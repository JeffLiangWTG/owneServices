using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDAManifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("3131DE70-AEEF-4EB2-AA5E-AD50E6B91E1D", "ASYCUDA Manifest (&{0})", Header.CountryName);

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();
			var messageLabel = ResString.GetMultilingualString("E79AF7FA-E479-4261-B98A-7D8359A7CD7F", "Manifest");
			if (!Header.IsPackedItemLevelManifestType && !Header.IsBillLevelManifestType)
			{
				ASYCUDA.GUI.MenuBuilderHelper.AddSendManifestMenuItem(mainForm, menuItems, Header, CreateManifestLevelMessage, messageLabel);
			}
			else
			{
				throw new NotSupportedException("ASYCUDA only supports manifest level manifests.");
			}
			return menuItems.ToArray();
		}

		void CreateManifestLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			Globals.Message.Show(new ASYCUDAManifestUniversalMessagingHelper(new NullLogger()).SendViaEHub(header, header.AMA_ManifestType, messageSubType, new ASYCUDA.Business.IMessageParent[] { Header }));
		}

		class NullLogger : INotifications
		{
			public void Add(INotification notification)
			{
				// Do nothing, because probably the person who write this didn't care about these logs.
			}
		}
	}
}
