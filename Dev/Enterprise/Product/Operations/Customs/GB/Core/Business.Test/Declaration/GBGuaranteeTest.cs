using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(GBGuarantee))]
	public class GBGuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var guarantee = Factory.New<GBGuarantee>();
			AssertType<GBGuaranteeLookups>(guarantee.Lookups);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			guarantee.PW_ParentID = declaration.PK;
			guarantee.PW_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AssertEquals(typeof(GBGuaranteeLookups), guarantee.Lookups.GetType());
		}

		public void TestReferenceNumberFieldType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = Factory.New<GBGuarantee>();

			guarantee.PW_BondType = GuaranteeTypeList.Codes.Guarantee;
			AssertEquals(guarantee.ReferenceNumberFieldType, nameof(FieldType.TextCodeFindBox));
		}

		public void TestValidationSettingCodeAfterHolderId()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = declaration.Guarantees.AddNew();

			guarantee.PW_HolderIdentification = "424242";
			AssertEquals("Pre-requisite: PW_HolderIdentification copied to PW_BondNumber2", "424242", guarantee.PW_BondNumber2);
			AssertEquals("Pre-requisite: PW_BondNumber", ZString.Empty, guarantee.PW_BondNumber);

			guarantee.PW_Password = "Y";
			AssertEquals("PW_HolderIdentification copied to PW_BondNumber", "424242", guarantee.PW_BondNumber);
			AssertEquals("PW_BondNumber2", ZString.Empty, guarantee.PW_BondNumber2);

			AssertNoMessageErrors(guarantee.PW_BondNumberInfo);
		}
		public void TestSyncroniseWithGuaranteeHeaderLogic_DoesNot_ApplyToCDSdeclaration()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMP11111111");

			var declarant = Factory.New<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, "DEC22222222");

			var gua1 = Factory.NewWithValidTestData<EU.Business.CusGuaranteeHeader>();
			gua1.CPH_Type = Customs.Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			gua1.CPH_Number = "DEC22222222";
			gua1.CPH_OH_PermitHolder = importer.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var guarantee = declaration.Guarantees.AddNew();
			declaration.JE_OH_Importer = importer.PK;
			declaration.Declarant.OA_OH = declarant.PK;
			guarantee.PW_HolderIdentification = "DEC22222222";
			guarantee.PW_Password = "Y";
			AssertEquals("PW_HolderIdentification copied to PW_BondNumber", "GBIMP11111111", guarantee.PW_BondNumber);
			AssertEquals("Pre-requisite: PW_BondNumber2", ZString.Empty, guarantee.PW_BondNumber2);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return declaration.Guarantees.AddNew();
		}
	}
}
