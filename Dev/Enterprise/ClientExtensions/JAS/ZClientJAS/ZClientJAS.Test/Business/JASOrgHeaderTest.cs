using CargoWise.Types;
using Enterprise.Client.JAS.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(JASOrgHeader))]
	internal class JASOrgHeaderTest : OrgHeaderTest
	{
		[ExpectNoExceptions]
		public void TestCompanyDataHeaderShouldNotBeNullWhenItBelongsToUncommittedClientOrgClass()
		{
			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory);
			JASOrgHeader uncommittedOrg = (JASOrgHeader)((System.ComponentModel.IBindingList)orgCollection).AddNew();
			AssertEquals("Header should return JasOrg", uncommittedOrg, uncommittedOrg.CompanyData.Header);
			uncommittedOrg.OH_RL_NKClosestPort = "AUBNE";
		}

		public void TestFindOrgHeaderByOfficeAndNettingCode()
		{
			// to make the table clean
			Factory.Save();
			JASOrgHeader result = JASOrgHeader.FindOrgHeaderByOfficeAndNettingCode(Factory, "AUADL", "AUCOR");
			AssertNull("Pre-condition, should not be found", result);
			JASOrgHeader testOrg = Factory.NewWithValidTestData<JASOrgHeader>();
			testOrg.NettingCode = "AUCOR";
			Factory.Save();
			result = JASOrgHeader.FindOrgHeaderByOfficeAndNettingCode(Factory, "AUADL", "AUCOR");
			AssertNull("TestOrg does not have office code", result);
			testOrg.OfficeCode = "AUADL";
			Factory.Save();
			result = JASOrgHeader.FindOrgHeaderByOfficeAndNettingCode(Factory, "AUADL", "AUCOR");
			AssertEquals(testOrg.PK, result.PK);
			testOrg.NettingCode = "ITMIL";
			Factory.Save();
			result = JASOrgHeader.FindOrgHeaderByOfficeAndNettingCode(Factory, "AUADL", "AUCOR");
			AssertNull("TestOrg has different netting code", result);
		}

		public void TestFindOrgHeaderByOfficeCode()
		{
			// to make the table clean
			Factory.Save();
			JASOrgHeader result = JASOrgHeader.FindOrgHeaderByOfficeCode(Factory, "AUADL");
			AssertNull("Pre-condition, should not be found", result);
			JASOrgHeader testOrg = Factory.NewWithValidTestData<JASOrgHeader>();
			testOrg.OfficeCode = "AUADL";
			Factory.Save();
			result = JASOrgHeader.FindOrgHeaderByOfficeCode(Factory, "AUADL");
			AssertEquals(testOrg.PK, result.PK);
		}

		public void TestOfficeCode()
		{
			JASOrgHeader testOrg = Factory.New<JASOrgHeader>();
			AssertEquals("Pre-condition", "", testOrg.OfficeCode);
			OrgCusCode officeCodeObj = testOrg.CustomsCodes.AddNew();
			officeCodeObj.OK_CodeType = OrgCusCode.CodeTypes.UniversalOfficeCode;
			officeCodeObj.OK_CustomsRegNo = "AUPER";
			AssertEquals("AUPER", testOrg.OfficeCode);
			testOrg.OfficeCode = "AUADL";
			AssertEquals("AUADL", testOrg.OfficeCode);
			AssertEquals("If OrgCusCode already exists, should not be creating a new one", officeCodeObj.PK, testOrg.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.UniversalOfficeCode, (RefCountry)null).PK);
			testOrg.CustomsCodes.RemoveAll();
			AssertEquals("", testOrg.OfficeCode);
			testOrg.OfficeCode = "AUBNE";
			AssertEquals(1, testOrg.CustomsCodes.Count);
			AssertEquals("AUBNE", testOrg.OfficeCode);
		}

		public void TestNettingCode()
		{
			JASOrgHeader testOrg = Factory.New<JASOrgHeader>();
			AssertEquals("Pre-condition", "", testOrg.NettingCode);
			OrgCusCode nettingCodeObj = testOrg.CustomsCodes.AddNew();
			nettingCodeObj.OK_CodeType = OrgCusCode.CodeTypes.UniversalNettingCode;
			nettingCodeObj.OK_CustomsRegNo = "AUCOR";
			AssertEquals("AUCOR", testOrg.NettingCode);
			testOrg.NettingCode = "USCOR";
			AssertEquals("USCOR", testOrg.NettingCode);
			AssertEquals("If OrgCusCode already exists, should not be creating a new one", nettingCodeObj.PK, testOrg.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.UniversalNettingCode, (RefCountry)null).PK);
			testOrg.CustomsCodes.RemoveAll();
			AssertEquals("", testOrg.NettingCode);
			testOrg.NettingCode = "ITMIL";
			AssertEquals(1, testOrg.CustomsCodes.Count);
			AssertEquals("ITMIL", testOrg.NettingCode);
		}

		public void TestJASWWMappedCode()
		{
			try
			{
				JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
				JASOrgHeader testOrg = Factory.New<JASOrgHeader>();
				AssertEquals("", testOrg.JASWWMappedCode);
				OrgPatternMatchOverride override1 = testOrg.CreatePatternMatchOverrideForTest();
				override1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
				override1.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
				override1.OO_ForeignCode = "ABC";
				OrgPatternMatchOverride override2 = testOrg.CreatePatternMatchOverrideForTest();
				override2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
				override2.OO_LocalGuid = ZGuid.NewZGuid();
				override2.OO_ForeignCode = "123";
				OrgPatternMatchOverride override3 = testOrg.CreatePatternMatchOverrideForTest();
				override3.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
				override3.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
				override3.OO_ForeignCode = "XYZ";
				AssertEquals("XYZ", testOrg.JASWWMappedCode);
			}
			finally
			{
				JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
			}
		}

		public void TestFindOrgHeaderByJASWWMappedCode()
		{
			try
			{
				JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
				JASOrgHeader testOrg1 = CreateNewOrganisationWithJASWWMappedCode("ORG1", "ABC");
				JASOrgHeader testOrg2 = CreateNewOrganisationWithJASWWMappedCode("ORG2", "BCD");
				JASOrgHeader testOrg3 = CreateNewOrganisationWithJASWWMappedCode("ORG3", "123");
				AssertEquals("Should find a match", "ORG1", JASOrgHeader.FindOrgHeaderByJASWWMappedCode(Factory, "ABC").OH_Code);
				AssertEquals("Should find a match", "ORG2", JASOrgHeader.FindOrgHeaderByJASWWMappedCode(Factory, "BCD").OH_Code);
				AssertEquals("Should find a match", "ORG3", JASOrgHeader.FindOrgHeaderByJASWWMappedCode(Factory, "123").OH_Code);
				AssertNull("Should not find a match", JASOrgHeader.FindOrgHeaderByJASWWMappedCode(Factory, "895"));
			}
			finally
			{
				JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
			}
		}

		public void TestIsJASOffice()
		{
			JASOrgHeader orgHeader = Factory.New<JASOrgHeader>();
			Assert("Both Office and Netting Code have to be specified", !orgHeader.IsJASOffice);
			orgHeader.NettingCode = "ABC";
			Assert("Both Office and Netting Code have to be specified", !orgHeader.IsJASOffice);
			orgHeader.NettingCode = "";
			orgHeader.OfficeCode = "ORG1";
			Assert("Both Office and Netting Code have to be specified", !orgHeader.IsJASOffice);
			orgHeader.NettingCode = "ABC";
			Assert("Both Office and Netting Code have to be specified", orgHeader.IsJASOffice);
		}

		#region Implementation
		JASOrgHeader CreateNewOrganisationWithJASWWMappedCode(ZString oH_Code, ZString jASWWMappedCode)
		{
			JASOrgHeader result = Factory.New<JASOrgHeader>();
			result.OH_Code = oH_Code;
			OrgPatternMatchOverride @override = result.CreatePatternMatchOverrideForTest();
			@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			@override.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
			@override.OO_ForeignCode = jASWWMappedCode;
			return result;
		}
		#endregion
	}
}
