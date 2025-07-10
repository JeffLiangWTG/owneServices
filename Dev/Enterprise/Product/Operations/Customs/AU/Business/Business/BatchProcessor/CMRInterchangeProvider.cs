using System;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInterchangeProvider : InterchangeProviderBase
	{
		public CMRInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		#region Overrides

		protected override int MaximumInterchangeSize
		{
			get
			{
				int result = AUCustomsDataRegistry.Instance.MaximumInterchangeSize.Value - 100;
				if (result < 1)
				{
					result = 0;
				}

				return result;
			}
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			SetMessageNumber(messages);
			SetInterchangeValuesForTransmit(interchange, messages, messages[0].EM_MessageType, CMRRecipientID, GetCustomsRegNumber(messages[0]));
			ZString interchangeOwner = messages[0].EM_MessageOwner;
			if (interchangeOwner.IsEmpty)
			{
				interchangeOwner = interchange.EI_From;
			}

			string uNB = GetUNB(PreparedTime.ToZDateTime(), interchange.EI_To, ZString.Empty, interchange.EI_From, ZString.Empty, interchangeOwner, "UNOC", "3", ZString.Empty, messages[0].EM_MessageType != "CTL", messages[0].EM_IsTestMessage || GetCMRTestMode(messages[0]), ZString.Empty).ToString(CurrentUNCharacterSet);
			interchange.EI_HeaderText = EDIInterchange.UNOAUNAString + uNB;
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "Please check with Customs, obtain a site ID and enter the ID on the Config > System > Companies > Current Company > Customs Reg No."; }
		}

		protected override UNCharacterSet CurrentUNCharacterSet
		{
			get { return new UNOCCMRCharacterSet(); }
		}

		protected override Type InterchangeType
		{
			get { return typeof(CMRInterchange); }
		}

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.SendPending;

		#endregion

		#region Implementation

		void SetMessageNumber(NonDependentEDIMessageCollection messages)
		{
			int messageNumber = 0;
			foreach (EDIMessage message in messages)
			{
				messageNumber++;
				message.EM_MessageNum = messageNumber.ToString();
				message.EM_MessageText = message.EM_MessageText.Replace(EDIMessage.MessageNumberPlaceHolder, message.EM_MessageNum);
			}
		}

		public bool GetCMRTestMode(EDIMessage message)
		{
			return Env.Registry.GetCMRTestMode(message.Branch.Company.PK.ToGuid());
		}

		public static string GetCustomsRegNumber(EDIMessage message)
		{
			return message.Branch.Company.GC_CustomsRegistrationNo;
		}

		public static string CMRRecipientID
		{
			get { return "AAA336C"; }
		}

		#endregion
	}
}
