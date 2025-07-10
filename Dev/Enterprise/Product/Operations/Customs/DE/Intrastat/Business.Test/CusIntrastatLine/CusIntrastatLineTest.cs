using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatLine))]
	sealed class CusIntrastatLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => transactionLine;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => IntrastatTestDataHelper.New(factory).NewCusIntrastatLineWithValidData();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => transactionLine;

		protected override void SetUp()
		{
			var helper = IntrastatTestDataHelper.New(Factory);
			transaction = (CusIntrastatHeader)helper.NewCusIntrastatHeaderWithValidData<CusIntrastatHeader>();
			transactionLine = (CusIntrastatLine)helper.NewCusIntrastatLineWithValidData(transaction);
		}
		CusIntrastatHeader transaction;
		CusIntrastatLine transactionLine;
	}
}
