using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class JobDeclarationFountainProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new JobDeclarationFountainProvider(null));
		AssertNoExceptionThrown(() => new JobDeclarationFountainProvider(Factory.New<JobDeclaration>()));
	}

	public void TestProperties()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_CustomsProfile = "1234-DEC1";
		CombineAssertions(() =>
		{
			var provider = new JobDeclarationFountainProvider(declaration);
			AssertEquals("Node", "1234", provider.Node);
			AssertEquals("FountainType", NumberRangeTypeList.Codes.CustomsDeclarations, provider.FountainType);
			AssertSame("Company", declaration.Company, provider.Company);
		});
	}
}
