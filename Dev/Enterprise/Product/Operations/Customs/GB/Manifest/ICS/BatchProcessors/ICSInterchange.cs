using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSInterchange : EDIInterchange
	{
		public ICSInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.GbCustomsDeclarationServices;
		}

		public ICSGBCustomsBusinessResponse GBCustomsBusinessResponse => new ICSGBCustomsBusinessResponse(EI_BodyText);

		protected override bool ShouldSendViaEHubCore => true;
	}
}
