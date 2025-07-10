using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaBaseAdditionalInfoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeBySubType()
		{
			AssertEquals("When Sub Type is INF the type should be AsycudaAdditionalInfo", typeof(AsycudaAdditionalInfo), new AsycudaBaseAdditionalInfoTypeDecider().GetTypeBySubType("INF"));
			AssertEquals("When Sub Type is TRA the type should be AsycudaTransportDocumentInfo", typeof(AsycudaTransportDocumentInfo), new AsycudaBaseAdditionalInfoTypeDecider().GetTypeBySubType("TRA"));
			AssertEquals("When Sub Type is other the type should be null", null, new AsycudaBaseAdditionalInfoTypeDecider().GetTypeBySubType(""));
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals("The type should be null for binding", null, new AsycudaBaseAdditionalInfoTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var typeDecider = new AsycudaBaseAdditionalInfoTypeDecider();
			var additionalInfo = Factory.NewWithValidTestData<AsycudaAdditionalInfo>();
			var transportDocument = Factory.NewWithValidTestData<AsycudaTransportDocumentInfo>();
			AssertEquals("When CSI_SubType is INF the type should be AsycudaAdditionalInfo for load", typeof(AsycudaAdditionalInfo), typeDecider.GetTypeForLoad(((IBusinessObjectInternals)additionalInfo).Row, Factory));
			AssertEquals("When CSI_SubType is TRA the type should be AsycudaTransportDocumentInfo for load", typeof(AsycudaTransportDocumentInfo), typeDecider.GetTypeForLoad(((IBusinessObjectInternals)transportDocument).Row, Factory));
			AssertEquals("When null the type should be null for load", null, typeDecider.GetTypeForLoad(null, Factory));
		}

		public void TestGetTypeForNew()
		{
			AssertEquals("The type should be null for new", null, new AsycudaBaseAdditionalInfoTypeDecider().GetTypeForNew());
		}
	}
}
