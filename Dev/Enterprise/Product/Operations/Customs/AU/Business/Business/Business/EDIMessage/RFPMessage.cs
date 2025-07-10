using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPMessage : EDIMessage, Integration.Customs.AU.IEXDOCMessage
	{
		public new class Status : EDIMessage.Status
		{
			public const string NotSent = "NOT";
			public const string AwaitingResponse = "AWT";
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => EM_FormattedMessageText;
			set => base.EM_MessageInterpretation = value;
		}

		public static string SenderID
		{
			get { return EDIInterchange.InterchangePartyIDs.EXDOCSendersMailbox; } //There is no senders reference uses email address for routing
		}

		public static string ReceiverID
		{
			get { return EDIInterchange.InterchangePartyIDs.EXDOCReceiversMailbox; } // No receiver either
		}

		public RFPMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.EXDOC;
			EM_IsTestMessage = Env.Registry.AQISMessagingTestMode;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_Status = Status.Queued;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", SenderID, ReceiverID).GetNextFormatted(Factory) + "00";
		}

		protected override string GetSendersReference()
		{
			QuarantineExDocHeader quarantineExDocHeader = EM_LinkedObject as QuarantineExDocHeader;
			if (quarantineExDocHeader != null)
			{
				quarantineExDocHeader.Declaration.PopulateJE_DeclarationReferenceIfNeeded();
				return quarantineExDocHeader.Declaration.JE_DeclarationReference.ToUpper();
			}
			throw new OdysseyException("Don't know how to get the senders reference for this message.");
		}

		protected override IStreamFormatter MessageStreamFormatter
		{
			get
			{
				return new RFPMessageStreamFormatter();
			}
		}
	}
}
