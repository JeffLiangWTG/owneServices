using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Edifact;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CukFsrCreator : CusAwbToInventoryMessageGenerator
	{
		public CukFsrCreator(ICcsukCusAwb cusAwb, CcsukTransmissionMessageFunction messageFunction)
			: base(cusAwb)
		{
			charSet = new UkCharSet();
			source = cusAwb.GetAwbToFsrProvider(messageFunction);
			result = new CUKFSR();
			awbReferenceNumber = cusAwb.ReferenceNumber;
			this.messageFunction = messageFunction;
		}

		public CukFsrCreator(IFSR source, ZString awbReferenceNumberForInterpretation, CcsukTransmissionMessageFunction messageFunction, BusinessObjectFactory factory, UNCharacterSet charSet)
			: this(source, awbReferenceNumberForInterpretation, messageFunction, factory)
		{
			this.charSet = charSet;
		}

		public CukFsrCreator(IFSR source, ZString awbReferenceNumberForInterpretation, CcsukTransmissionMessageFunction messageFunction, BusinessObjectFactory factory)
			: base(factory)
		{
			charSet = new UkCharSet();
			this.source = source;
			result = new CUKFSR();
			awbReferenceNumber = awbReferenceNumberForInterpretation;
			this.messageFunction = messageFunction;
		}

		public override string MakeMessageText()
		{
			MakeUnh();
			MakeBgm();
			MakeLoc();
			MakeCom();
			MakeUNT();
			return CUSCARGeneratorBase.ReplaceFakeSegmentNames(charSet, result.ToString(charSet));
		}

		void MakeUnh()
		{
			var unh = result.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.MessageIdentifier.MessageType = "CUKFSR";
			unh.MessageIdentifier.MessageVersionNumber = "1";
			unh.MessageIdentifier.MessageReleaseNumber = "912";
			unh.MessageIdentifier.ControllingAgency = "BT";
			unh.CommonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder;
		}

		void MakeBgm()
		{
			var bgm = result.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentName = "";
			bgm.DocumentMessageIdentification.DocumentIdentifier = source.AirwaybillPrefixAndAirwaybillNumber;

			if (!source.SplitReference.IsEmpty)
			{
				bgm.ReferenceC506.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("ACD");
				bgm.ReferenceC506.DocumentLineIdentifier = source.SplitReference;
			}
			//else ??? Need to check this
			if (!source.HousewaybillNumber.IsEmpty)
			{
				bgm.ReferenceC506.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("HWB");
				bgm.ReferenceC506.ReferenceIdentifier = source.HousewaybillNumber;
			}

			bgm.DocumentTypeCode4343 = source.ResponseRequiredIndicator ==
				CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode ? CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode : source.ResponseRequiredIndicator.ToString();
		}

		void MakeLoc()
		{
			if (!(messageFunction is CcsukTransmissionMessageFunction.CUKFSR.FsaWithoutShed) && (!source.ShedOperatorIdentity.IsEmpty || !source.Airport.IsEmpty))
			{
				var loc = result.LOC.InstantiateAChildAndAddItToChildrenCollection();
				CUSCARGeneratorBase.MakeLOCAndAddType11ForAOA(loc, source.Airport, source.ShedOperatorIdentity);
			}
		}

		void MakeCom()
		{
			if (source.ResponseRequiredIndicator == FsrRequestType.Codes.FsnRetransmission)
			{
				var com = result.COM.InstantiateAChildAndAddItToChildrenCollection();
				com.CommunicationContact.CommunicationNumber = source.RecipientID;
				com.CommunicationContact.CommunicationNumberCodeQualifier = CommunicationNumberCodeQualifierList.GetFromString("EI");
			}
		}

		void MakeUNT()
		{
			var unt = result.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = result.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		public override ZString MessageInterpretation
		{
			get
			{
				return string.Format(
						@"{2}
						<h3>Freight Status Request</h3>
							<p>
								<b>{0}</b>
							</p>
							<p>
								Response requested: {1}
							</p>
						", awbReferenceNumber, source.ResponseRequiredIndicator, MessagePrettierCss.CSS);
			}
		}

		readonly UNCharacterSet charSet;
		readonly IFSR source;
		readonly CUKFSR result;
		readonly ZString awbReferenceNumber;
		readonly CcsukTransmissionMessageFunction messageFunction;
	}
}
