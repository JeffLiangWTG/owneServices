using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDISQueryInterchange : EDIInterchange
	{
		public CDSDISQueryInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.UniversalDataMessaging;
		}

		protected override bool ShouldSendViaEHubCore => true;
	}
}
