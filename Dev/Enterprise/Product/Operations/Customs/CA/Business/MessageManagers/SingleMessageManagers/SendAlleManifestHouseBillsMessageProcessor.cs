using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class SendAlleManifestHouseBillsMessageProcessor : Integration.Customs.CA.ISendAlleManifestHouseBillsMessageProcessor, IProcessor
	{
		public SendAlleManifestHouseBillsMessageProcessor(CusCAeMHMaster cusCaeMhMaster)
		{
			this.cusCaeMhMaster = Argument.NotNull(cusCaeMhMaster, "CusCAeMHMaster");
		}
		readonly CusCAeMHMaster cusCaeMhMaster;

		#region IProcessor Members

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var wrapper = new CusCAeMHMasterCloseWrapper(cusCaeMhMaster);
			wrapper.AllCCNs.ForEach(inst => inst.IsShouldSend = true);

			foreach (var bill in cusCaeMhMaster.HouseBills)
			{
				if (!new ACIHouseBillMessageManager(bill, new UserNotificationWrapper(notifications)).SendMessage(MessageSubTypes.Create))
				{
					notifications.AddWarning(Res.GetString("3C149317-DB15-43C5-9A87-52A03FFC730D", "Sending the CA eManifest Close Message for House Bill {0} failed", bill.BW_HouseCCN));
				}
			}
		}
		#endregion
	}
}
