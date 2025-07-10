using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(JPCUniversalCustomsMessageProcessor))]
sealed class JPCUniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<JPCUniversalCustomsMessageProcessor>
{
	public void TestGetLinkedBusinessObjectMetaData()
	{
		var loggerMock = new Mock<LoggingInformation>();

		message.EM_LinkedObject = null;

		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty);
		var actualResult = processor.GetLinkedBusinessObjectMetaData(message, loggerMock.Object);
		AssertEquals(expectedResult, actualResult);

		var header = Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
		header.AMA_JobReference = "JPC20250201";
		message.EM_LinkedObject = (BusinessObject)header;

		CombineAssertions(() =>
		{
			expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, header.AMA_JobReference));
			actualResult = processor.GetLinkedBusinessObjectMetaData(message, loggerMock.Object);

			AssertEquals(expectedResult, actualResult);
		});
	}

	public void TestGetBranch()
	{
		var company = Factory.New<GlbCompany>();
		var branch = company.Branches.AddNew();

		var header = Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
		header.AMA_JobReference = "JPC20250201";

		message.EM_LinkedObject = (BusinessObject)header;
		header.AMA_GB = branch.PK;

		var loggerMock = new Mock<LoggingInformation>();

		CombineAssertions(() =>
		{
			var expectedResult = ProcessingResult.New(header.AMA_GB);
			var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, loggerMock.Object);
			var actualResult = processor.GetBranch(message, loggerMock.Object, linkedBusinessObjectMetaData.ReturnValue.BranchPk);

			AssertEquals("The branch key of the message has been populated in the Unpacker, so we just return the input branch key.", expectedResult, actualResult);
		});
	}

	public void TestGetSerializationKeysResult()
	{
		var header = Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
		((BusinessObject)header).FillWithValidTestData();

		header.AMA_JobReference = "JPC20250201";
		message.EM_LinkedObject = (BusinessObject)header;

		Factory.Save();

		var loggerMock = new Mock<LoggingInformation>();

		CombineAssertions(() =>
		{
			var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { header.AMA_JobReference }));
			var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, loggerMock.Object);
			var actualResult = processor.GetSerializationKeysResult(message, loggerMock.Object, linkedBusinessObjectMetaData.ReturnValue);

			AssertEquals(expectedResult, actualResult);
		});
	}

	public void TestShouldMessageBeProcessedInASeparateFactory()
	{
		Assert("No need to process messages in different factories.", !processor.ShouldMessageBeProcessedInASeparateFactory(null));
	}

	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.JPCustoms;

	protected override void SetUp()
	{
		base.SetUp();
		processor = new JPCUniversalCustomsMessageProcessor();
		message = Factory.New<EDIMessage>();
	}
	IUniversalCustomsMessageProcessor processor;
	EDIMessage message;
}
