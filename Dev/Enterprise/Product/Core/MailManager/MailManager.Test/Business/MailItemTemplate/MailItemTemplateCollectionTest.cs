using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MailManager.Business.Testing
{
	[TestedType(typeof(MailItemTemplateCollection))]
	sealed class MailItemTemplateCollectionTest : ActiveBusinessObjectCollectionTestCase<MailItemTemplateCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return (MailItemTemplate)base.GetNewElementToAddToTheCollection();
		}

		protected override MailItemTemplateCollection GetCollectionToTest()
		{
			return new MailItemTemplateCollection(Factory);
		}

		public void TestRelationshipFilter()
		{
			Env.Security.ViewAllEmailTemplates.IsAllowed = true;
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var collection = new MailItemTemplateCollection(Factory);
			var template1 = collection.AddNew();
			template1.MIT_Category = MailTemplateCategoryList.Codes.None;
			template1.MIT_Name = "template1";
			template1.MIT_GC_Company = GlbCompany.CurrentCompany.PK;
			template1.MIT_GB_Branch = GlbBranch.CurrentBranch.PK;
			template1.MIT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			var template2 = collection.AddNew();
			template2.MIT_Category = MailTemplateCategoryList.Codes.AgencyBillOfLading;
			template2.MIT_Name = "template2";
			var template3 = collection.AddNew();
			template3.MIT_Category = MailTemplateCategoryList.Codes.AgencyBillOfLading;
			template3.MIT_Name = "template3";
			template3.MIT_GC_Company = anotherCompany.PK;
			Factory.Save();
			AssertEquals("Precondition", 3, collection.Count);

			Env.Security.ViewAllEmailTemplates.IsAllowed = false;
			collection = new MailItemTemplateCollection(Factory);
			AssertEquals(2, collection.Count);

			collection = new MailItemTemplateCollection(Factory);
			collection.CategoryCodeToFilter = MailTemplateCategoryList.Codes.None;
			AssertEquals("Number of templates in NONE category", 1, collection.Count);
			AssertCollectionContains(template1, collection);

			collection = new MailItemTemplateCollection(Factory);
			collection.CategoryCodeToFilter = MailTemplateCategoryList.Codes.AgencyBillOfLading;
			AssertEquals("Number of templates in AgencyBillOfLading category", 1, collection.Count);
			AssertCollectionContains(template2, collection);
		}

		public void TestFilterByCurrentLoginDetails()
		{
			var collection = new MailItemTemplateCollection(Factory);
			var template1 = collection.AddNew();
			template1.MIT_Category = MailTemplateCategoryList.Codes.None;
			template1.MIT_Name = "template1";
			template1.MIT_GC_Company = ZGuid.NewZGuid();

			collection.CategoryCodeToFilter = MailTemplateCategoryList.Codes.None;
			collection.FilterByCurrentLoginDetails();
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Company:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Branch:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Department:Property"));
		}

		public void TestGetPKOfFirstValidTemplateForCurrentCompany()
		{
			var collection = new MailItemTemplateCollection(Factory);
			var template1 = collection.AddNew();
			template1.MIT_Category = MailTemplateCategoryList.Codes.None;
			template1.MIT_Name = "template1";
			template1.MIT_GC_Company = GlbCompany.CurrentCompany.PK;
			template1.MIT_GB_Branch = GlbBranch.CurrentBranch.PK;
			template1.MIT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			var template2 = collection.AddNew();
			template2.MIT_GC_Company = GlbCompany.CurrentCompany.PK;
			template2.MIT_Category = MailTemplateCategoryList.Codes.AgencyBillOfLading;
			template2.MIT_Name = "template2";
			Factory.Save();
			AssertEquals("Precondition", 2, collection.Count);

			AssertEquals(template1.PK, collection.GetPKOfFirstValidTemplateForCurrentCompany());
			collection.CategoryCodeToFilter = MailTemplateCategoryList.Codes.AgencyBillOfLading;
			AssertEquals(template2.PK, collection.GetPKOfFirstValidTemplateForCurrentCompany());
		}
	}
}
