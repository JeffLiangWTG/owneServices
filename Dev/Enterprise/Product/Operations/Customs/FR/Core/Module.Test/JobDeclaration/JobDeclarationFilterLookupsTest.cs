using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestDeltaModeList()
		{
			AssertEquals("G1, G2", lookups.DeltaModeList.CodesAsString);
		}

		public void TestApplicationCodeList()
		{
			AssertEquals(Factory.GetCachedValue<DeclarationApplicationCodeList>(), lookups.ApplicationCodeList());
		}

		public void TestGetDataGroupingCodesForSupportingDocumentList()
		{
			CombineAssertions(() =>
			{
				var expectedDataGroupingCodes = new ZString[] { "FR", "DIE" };
				AssertGetDataGroupingCodesForSupportingDocumentList(new ZString[] { "FR" }, false, false);
				AssertGetDataGroupingCodesForSupportingDocumentList(expectedDataGroupingCodes, true, false);
				AssertGetDataGroupingCodesForSupportingDocumentList(expectedDataGroupingCodes, true, true);
				AssertGetDataGroupingCodesForSupportingDocumentList(expectedDataGroupingCodes, false, true);
			});
		}

		void AssertGetDataGroupingCodesForSupportingDocumentList(ZString[] expectedDataGroupingCodes, bool enableDeltaIEForImports, bool enableDeltaIEForExports)
		{
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, enableDeltaIEForImports))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, enableDeltaIEForExports))
			{
				var result = (ZString[])lookups.GetType().GetProperty("GetDataGroupingCodesForSupportingDocumentList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(lookups);
				AssertEquals("Length", expectedDataGroupingCodes.Length, result.Length);
				AssertContainsExactElementsInAnyOrder("Values", expectedDataGroupingCodes, result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
			lookups = filterBizObj.Lookups;
		}
		JobDeclarationFilterBusinessObject filterBizObj;
		JobDeclarationFilterLookups lookups;
	}
}
