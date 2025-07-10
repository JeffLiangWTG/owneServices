using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CusUnderbond_ISR : ZUserControl
	{
		public CusUnderbond_ISR()
		{
			InitializeComponent();
			var sendMenuItem = new ZMenuItem(Res.GetData("a6c4e772-b3d6-4967-99c3-95e3dc6bd006", "Send ISR message now"), SendISR);
			IsrsGrid.ContextMenu.MenuItems.Add(0, sendMenuItem);
		}

		void SendISR(object sender, EventArgs e)
		{
			BusinessObject bizO = null;
			UnderbondSenderHelper.GetBusinessObjectFromGrid(IsrsGrid, out bizO);
			if (bizO != null)
			{
				UnderbondSenderHelper.SendMessage(bizO, new CcsukTransmissionMessageFunction.CUSDEC.ISR(), new SendsMessagesToCustomsGUI());
			}
		}
	}
}
