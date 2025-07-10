using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSSplitCusTempStorageLineCollection))]
	class CUSPCSSplitCusTempStorageLineCollectionTest : EU.Business.CusTempStorage.Testing.CusTempStorageLineCollectionToTest
	{
		public void TestSetDefaults()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);

			var custodianOrg = Factory.New<OrgHeader>();
			var custodianAddress1 = custodianOrg.Addresses.AddNew();

			custodianOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "Z123456789", Core.Constants.CountryCodes.Greece);
			var branchCode1 = custodianOrg.CustomsCodes.AddNew();
			branchCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			branchCode1.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			branchCode1.OK_CustomsRegNo = "0001";
			branchCode1.OK_OA_PremisesAddress = custodianAddress1.PK;

			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageDec.ConsolidatedCusTempStorageLine.TSL_OA_Custodian = custodianAddress1.PK;

			var storageLine = storageDec.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.AddNew();
			AssertEquals(custodianAddress1.PK, storageLine.TSL_OA_Custodian);
			AssertEquals(custodianAddress1, storageLine.Custodian);
			AssertEquals("GRZ123456789", storageLine.TSL_CustodianIdentifier);
			AssertEquals("0001", storageLine.TSL_CustodianIdentifierBranchNo);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CUSPCSSplitCusTempStorageLine>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			return storageDec.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo;
		}
	}
}
