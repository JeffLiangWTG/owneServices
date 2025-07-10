using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(CusSupplyChainActorReference))]
	class CusSupplyChainActorReferenceTest : CusReferenceAbstractTest<CusSupplyChainActorReference>
	{
		public void TestFieldCaption()
		{
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();

			var referenceRESAttribute = cusSupplyChainActorReference.CFR_ReferenceInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Identification Number", referenceRESAttribute.Caption);
			AssertEquals("ID No.", referenceRESAttribute.ShortCaption);

			var codeRESAttribute = cusSupplyChainActorReference.CFR_CodeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Code", codeRESAttribute.Caption);

			var organizationRESAttribute = cusSupplyChainActorReference.OwnerOrgPKInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Organization", organizationRESAttribute.Caption);
		}

		public void TestGetReferenceFromOrganisation_EORI()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);

			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertEquals("DEEOR1", cusSupplyChainActorReference.CFR_Reference);
		}

		public void TestCFR_Reference_MaxLength()
		{
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			AssertEquals(17, cusSupplyChainActorReference.CFR_ReferenceInfo.MaxLength);
		}

		public void TestCFR_Code_MaxLength()
		{
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			AssertEquals(3, cusSupplyChainActorReference.CFR_CodeInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<CusSupplyChainActorReference>();

		protected override IEnumerable<CusSupplyChainActorReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var bill = header.Bills.AddNew();
			yield return FillWithValidData(header.CusSupplyChainActorReferences.AddNew());
			yield return FillWithValidData(bill.CusSupplyChainActorReferences.AddNew());
			yield return FillWithValidData(bill.Packs.AddNew().CusSupplyChainActorReferences.AddNew());
		}

		CusSupplyChainActorReference FillWithValidData(CusSupplyChainActorReference reference)
		{
			reference.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			reference.CFR_Reference = "111";
			return reference;
		}
	}
}
