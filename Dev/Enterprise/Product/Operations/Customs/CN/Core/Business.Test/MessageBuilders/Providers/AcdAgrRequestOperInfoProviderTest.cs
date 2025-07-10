using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class AcdAgrRequestOperInfoProviderTest : TestCaseWithFactory
	{
		public void TestDeclarantCustomsCode()
		{
			var testData = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var proxy = Factory.New<OrgHeader>();
			var ccdCode = proxy.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = "CCD";
			ccdCode.OK_CustomsRegNo = "CCD10";
			testData.JobDeclaration.Branch.GB_OH_OrgProxy = proxy.PK;
			var operInfo = new AcdAgrRequestOperInfoProvider(testData.EntryHeader);
			AssertEquals("Should be DeclarantCCD of EntryHeader", "CCD10", operInfo.DeclarantCustomsCode);
		}

		public void TestSign()
		{
			AssertEquals("Should be empty", string.Empty, OperInfo.Sign);
		}

		public void TestOperationType()
		{
			AssertEquals("Should be 1", "1", OperInfo.OperationType);
		}

		AcdAgrRequestOperInfoProvider OperInfo => operInfo ?? (operInfo = new AcdAgrRequestOperInfoProvider(Factory.New<CusEntryHeader>()));
		AcdAgrRequestOperInfoProvider operInfo;
	}
}
