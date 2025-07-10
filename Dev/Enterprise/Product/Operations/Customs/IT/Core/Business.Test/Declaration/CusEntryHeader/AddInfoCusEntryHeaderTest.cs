using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(AddInfoCusEntryHeader))]
sealed class AddInfoCusEntryHeaderTest : EU.Business.Declaration.Testing.AddInfoCusEntryHeaderBOTest
{
	public void TestZG_AmendmentStatus()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		entryHeader.ZG_AmendmentStatus = AmendmentStatusList.Codes.Amendment;
		Factory.Save();
		AssertContains($"AmendmentStatus={entryHeader.ZG_AmendmentStatus}", entryHeader.CH_AddInfo);
	}

	protected override BusinessObject GetNewBusinessObject() => new AddInfoCusEntryHeader(Factory.New<CusEntryHeader>().CH_AddInfoInfo);
}
