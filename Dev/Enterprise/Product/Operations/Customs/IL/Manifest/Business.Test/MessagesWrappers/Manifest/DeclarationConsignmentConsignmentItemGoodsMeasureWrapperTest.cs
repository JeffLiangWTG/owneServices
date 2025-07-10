using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemGoodsMeasureWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemGoodsMeasure>
	{
		public void TestGrossMassMeasure()
		{
			AssertEquals("Weight should be 100", 100m, Provider.GrossMassMeasure.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemGoodsMeasureWrapper.NewOrNull(null));
			AssertNotNull(DeclarationConsignmentConsignmentItemGoodsMeasureWrapper.NewOrNull(Factory.New<AsycudaPack>()));
		}

		protected override IDeclarationConsignmentConsignmentItemGoodsMeasure GetProvider()
		{
			var asycudaPack = Factory.New<AsycudaPack>();
			asycudaPack.APA_Weight = 100;

			return DeclarationConsignmentConsignmentItemGoodsMeasureWrapper.NewOrNull(asycudaPack);
		}
	}
}
