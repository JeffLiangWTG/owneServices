using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CusUnderbond_FBK : ZUserControl
	{
		public CusUnderbond_FBK()
		{
			InitializeComponent();
			var sendMenuItem = new ZMenuItem(Res.GetData("2e07edde-5aba-4886-abbb-ce1c72d4cb65", "Send FBK message now"), SendFBK);
			GridForFBK.ContextMenu.MenuItems.Add(0, sendMenuItem);
		}

		void SendFBK(object sender, EventArgs e)
		{
			BusinessObject bizO = null;
			UnderbondSenderHelper.GetBusinessObjectFromGrid(GridForFBK, out bizO);
			if (bizO != null)
			{
				UnderbondSenderHelper.SendMessage(bizO, new CcsukTransmissionMessageFunction.CUSDEC.FBK(), new SendsMessagesToCustomsGUI());
			}
		}
	}
}
