using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCOLSInterchangeProvider : OneInterchangeToOneMessageInterchangeProvider
	{
		public AUCOLSInterchangeProvider(NonDependentEDIMessageCollection messages)
				: base(messages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID => string.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = messages[0];
			SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, "xT", message.Company.LicenceKeyIdentifier);
			SetInterchangeHeaderText(interchange, message);
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.COLS;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_GB = message.EM_GB;
		}

		protected void SetInterchangeHeaderText(EDIInterchange interchange, EDIMessage message)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add(CustomMsgAttributes.SubscriptionKey, GetCOLSSubscriptionKey(interchange.Factory));
			switch (message.EM_MessageType)
			{
				case AUCOLSMessageTypeList.Codes.AddAdditionalDocument:
				case AUCOLSMessageTypeList.Codes.LodgementStatus:
				case AUCOLSMessageTypeList.Codes.AddAttachment:
				case AUCOLSMessageTypeList.Codes.SwitchAepLodgement:
					dictionary.Add(CustomMsgAttributes.Lrn, message.EM_ApplicationReference);
					break;
				case AUCOLSMessageTypeList.Codes.PaymentStatus:
					dictionary.Add(Enterprise.xTMessaging.Shared.Constants.CustomMsgAttributes.ReferenceNumber , message.EM_ApplicationReference);
					break;
			}
			interchange.SetHeaderTextWithAttributeDictionary(dictionary, true);
		}

		protected override StringBuilder MessageBody(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			return new StringBuilder(GetInterchangeBodyText(messages, interchange));
		}

		string GetInterchangeBodyText(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			_ = base.MessageBody(messages, interchange).ToString();
			var message = messages[0];
			return message.EM_MessageText;
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}

		protected override Type InterchangeType
		{
			get { return typeof(COLSInterchange); }
		}

		ZString GetCOLSSubscriptionKey(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AUCOLSInterchangeProvider|COLSSubscriptionKey", () =>
			{
				var result = ZString.Empty;
				var now = ZDateTime.Now;
				var refSysConfigLoader = new RefSysConfig.Loader(factory);
				var configEntry = refSysConfigLoader.Load(AUConstants.RefSysConfigCodes.AUCOLSSubscriptionKey, now);
				if (configEntry != null && (!configEntry.ZRC_EndDate.IsValid || now <= configEntry.ZRC_EndDate))
				{
					result = configEntry.ZRC_StringValue;
				}
				return result;
			});
		}
	}

	class CustomMsgAttributes
	{
		public const string Lrn = "custom.Lrn";
		public const string SubscriptionKey = "custom.AU.SubscriptionKey";
		public const string ContentTypeKey = "httpclient.content-type";
	}
}

