using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N01;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N01MessageProcessor : MessageProcessorWithEmailNotification<Ie3N01Type>
	{
		public IE3N01MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4758af52-ffd1-4eee-91b5-6de03e858907", "ENS Life-cycle Validation Error Notification");

		protected override Func<Ie3N01Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N01Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.VAL;
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N01Type messageObject)
		{
			return Res.GetString("0859e577-7e25-43bf-8115-0213ede9fc2d", "ICS2 Validation Error Notification for {0}/{1}", manifestHeader.AMA_JobReference, GetMasterReferenceNumber(messageObject));
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N01Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder();
			htmlBuilder.AppendLine();

			var messageInfoTableBuilder = new HtmlTableCreator(new string[] { Res.GetString("430bf0e1-868d-4992-ab47-ed98aab962d0", "Code"), Res.GetString("96b0bdad-9858-449a-a3b3-cd777ed8d1d6", "Description") });
			foreach (var error in messageObject.Error)
			{
				messageInfoTableBuilder.WriteRow(error.ValidationCode, error.Description);
			}

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;
	}
}
