namespace Enterprise.Customs.CA.Business
{
	using System.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Core;

	public class RNSRequestMessage : EDIMessage
	{
		public RNSRequestMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.RNSRequest;
		}

		#region Properties

		public override ZString CargoControlNumber
		{
			get
			{
				SetNumbers();
				return cargoControlNumber;
			}
		}
		ZString cargoControlNumber;

		public override ZString TransactionNumber
		{
			get
			{
				SetNumbers();
				return transactionNumber;
			}
		}
		ZString transactionNumber;

		void SetNumbers()
		{
			if (!numbersSet)
			{
				var cusrepMessage = EdifactMessage as Enterprise.Edifact.D96A.Messages.CUSREP.CUSREPMessage;
				if (cusrepMessage != null && cusrepMessage.Group1.Count > 0)
				{
					foreach (Enterprise.Edifact.D96A.Segments.RFFSegment rff in cusrepMessage.Group1[0].RFF)
					{
						if (rff.Reference.ReferenceQualifier == Enterprise.Edifact.D96A.Elements.ReferenceQualifierList.TransactionReferenceNumber)
						{
							transactionNumber = rff.Reference.ReferenceNumber;
						}
						else if (rff.Reference.ReferenceQualifier == Enterprise.Edifact.D96A.Elements.ReferenceQualifierList.CustomsDeclarationNumber)
						{
							cargoControlNumber = rff.Reference.ReferenceNumber;
						}
					}
				}
				numbersSet = true;
			}
		}
		ZBool numbersSet;

		public override ZString CBSAOffice
		{
			get
			{
				var cusrepMessage = EdifactMessage as Enterprise.Edifact.D96A.Messages.CUSREP.CUSREPMessage;
				if (cusrepMessage != null && cusrepMessage.Group2.Count > 0 && cusrepMessage.Group2[0].LOC.Count > 0)
				{
					return cusrepMessage.Group2[0].LOC[0].LocationIdentification.PlaceLocationIdentification;
				}
				return ZString.Empty;
			}
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new RNSMessageTypes(); }
		}

		#endregion

		#endregion

		internal static RNSRequestMessage GetRecentArrivalCertificationMessage(Enterprise.Messaging.Business.EDIMessageCollection messages)
		{
			return (RNSRequestMessage)messages.GetLastMessage(
				EDIMessage.ApplicationCodes.CAIMP,
				MessageTypeList.Codes.RNSRequest,
				EDIMessage.Direction.Transmit,
				ZString.Empty,
				new ZString[] { RNSMessageTypes.Codes.ArrivalCertification });
		}
	}
}
