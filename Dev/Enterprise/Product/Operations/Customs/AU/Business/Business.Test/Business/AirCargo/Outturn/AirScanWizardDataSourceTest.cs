using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirScanWizardDataSource))]
	sealed class AirScanWizardDataSourceTest : ScanWizardDataSourceTest
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			var mawb = Factory.New<CusMAWB>();
			var consol = Factory.New<ForwardingConsol>();
			mawb.CM_JK = consol.PK;
			return new AirScanWizardDataSource(new ScanCusMAWB(mawb));
		}
	}
}
