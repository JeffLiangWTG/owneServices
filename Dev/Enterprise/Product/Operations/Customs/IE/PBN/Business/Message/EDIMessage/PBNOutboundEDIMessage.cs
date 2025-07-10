using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.PBN.Business
{
	public sealed class PBNOutboundEDIMessage : OutboundEDIMessage
	{
		public PBNOutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsPBN;
		}

		protected override TDataProvider GetDataProviderCore<TDataProvider>() => default;

		protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.IEMessageControlNumber(EDIMessage.ApplicationCodes.IECustomsPBN).GetNextFormatted(Factory) : (string)EM_MessageNum;
	}
}
