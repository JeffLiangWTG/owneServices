using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CusUnderbond_IAR : ZUserControl
	{
		public CusUnderbond_IAR()
		{
			InitializeComponent();
			var sendMenuItem = new ZMenuItem(Res.GetData("a3fe29d2-f79f-4d87-95c9-1080d15ad6c6", "Send IAR message now"), SendIAR);
			GridForIAR.ContextMenu.MenuItems.Add(0, sendMenuItem);
		}

		void SendIAR(object sender, EventArgs e)
		{
			BusinessObject bizO = null;
			UnderbondSenderHelper.GetBusinessObjectFromGrid(GridForIAR, out bizO);
			if (bizO != null)
			{
				UnderbondSenderHelper.SendMessage(bizO, new CcsukTransmissionMessageFunction.CUSDEC.IAR(), new SendsMessagesToCustomsGUI());
			}
		}
	}
}
