using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolidateByProvinceOfClearanceStrategyTest : TestCaseWithFactory
	{
		public void TestHasAcknowledged()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "GBA";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration);
			CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByProvinceOfClearance", new ConsolidateByProvinceOfClearanceStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective ConsolidateByProvinceOfClearance", !new ConsolidateByProvinceOfClearanceStrategy(wrapper).HasAcknowledged);
			declaration.JE_OH_Importer = importer.PK;
			var orgImpAddInfo = OrgImpAddInfo.Get(declaration.Importer);
			orgImpAddInfo.ZO_IsConsolidateByProvinceofClearance = false;
			Assert("Effective ConsolidateByProvinceOfClearance", !new ConsolidateByProvinceOfClearanceStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByProvinceOfClearance", new ConsolidateByProvinceOfClearanceStrategy(wrapper).HasAcknowledged);
			orgImpAddInfo.ZO_IsConsolidateByProvinceofClearance = true;
			CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByProvinceOfClearance", new ConsolidateByProvinceOfClearanceStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective ConsolidateByProvinceOfClearance", new ConsolidateByProvinceOfClearanceStrategy(wrapper).HasAcknowledged);
		}

		public void TestAddMatchingFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PA");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "1111";
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration);
			var strategy = new ConsolidateByProvinceOfClearanceStrategy(wrapper, true);
			var query = new ZQuery();
			strategy.AddMatchingFilter(query);
			var whereClause = query.GetAsWhereClause(true);
			Assert("ZQuery contains JE_ProvinceOfClearance = 'PA'", whereClause.Contains("JE_ProvinceOfClearance = 'PA'"));

			declaration.JE_CustomsOffice = ZString.Empty;
			strategy = new ConsolidateByProvinceOfClearanceStrategy(wrapper, true);
			query = new ZQuery();
			strategy.AddMatchingFilter(query);
			whereClause = query.GetAsWhereClause(true);
			Assert("ZQuery not contains JE_ProvinceOfClearance", !whereClause.Contains("JE_ProvinceOfClearance"));
		}

		public void TestGetProvinceOfClearanceQuery()
		{
			var query = ConsolidateByProvinceOfClearanceStrategy.GetProvinceOfClearanceQuery("123");
			var whereClause = query.GetAsWhereClause(true);
			Assert("ZQuery contains JE_ProvinceOfClearance = '123'", whereClause.Contains("JE_ProvinceOfClearance = '123'"));
		}

		public void TestIsMatching()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PA");
			Factory.Save();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_CustomsOffice = "1111";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CA_ProvinceOfClearance = "PA";
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration1);
			var strategy = new ConsolidateByProvinceOfClearanceStrategy(wrapper, true);

			Assert("declaration.CA_ProvinceOfClearance == Wrapper.Province", strategy.IsMatching(declaration2));

			declaration2.CA_ProvinceOfClearance = "CA";
			Assert("declaration.CA_ProvinceOfClearance != Wrapper.Province", !strategy.IsMatching(declaration2));

			declaration2.CA_ProvinceOfClearance = ZString.Empty;
			Assert("declaration.CA_ProvinceOfClearance.IsEmpty", strategy.IsMatching(declaration2));

			declaration2.CA_ProvinceOfClearance = "CA";
			strategy = new ConsolidateByProvinceOfClearanceStrategy(wrapper, false);
			Assert("!HasAcknowledged", strategy.IsMatching(declaration2));

			declaration1.JE_CustomsOffice = ZString.Empty;
			wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration1);
			strategy = new ConsolidateByProvinceOfClearanceStrategy(wrapper, true);
			Assert("Wrapper.Province.IsEmpty", strategy.IsMatching(declaration2));
		}

		public void TestFillDataForNewDeclaration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PA");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2222", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_CustomsOffice = "1111";
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration1);
			var strategy = new ConsolidateByProvinceOfClearanceStrategy(wrapper, true);

			var declaration2 = Factory.New<JobDeclaration>();
			strategy.FillDataForNewDeclaration(declaration2);
			AssertEquals(declaration2.CA_ProvinceOfClearance, "PA");

			declaration2.CA_ProvinceOfClearance = "CA";
			strategy.FillDataForNewDeclaration(declaration2);
			AssertEquals(declaration2.CA_ProvinceOfClearance, "CA");

			declaration2.CA_ProvinceOfClearance = ZString.Empty;
			declaration1.JE_CustomsOffice = "2222";
			wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration1);
			strategy = new ConsolidateByProvinceOfClearanceStrategy(wrapper, true);
			AssertEquals(declaration2.CA_ProvinceOfClearance, ZString.Empty);
		}
	}
}
