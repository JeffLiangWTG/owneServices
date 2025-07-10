using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSAddInfoJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeferredSubmissionList()
		{
			AssertEquals("0, 1, 2", lookups.DeferredSubmissionList.CodesAsString);
		}

		public void TestGuarantorTypeList_Cached()
		{
			var list = lookups.GuarantorTypeList;
			AssertSame(list, lookups.GuarantorTypeList);
		}

		public void TestGuarantorTypeList_IsDeclarantTypeConsignor()
		{
			SetupGuarantorTypes();
			declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("1", lookups.GuarantorTypeList.CodesAsString);
		}

		public void TestGuarantorTypeList_IsDeclarantTypeConsignee()
		{
			SetupGuarantorTypes();
			declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
			AssertEquals("1, 2", lookups.GuarantorTypeList.CodesAsString);
		}

		public void TestOriginTypeListIfDeclarationIsConsolidatedDocument()
		{
			var list = lookups.OriginTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("No ConsolidatedDocument", "1, 2, 3", lookups.OriginTypeList.CodesAsString);
				AssertSame("No ConsolidatedDocument: Cached", list, lookups.OriginTypeList);

				declaration.SetConsolidatedDocument();
				list = lookups.OriginTypeList;
				AssertEquals("Is ConsolidatedDocument", "1", lookups.OriginTypeList.CodesAsString);
				AssertSame("Is ConsolidatedDocument: Cached", list, lookups.OriginTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			lookups = declaration.AddInfoLookups;
		}
		EMCSAddInfoJobDeclarationLookups lookups;
		EMCSJobDeclaration declaration;

		void SetupGuarantorTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, "Excise Movement Control System (EMCS) Guarantor Type");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.IsJointGuarantor, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var code1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IsJointGuarantor, YesNoList.Codes.No);
			var code2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IsJointGuarantor, YesNoList.Codes.Yes);
			Factory.Save();
		}
	}
}
