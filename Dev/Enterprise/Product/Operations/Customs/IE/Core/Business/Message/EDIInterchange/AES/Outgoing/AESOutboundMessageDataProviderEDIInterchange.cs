using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public sealed class AESOutboundMessageDataProviderEDIInterchange : OutboundMessageDataProviderEDIInterchange
	{
		public AESOutboundMessageDataProviderEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.IECustomsExport;
		}
	}
}
