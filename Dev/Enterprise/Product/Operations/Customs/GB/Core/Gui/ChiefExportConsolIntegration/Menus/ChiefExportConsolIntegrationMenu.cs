using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration
{
	public partial class ChiefExportConsolIntegrationMenu : EDIMenu
	{
		readonly ZMenuItem eaaMenu;
		readonly ZMenuItem ealMenu;
		readonly ZMenuItem edlMenu;
		readonly CustomsExportConsolIntegrationWrapper wrapper;

		public ChiefExportConsolIntegrationMenu(CustomsExportConsolIntegrationWrapper wrapper)
		{
			this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));

			Text = "CHIEF";

			eaaMenu = new ZMenuItem("Anticipate Goods' Arrival (EAA)", SendChiefAnticipateArrivalMessage);
			ealMenu = new ZMenuItem("Arrive Goods (EAL)", SendChiefArrivalMessage);
			edlMenu = new ZMenuItem("Depart Goods (EDL)", SendChiefDepartMessage);

			MenuItems.Clear();
			MenuItems.AddRange(new MenuItem[]
			{
				new ZMenuItem("Close Master (EAC)", SendChiefCloseMasterMessage),
				new ZMenuItem("Query Master (DEC)", SendChiefQueryDecMasterMessage),
				eaaMenu,
				ealMenu,
				edlMenu,
				new ZMenuItem("Close Master Using A Declaration", SendChiefCloseMasterMessageUsingDeclaration) // legacy function, allows user to send a message to Chief using one of the consol's shipments' declarations.
			});
		}

		public override void RefreshMenu()
		{
			var ccsukIsEnabled = wrapper.IsChiefCcsukEnabled
								&& !(wrapper.ForwardingConsol?.JK_MasterBillNum ?? ZString.Empty).IsEmpty;
			var isDep = wrapper.CredentialIsDEP;
			eaaMenu.Enabled = ccsukIsEnabled && isDep;
			ealMenu.Enabled = ccsukIsEnabled && isDep;
			edlMenu.Enabled = ccsukIsEnabled && isDep;
		}

		void SendChiefQueryDecMasterMessage(object sender, EventArgs args)
		{
			wrapper.QueryMasterDEC();
		}

		void SendChiefCloseMasterMessage(object sender, EventArgs args)
		{
			wrapper.CloseMasterUcrOnChiefDirectlyOnConsol();
		}

		void SendChiefAnticipateArrivalMessage(object sender, EventArgs args)
		{
			wrapper.AnticipateArrivalOnChief();
		}

		void SendChiefArrivalMessage(object sender, EventArgs args)
		{
			wrapper.ArriveGoodsOnChief();
		}

		void SendChiefDepartMessage(object sender, EventArgs args)
		{
			wrapper.DepartGoodsOnChief();
		}

		void SendChiefCloseMasterMessageUsingDeclaration(object sender, EventArgs args)
		{
			wrapper.CloseMasterUcrOnChiefUsingExistingEntry();
		}
	}
}
