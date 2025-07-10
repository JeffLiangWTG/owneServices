using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.CA.Business
{
	class CADInterchangeProvider : InterchangeProviderBase
	{
		public CADInterchangeProvider(NonDependentEDIMessageCollection messages)
				: base(messages) { }

		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message)
		{
			return DoNotCollateType;
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get
			{
				return "Client Netwotk ID hasn't been setup. Please set it up in " + BatchProcessorUtilities.RegistryLocation(CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries);
			}
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var currentBranch = (messages[0].Branch ?? GlbBranch.CurrentBranch).PK.ToGuid();
				using (DisposableEnvironment.ForBranch(currentBranch))
				{
					var messageType = messages[0].EM_MessageType;
					bool testMode = messages[0].EM_IsTestMessage;
					SetInterchangeValuesForTransmit(interchange, messages, messageType, BatchProcessorUtilities.CBSAClientID(testMode), BatchProcessorUtilities.MailBoxID);
					interchange.EI_GB = messages[0].EM_GB;
					if (!interchange.ShouldSendViaEHub)
					{
						interchange.EI_Status = EDIInterchange.Status.SendPending;
					}
				}
			}
		}

		protected override Type InterchangeType
		{
			get { return typeof(CAEDIInterchange); }
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return ZString.Empty;
		}
	}
}
