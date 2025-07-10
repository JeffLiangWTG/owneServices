using System;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.HK.Business;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.HK.ServiceTasks
{
	public class TraxonInterchangeProvider : OneInterchangeToOneMessageInterchangeProvider
	{
		public TraxonInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = messages[0];
			var companyPK = message.Branch.GB_GC.ToGuid();
			interchange.EI_GB = message.EM_GB;
			SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, TraxonMailbox, HKDataRegistry.Instance.HKTraxonSenderID.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));

			var interchangeOwner = message.EM_MessageOwner;
			var uNBString = GetUNB(PreparedTime.ToZDateTime(), interchange.EI_To, "PIMA", interchange.EI_From, "PIMA", interchangeOwner, "UNOA", "1", ZString.Empty, false, false, HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)).ToString(new UNOACharacterSet());
			interchange.EI_HeaderText = EDIInterchange.UNOAUNAString + uNBString;
			//Sample: UNA:+.? 'UNB+UNOA:1+RHKAGT021332880/HKG81:PIMA+RHKCCS83GLSCMMS:PIMA+040323:1650+1+PRDAGENT027'
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "Please set a value in Config > System > Registry > Customs > Hong Kong > ISAC Sender ID."; }
		}

		protected override Type InterchangeType
		{
			get { return typeof(TraxonInterchange); }
		}

		public const string TraxonMailbox = "RHKCCS83GLSCMMS";
	}
}
