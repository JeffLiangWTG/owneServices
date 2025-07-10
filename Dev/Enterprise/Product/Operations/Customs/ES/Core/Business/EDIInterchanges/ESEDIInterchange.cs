using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.EDIInterchanges
{
	public class ESEDIInterchange : EDIInterchange, Integration.Customs.ES.IEDIInterchange
	{
		public ESEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EI_ApplicationCode = ApplicationCodes.ESCustomsMessage;
		}

		protected override bool ShouldSendViaEHubCore => EI_TransportType != EDIInterchange.TransportType.xT;
	}
}
