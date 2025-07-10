namespace Enterprise.Customs.CA.Business
{
	using System.Data;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Common.CA;
	using Enterprise.Edifact.D99B.Elements;
	using Enterprise.Edifact.D99B.Messages.CUSDEC;
	using Enterprise.Edifact.D99B.Segments;
	using Enterprise.Environment;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Core;

	public class B3XMessage : EDIMessageWithBatchNumber
	{
		public B3XMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString TransactionNumber
		{
			get
			{
				if (EdifactMessage is CUSDECMessage cusdec)
				{
					return (from SegmentGroup1 grp in cusdec.Group1
							from RFFSegment seg in grp.RFF
							where seg.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.TransactionReferenceNumber
							select seg.Reference.ReferenceIdentifier.PadLeft(9, '0')).FirstOrDefault();
				}
				return ZString.Empty;
			}
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIInterchange.ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.XTypeEntry;
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return EM_ReceiveTransmit == EDIInterchange.Direction.Receive ? new B3EntryStatusList() : base.MessageSubTypeList; }
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}
	}
}
