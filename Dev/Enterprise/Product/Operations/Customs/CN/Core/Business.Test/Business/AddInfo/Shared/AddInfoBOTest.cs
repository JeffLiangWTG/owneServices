using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	abstract class AddInfoBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Lookups", GetExpectedLookupsType(), addInfo.Lookups.GetType());
		}

		protected abstract Type GetExpectedLookupsType();
		public void TestValidation()
		{
			var addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Validation", GetExpectedValidationType(), addInfo.Validation.GetType());
		}

		protected abstract Type GetExpectedValidationType();
		public void TestResourceStrings()
		{
			var addInfo = (AddInfo)GetNewBusinessObject();
			var properties = addInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => !x.HasUserDescription);
			Assert("Resource String missed in CNAddInfoResourceStrings.xml for the following AddInfo fields:\r\n\t" + string.Join(", ", properties.Select(x => x.Name).ToArray()), !properties.Any());
		}
	}
}
