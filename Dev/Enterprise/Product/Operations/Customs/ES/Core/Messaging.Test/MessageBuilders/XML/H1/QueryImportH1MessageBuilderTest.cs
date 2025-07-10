using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ConsultaImportacionV2Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(QueryImportH1MessageBuilder))]
sealed class QueryImportH1MessageBuilderTest : H1ImportAbstractMessageBuilderTest<QueryImportH1MessageBuilder, IQueryImportH1MessageDataProvider, ConsultaImportacionV2Ent>
{
	#region Tests

	public void TestATCEmpty()
	{
		mockProvider.Setup(m => m.ATC).Returns(ZString.Empty);
		var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
		AssertNotContains("ATC", messageText);
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportH1Query;

	protected override QueryImportH1MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new QueryImportH1MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override QueryImportH1MessageBuilder CreateMessageBuilderWithNullProvider() => MockRandomGenerator(new QueryImportH1MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType));

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H1TestFilePath, "TestQueryImportH1Message.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.DataProviderMRN.MRN).Returns("PRLSVNE000006");
		mockProvider.Setup(m => m.CustomsRegistrationNumber).Returns("CRN");
		mockProvider.Setup(m => m.ATC).Returns("S");
	}

	#endregion
}
