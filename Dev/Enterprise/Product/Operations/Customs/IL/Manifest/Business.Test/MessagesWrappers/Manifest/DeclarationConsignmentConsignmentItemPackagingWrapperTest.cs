using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemPackagingWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemPackaging>
	{
		public void TestMarksNumbers()
		{
			AssertEquals("MarksNumbers must have expected value", "Marks and numbers", Provider.MarksNumbers.Value);
		}

		public void TestQuantityQuantity()
		{
			AssertEquals("Quantity must have expected value", 100m, Provider.QuantityQuantity.Value);
		}

		public void TestTypeCode()
		{
			AssertEquals("TypeCode must have expected value", "XYZ", Provider.TypeCode.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemPackagingWrapper.NewOrNull(null));
			AssertNotNull(DeclarationConsignmentConsignmentItemPackagingWrapper.NewOrNull(Factory.New<AsycudaPack>()));
		}

		protected override IDeclarationConsignmentConsignmentItemPackaging GetProvider()
		{
			var asycudaPack = Factory.New<AsycudaPack>();
			asycudaPack.APA_MarksAndNumbers = "Marks and numbers";
			asycudaPack.APA_PackQty = 100;
			asycudaPack.APA_PackUQ = "XYZ";

			return DeclarationConsignmentConsignmentItemPackagingWrapper.NewOrNull(asycudaPack);
		}
	}
}
