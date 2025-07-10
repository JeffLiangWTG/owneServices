using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeaderDocManagerInfo))]
	sealed class CusExitHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestCusExitHeaderDocManagerInfo()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			var manager = ((IDocManagerSupport)cusExitHeader).DocManagerInfo;
			CombineAssertions(() =>
			{
				AssertType(typeof(CusExitHeaderDocManagerInfo), manager);

				var cusExitConsignment = cusExitHeader.CusExitConsignments.AddNew();
				var cusExitReport = cusExitHeader.CusExitReports.AddNew();
				var testManager = new CusExitHeaderDocManagerInfoForTest(cusExitHeader);
				AssertCollectionContains(cusExitConsignment, testManager.GetRelatedObjects_Exposed());
				AssertCollectionContains(cusExitReport, testManager.GetRelatedObjects_Exposed());
			});
		}

		public void TestGetEDocsChildrenForAFreightJobToDisplay()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			var manager = ((IDocManagerSupport)cusExitHeader).DocManagerInfo;
			var cusExitConsignment = cusExitHeader.CusExitConsignments.AddNew();
			var cusExitReport = cusExitHeader.CusExitReports.AddNew();
			var testManager = new CusExitHeaderDocManagerInfoForTest(cusExitHeader);
			CombineAssertions(() =>
			{
				AssertCollectionContains(cusExitConsignment, testManager.GetEDocsChildrenForAFreightJobToDisplay_Exposed());
				AssertCollectionContains(cusExitReport, testManager.GetEDocsChildrenForAFreightJobToDisplay_Exposed());
			});
		}

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<CusExitHeader>();

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			cusExitHeader.CusExitConsignments.AddNew();
			cusExitHeader.CusExitReports.AddNew();
			return cusExitHeader;
		}

		class CusExitHeaderDocManagerInfoForTest : CusExitHeaderDocManagerInfo
		{
			public CusExitHeaderDocManagerInfoForTest(CusExitHeader cusExitHeader)
				: base(cusExitHeader)
			{ }

			public BusinessObject[] GetEDocsChildrenForAFreightJobToDisplay_Exposed() => base.GetEDocsChildrenForAFreightJobToDisplay();

			public BusinessObject[] GetRelatedObjects_Exposed() => base.GetRelatedObjects();
		}
	}
}
