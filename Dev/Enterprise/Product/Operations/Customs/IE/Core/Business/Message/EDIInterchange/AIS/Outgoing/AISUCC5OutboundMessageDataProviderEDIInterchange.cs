using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public sealed class AISUCC5OutboundMessageDataProviderEDIInterchange : OutboundMessageDataProviderEDIInterchange
	{
		public AISUCC5OutboundMessageDataProviderEDIInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.IECustomsUCC5Import;
		}
	}
}
