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
	public class SendCAeManifestCloseMessageProcessor : Integration.Customs.CA.ISendCAeManifestCloseMessageProcessor, IProcessor
	{
		public SendCAeManifestCloseMessageProcessor(CusCAeMHMaster cusCaeMhMaster)
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

			if (!new ACIForwarderCloseMessageManager(wrapper, new UserNotificationWrapper(notifications), true).SendMessage(MessageSubTypes.Create, false))
			{
				notifications.AddWarning(Res.GetString("8A3237D4-9053-4D09-9EDF-71D4B625C1BA", "Sending the CA eManifest Close Message for {0} failed", cusCaeMhMaster.BP_MessageReference));
			}
		}
		#endregion
	}
}
