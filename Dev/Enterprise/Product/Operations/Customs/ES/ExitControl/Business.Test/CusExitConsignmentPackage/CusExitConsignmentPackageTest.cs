using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentPackage))]
	sealed class CusExitConsignmentPackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCXP_Sequence_ReadOnly()
		{
			var consignmentPackage = (CusExitConsignmentPackage)GetNewBusinessObject();
			AssertEquals("CXP_Sequence ReadOnly", false, consignmentPackage.CXP_SequenceInfo.ReadOnly);
		}

		public void TestValidation()
		{
			var consignmentPackage = (CusExitConsignmentPackage)GetNewBusinessObject();
			AssertType<CusExitConsignmentPackageValidation>("Validation Type", consignmentPackage.Validation);
		}

		public void TestLookups()
		{
			var consignmentPackage = (CusExitConsignmentPackage)GetNewBusinessObject();
			AssertType<CusExitConsignmentPackageUcc6Lookups>("Lookups Type", consignmentPackage.Lookups);
		}

		public void TestCusExitReportItems()
		{
			var consignmentPackage = (CusExitConsignmentPackage)GetNewBusinessObject();
			AssertType<CusExitReportItemCollection<CusExitReportItem>>(consignmentPackage.CusExitReportItems);
		}

		public void TestStatusIsMissing()
		{
			CombineAssertions(() =>
			{
				var consignmentPackage = (CusExitConsignmentPackage)GetNewBusinessObject();
				AssertEquals("When CXP_MarksAndNumbersStatus is empty", false, consignmentPackage.StatusIsMissing);

				consignmentPackage.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				AssertEquals("When CXP_MarksAndNumbersStatus is MIS", true, consignmentPackage.StatusIsMissing);

				consignmentPackage.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				AssertEquals("When CXP_MarksAndNumbersStatus is DIF", false, consignmentPackage.StatusIsMissing);
			});
		}

		public void TestStatusIsDifferencesToDeclared()
		{
			CombineAssertions(() =>
			{
				var consignmentPackage = (CusExitConsignmentPackage)GetNewBusinessObject();
				AssertEquals("When CXP_MarksAndNumbersStatus is empty", false, consignmentPackage.StatusIsDifferencesToDeclared);

				consignmentPackage.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				AssertEquals("When CXP_MarksAndNumbersStatus is DIF", true, consignmentPackage.StatusIsDifferencesToDeclared);

				consignmentPackage.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				AssertEquals("When CXP_MarksAndNumbersStatus is MIS", false, consignmentPackage.StatusIsDifferencesToDeclared);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewConsignmentPackage();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewConsignmentPackage();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => Factory.NewWithValidTestData<CusExitConsignmentPackage>();

		CusExitConsignmentPackage GetNewConsignmentPackage()
		{
			var exitHeader = CusExitHeaderTest.GetNewBusinessObject(Factory);
			var exitConsignment = exitHeader.CusExitConsignments.AddNew();
			var exitConsignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem.CCI_LineNumber = 1;
			var exitConsignmentPackagePivots = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			return (CusExitConsignmentPackage)exitConsignmentPackagePivots.Package;
		}
	}
}
