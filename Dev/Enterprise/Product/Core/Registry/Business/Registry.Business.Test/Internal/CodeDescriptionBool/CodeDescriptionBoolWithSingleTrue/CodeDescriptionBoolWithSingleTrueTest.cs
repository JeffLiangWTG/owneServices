using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithSingleTrue))]
	sealed class CodeDescriptionBoolWithSingleTrueTest : CodeDescriptionBoolTest
	{
		public void TestSingleTrue()
		{
			CodeDescriptionBoolWithSingleTrueCollection collection = new CodeDescriptionBoolWithSingleTrueCollection();
			CodeDescriptionBoolWithSingleTrue setting1 = collection.AddNew();
			CodeDescriptionBoolWithSingleTrue setting2 = collection.AddNew();

			setting1.Code = "AAA";
			setting1.Description = (NoResString)"DescriptionA";
			setting1.Bool = true;
			setting2.Code = "BBB";
			setting2.Description = (NoResString)"DescriptionB";
			setting2.Bool = false;
			collection.RunPreSaveValidation();
			AssertNoErrors(setting1.BoolInfo);
			AssertNoErrors(setting2.BoolInfo);

			setting1.Bool = false;
			setting2.Bool = false;
			collection.RunPreSaveValidation();
			AssertHasErrors("Should be exactly one tick", setting1.BoolInfo);
			AssertHasErrors("Should be exactly one tick", setting2.BoolInfo);

			setting1.Bool = true;
			setting2.Bool = true;
			collection.RunPreSaveValidation();
			AssertHasErrors("Should be exactly one tick", setting1.BoolInfo);
			AssertHasErrors("Should be exactly one tick", setting2.BoolInfo);
		}

		public void TestValidateURL()
		{
			var item = new CodeDescriptionBoolWithSingleTrue();
			item.EnglishDescription = "http://123456.com";
			AssertNoErrors(item.EnglishDescriptionInfo);

			item.EnglishDescription = " ";
			AssertHasErrors("URL should not be empty.", item.EnglishDescriptionInfo);

			item.EnglishDescription = "balabala";
			AssertHasErrors("URL format is incorrect.", item.EnglishDescriptionInfo);

			item.EnglishDescription = "https://123456.com";
			AssertNoErrors(item.EnglishDescriptionInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CodeDescriptionBoolWithSingleTrue)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.CodeList = new CodeDescriptionPairList();

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CodeDescriptionBoolWithSingleTrue();
		}

		#endregion
	}
}
