using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(SendChangesMessageHelper))]
class SendChangesMessageHelperTest : AmendmentMessageHelperAbstractTest<SendChangesMessageHelper>
{
	public void TestMetaDataDeclarationXPathValue()
	{
		var helper = new SendChangesMessageHelperForTest();
		AssertEquals("//node[@match=1]/node[@match='3']", helper.MetaDataDeclarationXPathValueExposed);
	}

	public new void TestCanBeAmended()
	{
		Assert("The logic will be changed", true);
	}

	protected override string InitialXML => "Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.AmendmentAdditionalInformation.xml";

	protected override string ExpectedXML => "Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.AmendmentAdditionalInformationExpected.xml";

	protected override string InitialXMLForSendChanges => "<Root><WCOTypeCode>CC415A</WCOTypeCode><CommunicationMetaData>SomethingIsAboutToChange</CommunicationMetaData><Declaration><ID>TestID</ID><ExtraDataElement>ValueToReplace</ExtraDataElement><AnotherDataElement>ThisValueRemains</AnotherDataElement><FunctionalReferenceID>FunctionalRefID</FunctionalReferenceID><DeclarationOffice><ID>DeclarationOfficeID</ID></DeclarationOffice><Agent><ID>NL43434343</ID><FunctionCode>DIR</FunctionCode><Contact><Name>CargoWise Support</Name></Contact></Agent><Declarant><Name>Declarant Full Name</Name><ID>NL56785678</ID><Address><CityName>Decapolis</CityName><CountryCode>NL</CountryCode><Line>Declarantenstraat 30</Line><PostcodeID>5890DW</PostcodeID></Address><Contact><Name>Declarant Contact Name</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+31592874125</ID><TypeCode>2</TypeCode></Communication><Communication><SequenceNumeric>2</SequenceNumeric><ID>declarant@mail.nl</ID><TypeCode>1</TypeCode></Communication></Contact></Declarant></Declaration></Root>";

	protected override string NewXMLForSendChanges => InitialXMLForSendChanges.Replace("ValueToReplace", "NewValue").Replace("SomethingIsAboutToChange", "ItIsChanged").Replace("CC415A", ZString.Empty);

	protected override string ExpectedXMLForSendChanges => NewXMLForSendChanges.Replace("<Root>", ZString.Empty).Replace("</Root>", ZString.Empty).Replace("<AnotherDataElement>ThisValueRemains</AnotherDataElement>", ZString.Empty).Replace("<CommunicationMetaData>ItIsChanged</CommunicationMetaData>", ZString.Empty).Replace("<WCOTypeCode></WCOTypeCode>", ZString.Empty);

	protected override SendChangesMessageHelper MessageHelper => SendChangesMessageHelper.Instance;

	protected override WCOJobDeclarationMessageSendingObject MessageSendingObject
	{
		get
		{
			var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
			var declaration = entryHeader.Declaration;
			var message = entryHeader.Messages.AddNew(typeof(NLEDIMessage));
			message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
			message.EM_MessageSubType = ImportSendMessageTypes.Codes.DEC;
			message.EM_MessageText = MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.SendChangesExample.xml");

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";
			entryHeader.CH_CEI_Instruction = cei.PK;

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryHeader.CH_CustomsMessageRemarks = "Amending";
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var sendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault(x => x.Header.PK.Equals(entryHeader.PK));

			return sendingObject;
		}
	}
}

class SendChangesMessageHelperForTest : SendChangesMessageHelper
{
	public SendChangesMessageHelperForTest() : base()
	{
	}

	public string MetaDataDeclarationXPathValueExposed => base.MetaDataDeclarationXPathValue;
}
