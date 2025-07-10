using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderCloneTest : TestCaseWithFactory
{
	public void TestCloneRepresentationType()
	{
		nctsHeader.RepresentationType = "1";
		var clonedNctsHeader = (NctsHeader)nctsHeader.Clone();
		AssertEquals("RepresentationType", "1", clonedNctsHeader.RepresentationType);
	}

	public void TestCloneDeclarantAddress()
	{
		var declarant = Factory.New<OrgHeader>();
		nctsHeader.DeclarantAddressPK = declarant.MainAddress.PK;
		var clonedNctsHeader = (NctsHeader)nctsHeader.Clone();
		AssertEquals("DeclarantAddressPK", declarant.MainAddress.PK, clonedNctsHeader.DeclarantAddressPK);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
	}

	NctsHeader nctsHeader;
}
