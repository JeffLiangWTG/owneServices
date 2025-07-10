using System;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCInterchangeProvider : OneInterchangeToOneMessageInterchangeProvider
	{
		public EXDOCInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		public abstract class ApplicationReference
		{
			public const string exdoc = "EXDOC";
		}

		#region Overrides

		protected override UNCharacterSet CurrentUNCharacterSet
		{
			get { return new UNOBCharacterSet(); }
		}

		protected override Type InterchangeType
		{
			get { return typeof(EXDOCInterchange); }
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			SetInterchangeValuesForTransmit(interchange, messages, messages[0].EM_MessageType, EDIInterchange.InterchangePartyIDs.EXDOCReceiversMailbox, EDIInterchange.InterchangePartyIDs.EXDOCSendersMailbox);
			interchange.EI_HeaderText = GetUNB(PreparedTime.ToZDateTime(), EDIInterchange.InterchangePartyIDs.EXDOCReceiversMailbox, ZString.Empty, EDIInterchange.InterchangePartyIDs.EXDOCSendersMailbox, ZString.Empty, messages[0].EM_MessageOwner, "UNOB", "2", ApplicationReference.exdoc, false, messages[0].EM_IsTestMessage, ZString.Empty, "EDI").ToString(CurrentUNCharacterSet);
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "Please contact Cargowise Support to get a new Registration File applied to your system. There is not enough information in your current system registration to generate a EXDOC Site ID."; }
		}
		#endregion
	}
}
