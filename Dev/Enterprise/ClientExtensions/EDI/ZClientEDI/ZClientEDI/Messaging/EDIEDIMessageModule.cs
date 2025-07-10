namespace Enterprise.Client.EDI.Messaging
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using CargoWise.EntityFramework;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.Module;
	using Enterprise.ZArchitecture.GUI;

	public class EDIEDIMessageModule : EDIMessageModule
	{
		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIEDIMessageFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("View Decoded Customer Service Message", new EventHandler(DecodeMessage_Click)));
			return result.ToArray();
		}

		void DecodeMessage_Click(object sender, EventArgs e)
		{
			foreach (BusinessObject bizO in Grid.SelectedElements)
			{
				EDIMessage ediMessage = bizO as EDIMessage;
				EHubMessageDecodeForm form = new EHubMessageDecodeForm(ediMessage);
				form.Show();
			}
		}
	}
}
