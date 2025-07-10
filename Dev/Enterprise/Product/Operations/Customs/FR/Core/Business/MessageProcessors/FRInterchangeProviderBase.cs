using System;
using System.Text;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class FRInterchangeProviderBase : InterchangeProviderBase
	{
		protected FRInterchangeProviderBase(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}
		protected string RecipientId => FRCustomsDataRegistry.Instance.RecipientID.Value;
		protected abstract string GenericMessageInterchangeType { get; }

		protected override string InstructionHowToSetInterchangeSenderID => string.Empty;

		protected override string GetCollationKey(EDIMessage message)
		{
			string applicationReference = message.EM_ApplicationReference;
			return string.IsNullOrEmpty(applicationReference) ? DoNotCollateType : applicationReference;
		}

		protected override Type InterchangeType => typeof(FRInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var recipientId = RecipientId;
				var senderID = GlbCompany.CurrentCompany.LicenceKeyIdentifier;

				SetInterchangeValuesForTransmit(interchange, messages, GenericMessageInterchangeType, recipientId, senderID);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
				interchange.EI_HeaderText = GetHeaderText(recipientId, senderID);
			}
		}

		ZString GetHeaderText(ZString recipientId, ZString senderID)
		{
			var strBuilder = new StringBuilder();
			var writer = XmlTextWriter.Create(strBuilder, new XmlWriterSettings() { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
			writer.WriteElementString("SenderID", senderID);
			writer.WriteElementString("RecipientID", recipientId);
			writer.WriteElementString("InterchangeType", GenericMessageInterchangeType);
			writer.WriteElementString("InterchangeNumber", EDIInterchange.InterchangeNumberPlaceHolder);
			writer.Flush();
			writer.Close();

			return strBuilder.ToString();
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}
	}
}
