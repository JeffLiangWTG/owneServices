using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestUniversalCustomsDataObjectProviderType()
		{
			AssertType<UniversalCustomsDataObjectProvider>("UniversalCustomsDataObjectProvider Type", Factory.BOFactory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.France));
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var provider = new UniversalCustomsDataObjectProvider();
			AssertType<DeclarationDataObjectWriter>(provider.GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration))));
		}

		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			AssertType<JobDeclarationDataObjectReader>(provider.GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null));
		}
	}
}
