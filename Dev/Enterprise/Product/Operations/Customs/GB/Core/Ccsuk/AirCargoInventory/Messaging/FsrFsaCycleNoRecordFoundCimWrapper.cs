using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class FsrFsaCycleNoRecordFoundCimWrapper : CusAwbToCimWrapper
	{
		public FsrFsaCycleNoRecordFoundCimWrapper(CcsukTransmissionMessageFunction.CIM.FSA how)
			: base(how.IncomingMessage.Factory, how)
		{
			howAsCimFsa = how;
		}

		protected override CargoImpBase GetCargoImpBaseMessage()
		{
			return new CIMFSA_OSI(howAsCimFsa.IncomingMessage.EM_ApplicationReference, howAsCimFsa.OtherShipmentInformationLine, null);
		}

		public override ZString RecipientPima
		{
			get { return howAsCimFsa.IncomingMessage.Interchange.EI_From; }
		}

		protected override EDIMessage GetNewMessageCore()
		{
			var newMessage = howAsCimFsa.IncomingMessage.Factory.New<GbEDIMessage>();
			howAsCimFsa.IncomingMessage.EM_LinkedObject = newMessage;
			return newMessage;
		}

		protected override ZString SenderPimaCore
		{
			get { return howAsCimFsa.IncomingMessage.Interchange.EI_To.Replace("/", ""); }
		}

		readonly CcsukTransmissionMessageFunction.CIM.FSA howAsCimFsa;
	}
}
