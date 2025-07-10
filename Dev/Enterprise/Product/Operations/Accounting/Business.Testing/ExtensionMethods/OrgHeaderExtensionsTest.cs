using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	class OrgHeaderExtensionsTest : TestCaseWithFactory
	{
		public void TestIsGlobalCreditGroupParent()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var org = testObjectCreator.CreateOrgHeaderWithGlobalCreditLimitParent("GOrgParent", testObjectCreator.AUD.RX_Code, 500m);
			Assert(org.CompanyData.OB_IsDebtor);
			Assert(org.OH_IsActive);
			Assert(org.MiscServ.OM_ARGlobalCreditLimit > 0);
			Assert("This organization is a global credit group parent", OrgHeaderExtensions.IsGlobalCreditGroupParent(org));

			org.CompanyData.OB_IsDebtor = false;
			Factory.Save();
			Assert(!org.CompanyData.OB_IsDebtor);
			Assert(org.OH_IsActive);
			Assert(org.MiscServ.OM_ARGlobalCreditLimit > 0);
			Assert(!OrgHeaderExtensions.IsGlobalCreditGroupParent(org));

			org.CompanyData.OB_IsDebtor = true;
			org.OH_IsActive = false;
			Factory.Save();
			Assert(org.CompanyData.OB_IsDebtor);
			Assert(!org.OH_IsActive);
			Assert(org.MiscServ.OM_ARGlobalCreditLimit > 0);
			Assert(!OrgHeaderExtensions.IsGlobalCreditGroupParent(org));

			org.OH_IsActive = true;
			org.MiscServ.OM_ARGlobalCreditLimit = 0;
			Factory.Save();
			Assert(org.CompanyData.OB_IsDebtor);
			Assert(org.OH_IsActive);
			AssertEquals(0m, org.MiscServ.OM_ARGlobalCreditLimit);
			Assert(!OrgHeaderExtensions.IsGlobalCreditGroupParent(org));
		}

		public void TestIsGlobalCreditGroupChild()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var org = testObjectCreator.CreateOrgHeaderWithGlobalCreditLimitChild("GOrgChild", testObjectCreator.AUD.RX_Code, 500m);
			Assert("This organization is a global credit group child", OrgHeaderExtensions.IsGlobalCreditGroupChild(org));
		}

		public void TestHasWithholdTaxExemption()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var creditor = testObjectCreator.CreateOrgHeader("CRD001", creditor: true, false);
			Assert("Pre-condition: no RequiredDocuments", !creditor.RequiredDocuments.Any());
			Assert("HasWithholdTaxExemption is false", !creditor.HasWithholdTaxExemption());
			Assert("HasWithholdTaxExemption is false", !creditor.HasWithholdTaxExemption(ZDateTime.Today, CountryCodes.Australia));
			Assert("HasWithholdTaxExemption is false", !creditor.HasWithholdTaxExemption(ZDateTime.Today, CountryCodes.UnitedStates));

			var document = Factory.New<JobRequiredDocument>();
			document.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			document.EQ_DocType = RefDocTypes.WithholdingTaxExemption;
			document.EQ_RN_NKRelatedCountry = CountryCodes.Australia;
			document.EQ_ValidToDate = ZDateTime.Today.AddDays(1);
			creditor.RequiredDocuments.Add(document);

			Assert("HasWithholdTaxExemption is true", creditor.HasWithholdTaxExemption(ZDateTime.Today, CountryCodes.Australia));
			Assert("HasWithholdTaxExemption is true", creditor.HasWithholdTaxExemption(null, null));
			Assert("HasWithholdTaxExemption is true", creditor.HasWithholdTaxExemption(document.EQ_ValidToDate, CountryCodes.Australia));
			Assert("HasWithholdTaxExemption is false as ValidToDate has passed", !creditor.HasWithholdTaxExemption(document.EQ_ValidToDate.AddMinutes(1), CountryCodes.Australia));
			Assert("HasWithholdTaxExemption is false for other country", !creditor.HasWithholdTaxExemption(ZDateTime.Today, CountryCodes.UnitedStates));
		}
	}
}
