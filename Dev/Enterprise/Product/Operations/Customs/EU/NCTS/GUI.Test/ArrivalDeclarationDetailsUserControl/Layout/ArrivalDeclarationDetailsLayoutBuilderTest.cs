using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ArrivalDeclarationDetailsLayoutBuilder<NctsHeader>))]
	sealed class ArrivalDeclarationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ArrivalDeclarationDetailsLayoutBuilder<NctsHeader>, NctsHeader, ArrivalDeclarationDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override ArrivalDeclarationDetailsLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new ArrivalDeclarationDetailsLayoutBuilder<NctsHeader>();

		public void TestDefaultCaptions()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var layout = ColumnLayoutBuilderForTesting.Build();

			layout.TryGetCaption(ArrivalDeclarationDetailsControlBag.Instance.AcceptanceDateEdit, nctsHeader, out var resourceStringData);
			AssertEquals("AcceptanceDateEdit Caption", "Acceptance Date", resourceStringData.Caption);
		}
	}
}
