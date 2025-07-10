using System;
using System.Linq;
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
	class ACIInterchangeProvider : InterchangeProviderBase
	{
		public ACIInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		#region Implementation

		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message)
		{
			return (message.EM_MessageType == MessageTypeList.Codes.ACIForwarderClose ?
					MessageTypeList.Codes.ACIHouseBill : message.EM_MessageType.ToString()) + message.EM_GB.ToStringKey();
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

		protected override ZString GetInterchangeFooterWithUNTsInBody(int messageCount, int uneCount)
		{
			return GetUNZString(uneCount.ToString()).ToString();
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var currentBranch = (messages[0].Branch ?? GlbBranch.CurrentBranch).PK.ToGuid();
				using (DisposableEnvironment.ForBranch(currentBranch))
				{
					var aciMessageType = messages[0].EM_MessageType;
					switch (aciMessageType)
					{
						case MessageTypeList.Codes.SupplementaryCargoReport:
							PopulateInterchangeForSupplementaryMessages(messages, interchange);
							break;
						case MessageTypeList.Codes.ACIHouseBill:
						case MessageTypeList.Codes.ACIForwarderClose:
							PopulateInterchangeForEManifestForwarderMessages(messages, interchange);
							break;
						default:
							throw new ApplicationException("Invalid ACI Message Type: " + aciMessageType);
					}
					interchange.EI_GB = messages[0].EM_GB;
					if (!interchange.ShouldSendViaEHub)
					{
						interchange.EI_Status = EDIInterchange.Status.SendPending;
					}
				}
			}
		}

		void PopulateInterchangeForSupplementaryMessages(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var testMode = !Env.Instance.IsProductionSystem;
			SetInterchangeValuesForTransmit(interchange, messages, MessageTypeList.Codes.SupplementaryCargoReport, BatchProcessorUtilities.CBSAClientID(testMode), BatchProcessorUtilities.MailBoxID);

			string uNB = GetUNB(PreparedTime.ToLocalZDateTime(), interchange.EI_To, ZString.Empty, interchange.EI_From, ZString.Empty, ZString.Empty, "UNOA", "3", ZString.Empty, false, false, ZString.Empty).ToString(new CACharSet());
			string uNG = GetUNG(PreparedTime.ToLocalZDateTime(), "GSMCAR", CACustomsDataRegistry.Instance.TransmissionSite.Value, CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value,
											testMode ? "SRT" : "SRP", "UN", "D", "00A", "SUPRPT", ZString.Empty).ToString(new CACharSet());
			interchange.EI_HeaderText = EDIInterchange.UNOAUNAString + uNB + uNG;
		}

		void PopulateInterchangeForEManifestForwarderMessages(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var houseBillMessages = new NonDependentEDIMessageCollection(interchange.Factory);
			houseBillMessages.AddRange(messages.Cast<Enterprise.Messaging.Business.EDIMessage>().Where(x => x.EM_MessageType == MessageTypeList.Codes.ACIHouseBill));
			var closeMessages = new NonDependentEDIMessageCollection(interchange.Factory);
			closeMessages.AddRange(messages.Cast<Enterprise.Messaging.Business.EDIMessage>().Where(x => x.EM_MessageType == MessageTypeList.Codes.ACIForwarderClose));

			var testMode = !Env.Instance.IsProductionSystem;
			var includeAssociationAssignedCode = CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.Value;

			ZString interchangeBody = ZString.Empty;
			int groupNumber = 0;

			if (houseBillMessages.Count > 0)
			{
				groupNumber++;
				var groupNumberString = groupNumber.ToString();
				var uNG = GetUNG(PreparedTime.ToLocalZDateTime(), "GOVCBR", CACustomsDataRegistry.Instance.TransmissionSite.Value, CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value,
								testMode ? "ACIHGT" : "ACIHGP", "UN", "D", "11B", includeAssociationAssignedCode ? new ZString("ACIHG") : ZString.Empty, ZString.Empty, groupNumberString).ToString(new CACharSet());
				var uNE = GetUNEString(houseBillMessages.Count.ToString(), groupNumberString);
				interchangeBody += uNG + MessageBody(houseBillMessages, interchange).ToString() + uNE;
			}

			if (closeMessages.Count > 0)
			{
				groupNumber++;
				var groupNumberString = groupNumber.ToString();
				var uNG = GetUNG(PreparedTime.ToLocalZDateTime(), "GOVCBR", CACustomsDataRegistry.Instance.TransmissionSite.Value, CACustomsDataRegistry.Instance.ControlOfficeAppliesAllCountries.Value,
								testMode ? "ACIHCMGT" : "ACIHCMGP", "UN", "D", "11B", includeAssociationAssignedCode ? new ZString("ACIHCM") : ZString.Empty, ZString.Empty, groupNumberString).ToString(new CACharSet());
				var uNE = GetUNEString(closeMessages.Count.ToString(), groupNumberString);
				interchangeBody += uNG + MessageBody(closeMessages, interchange).ToString() + uNE;
			}

			SetInterchangeValuesForTransmit(interchange, new NonDependentEDIMessageCollection(interchange.Factory), MessageTypeList.Codes.ManifestForwardHouse, BatchProcessorUtilities.CBSAClientID(testMode), BatchProcessorUtilities.MailBoxID);
			string uNB = GetUNB(PreparedTime.ToLocalZDateTime(), interchange.EI_To, ZString.Empty, interchange.EI_From, ZString.Empty, ZString.Empty, "UNOA", "3", ZString.Empty, false, false, ZString.Empty).ToString(new CACharSet());
			interchange.EI_HeaderText = EDIInterchange.UNOAUNAString + uNB;
			interchange.EI_BodyText = interchangeBody;
		}

		#endregion
	}
}
