using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N10;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class IE3N10MessageProcessor : MessageProcessorWithEmailNotification<Ie3N10Type>
	{
		public IE3N10MessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("904FE80A-DEDB-4224-A803-918E0594FA18", "Amendment Notification");

		protected override Func<Ie3N10Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N10Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder();
			htmlBuilder.AppendLine(Res.GetString("521580FB-AE9F-45CE-8213-96F4CEB01D40", "ICS2 Amendment Acknowledged {0}/{1}", manifestHeader.AMA_JobReference, manifestHeader.AMA_MasterBill));

			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N10Type messageObject)
		{
			return Res.GetString("6CE8C485-F403-4A83-B3EA-11188646A4D1", "ICS2 Amendment Acknowledged {0}/{1}", manifestHeader.AMA_JobReference, manifestHeader.AMA_MasterBill);
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N10Type messageObject)
		{
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.ACP;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;
	}
}
