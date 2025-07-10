using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.CA.Business
{
	class IMPInterchangeProvider : InterchangeProviderBase
	{
		public IMPInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		#region Implementation

		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message)
		{
			var builder = new ZStringBuilder();
			builder.Append(message.EM_MessageType);
			if (message.EM_MessageType == MessageTypeList.Codes.Query)
			{
				builder.Append(message.EM_MessageSubType);
			}
			builder.AppendIfNotEmpty(BatchProcessorUtilities.GetAccountSecurityCodeFromEDIMessage(message));
			builder.Append(message.EM_GB.ToStringKey());
			builder.Append(message.EM_IsTestMessage.ToString());
			return builder.ToString();
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get
			{
				return "Client Netwotk ID hasn't been setup. Please set it up in " + BatchProcessorUtilities.RegistryLocation(CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries);
			}
		}

		protected override Type InterchangeType
		{
			get { return typeof(CAEDIInterchange); }
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return GetUNEString(messageCount.ToString()) + GetUNZString("1");
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var currentBranch = (messages[0].Branch ?? GlbBranch.CurrentBranch).PK.ToGuid();
				using (DisposableEnvironment.ForBranch(currentBranch))
				{
					var accountSecurityCodeAndPassword = BatchProcessorUtilities.GetAccountSecurityCodeFromEDIMessage(messages[0]) + BatchProcessorUtilities.GetAccountSecurityPasswordFromEDIMessage(messages[0]);
					var impMessageType = messages[0].EM_MessageType;
					bool testMode;
					switch (impMessageType)
					{
						case MessageTypeList.Codes.EDIRelease:
						case MessageTypeList.Codes.RNSRequest:
						case MessageTypeList.Codes.Query:
						case MessageTypeList.Codes.TradeChainPartner:
						case MessageTypeList.Codes.CSARevenueSummaryForm:
						case MessageTypeList.Codes.XTypeEntry:
							testMode = messages[0].EM_IsTestMessage;
							break;
						case MessageTypeList.Codes.TestMessage:
							testMode = true;
							break;
						default:
							throw new ApplicationException("Invalid IMP Message Type: " + impMessageType);
					}
					SetInterchangeValuesForTransmit(interchange, messages, impMessageType, BatchProcessorUtilities.CBSAClientID(testMode), BatchProcessorUtilities.MailBoxID);

					string uNB;
					string uNG;
					switch (impMessageType)
					{
						case MessageTypeList.Codes.EDIRelease:
							uNB = GetUNB(interchange, "1", "");
							uNG = GetUNG("", "CUSDEC", testMode ? "RT" : "RP", "D", "96A", accountSecurityCodeAndPassword);
							break;
						case MessageTypeList.Codes.XTypeEntry:
							uNB = GetUNB(interchange, "3", "");
							uNG = GetUNG(CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value, "CUSDEC", "KI", "S", "99B", accountSecurityCodeAndPassword);
							break;
						case MessageTypeList.Codes.Query:
							uNB = GetUNB(interchange, "3", "");
							var recipientId = messages[0].EM_MessageSubType == QueryMessageSubType3CharCodes.Codes.QRCLASSTAR ? "QA" : "QE";
							uNG = GetUNG(CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value, "CUSDEC", recipientId, "S", "99B", accountSecurityCodeAndPassword);
							break;
						case MessageTypeList.Codes.RNSRequest:
							uNB = GetUNB(interchange, "3", "CUSREP");
							uNG = GetUNG("", "CUSREP", testMode ? "PARSTST" : "PARSPDN", "D", "96A", "");
							break;
						case MessageTypeList.Codes.TradeChainPartner:
							uNB = GetUNB(interchange, "3", "CUSPED");
							uNG = GetUNG("", "CUSPED", "CSAUPDATE", "S", "99B", accountSecurityCodeAndPassword);
							break;
						case MessageTypeList.Codes.CSARevenueSummaryForm:
							uNB = GetUNB(interchange, "3", "CUSDEC");
							uNG = GetUNG("", "CUSDEC", testMode ? "CT" : "CP", "S", "99B", "");
							break;
						case MessageTypeList.Codes.TestMessage:
							uNB = GetUNB(interchange, "3", "");
							uNG = GetUNG(CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value, "CUSDEC", "TMG", "S", "99B", "");
							break;
						default:
							throw new ApplicationException("Invalid IMP Message Type: " + impMessageType);
					}
					interchange.EI_HeaderText = EDIInterchange.UNOAUNAString + uNB + uNG;
					interchange.EI_GB = messages[0].EM_GB;
					if (!interchange.ShouldSendViaEHub)
					{
						interchange.EI_Status = EDIInterchange.Status.SendPending;
					}
				}
			}
		}

		string GetUNB(EDIInterchange interchange, string version, string applicationReference)
		{
			return GetUNB(PreparedTime.ToLocalZDateTime(), interchange.EI_To, ZString.Empty, interchange.EI_From, ZString.Empty, ZString.Empty, "UNOA", version, applicationReference, false, false, ZString.Empty).ToString(new CACharSet());
		}

		string GetUNG(string controlOffice, string messageType, string recipientId, string msgVer, string msgRel, string accountSecurityCodeAndPassword)
		{
			return GetUNG(PreparedTime.ToLocalZDateTime(), messageType, CACustomsDataRegistry.Instance.TransmissionSite.Value, controlOffice, recipientId, "UN", msgVer, msgRel, "",
				accountSecurityCodeAndPassword).ToString(new CACharSet());
		}

		#endregion
	}
}
