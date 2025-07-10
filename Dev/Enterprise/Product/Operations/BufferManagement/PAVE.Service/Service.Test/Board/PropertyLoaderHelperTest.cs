using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Service.Test
{
	public class PropertyLoaderHelperTest : TestCaseWithFactory
	{
		public void TestShouldCachePropertyInfos()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.Job, "<CompanyData.Company.Address1>", PropertyTypeList.Codes.Text);

			var calls = new List<(Type, string)>();
			PropertyInfo propertyInfoGetter(Type @type, string propName)
			{
				calls.Add((@type, propName));
				return @type.GetProperty(propName);
			}

			org1.CompanyData.Company.Address1 = "Neverland";

			var cache = new PropertyCache();
			var properties1 = PropertyLoaderHelper.LoadPropertiesForLayout(org1, orgCustomisation, PropertySourceList.Codes.Job, cache, propertyInfoGetter);
			var expectedOrg1JobProperties = new Dictionary<string, object>()
			{
				["companyDataCompanyAddress1"] = (ZString)"Neverland"
			};
			AssertionHelper.AssertDTOProperties(expectedOrg1JobProperties, properties1);

			org2.CompanyData.Company.Address1 = "Narnia";

			var properties2 = PropertyLoaderHelper.LoadPropertiesForLayout(org2, orgCustomisation, PropertySourceList.Codes.Job, cache, propertyInfoGetter);
			var expectedOrg2JobProperties = new Dictionary<string, object>()
			{
				["companyDataCompanyAddress1"] = (ZString)"Narnia"
			};
			AssertionHelper.AssertDTOProperties(expectedOrg2JobProperties, properties2);

			var expectedCalls = new (Type, string)[] {
				(typeof(OrgHeader), "CompanyData"),
				(typeof(OrgCompanyData), "Company"),
				(typeof(GlbCompany), "Address1"),
			};
			AssertContainsExactElementsInAnyOrder("PropertyInfo should be cached, so property getter should be called just once for every property", expectedCalls, calls);
		}

		public void TestShouldCachePropertyValues()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.Job, "<CompanyData.Company.Address1>", PropertyTypeList.Codes.Text);
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.Job, "<CompanyData.Company.Address2>", PropertyTypeList.Codes.Text);

			var calls = new List<(string, object)>();
			object propertyValueGetter(object target, PropertyInfo propertyInfo, string valueKey)
			{
				var value = propertyInfo.GetValue(target);
				calls.Add((valueKey, value));
				return value;
			}

			org.CompanyData.Company.Address1 = "Neverland";
			org.CompanyData.Company.Address2 = "Where is it exactly?";

			var cache = new PropertyCache();
			var properties = PropertyLoaderHelper.LoadPropertiesForLayout(org, orgCustomisation, PropertySourceList.Codes.Job, cache, propertyValueGetter: propertyValueGetter);
			var expectedOrgJobProperties = new Dictionary<string, object>()
			{
				["companyDataCompanyAddress1"] = (ZString)"Neverland",
				["companyDataCompanyAddress2"] = (ZString)"Where is it exactly?"
			};
			AssertionHelper.AssertDTOProperties(expectedOrgJobProperties, properties);

			var expectedCalls = new (string, object)[] {
				("CompanyData", org.CompanyData),
				("CompanyDataCompany", org.CompanyData.Company),
				("CompanyDataCompanyAddress1", (ZString)"Neverland"),
				("CompanyDataCompanyAddress2", (ZString)"Where is it exactly?"),
			};
			AssertContainsExactElementsInAnyOrder("PropertyInfo should be cached, so property getter should be called just once for every property", expectedCalls, calls);
		}
	}
}
