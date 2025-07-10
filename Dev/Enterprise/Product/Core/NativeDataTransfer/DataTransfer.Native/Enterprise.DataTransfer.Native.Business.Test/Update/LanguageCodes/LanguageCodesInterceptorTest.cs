using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.LanguageCodes
{
	public class LanguageCodesInterceptorTest : TransactionedTestCase
	{
		public void TestImportLanguageMatchLocalLanguageByCode_NonSystem_Active()
		{
			CreateLocalLanguage(true, true);

			var sessionServices = new AncillaryImportServices();
			var updateSetting = SetupSetting(sessionServices);
			var handler = new LanguageCodesInterceptor(updateSetting, sessionServices) { Function = DummyMethod };

			var organizationEntitySet = PrepareOrganizationEntitySet(sessionServices, "aa-gb", "BB", "AA - GB");
			var root = organizationEntitySet.Root;
			handler.Invoke(organizationEntitySet);
			AssertEquals("AA-GB", root["Language"]);
			var addressEntity = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgAddress");
			AssertEquals("BB", addressEntity["Language"]);
			var contactEntity = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgContact");
			AssertEquals("EN", contactEntity["Language"]);
		}

		public void TestImportLanguageMatchLocalLanguageByDescription_NonSystem_Active()
		{
			CreateLocalLanguage(true, false);

			var sessionServices = new AncillaryImportServices();
			var updateSetting = SetupSetting(sessionServices);
			var handler = new LanguageCodesInterceptor(updateSetting, sessionServices) { Function = DummyMethod };

			var organizationEntitySet = PrepareOrganizationEntitySet(sessionServices, "Test Language", "test language", "TestLanguage");
			var root = organizationEntitySet.Root;
			handler.Invoke(organizationEntitySet);
			AssertEquals("AA-GB", root["Language"]);
			var addressEntity = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgAddress");
			AssertEquals("AA-GB", addressEntity["Language"]);
			var contactEntity = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgContact");
			AssertEquals("EN", contactEntity["Language"]);
		}

		public void TestImportLanguageMatchLocalLanguageByCode_NonSystem_Inactive()
		{
			CreateLocalLanguage(false, false);

			var sessionServices = new AncillaryImportServices();
			var updateSetting = SetupSetting(sessionServices);
			var handler = new LanguageCodesInterceptor(updateSetting, sessionServices) { Function = DummyMethod };

			var organizationEntitySet = PrepareOrganizationEntitySet(sessionServices, "AA-GB", null, null);
			var root = organizationEntitySet.Root;
			handler.Invoke(organizationEntitySet);
			AssertEquals("EN", root["Language"]);
		}

		public void TestMatchOrgAddressBySingleOrgCusCode()
		{
			var sessionServices = new AncillaryImportServices();
			var updateSetting = SetupSetting(sessionServices);
			var handler = new LanguageCodesInterceptor(updateSetting, sessionServices) { Function = DummyMethod };

			var organizationEntitySet = PrepareOrganizationEntitySet(sessionServices, "CHS", "CHS", "BAD");
			var root = organizationEntitySet.Root;
			handler.Invoke(organizationEntitySet);
			AssertEquals("Languages should be updated if exists in Old language mapping", "ZH-CN", root["Language"]);
			var addressEntity = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgAddress");
			AssertEquals("Languages should be updated if exists in Old language mapping", "ZH-CN", addressEntity["Language"]);
			var contactEntity = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgContact");
			AssertEquals("Languages should be defaulted to EN if not exist", "EN", contactEntity["Language"]);
		}

		#region Implementation

		void CreateLocalLanguage(bool isActive, bool createSecondLanguage)
		{
			var factory = new BusinessObjectFactory();
			var localLanguage1 = factory.New<IRefLocalLanguage>();
			localLanguage1.RA_Code = "AA";
			localLanguage1.RA_RN_NKCountryCode = "GB";
			localLanguage1.RA_Description = "Test Language";
			localLanguage1.RA_IsActive = isActive;
			localLanguage1.RA_IsSystem = false;

			if (createSecondLanguage)
			{
				var localLanguage2 = factory.New<IRefLocalLanguage>();
				localLanguage2.RA_Code = "BB";
				localLanguage2.RA_IsActive = true;
				localLanguage2.RA_IsSystem = false;
			}

			factory.Save();
		}

		protected EntitySet PrepareOrganizationEntitySet(AncillaryImportServices sessionServices, string orgLanguage, string addressLanguage, string contactLanguage)
		{
			var organization = TestUtil.PrepareOrgHeaderEntity(sessionServices);
			organization["Language"] = orgLanguage;

			if (addressLanguage != null)
			{
				var orgAddressDef = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgAddress");
				var addressEntity = new Entity(orgAddressDef, sessionServices);
				addressEntity["Language"] = addressLanguage;
				organization.ChildrenCollection.Add(addressEntity);
			}

			if (contactLanguage != null)
			{
				var orgContactDef = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgContact");
				var contactEntity = new Entity(orgContactDef, sessionServices);
				contactEntity["Language"] = contactLanguage;
				organization.ChildrenCollection.Add(contactEntity);
			}

			return new EntitySet("Organization") { Root = organization };
		}

		static LanguageCodesSetting SetupSetting(AncillaryImportServices sessionServices)
		{
			var result = new LanguageCodesSetting();
			var context = new EntityContext(sessionServices, new FactoryProvider());
			result.Context = context;
			return result;
		}

		static void DummyMethod(IEntitySet entitySet)
		{
		}

#endregion
	}
}
