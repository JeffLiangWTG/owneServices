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
	class EXPInterchangeProvider : InterchangeProviderBase
	{
		public EXPInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		#region Implementation

		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message)
		{
			return message.EM_MessageType + message.EM_GB.ToStringKey();
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
					var eXPMessageType = messages[0].EM_MessageType;
					var isTestSystem = !Env.Instance.IsProductionSystem;
					SetInterchangeValuesForTransmit(interchange, messages, eXPMessageType, BatchProcessorUtilities.CBSAClientID(isTestSystem), BatchProcessorUtilities.MailBoxID);
					var uNB = GetUNB(PreparedTime.ToLocalZDateTime(), interchange.EI_To, ZString.Empty, interchange.EI_From, ZString.Empty, ZString.Empty, "UNOA", "3", ZString.Empty, false, false, ZString.Empty).ToString(new CACharSet());
					string uNG;
					if (eXPMessageType == MessageTypeList.Codes.G7Export)
					{
						uNG = GetUNG(isTestSystem ? "ET" : "EP", "EX1STP");
					}
					else
					{
						throw new ApplicationException("Invalid EXP Message Type: " + eXPMessageType);
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

		string GetUNG(ZString testFlag, ZString eXPMessageSubType)
		{
			return GetUNG(PreparedTime.ToLocalZDateTime(), "GSIMEX", CACustomsDataRegistry.Instance.TransmissionSite.Value, CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value,
	testFlag, "CC", "D", "00A", eXPMessageSubType, "").ToString(new CACharSet());
		}

		#endregion
	}
}
