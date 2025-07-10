using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

class EdecPermitDetailDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Argument == null", EdecPermitDetailDataProvider.New(null));
			AssertNotNull("Argument != null", EdecPermitDetailDataProvider.New(permitItemDetail));
		});
	}

	public void TestKey()
	{
		CombineAssertions(() =>
		{
			permitItemDetail.CY_Code = "12";
			AssertEquals("When not empty", "12", dataProvider.Key);

			permitItemDetail.CY_Code = ZString.Empty;
			AssertEquals("When empty", "0", dataProvider.Key);
		});
	}

	public void TestType()
	{
		CombineAssertions(() =>
		{
			permitItemDetail.CY_Data = "Data5678901234567890123456789012345678901234567890";
			AssertEquals("When not empty", "Data5678901234567890123456789012345678901234567890", dataProvider.Type);

			permitItemDetail.CY_Data = ZString.Empty;
			AssertEquals("When empty", string.Empty, dataProvider.Type);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		permitItemDetail = Factory.New<PermitItemDetail>();
		dataProvider = EdecPermitDetailDataProvider.New(permitItemDetail);
	}
	PermitItemDetail permitItemDetail;
	EdecPermitDetailDataProvider dataProvider;
}
