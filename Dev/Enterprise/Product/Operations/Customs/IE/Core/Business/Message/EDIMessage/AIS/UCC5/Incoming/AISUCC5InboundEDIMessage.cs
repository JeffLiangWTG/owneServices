using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5InboundEDIMessage : InboundEDIMessage
	{
		public AISUCC5InboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new AISUCC5InboundEDIMessageLookups Lookups => (AISUCC5InboundEDIMessageLookups)base.Lookups;
		protected override Enterprise.Messaging.Business.EDIMessageLookups GetNewLookups() => new AISUCC5InboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsUCC5Import;
		}
	}
}
