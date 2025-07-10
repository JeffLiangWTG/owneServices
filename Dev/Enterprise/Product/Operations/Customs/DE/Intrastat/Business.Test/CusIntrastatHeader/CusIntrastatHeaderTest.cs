using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatHeader))]
	sealed class CusIntrastatHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => transaction;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => IntrastatTestDataHelper.New(factory).NewCusIntrastatHeaderWithValidData<CusIntrastatHeader>();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => transaction;

		protected override void SetUp()
		{
			transaction = (CusIntrastatHeader)IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData<CusIntrastatHeader>();
		}
		CusIntrastatHeader transaction;
	}
}
