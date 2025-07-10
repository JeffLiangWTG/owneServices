using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(CusSupplyChainActorReference))]
	public class CusSupplyChainActorReferenceTest : CusSupplyChainActorReferenceAbstractTest<CusSupplyChainActorReference>
	{
		public void TestGetReferenceFromOwner_NoCusCodes()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals(ZString.Empty, cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NotNAT_NifNotES()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.Business;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			customCode.OK_CustomsRegNo = "123456789A";
			customCode.OK_RN_NKCodeCountry = "IT";
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals(ZString.Empty, cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NotNAT_NifNotES_Empty()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.Business;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			customCode.OK_CustomsRegNo = ZString.Empty;
			customCode.OK_RN_NKCodeCountry = "IT";
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals(ZString.Empty, cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NotNAT_EorNotES_WithoutPrefix()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.Business;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "123456789A";
			customCode.OK_RN_NKCodeCountry = "IT";
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("IT123456789A", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NotNAT_EorNotES_WithPrefix()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.Business;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "IT123456789A";
			customCode.OK_RN_NKCodeCountry = "IT";
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("IT123456789A", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NotNAT_NifES()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.Business;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			customCode.OK_CustomsRegNo = "123456789A";
			customCode.OK_RN_NKCodeCountry = "ES";
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("ES123456789A", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NotNAT_NifAndEorES()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.Business;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			customCode.OK_RN_NKCodeCountry = "ES";
			customCode.OK_CustomsRegNo = "A56789123";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("ESA12345678", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NAT_NifNotES()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			customCode.OK_CustomsRegNo = "12456789A";
			customCode.OK_RN_NKCodeCountry = "IT";
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals(ZString.Empty, cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NAT_NifES_Empty()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			customCode.OK_RN_NKCodeCountry = "ES";
			customCode.OK_CustomsRegNo = ZString.Empty;
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals(ZString.Empty, cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NAT_EorES()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_RN_NKCodeCountry = "ES";
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "123456789A";
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("ES123456789A", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_NAT_NifES()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_RN_NKCodeCountry = "ES";
			customCode.OK_CustomsRegNo = "123456789A";
			customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("Expected correct value when category is NAT and customsCode country is ES and codeType is NIF", "123456789A", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_PAS()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var customCode = orgHeader.CustomsCodes.AddNew();

			customCode.OK_RN_NKCodeCountry = "FR";
			customCode.OK_CustomsRegNo = "123456789A";
			customCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("Expected correct value (PAS) when NIF and EOR are not declared", "123456789A", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestGetReferenceFromOwner_ExceedMaxLength()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Replicate('A', 36), Core.Constants.CountryCodes.Spain);
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("ES".PadRight(35, 'A'), cusSupplyChainActorReference.CFR_Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
		}
		CusSupplyChainActorReference cusSupplyChainActorReference;
	}
}
