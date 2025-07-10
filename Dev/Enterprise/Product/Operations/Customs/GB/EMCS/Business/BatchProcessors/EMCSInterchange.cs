using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSInterchange : EDIInterchange
	{
		public EMCSInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.GbCustomsEMCS;
		}

		public EMCSCustomsBusinessResponse EMCSCustomsBusinessResponse => new EMCSCustomsBusinessResponse(EI_BodyText);
	}
}
