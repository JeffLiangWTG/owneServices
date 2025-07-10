using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(EDIReleaseMessage))]
	public class EDIReleaseMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRNSStatusAndNoticesProperties()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+10207000007531+11'
LOC+22+0497:129::4570'
DTM+9:201411232205:203'
GIS+34'
RFF+XC:803629102014'
UNT+7+1'
".Replace("\r\n", "");
			AssertEquals("ProcessingIndicator", "WTP - Declaration Accepted, Awaiting Customs Processing", ((EDIReleaseMessage)message).ProcessingIndicator);
			AssertEquals("ServiceOption", "125 - Pre-arrival EDI Release", ((EDIReleaseMessage)message).ServiceOption);
			AssertEquals("DocumentReference", "10207000007531", ((EDIReleaseMessage)message).DocumentReference);
			AssertEquals("ContainerNumbers", ZString.Empty, ((EDIReleaseMessage)message).ContainerNumbers);
		}

		public void TestErrorDescription()
		{
			var ediReleaseMessage = message as EDIReleaseMessage;
			AssertNotNull(ediReleaseMessage);
			AssertEquals("CCN not on file", ediReleaseMessage.GetErrorDescription("01"));
			AssertEquals("Work Location Code (Destination): INVALID OFFICE CODE (NOT A VALID OFFICE)", ediReleaseMessage.GetErrorDescription("06"));
			AssertEquals("Work Location Code (Destination): ELEMENT VALUE IS BELOW SIZE OF FIELD", ediReleaseMessage.GetErrorDescription("07"));
			AssertEquals("EDIFACT conformance (syntax) check error", ediReleaseMessage.GetErrorDescription("08"));
			AssertEquals("Work Location Code (Destination): ARRIVAL OFFICE DOES NOT MATCH RELEASE", ediReleaseMessage.GetErrorDescription("09"));
			AssertEquals("Work Location Code (Destination): ARRIVALS ARE NOT ALLOWED FOR IN TRANSIT CARGO", ediReleaseMessage.GetErrorDescription("16"));
			AssertEquals("Client Supplied Request ID: EDI arrival not accepted - the cargo or House Bill is rejected", ediReleaseMessage.GetErrorDescription("28"));
			AssertEquals("Client Supplied Request ID: Request already in arrived status", ediReleaseMessage.GetErrorDescription("34"));
			AssertEquals("Warehouse Office: Warehouse is mandatory", ediReleaseMessage.GetErrorDescription("30"));
		}

		public void TestCUSDECProperties()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = @"UNH+154+CUSDEC:D:96A:UN'
BGM++10207000000179+4'
CST++125:105+1:117+842957342RM0001:58'
LOC+22+0495+4570'
DTM+232:201009300000:203'
MEA+WT+AAD+KGM:50'
MEA+WT+AAC+KGM:45'
RFF+CN:9463TKWB1438BB'
PAC+8195++:::PKG'
NAD+IM+++ITO EN (NORTH AMERICA) INC.+45 MAIN STREET+BROOKYLN+NY+11201+US'
NAD+AE+++LEON BALL'
MOA+39:84361'
UNS+D'
UNS+S'
UNT+53+154'".Replace("\r\n", "");
			AssertEquals("Sub-Location", "4570", ((EDIReleaseMessage)message).SubLocation);
			AssertEquals("CBSA Office", "0495", ((EDIReleaseMessage)message).CBSAOffice);
		}

		public void TestCUSDECPropertiesNoSubLocation()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = @"UNH+154+CUSDEC:D:96A:UN'
BGM++10207000000179+4'
CST++125:105+1:117+842957342RM0001:58'
LOC+22+0495'
DTM+232:201009300000:203'
MEA+WT+AAD+KGM:50'
MEA+WT+AAC+KGM:45'
RFF+CN:9463TKWB1438BB'
PAC+8195++:::PKG'
NAD+IM+++ITO EN (NORTH AMERICA) INC.+45 MAIN STREET+BROOKYLN+NY+11201+US'
NAD+AE+++LEON BALL'
MOA+39:84361'
UNS+D'
UNS+S'
UNT+53+154'".Replace("\r\n", "");
			AssertEquals("Sub-Location", ZString.Empty, ((EDIReleaseMessage)message).SubLocation);
		}

		public void TestCUSRESProperties()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+14'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");
			AssertEquals("Sub-location", "3072", ((EDIReleaseMessage)message).SubLocation);
			AssertEquals("CBSA Office", "0497", ((EDIReleaseMessage)message).CBSAOffice);
		}

		public void TestCUSRESPropertiesWithMinimalMessage()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257++11'
LOC+22+0497'
DTM+58:201011250820:203'
GIS+14'
RFF+TN:37132536987'
UNT+7+1'".Replace("\r\n", "");
			AssertEquals("Sub-location", ZString.Empty, ((EDIReleaseMessage)message).SubLocation);
		}

		public void TestMessageSubTypeDescription()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsRequiredForExamination;
			AssertEquals("EM_MessageSubTypeDescription", EDIReleaseImportEntryStatusList.Descriptions.GoodsRequiredForExamination, message.EM_MessageSubTypeDescription);

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			AssertEquals("EM_MessageSubTypeDescription", MessageSubTypeCodes.Descriptions.Original, message.EM_MessageSubTypeDescription);
		}

		public override void TestCloneAuditProperties()
		{
			Assert("EM_SystemCreateTimeUtc cloning required by EDIReleaseResponseMessageProcessor", true);
		}

		public void TestSystemDefinedValues()
		{
			var testMessage = Factory.New<EDIReleaseMessage>();
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			testMessage.RNSReleaseDate = new ZDateTime(2011, 9, 1);
			testMessage.RNSProcessingDate = new ZDateTime(2011, 9, 2);
			testMessage.SetSystemDefinedValue(Business.EDIMessage.Schema.TransactionNumber, new ZString("10207000000179"));
			testMessage.SetSystemDefinedValue(ReleaseStatus.Schema.RL_ReleaseOffice, new ZString("0497"));
			testMessage.SetSystemDefinedValue(ReleaseStatus.Schema.RL_WarehouseCode, new ZString("4570"));

			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var messageInNewFactory = factory2.Load<EDIReleaseMessage>(testMessage.PK);
			AssertEquals("RNSReleaseDate", new ZDateTime(2011, 9, 1), messageInNewFactory.RNSReleaseDate);
			AssertEquals("RNSProcessingDate", new ZDateTime(2011, 9, 2), messageInNewFactory.RNSProcessingDate);
			AssertEquals("RNSProcessingDate", "10207000000179", messageInNewFactory.TransactionNumber);
			AssertEquals("WarehouseCode", "4570", messageInNewFactory.WarehouseCode);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (EDIReleaseMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<EDIReleaseMessage>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals(EDIMessage.ApplicationCodes.CAIMP, message.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.EDIRelease, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		[UseSnapshotProtection]
		public void TestMessageNumberFilledIn()
		{
			Db.Connection.BeginTransaction();
			try
			{
				Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAIMP).SetNext(Factory, 12348);
			}
			finally
			{
				Db.Connection.CommitTransaction();
			}
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = 12348", message.EM_MessageText);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R06", "Work Location Code (Destination): INVALID OFFICE CODE (NOT A VALID OFFICE)", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R07", "Work Location Code (Destination): ELEMENT VALUE IS BELOW SIZE OF FIELD", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R08", "EDIFACT conformance (syntax) check error", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R09", "Work Location Code (Destination): ARRIVAL OFFICE DOES NOT MATCH RELEASE", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R16", "Work Location Code (Destination): ARRIVALS ARE NOT ALLOWED FOR IN TRANSIT CARGO", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R28", "Client Supplied Request ID: EDI arrival not accepted - the cargo or House Bill is rejected", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "W34", "Client Supplied Request ID: Request already in arrived status", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R30", "Warehouse Office: Warehouse is mandatory", startDate, endDate);
			Factory.Save();

			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}
		EDIMessage message;

		public static string GetEntryStatusByProcessingIndicatorCoded(string processingIndicator)
		{
			return EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(processingIndicator);
		}
	}
}
