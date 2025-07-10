using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CusUnderbond_TSR : ZUserControl
	{
		public CusUnderbond_TSR()
		{
			InitializeComponent();
			var sendMenuItem = new ZMenuItem(Res.GetData("041baafc-4d21-4d4b-825b-1ba811fe5008", "Send TSR message now"), SendTSR);
			TsrsGrid.ContextMenu.MenuItems.Add(0, sendMenuItem);
		}

		void SendTSR(object sender, EventArgs e)
		{
			BusinessObject bizO = null;
			UnderbondSenderHelper.GetBusinessObjectFromGrid(TsrsGrid, out bizO);
			if (bizO != null)
			{
				UnderbondSenderHelper.SendMessage(bizO, new CcsukTransmissionMessageFunction.CUSDEC.TSR(), new SendsMessagesToCustomsGUI());
			}
		}
	}
}
