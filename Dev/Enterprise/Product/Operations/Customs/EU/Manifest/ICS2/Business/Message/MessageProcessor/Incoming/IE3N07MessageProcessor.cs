using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N07;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N07MessageProcessor : MessageProcessorWithEmailNotification<Ie3N07Type>
	{
		public IE3N07MessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override Func<Ie3N07Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string MessageFriendlyNameCore => Res.GetString("6516F8AF-DA17-4103-8B4F-EBB0F9969B7A", "ENS Registration Response");

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N07Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("949611DC-5C47-4EA1-BBD0-D23A742E2552", "ICS2 ENS In Incorrect State {0}", manifestHeader.AMA_JobReference));
			htmlBuilder.AppendLine();

			var messageInfoTableBuilder = new HtmlTableCreator(new string[] { Res.GetString("3E20B809-6C76-4C63-B581-43856CA98183", "Code"), Res.GetString("63478985-18AF-4E83-9233-24287392F67E", "Description") });

			foreach (var item in messageObject.Error)
			{
				messageInfoTableBuilder.WriteRow(item.ValidationCode, item.Description);
			}

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N07Type messageObject)
		{
			return Res.GetString("DCB562E9-4CA6-4D61-B0D5-5C20DAF3A7E1", "ICS2 ENS In Incorrect State {0}", manifestHeader.AMA_JobReference);
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore()
		{
			return ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N07Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.INS;
		}
	}
}
