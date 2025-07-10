using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ES.DataTransfer.Universal.Testing
{
	class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestUniversalCustomsDataObjectProviderType()
		{
			AssertType<UniversalCustomsDataObjectProvider>("UniversalCustomsDataObjectProvider Type", Factory.BOFactory.GetUniversalCustomsDataObjectProvider("ES"));
		}

		public void TestDeclarationDataObjectWriterType()
		{
			var declaration = Factory.BOFactory.New<JobDeclaration>();
			var provider = new UniversalCustomsDataObjectProvider();
			AssertType<DeclarationDataObjectWriter>("DeclarationDataObjectWriter type", provider.GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration))));
		}

		public void TestDeclarationDataObjectReaderType()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			AssertType<JobDeclarationDataObjectReader>("DeclarationDataObjectReader type", provider.GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null));
		}
	}
}
