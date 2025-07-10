using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Customs.GB.Chief;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk
{
	/// <summary>
	/// Works out an ediMessage's application code so that it can go into the DB with the correct code.
	/// Looks at the sender and receiver mainly.
	/// This will allow the CCSUK sender/receiver to receive messages destined for many applications, not just for the UK CusRes processor.
	/// This will allow us to download messages for, say, CargoIMP applications and let that application have them. 
	/// </summary>
	public static class CcsukEdiMessageDiverter
	{
		public static ZString GetTargetApplicationCodeBasedOnInterchangeParties(EDIInterchange interchange)
		{
			string result = interchange.EI_ApplicationCode;
			if (InterchangeIsFromChiefOrCukExports(interchange.EI_From))
			{
				if (InterchangesMessageIsAGenral(interchange) || InterchangesMessageIsCimFsn(interchange) || InterchangesMessageIsControlFromCcsukExports(interchange))
				{
					result = ApplicationCodeList.Codes.GbCcsuk;
				}
				else
				{
					result = ApplicationCodeList.Codes.GbEdifactShared;
				}
			}
			else if (InterchangeIsFromCcsuk(interchange.EI_From))
			{
				result = ApplicationCodeList.Codes.GbCcsuk;
			}
			return result;
		}

		static bool InterchangesMessageIsControlFromCcsukExports(EDIInterchange interchange)
		{
			if (interchange != null && interchange.ContainedMessages.Count > 0)
			{
				var msg = interchange.ContainedMessages[0];
				return interchange.EI_From == PimaForCcsukNes && msg.EM_MessageText.Contains(msg.CharacterSet.ElementDelimiter + "CONTRL" + msg.CharacterSet.SubElementDelimiter);
			}
			return false;
		}

		static bool InterchangesMessageIsCimFsn(EDIInterchange interchange)
		{
			if (interchange != null && interchange.ContainedMessages.Count > 0)
			{
				var msg = interchange.ContainedMessages[0];
				return msg.EM_MessageText.Contains(msg.CharacterSet.ElementDelimiter + "CIMFSN" + msg.CharacterSet.SubElementDelimiter);
			}
			return false;
		}

		static bool InterchangesMessageIsAGenral(EDIInterchange interchange)
		{
			if (interchange != null && interchange.ContainedMessages.Count > 0)
			{
				var msg = interchange.ContainedMessages[0];
				return msg.EM_MessageText.Contains(msg.CharacterSet.ElementDelimiter + GenralMessageGenerator.GenralMessageCode + msg.CharacterSet.SubElementDelimiter);
			}
			return false;
		}

		static bool InterchangeIsFromCcsuk(ZString sender)
		{
			return sender == PimaForCcsuk || sender == PimaForCommunityDatabase;
		}

		static bool InterchangeIsFromChiefOrCukExports(ZString sender)
		{
			return sender.StartsWith(ChiefConstants.ChiefCcsukPimaPrefix)
					|| sender.StartsWith(ChiefConstants.ChiefCcsukPrintServerPima_Live)
					|| sender.StartsWith(ChiefConstants.ChiefCcsukPrintServerPima_Test)
					|| sender == PimaForCcsukNes;
		}

		public const string PimaForCcsukNes = "CUKSYS98CCSNES";
		public const string PimaForCcsuk = "CUKCCS98CCSCUK";
		public const string PimaForCommunityDatabase = "CUKSYS98COMMDB";
	}
}
