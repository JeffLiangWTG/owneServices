using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5OutboundEDIMessage : AISCommonOutboundEDIMessage
	{
		public AISUCC5OutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new AISUCC5OutboundEDIMessageLookups Lookups => (AISUCC5OutboundEDIMessageLookups)base.Lookups;
		protected override Enterprise.Messaging.Business.EDIMessageLookups GetNewLookups() => new AISUCC5OutboundEDIMessageLookups(this);

		protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.IEMessageControlNumber(EDIMessage.ApplicationCodes.IECustomsUCC5Import).GetNextFormatted(Factory) : (string)EM_MessageNum;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsUCC5Import;
		}
	}
}
