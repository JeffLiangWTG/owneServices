using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSOutboundEDIMessage : GbEDIMessage
	{
		public EMCSOutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			MessageNumberStrategy = new GbMessageNumberStrategy(factory, EDIMessage.ApplicationCodes.GbCustomsEMCS);
		}

		public new EMCSOutboundEDIMessageLookups Lookups => (EMCSOutboundEDIMessageLookups)base.Lookups;

		protected override EDIMessageLookups GetNewLookups() => new EMCSOutboundEDIMessageLookups(this);

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();

			EM_MessageText = EM_MessageText.Replace(EDIMessage.MessageNumberPlaceHolderHtml, EM_MessageNum);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.GbCustomsEMCS;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}
	}
}
