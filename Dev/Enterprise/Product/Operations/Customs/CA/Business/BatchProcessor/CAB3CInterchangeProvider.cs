using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.CA.Business.BatchProcessor
{
	class CAB3CInterchangeProvider : InterchangeProviderBase
	{
		public CAB3CInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message) => DoNotCollateType;

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
					SetInterchangeValuesForTransmit(interchange, messages, MessageTypeList.Codes.B3CUSDEC, BatchProcessorUtilities.CBSAClientID(messages[0].EM_IsTestMessage), BatchProcessorUtilities.MailBoxID);

					interchange.EI_HeaderText = EDIInterchange.UNOAUNAString
						+ GetUNB(PreparedTime.ToLocalZDateTime(), interchange.EI_To, ZString.Empty, interchange.EI_From, ZString.Empty, ZString.Empty, "UNOA", "3", "", false, false, ZString.Empty).ToString(new CACharSet())
						+ GetUNG(PreparedTime.ToLocalZDateTime(), "CUSDEC", CACustomsDataRegistry.Instance.TransmissionSite.Value, CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value, "KI", "UN", "S", "99B", "", accountSecurityCodeAndPassword).ToString(new CACharSet());
					interchange.EI_GB = messages[0].EM_GB;
					if (!interchange.ShouldSendViaEHub)
					{
						interchange.EI_Status = EDIInterchange.Status.SendPending;
					}
				}
			}
		}
	}
}
