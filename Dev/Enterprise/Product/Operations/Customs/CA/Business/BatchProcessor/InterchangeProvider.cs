using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.CA.Business
{
	class InterchangeProvider : InterchangeProviderBase
	{
		public InterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		#region Implementation
		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message)
		{
			return message.PK.ToString();
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "See CA Customs registry."; }
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return ZString.Empty;
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count != 1)
			{
				throw new ArgumentException("Messages collection must have one element only - had " + messages.Count, nameof(messages));
			}

			Enterprise.Messaging.Business.EDIMessage message = messages[0];
			message.EM_Status = EDIMessage.Status.Sent;
			interchange.ContainedMessages.Add(message);
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = GlbCompany.CurrentCompany.GC_Code;
			interchange.EI_To = "CAC";
			interchange.EI_BodyText = message.EM_MessageText;
		}

		protected override Type InterchangeType
		{
			get { return typeof(EDIInterchange); }
		}

		#endregion
	}
}
