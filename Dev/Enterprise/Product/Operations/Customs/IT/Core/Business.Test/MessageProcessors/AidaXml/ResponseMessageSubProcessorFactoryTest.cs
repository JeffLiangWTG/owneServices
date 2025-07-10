using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ResponseMessageSubProcessorFactoryTest : TestCaseWithFactory
{
	public void TestGetSubProcessorWithMessageTypeCAN()
	{
		var cancellationMessage = entryHeader.Messages.AddNew();
		cancellationMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.Ucc6CancellationType;

		var subProcessor = ResponseMessageSubProcessorFactory.GetSubProcessor(cancellationMessage);

		AssertNotNull("Sub Processor", subProcessor);
		AssertType<CancellationResponseMessageSubProcessor>("Sub Processor Type", subProcessor);
	}

	public void TestGetSubProcessorDefault()
	{
		var ediMessage = entryHeader.Messages.AddNew();
		ediMessage.EM_MessageType = "OTH";

		var subProcessor = ResponseMessageSubProcessorFactory.GetSubProcessor(ediMessage);

		AssertNotNull("Sub Processor", subProcessor);
		AssertType<NewResponseMessageSubProcessor>("Sub Processor Type", subProcessor);
	}

	public void TestGetSubProcessorWithMessageTypeAMD()
	{
		var amendmentMessage = entryHeader.Messages.AddNew();
		amendmentMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType;

		var subProcessor = ResponseMessageSubProcessorFactory.GetSubProcessor(amendmentMessage);

		AssertNotNull("Sub Processor", subProcessor);
		AssertType<AmendmentResponseMessageSubProcessor>("Sub Processor Type", subProcessor);
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration jobDeclaration;
	CusEntryHeader entryHeader;
}
