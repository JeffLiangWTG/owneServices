using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTS019EdiMessageParsingXMLPrettyDataProviderTest : TestCaseWithFactory
	{
		public void TestMessage() => Assert(ReferenceEquals(message, parsingXMLProvider.Message));
		public void TestMessageType() => AssertEquals("CC019C", parsingXMLProvider.MessageType);
		public void TestMessageRecipient() => AssertEquals("GB123456789000", parsingXMLProvider.MessageRecipient);
		public void TestMRN() => AssertEquals("23XI000081RN3DBHJ0", parsingXMLProvider.MRN);
		public void TestCustomsOfficeOfDeparture() => AssertEquals("XI000081", parsingXMLProvider.CustomsOfficeOfDeparture);

		[TestDate(2023, 08, 02)]
		public void TestDiscrepanciesNotificationDate() => AssertEquals(ZDateTime.Today.ToDateTime(), parsingXMLProvider.DiscrepanciesNotificationDate);

		public void TestDiscrepanciesNotificationText() => AssertEquals("unsatisfactory", parsingXMLProvider.DiscrepanciesNotificationText);

		public void TestHolderOfTheTransitProcedure() => AssertEquals("XI175521246821", parsingXMLProvider.HolderOfTheTransitProcedure);

		public void TestGuarantorIdentificationNumber() => AssertEquals("G1", parsingXMLProvider.GuarantorIdentificationNumber);

		public void TestGuarantorName() => AssertEquals(ZString.Empty, parsingXMLProvider.GuarantorName);

		public void TestGuarantorAddress()
		{
			NCTSPrettierIncidentLocationAddressData values = new(country: "XI", postCode: "4321", streetAndNumber: "1 LOW ST");
			var expectedAddress = values;
			AssertEquals(expectedAddress, parsingXMLProvider.GuarantorAddress);
		}

		public void TestMakeInboundPrettyForInterpretation()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			message.EM_MessageText = "Not a NCTS 5 Message";
			prettier = new NCTS019ResponsePrettyFormatter(message);
			AssertEquals(message.HumanReadableMessage, prettier.MakeInboundPrettyForInterpretation(header));
		}

		public void TestMakeInboundPrettyForPhase5Interpretation()
		{
			CreateZZRefTestValuesIfNeeded("XI000081", Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, ["DES"], "NI Auth Consignor/nees");
			AssertMultilineASCIIEquals("NCTS 019", expectedHtml, prettier.MakeInboundPrettyForInterpretation(header));
		}

		public static void CreateZZRefTestValuesIfNeeded(ZString officeCode, ZString dataGroupingCode, ZString[] officePurpose, ZString desc)
		{
			var factory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);

			var eunzzz = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunzzz);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, desc, officePurpose);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			factory.Save();
		}
		protected override void SetUp()
		{
			base.SetUp();

			var retriever = new EmbeddedResourceRetriever();
			var testMessageContent = retriever.GetString(TestFilePath);

			message = Factory.New<EDIMessage>();
			message.EM_MessageText = testMessageContent;
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			parsingXMLProvider = new NCTS019EdiMessageParsingXMLPrettyDataProvider(message);
			prettier = new NCTS019ResponsePrettyFormatter(message);
		}

		EDIMessage message;
		NCTS019EdiMessageParsingXMLPrettyDataProvider parsingXMLProvider;
		NCTS019ResponsePrettyFormatter prettier;
		NctsHeader header;
		public const string TestFilePath = "Enterprise.Customs.EU.NCTS.Business.Testing.Ncts.EdiMessage.Prettier.TestFiles.Incoming.NCTS019TestMessage.xml";
		readonly string expectedHtml = @"<h2>Discrepancies at Destination</h2><table><tr><td>MRN:</td><td>23XI000081RN3DBHJ0.1</td></tr><tr><td>Customs Office of Departure:</td><td>XI000081 NI Auth Consignor/nees</td></tr></table><h3>Notification</h3><table><tr><td>Notification Text:</td><td>unsatisfactory</td></tr><tr><td>Notification Date:</td><td>02-08-2023</td></tr></table><h4>Guarantor</h4><table><tr><td>Guarantor Identification Number:</td><td>G1</td></tr><tr><td>Guarantor Address:</td><td>1 LOW ST 4321 </td></tr></table><h4>Holder Of The Transit Procedure</h4><table><tr><td>Holder Of The Transit Procedure Identification Number:</td><td>XI175521246821</td></tr></table>";
	}
}
