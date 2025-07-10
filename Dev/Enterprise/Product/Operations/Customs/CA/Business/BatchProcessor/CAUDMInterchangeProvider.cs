using System;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class CAUDMInterchangeProvider : InterchangeProviderBase
	{
		public CAUDMInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		#region Constants

		const string RootXmlTag = "UniversalInterchange";
		const string RootNamespce = "xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"";
		const string HeaderXmlTag = "Header";
		const string BodyXmlTag = "Body";

		#endregion

		#region Implementation

		protected override Type InterchangeType
		{
			get { return typeof(CAUniversalXMLInterchange); }
		}

		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message) => DoNotCollateType;

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get
			{
				return Res.GetString("abfe024f-0149-4b2e-92ee-29d4166c62a4", "Client Network ID hasn't been setup. Please set it up in {0}", BatchProcessorUtilities.RegistryLocation(CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries));
			}
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return ZString.Empty;
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var pivotMessage = messages[0];
				var messagesCompany = pivotMessage.Company;
				var senderId = messagesCompany != null ? messagesCompany.LicenceKeyIdentifier : ZString.Empty;

				var isTestMode = !Env.Instance.IsProductionSystem;
				var recipientId = BatchProcessorUtilities.CBSAeHubID(isTestMode);

				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				interchange.EI_GB = pivotMessage.EM_GB;
				SetInterchangeValuesForTransmit(interchange, messages, EDIInterchangeTypeList.Codes.XDC, recipientId, senderId);
			}
		}

		protected override StringBuilder MessageBody(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var result = new StringBuilder();
			result.AppendFormat("<{0} {1}>", RootXmlTag, RootNamespce);
			result.AppendFormat("<{0}>", HeaderXmlTag);
			result.AppendFormat("<{0}>{1}</{0}>", "SenderID", CAUniversalXMLInterchange.SenderNetworkIDPlaceHolder);
			result.AppendFormat("<{0}>{1}</{0}>", "RecipientID", CAUniversalXMLInterchange.RecipientNetworkIDPlaceHolder);
			result.AppendFormat("<{0}>{1}</{0}>", "InterchangeNumber", EDIInterchange.InterchangeNumberPlaceHolder);
			result.AppendFormat("</{0}>", HeaderXmlTag);
			result.AppendFormat("<{0}>", BodyXmlTag);
			result.Append(base.MessageBody(messages, interchange));
			result.AppendFormat("</{0}>", BodyXmlTag);
			result.AppendFormat("</{0}>", RootXmlTag);
			return result;
		}

		#endregion

	}
}
