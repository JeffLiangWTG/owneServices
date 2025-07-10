using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentPivot))]
	sealed class CusExitConsignmentPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusExitConsignmentPivotValidation>(GetNewBusinessObject(Factory).Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		CusExitConsignmentPivot GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<CusExitHeader>();
			var package = header.CusExitConsignmentPackages.AddNew();
			package.CXP_Sequence = 1;
			var consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			var pivot = consignmentItem.CusExitConsignmentPivots.AddNew();
			pivot.CNP_CXP_Package = package.PK;
			return pivot;
		}
	}
}
