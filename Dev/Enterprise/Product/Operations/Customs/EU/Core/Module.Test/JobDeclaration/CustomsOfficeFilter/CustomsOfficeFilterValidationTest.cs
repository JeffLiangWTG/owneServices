using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module.Testing
{
	class CustomsOfficeFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPurpose()
		{
			Filter.Purpose = "ENT";
			AssertNoNotifications(Filter.PurposeInfo);

			Filter.Purpose = "XXX";
			AssertHasWarning(Filter.PurposeInfo, "You have not entered a valid code.");

			Filter.Purpose = "";
			AssertNoNotifications(Filter.PurposeInfo);
		}

		CustomsOfficeFilter Filter
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair("ENT", "Description 1");
				return filter ?? (filter = new CustomsOfficeFilter("description", delegate
				{ return new ZQuery(); }, list));
			}
		}
		CustomsOfficeFilter filter;
	}
}
