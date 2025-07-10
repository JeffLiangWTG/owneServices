using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.CN;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EnterpriseQualification))]
	class EnterpriseQualificationTest : Customs.Business.Testing.CusCodeDataTest<EnterpriseQualification>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((EnterpriseQualification)BusinessObject).SupportsNotes);
		}

		public void TestDefaultValues()
		{
			var epq = (EnterpriseQualification)GetNewBusinessObject();
			AssertEquals(Constants.CusCodeDataTypes.Codes.EnterpriseQualification, epq.CY_Type);
		}

		public void TestCY_CodeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var epq = declaration.CustomsEntryInstructions.AddNew().EnterpriseQualifications.AddNew();
			AssertEquals("", epq.Description);
			epq.CY_Code = EnterpriseQualificationList.Codes._200;
			AssertEquals("卫生司类", epq.Description);
			epq.CY_Code = "999";
			AssertEquals("", epq.Description);
		}

		public void TestCY_Code()
		{
			var epq = (EnterpriseQualification)GetNewBusinessObject();
			AssertEquals(3, epq.CY_CodeInfo.MaxLength);
		}

		public void TestCY_Data()
		{
			var epq = (EnterpriseQualification)GetNewBusinessObject();
			AssertEquals(40, epq.CY_DataInfo.MaxLength);
		}

		public void TestValidation()
		{
			var epq = (EnterpriseQualification)GetNewBusinessObject();
			AssertEquals(typeof(EnterpriseQualificationValidation), epq.Validation.GetType());
		}

		public void TestLookups()
		{
			var epq = (EnterpriseQualification)GetNewBusinessObject();
			AssertEquals(typeof(EnterpriseQualificationLookups), epq.Lookups.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration.CustomsEntryInstructions.AddNew().EnterpriseQualifications.AddNew();
		}

		public void TestAutoFillTypeWhenSelectNumber()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "A";
			orgHeader1.CustomsCodes.AddNew("CCD", "1234567890", "CN");
			orgHeader1.CustomsCodes.AddNew("101", "A0001", "CN");
			orgHeader1.CustomsCodes.AddNew("303", "A0002", "CN");
			orgHeader1.CustomsCodes.AddNew("306", "A0003", "CN");
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "B";
			orgHeader2.CustomsCodes.AddNew("USC", "123456789012345678", "CN");
			orgHeader2.CustomsCodes.AddNew("101", "B0001", "CN");
			orgHeader2.CustomsCodes.AddNew("303", "B0002", "CN");
			orgHeader2.CustomsCodes.AddNew("306", "B0003", "CN");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = orgHeader1.PK;
			declaration.JE_OH_Buyer = orgHeader2.PK;
			var epq = declaration.CustomsEntryInstructions.AddNew().EnterpriseQualifications.AddNew();
			AssertEquals("new Enterprise Qualifications", "", epq.CY_Code);
			AssertEquals("new Enterprise Qualifications", "", epq.CY_Data);
			epq.CY_Data = "A0001";
			AssertEquals("After change to A0001", "101", epq.CY_Code);
			epq.CY_Data = "A0003";
			AssertEquals("After change to A0003, code should not change", "101", epq.CY_Code);
			var epq2 = declaration.CustomsEntryInstructions.AddNew().EnterpriseQualifications.AddNew();
			epq2.CY_Data = "B0006"; //invalid input
			AssertEquals("After change to B0006", "", epq2.CY_Code);
		}

		public void TestToString()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var epq = declaration.CustomsEntryInstructions.AddNew().EnterpriseQualifications.AddNew();
			epq.CY_Code = EnterpriseQualificationList.Codes._200;
			epq.CY_Data = "A0001";
			AssertEquals("200:A0001", epq.ToString());
		}
	}
}
