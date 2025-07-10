using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(GoodsDeclarationOppositeInformationDataProvider))]
sealed class GoodsDeclarationOppositeInformationDataProviderTest : BaseArrivalDataProviderTest<GoodsDeclarationOppositeInformationDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override GoodsDeclarationOppositeInformationDataProvider CreateDataProvider() => GoodsDeclarationOppositeInformationDataProvider.New(NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.First());

	public void TestNewCollection()
	{
		AssertNull("null argument", GoodsDeclarationOppositeInformationDataProvider.New(null));
	}

	public void TestReferenceNumber()
	{
		NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "1234";
		_ = MovementReferenceNumber;
		AssertEquals("1234-1", DataProvider.ReferenceNumber);
	}

	public void TestText()
	{
		_ = MovementReferenceNumber;
		AssertEquals($"CW-{GlbCompany.CurrentCompany.LicenceKeyIdentifier}", DataProvider.Text);
	}

	public void TestUnusedProperties()
	{
		_ = MovementReferenceNumber;
		AssertNull("Detail", DataProvider.Detail);
	}
}
