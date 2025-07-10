using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.Testing;

public class EntryStatusFilterValidationTest : TestCaseWithFactory
{
	public void TestCheckProperty()
	{
		filter.Property = "456";
		AssertHasWarning(filter.PropertyInfo, "You have not entered a valid code.");

		filter.Property = "123";
		AssertNoWarning(filter.PropertyInfo, "You have not entered a valid code.");
	}

	public void TestCheckFilterType()
	{
		filter.FilterType = "AAA";
		AssertHasError(filter.FilterTypeInfo, "Enter a valid selection.");

		filter.FilterType = EntryStatusFilterTypeList.Codes.All;
		AssertNoError(filter.FilterTypeInfo, "Enter a valid selection.");

		filter.FilterType = ZString.Empty;
		AssertHasError(filter.FilterTypeInfo, "Please enter a value.");
	}

	protected override void SetUp()
	{
		base.SetUp();
		filter = new EntryStatusFilter("Entry Status",
			delegate { return new ZQuery(); } ,
			GetEntryStatusList_ForTest);
	}

	CodeDescriptionPairList GetEntryStatusList_ForTest()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair("123", "Test Description");
		return list;
	}

	EntryStatusFilter filter;
}
