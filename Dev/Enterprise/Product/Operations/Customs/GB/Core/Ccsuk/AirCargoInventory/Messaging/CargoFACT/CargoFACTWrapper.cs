using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Edifact;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CargoFACTWrapper
	{
		public CargoFACTWrapper(CargoImpBase cargoImpDefintionToWrap)
		{
			wrappee = cargoImpDefintionToWrap;
		}

		public ZString Wrap(string commonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder)
		{
			CIMXXXMessage cim = new CIMXXXMessage();
			var unh = cim.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = "<<MSGNO PLACEHOLDER>>";
			unh.MessageIdentifier.MessageType = "CIM" + wrappee.CargoImpCode;
			unh.MessageIdentifier.MessageReleaseNumber = "0";
			unh.MessageIdentifier.MessageVersionNumber = wrappee.CargoImpVersion.ToString();
			unh.MessageIdentifier.ControllingAgency = "IA";
			unh.CommonAccessReference = commonAccessReference;

			int i = 1;
			FTXSegment ftx = null;
			foreach (var lineOfCargoImp in wrappee.AllCargoImpLinesIncludingType)
			{
				if (i == 1)
				{
					ftx = cim.FTX.InstantiateAChildAndAddItToChildrenCollection();
					ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("CIM");
				}

				if (i == 1)
				{ ftx.TextLiteral.FreeTextValue1 = lineOfCargoImp; }
				if (i == 2)
				{ ftx.TextLiteral.FreeTextValue2 = lineOfCargoImp; }
				if (i == 3)
				{ ftx.TextLiteral.FreeTextValue3 = lineOfCargoImp; }
				if (i == 4)
				{ ftx.TextLiteral.FreeTextValue4 = lineOfCargoImp; }
				if (i == 5)
				{
					ftx.TextLiteral.FreeTextValue5 = lineOfCargoImp;
					i = 0;
				}
				i++;
			}
			var unt = cim.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.MessageReferenceNumber = unh.MessageReferenceNumber;
			unt.NumberOfSegmentsInTheMessage = cim.CountIncludingUNT.ToString();
			return cim.ToString(Charset);
		}

		public UNCharacterSet Charset
		{
			private get
			{
				if (charset == null)
				{
					charset = new UkCharSet();
				}
				return charset;
			}
			set { charset = value; }
		}
		UNCharacterSet charset { get; set; }
		readonly CargoImpBase wrappee;
	}
}
