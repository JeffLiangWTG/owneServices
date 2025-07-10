using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using FilterConstants = Enterprise.Customs.BR.Business.Constants.FilterConstants;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(ForeignOperatorFilterBusinessObject))]
	class ForeignOperatorFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterForMessageType()
		{
			var foreignOperator1 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator1.BFR_AuthorityVersion = string.Empty;
			foreignOperator1.BFR_AuthorityIdentifier = string.Empty;
			var foreignOperator2 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator2.BFR_AuthorityVersion = string.Empty;
			foreignOperator2.BFR_AuthorityIdentifier = string.Empty;

			Factory.Save();

			Assert(foreignOperator1.MatchesFilter(filterBO.Filter));
			Assert(foreignOperator2.MatchesFilter(filterBO.Filter));
		}

		public void TestMessageStatus()
		{
			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_AuthorityVersion = "1";
			foreignOperator.BFR_AuthorityIdentifier = "A123";
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.NotSent;

			var foreignOperator2 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_AuthorityIdentifier = "B345";
			foreignOperator2.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;

			var foreignOperator3 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_AuthorityIdentifier = "C567";
			foreignOperator3.BFR_MessageStatus = BRMessageStatusList.Codes.Failed;

			var foreignOperator4 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator4.BFR_AuthorityVersion = "4";
			foreignOperator4.BFR_AuthorityIdentifier = "D789";
			foreignOperator4.BFR_MessageStatus = BRMessageStatusList.Codes.Rejected;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, BRMessageStatusList.Codes.AwaitingResponse, new[] { foreignOperator2 }, FilterConstants.ForeignOperator.MessageStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, BRMessageStatusList.Codes.Failed, new[] { foreignOperator3 }, FilterConstants.ForeignOperator.MessageStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, BRMessageStatusList.Codes.Rejected, new[] { foreignOperator4 }, FilterConstants.ForeignOperator.MessageStatus);
			});
		}

		public void TestCustomsStatus()
		{
			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_AuthorityVersion = "1";
			foreignOperator.BFR_AuthorityIdentifier = "A123";
			foreignOperator.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Active;

			var foreignOperator2 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_AuthorityIdentifier = "B345";
			foreignOperator2.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Inactive;

			var foreignOperator3 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_AuthorityIdentifier = "C567";
			foreignOperator3.BFR_CustomsStatus = string.Empty;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, ForeignOperatorCustomsStatusTypeList.Codes.Active, new[] { foreignOperator }, FilterConstants.ForeignOperator.Status);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, ForeignOperatorCustomsStatusTypeList.Codes.Inactive, new[] { foreignOperator2 }, FilterConstants.ForeignOperator.Status);
			});
		}

		public void TestAuthorityIdentifier()
		{
			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_AuthorityVersion = "1";
			foreignOperator.BFR_AuthorityIdentifier = "A123";

			var foreignOperator2 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_AuthorityIdentifier = "B234";

			var foreignOperator3 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_AuthorityIdentifier = "C345";

			var foreignOperator4 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator4.BFR_AuthorityVersion = string.Empty;
			foreignOperator4.BFR_AuthorityIdentifier = string.Empty;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, "A123", new[] { foreignOperator }, FilterConstants.ForeignOperator.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Contains, "2", new[] { foreignOperator, foreignOperator2 }, FilterConstants.ForeignOperator.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.StartsWith, "C", new[] { foreignOperator3 }, FilterConstants.ForeignOperator.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.DoesNotStartWith, "A", new[] { foreignOperator2, foreignOperator3, foreignOperator4 }, FilterConstants.ForeignOperator.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.NotContains, "2", new[] { foreignOperator3, foreignOperator4 }, FilterConstants.ForeignOperator.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsNotBlank, "", new[] { foreignOperator, foreignOperator2, foreignOperator3 }, FilterConstants.ForeignOperator.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsBlank, "", new[] { foreignOperator4 }, FilterConstants.ForeignOperator.AuthorityIdentifier);
			});
		}

		public void TestOwner()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.OH_Code = "TEST1";
			var ownerAddress1 = owner.Addresses.AddNew();
			ownerAddress1.OA_Address1 = "TEST1";

			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "TEST2";
			var ownerAddress2 = owner2.Addresses.AddNew();
			ownerAddress2.OA_Address1 = "TEST2";

			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_AuthorityVersion = "1";
			foreignOperator.BFR_AuthorityIdentifier = "A123";
			foreignOperator.BFR_OH_Owner = owner.PK;

			var foreignOperator2 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_AuthorityIdentifier = "B456";
			foreignOperator2.BFR_OH_Owner = owner2.PK;

			var foreignOperator3 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_AuthorityIdentifier = "C789";
			foreignOperator3.BFR_OH_Owner = owner2.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleGuidFilterResult<CusBRForeignOperator>(filterBO, owner.PK, new[] { foreignOperator }, FilterConstants.ForeignOperator.Owner);
				ModuleTestHelper.AssertModuleGuidFilterResult<CusBRForeignOperator>(filterBO, owner2.PK, new[] { foreignOperator2, foreignOperator3 }, FilterConstants.ForeignOperator.Owner);
			});
		}

		public void TestForeignOperator()
		{
			var foreignOp = Factory.NewWithValidTestData<OrgHeader>();
			foreignOp.OH_Code = "foreignOp";
			var foreignOpAddress1 = foreignOp.Addresses.AddNew();
			foreignOpAddress1.OA_Address1 = "Address foreignOp";

			var foreignOp2 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOp2.OH_Code = "foreignOp 2";
			var foreignOpAddress2 = foreignOp2.Addresses.AddNew();
			foreignOpAddress2.OA_Address1 = "Address foreignOp 2";

			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_AuthorityVersion = "1";
			foreignOperator.BFR_OH_ForeignOperator = foreignOp.PK;

			var foreignOperator2 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_OH_ForeignOperator = foreignOp2.PK;

			var foreignOperator3 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_OH_ForeignOperator = foreignOp2.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleGuidFilterResult<CusBRForeignOperator>(filterBO, foreignOp.PK, new[] { foreignOperator }, ForeignOperatorFilterBusinessObject.Schema.ForeignOperator);
				ModuleTestHelper.AssertModuleGuidFilterResult<CusBRForeignOperator>(filterBO, foreignOp2.PK, new[] { foreignOperator2, foreignOperator3 }, ForeignOperatorFilterBusinessObject.Schema.ForeignOperator);
			});
		}

		public void TestCountryFilter()
		{
			var foreignOp1 = Factory.New<OrgHeader>();
			foreignOp1.OH_Code = "foreignOp";
			var mainAddress1 = foreignOp1.MainAddress;
			mainAddress1.OA_Address1 = "Address foreignOp";
			mainAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var secondaryAddress1 = foreignOp1.Addresses.AddNew();
			secondaryAddress1.OA_Address1 = " SecAddress foreignOp";
			secondaryAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			var foreignOp2 = Factory.New<OrgHeader>();
			foreignOp2.OH_Code = "foreignOp 2";
			var mainAddress2 = foreignOp2.MainAddress;
			mainAddress2.OA_Address1 = "Address foreignOp 2";
			mainAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var secondaryAddress2 = foreignOp2.Addresses.AddNew();
			secondaryAddress2.OA_Address1 = " SecAddress foreignOp 2";
			secondaryAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.WallisAndFutunaIslands;

			var foreignOp3 = Factory.New<OrgHeader>();
			foreignOp3.OH_Code = "foreignOp 3";
			var mainAddress3 = foreignOp3.MainAddress;
			mainAddress3.OA_Address1 = "Address foreignOp 3";
			mainAddress3.OA_RN_NKCountryCode = string.Empty;
			var secondaryAddress3 = foreignOp3.Addresses.AddNew();
			secondaryAddress3.OA_Address1 = " SecAddress foreignOp 3";
			secondaryAddress3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var foreignOperator1 = Factory.New<CusBRForeignOperator>();
			foreignOperator1.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator1.BFR_AuthorityIdentifier = "1";
			foreignOperator1.BFR_AuthorityVersion = "1";
			foreignOperator1.BFR_OH_ForeignOperator = foreignOp1.PK;

			var foreignOperator2 = Factory.New<CusBRForeignOperator>();
			foreignOperator2.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator2.BFR_AuthorityIdentifier = "2";
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_OH_ForeignOperator = foreignOp2.PK;

			var foreignOperator3 = Factory.New<CusBRForeignOperator>();
			foreignOperator3.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator3.BFR_AuthorityIdentifier = "3";
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_OH_ForeignOperator = foreignOp3.PK;

			var foreignOperator4 = Factory.New<CusBRForeignOperator>();
			foreignOperator4.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator4.BFR_AuthorityIdentifier = "4";
			foreignOperator4.BFR_AuthorityVersion = "4";
			foreignOperator4.BFR_Name = "FO name 4";
			foreignOperator4.BFR_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			foreignOperator4.BFR_City = "City 4";

			var foreignOperator5 = Factory.New<CusBRForeignOperator>();
			foreignOperator5.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator5.BFR_AuthorityIdentifier = "5";
			foreignOperator5.BFR_AuthorityVersion = "5";
			foreignOperator5.BFR_Name = "PR name 5";
			foreignOperator5.BFR_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			foreignOperator5.BFR_City = "City 5";

			var foreignOperator6 = Factory.New<CusBRForeignOperator>();
			foreignOperator6.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator6.BFR_AuthorityIdentifier = "6";
			foreignOperator6.BFR_AuthorityVersion = "6";
			foreignOperator6.BFR_Name = "PR name 6";
			foreignOperator6.BFR_RN_NKCountryCode = string.Empty;
			foreignOperator6.BFR_City = "City 6";

			var foreignOperator7 = Factory.New<CusBRForeignOperator>();
			foreignOperator7.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator7.BFR_AuthorityIdentifier = "7";
			foreignOperator7.BFR_AuthorityVersion = "7";
			foreignOperator7.BFR_Name = "PR name 7";
			foreignOperator7.BFR_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			foreignOperator7.BFR_City = "City 7";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleNkFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.UnitedStates, new[] { foreignOperator1, foreignOperator7 }, ForeignOperatorFilterBusinessObject.Schema.Country);
				ModuleTestHelper.AssertModuleNkFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.UnitedStates, new[] { foreignOperator2, foreignOperator3, foreignOperator4, foreignOperator5, foreignOperator6 }, ForeignOperatorFilterBusinessObject.Schema.Country);
				ModuleTestHelper.AssertModuleNkFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.Brazil, new[] { foreignOperator2, foreignOperator5 }, ForeignOperatorFilterBusinessObject.Schema.Country);
				ModuleTestHelper.AssertModuleNkFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsBlank, "", new[] { foreignOperator3, foreignOperator6 }, ForeignOperatorFilterBusinessObject.Schema.Country);
				ModuleTestHelper.AssertModuleNkFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsNotBlank, "", new[] { foreignOperator1, foreignOperator2, foreignOperator4, foreignOperator5, foreignOperator7 }, ForeignOperatorFilterBusinessObject.Schema.Country);
			});
		}

		public void TestNameFilter()
		{
			var foreignOp1 = Factory.New<OrgHeader>();
			foreignOp1.OH_Code = "foreignOp";
			foreignOp1.OH_FullName = "A123";

			var foreignOp2 = Factory.New<OrgHeader>();
			foreignOp2.OH_Code = "foreignOp 2";
			foreignOp2.OH_FullName = "B234";

			var foreignOp3 = Factory.New<OrgHeader>();
			foreignOp3.OH_Code = "foreignOp 3";
			foreignOp3.OH_FullName = "C345";

			var foreignOp4 = Factory.New<OrgHeader>();
			foreignOp4.OH_Code = "foreignOp 4";
			foreignOp4.OH_FullName = string.Empty;

			var foreignOperator1 = Factory.New<CusBRForeignOperator>();
			foreignOperator1.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator1.BFR_AuthorityIdentifier = "1";
			foreignOperator1.BFR_AuthorityVersion = "1";
			foreignOperator1.BFR_OH_ForeignOperator = foreignOp1.PK;

			var foreignOperator2 = Factory.New<CusBRForeignOperator>();
			foreignOperator2.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator2.BFR_AuthorityIdentifier = "2";
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_OH_ForeignOperator = foreignOp2.PK;

			var foreignOperator3 = Factory.New<CusBRForeignOperator>();
			foreignOperator3.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator3.BFR_AuthorityIdentifier = "3";
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_OH_ForeignOperator = foreignOp3.PK;

			var foreignOperator4 = Factory.New<CusBRForeignOperator>();
			foreignOperator4.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator4.BFR_AuthorityIdentifier = "4";
			foreignOperator4.BFR_AuthorityVersion = "4";
			foreignOperator4.BFR_OH_ForeignOperator = foreignOp4.PK;

			var foreignOperator5 = Factory.New<CusBRForeignOperator>();
			foreignOperator5.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator5.BFR_AuthorityIdentifier = "5";
			foreignOperator5.BFR_AuthorityVersion = "5";
			foreignOperator5.BFR_Name = "FO name 5";

			var foreignOperator6 = Factory.New<CusBRForeignOperator>();
			foreignOperator6.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator6.BFR_AuthorityIdentifier = "6";
			foreignOperator6.BFR_AuthorityVersion = "6";
			foreignOperator6.BFR_Name = "FO name 2";

			var foreignOperator7 = Factory.New<CusBRForeignOperator>();
			foreignOperator7.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator7.BFR_AuthorityIdentifier = "7";
			foreignOperator7.BFR_AuthorityVersion = "7";
			foreignOperator7.BFR_Name = "PR name 7";

			var foreignOperator8 = Factory.New<CusBRForeignOperator>();
			foreignOperator8.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator8.BFR_AuthorityIdentifier = "8";
			foreignOperator8.BFR_AuthorityVersion = "8";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, "A123", new[] { foreignOperator1 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Contains, "2", new[] { foreignOperator1, foreignOperator2, foreignOperator6 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.StartsWith, "C", new[] { foreignOperator3 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.DoesNotStartWith, "A", new[] { foreignOperator2, foreignOperator3, foreignOperator4, foreignOperator5, foreignOperator6, foreignOperator7, foreignOperator8 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.NotContains, "2", new[] { foreignOperator3, foreignOperator4, foreignOperator5, foreignOperator7, foreignOperator8 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsNotBlank, "", new[] { foreignOperator1, foreignOperator2, foreignOperator3, foreignOperator5, foreignOperator6, foreignOperator7 }, ForeignOperatorFilterBusinessObject.Schema.Name);

				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, "FO name 5", new[] { foreignOperator5 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Contains, "name", new[] { foreignOperator5, foreignOperator6, foreignOperator7 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.StartsWith, "PR", new[] { foreignOperator7 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.DoesNotStartWith, "PR", new[] { foreignOperator1, foreignOperator2, foreignOperator3, foreignOperator4, foreignOperator5, foreignOperator6, foreignOperator8 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.NotContains, "4", new[] { foreignOperator1, foreignOperator4, foreignOperator5, foreignOperator6, foreignOperator7, foreignOperator8 }, ForeignOperatorFilterBusinessObject.Schema.Name);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsBlank, "", new[] { foreignOperator4, foreignOperator8 }, ForeignOperatorFilterBusinessObject.Schema.Name);
			});
		}

		public void TestTINFilter()
		{
			var foreignOp1 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOp1.OH_Code = "foreignOp";
			var foreignOpCusCode1 = foreignOp1.CustomsCodes.AddNew();
			foreignOpCusCode1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.TIN;
			foreignOpCusCode1.OK_CustomsRegNo = "A123";

			var foreignOp2 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOp2.OH_Code = "foreignOp 2";
			var foreignOpCusCode2 = foreignOp2.CustomsCodes.AddNew();
			foreignOpCusCode2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			foreignOpCusCode2.OK_CustomsRegNo = "A123";

			var foreignOpCusCode3 = foreignOp2.CustomsCodes.AddNew();
			foreignOpCusCode3.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.TIN;
			foreignOpCusCode3.OK_CustomsRegNo = "B234";

			var foreignOp3 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOp3.OH_Code = "foreignOp 3";
			var foreignOpCusCode4 = foreignOp3.CustomsCodes.AddNew();
			foreignOpCusCode4.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.TIN;
			foreignOpCusCode4.OK_CustomsRegNo = "C345";

			var foreignOp4 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOp4.OH_Code = "foreignOp 4";

			var foreignOp5 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOp5.OH_Code = "foreignOp 5";
			var foreignOpCusCode5 = foreignOp3.CustomsCodes.AddNew();
			foreignOpCusCode5.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.RSN;
			foreignOpCusCode5.OK_CustomsRegNo = "C345";

			var foreignOperator1 = Factory.New<CusBRForeignOperator>();
			foreignOperator1.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator1.BFR_AuthorityIdentifier = "1";
			foreignOperator1.BFR_AuthorityVersion = "1";
			foreignOperator1.BFR_OH_ForeignOperator = foreignOp1.PK;

			var foreignOperator2 = Factory.New<CusBRForeignOperator>();
			foreignOperator2.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator2.BFR_AuthorityIdentifier = "2";
			foreignOperator2.BFR_AuthorityVersion = "2";
			foreignOperator2.BFR_OH_ForeignOperator = foreignOp2.PK;

			var foreignOperator3 = Factory.New<CusBRForeignOperator>();
			foreignOperator3.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator3.BFR_AuthorityIdentifier = "3";
			foreignOperator3.BFR_AuthorityVersion = "3";
			foreignOperator3.BFR_OH_ForeignOperator = foreignOp3.PK;

			var foreignOperator4 = Factory.New<CusBRForeignOperator>();
			foreignOperator4.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator4.BFR_AuthorityIdentifier = "4";
			foreignOperator4.BFR_AuthorityVersion = "4";

			var foreignOperator5 = Factory.New<CusBRForeignOperator>();
			foreignOperator5.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator5.BFR_AuthorityIdentifier = "5";
			foreignOperator5.BFR_AuthorityVersion = "5";
			foreignOperator5.BFR_OH_ForeignOperator = foreignOp4.PK;

			var foreignOperator6 = Factory.New<CusBRForeignOperator>();
			foreignOperator6.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator6.BFR_AuthorityIdentifier = "6";
			foreignOperator6.BFR_AuthorityVersion = "6";
			foreignOperator6.BFR_OH_ForeignOperator = foreignOp5.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Equal, "A123", new[] { foreignOperator1 }, ForeignOperatorFilterBusinessObject.Schema.Tin);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.Contains, "2", new[] { foreignOperator1, foreignOperator2 }, ForeignOperatorFilterBusinessObject.Schema.Tin);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.StartsWith, "C", new[] { foreignOperator3 }, ForeignOperatorFilterBusinessObject.Schema.Tin);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.DoesNotStartWith, "A", new[] { foreignOperator2, foreignOperator3, foreignOperator4, foreignOperator5, foreignOperator6 }, ForeignOperatorFilterBusinessObject.Schema.Tin);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.NotContains, "2", new[] { foreignOperator3, foreignOperator4, foreignOperator5, foreignOperator6 }, ForeignOperatorFilterBusinessObject.Schema.Tin);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsNotBlank, "", new[] { foreignOperator1, foreignOperator2, foreignOperator3 }, ForeignOperatorFilterBusinessObject.Schema.Tin);
				ModuleTestHelper.AssertModuleTextFilterResult<CusBRForeignOperator>(filterBO, SQLComparisonOperator.IsBlank, "", new[] { foreignOperator4, foreignOperator5, foreignOperator6 }, ForeignOperatorFilterBusinessObject.Schema.Tin);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ForeignOperatorFilterBusinessObject();
		}

		ForeignOperatorFilterBusinessObject filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (ForeignOperatorFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
