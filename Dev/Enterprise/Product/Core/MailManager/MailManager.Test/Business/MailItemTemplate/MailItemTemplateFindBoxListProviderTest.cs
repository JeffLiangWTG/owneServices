using CargoWise.EntityFramework.Testing;

namespace Enterprise.MailManager.Business.Testing
{
	sealed class MailItemTemplateFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestGetBizObjFromCode()
		{
			var obj = ListProvider.GetBusinessObjectFromCode(Template1.TemplateID);
			AssertEquals(Template1.PK, obj.PK);

			obj = ListProvider.GetBusinessObjectFromCode(Template2.TemplateID);
			AssertEquals(Template2.PK, obj.PK);

			obj = ListProvider.GetBusinessObjectFromCode(Template3.TemplateID);
			AssertEquals(Template3.PK, obj.PK);

			obj = ListProvider.GetBusinessObjectFromCode("DOG_Grumpy");
			AssertNull(obj);

			obj = ListProvider.GetBusinessObjectFromCode("CAT_Coffee");
			AssertNull(obj);

			obj = ListProvider.GetBusinessObjectFromCode("Shibe");
			AssertNull(obj);
		}

		public void TestGetBizObjFromCodeWithoutFilter()
		{
			var obj = ListProvider.GetBusinessObjectFromCodeWithoutFilter(Template1.TemplateID);
			AssertEquals(Template1.PK, obj.PK);

			obj = ListProvider.GetBusinessObjectFromCodeWithoutFilter(Template2.TemplateID);
			AssertEquals(Template2.PK, obj.PK);

			obj = ListProvider.GetBusinessObjectFromCodeWithoutFilter(Template3.TemplateID);
			AssertEquals(Template3.PK, obj.PK);

			obj = ListProvider.GetBusinessObjectFromCodeWithoutFilter("DOG_Grumpy");
			AssertNull(obj);

			obj = ListProvider.GetBusinessObjectFromCodeWithoutFilter("CAT_Coffee");
			AssertNull(obj);

			obj = ListProvider.GetBusinessObjectFromCodeWithoutFilter("Shibe");
			AssertNull(obj);
		}

		public void TestNearestMatch()
		{
			string result = ListProvider.NearestMatch("CAT", true, -1).Item1;
			AssertEquals(Template1.TemplateID, result);

			result = ListProvider.NearestMatch("DOG", true, -1).Item1;
			AssertEquals(Template2.TemplateID, result);

			result = ListProvider.NearestMatch("NON", true, -1).Item1;
			AssertEquals(Template3.TemplateID, result);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var templates = new MailItemTemplateCollection(Factory);

			Template1 = templates.AddNew();
			Template1.MIT_Category = "CAT";
			Template1.MIT_Name = "Grumpy";

			Template2 = templates.AddNew();
			Template2.MIT_Category = "DOG";
			Template2.MIT_Name = "Shibe";

			Template3 = templates.AddNew();
			Template3.MIT_Category = "NON";
			Template3.MIT_Name = "Coffee";

			ListProvider = new MailItemTemplateFindBoxListProvider(templates);
		}

		MailItemTemplate Template1;
		MailItemTemplate Template2;
		MailItemTemplate Template3;
		MailItemTemplateFindBoxListProvider ListProvider;

		#endregion
	}
}
