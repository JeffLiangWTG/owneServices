using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(WebRefCountryFilterBusinessObject))]
	sealed class WebRefCountryFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		public void TestFilter_Contains()
		{
			var filter = (WebRefCountryFilterBusinessObject)GetNewBusinessObject();
			filter.Contains = true;
			filter.StartsWith = false;
			filter.RN_Desc = "A";

			var expectedFilter = $"{RefCountrySchema.RN_Code.Name} like '%A%' or {RefCountrySchema.RN_Desc.Name} like '%A%'";
			AssertEquals(expectedFilter, filter.Filter.LiteralTextADO);
		}

		public void TestFilter_StartsWith()
		{
			var filter = (WebRefCountryFilterBusinessObject)GetNewBusinessObject();
			filter.Contains = false;
			filter.StartsWith = true;
			filter.RN_Desc = "A";

			var expectedFilter = $"{RefCountrySchema.RN_Code.Name} like 'A%' or {RefCountrySchema.RN_Desc.Name} like 'A%'";
			AssertEquals(expectedFilter, filter.Filter.LiteralTextADO);
		}

		public void TestFilter_DescriptionDoesNotExceedCodeLength()
		{
			var filter = (WebRefCountryFilterBusinessObject)GetNewBusinessObject();

			var desc = new string('A', AutoRefCountry.Schema.RN_DescMaxLength);
			filter.RN_Desc = desc;

			var expectedFilter = $"{RefCountrySchema.RN_Desc.Name} = '{desc}'";
			AssertEquals(expectedFilter, filter.Filter.LiteralTextADO);
		}

		public void TestFilter_DescriptionDoesNotExceedDescLength()
		{
			var filter = (WebRefCountryFilterBusinessObject)GetNewBusinessObject();
			var desc = new string('A', AutoRefCountry.Schema.RN_DescMaxLength);
			filter.RN_Desc = desc + "A";

			var expectedFilter = $"{RefCountrySchema.RN_Desc.Name} = '{desc}'";
			AssertEquals(expectedFilter, filter.Filter.LiteralTextADO);
		}
	}
}
