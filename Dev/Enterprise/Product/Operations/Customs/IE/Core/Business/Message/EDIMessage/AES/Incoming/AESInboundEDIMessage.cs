using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class AESInboundEDIMessage : InboundEDIMessage
	{
		public AESInboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new AESInboundEDIMessageLookups Lookups => (AESInboundEDIMessageLookups)base.Lookups;
		protected override Enterprise.Messaging.Business.EDIMessageLookups GetNewLookups() => new AESInboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
		}
	}
}
