using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderCustomsMessageFountainProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderCustomsMessageFountainProvider(null));
		AssertNoExceptionThrown(() => new NctsHeaderCustomsMessageFountainProvider(Factory.New<NctsHeader>()));
	}

	public void TestProperties()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		Factory.Save();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_CustomsProfile = "1234-DEC1";
		CombineAssertions(() =>
		{
			var provider = new NctsHeaderCustomsMessageFountainProvider(nctsHeader);
			AssertEquals("Node", "1234", provider.Node);
			AssertEquals("FountainType", NumberRangeTypeList.Codes.CustomsDeclarations, provider.FountainType);
			AssertSame("Company", nctsHeader.Company, provider.Company);
		});
	}
}
