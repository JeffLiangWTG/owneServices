using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Edifact;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class IemFsrCreator
	{
		public IemFsrCreator(IFSR iCukFsr)
		{
			this.charSet = new UkCharSet();
			source = iCukFsr;
			result = new IEMFSR();
		}

		public string MakeIEMFSR()
		{
			MakeUnh();
			MakeBgm();
			MakeUNT();
			return result.ToString(charSet);
		}

		void MakeUnh()
		{
			var unh = result.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			// What were they smoking...?
			unh.MessageIdentifier.MessageType = "IEMFSR";
			unh.MessageIdentifier.MessageVersionNumber = "0";
			unh.MessageIdentifier.MessageReleaseNumber = "902";
			unh.MessageIdentifier.ControllingAgency = "Z1";
		}

		void MakeBgm()
		{
			var bgm = result.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString("740");
			bgm.DocumentMessageIdentification.DocumentIdentifier = source.AirwaybillPrefixAndAirwaybillNumber;
		}

		void MakeUNT()
		{
			var unt = result.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = result.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		readonly UNCharacterSet charSet;
		readonly IFSR source;
		readonly IEMFSR result;
	}
}
