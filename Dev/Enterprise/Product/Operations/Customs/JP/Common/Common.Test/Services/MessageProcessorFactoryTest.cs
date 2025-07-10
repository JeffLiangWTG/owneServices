using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class MessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestGetProcessor()
		{
			var logger = new LoggingInformation();
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;

			var processor = MessageProcessorFactory.GetMessageProcessor(message, logger);

			Assert(processor is Integration.Customs.JP.IDeclarationMessageProcessor);
			AssertEquals("Enterprise.Customs.JP.Business.MessageProcessors.DeclarationNACCSMessageProcessor", processor.GetType().FullName);

			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			processor = MessageProcessorFactory.GetMessageProcessor(message, logger);

			Assert(processor is Integration.Customs.JP.IManifestHeaderMessageProcessor);
			AssertEquals("Enterprise.Customs.JP.Manifest.Business.MessageProcessors.ManifestNACCSMessageProcessor", processor.GetType().FullName);

			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			message.EM_MessageType = JPMessageTypes.Codes.XER;

			processor = MessageProcessorFactory.GetMessageProcessor(message, logger);

			Assert(processor is ErrorMessageProcessor);
		}
	}
}
