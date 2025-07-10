using System;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GVMSEDIMessagePrettierTest : TestCaseWithFactory
	{
		[TestTimeZoneUNLOCO("IEGWY")]
		public void TestMakeHumanReadableSuccess()
		{
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
			var message = Factory.New<GVMSEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = @"{
			  ""messageId"": ""e164eb3a-8492-4f4b-8763-65de21cec0b5"",
			  ""gmrId"": ""GMRCI0MK3YL6"",
			  ""gmrStatusVersion"": 2,
			  ""createdDateTime"": ""2021-03-03T13:29:19.408Z"",
			  ""updatedDateTime"": ""2021-04-03T13:29:19.408Z"",
			  ""gmrState"": ""OPEN"",
			  ""reportToLocations"": [{
				  ""inspectionTypeId"": ""1"",
				  ""locationIds"": [
					""L0029A"", ""L0030A"", ""L0031A""
				  ]
				}, {
				  ""inspectionTypeId"": ""2"",
				  ""locationIds"": [
					""L0029A"", ""L0031A""
				  ]
				}
			  ]
			}
			";
			ZDateTime testLocalCreatedTime = Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 03, 03, 13, 29, 19, DateTimeKind.Utc));
			ZDateTime testLocalUpdatedTime = Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 04, 03, 13, 29, 19, DateTimeKind.Utc));
			Factory.Save();
			ZString prettyHtml = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" +
				"<H3>GVMS Response</H3><p>" +
				"<strong>GMR ID:</strong>GMRCI0MK3YL6<br/>" +
				"<strong>Status:</strong>OPEN - Open and awaiting processing<br/>" +
				"<strong>Created Date/Time:</strong>" + testLocalCreatedTime + "<br/>" +
				"<strong>Updated Date/Time:</strong>" + testLocalUpdatedTime + "<br/>" +
				"<strong>Version:</strong>2<br/>" +
				"<strong>Inspection(s) Details:</strong>" +
				"<ul style=\"margin-top: 3px;\">" +
				"<li>By CUSTOMS at Sevington (1, L0029A)</li>" +
				"<li>By CUSTOMS at Stop 24 (1, L0030A)</li>" +
				"<li>By CUSTOMS at Dover Western Docks (1, L0031A)</li>" +
				"<li>By DEFRA at Sevington (2, L0029A)</li>" +
				"<li>By DEFRA at Dover Western Docks (2, L0031A)</li>" +
				"</ul>";
			AssertEquals(prettyHtml, new GVMSEDIMessagePrettier(message).MakeHumanReadable());
		}

		public void TestMakeHumanReadableFailures()
		{
			var message = Factory.New<GVMSEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = @"{
			  ""messageId"": ""90b6abd2-f7f9-4223-97c0-29f0c10bbf8e"",
			  ""gmrId"": ""GMRA000002FK"",
			  ""gmrStatusVersion"": 2,
			  ""createdDateTime"": ""2021-07-11T10:58:12.384Z"",
			  ""updatedDateTime"": ""2021-07-11T10:58:12.384Z"",
			  ""gmrState"": ""NOT_FINALISABLE"",
			  ""ruleFailures"": [
				{
				  ""code"": ""007"",
				  ""technicalMessage"": ""Safety and security MRN not found"",
				  ""field"": ""$.customsDeclarations[3].sAndSMasterRefNum"",
				  ""value"": ""83736521""
				},
				{
				  ""code"": ""008"",
				  ""technicalMessage"": ""Transit declaration not found"",
				  ""field"": ""$.transitDeclarations[1].transitDeclarationId"",
				  ""value"": ""384716253""
				}]
			}
			";
			Factory.Save();
			ZDateTime testLocalTime = Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 07, 11, 10, 58, 12, DateTimeKind.Utc));
			ZString prettyHtml = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" +
				"<H3>GVMS Response</H3><p>" +
				"<strong>GMR ID:</strong>GMRA000002FK<br/>" +
				"<strong>Status:</strong>NOT_FINALISABLE - Not finalisable, requires amendment<br/>" +
				"<strong>Created Date/Time:</strong>" + testLocalTime + "<br/>" +
				"<strong>Updated Date/Time:</strong>" + testLocalTime + "<br/><strong>Version:</strong>2<br/>" +
				"<p><b>Rejection Details:</b><br/>" +
				"<strong>Description:</strong>Safety and security MRN not found<br/>" +
				"<strong>Field Name:</strong>customsDeclarations[3].sAndSMasterRefNum<br/>" +
				"<strong>Value:</strong>83736521<p><strong>Description:</strong>Transit declaration not found<br/>" +
				"<strong>Field Name:</strong>transitDeclarations[1].transitDeclarationId<br/>" +
				"<strong>Value:</strong>384716253<p>";
			AssertEquals(prettyHtml, new GVMSEDIMessagePrettier(message).MakeHumanReadable());
		}

		public void TestMakeHumanReadableFailuresNoValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ErrorCode, "ErrorCode");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom,
				RefCusCodeListTypes.Codes.ErrorCode,
				"007",
				"Safety and security MRN not found",
				ZDateTime.Today,
				ZDateTime.Today.AddDays(1),
				RefCusCodeListAttributeTypes.Codes.Category,
				"GVMS");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom,
				RefCusCodeListTypes.Codes.ErrorCode,
				"008",
				"008 from DB",
				ZDateTime.Today,
				ZDateTime.Today.AddDays(1),
				RefCusCodeListAttributeTypes.Codes.Category,
				"GVMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.ErrorCode, "008", "008 from DB", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();

			var message = Factory.New<GVMSEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = @"{
			  ""messageId"": ""90b6abd2-f7f9-4223-97c0-29f0c10bbf8e"",
			  ""gmrId"": ""GMRA000002FK"",
			  ""gmrStatusVersion"": 2,
			  ""createdDateTime"": ""2021-07-11T10:58:12.384Z"",
			  ""updatedDateTime"": ""2021-07-11T10:58:12.384Z"",
			  ""gmrState"": ""NOT_FINALISABLE"",
			  ""ruleFailures"": [
				{
				  ""code"": ""007"",
				  ""technicalMessage"": ""Safety and security MRN not found"",
				  ""field"": ""$.customsDeclarations[3].sAndSMasterRefNum"",
				  ""value"": ""83736521""
				},
				{
				  ""code"": ""008"",
				  ""technicalMessage"": ""Transit declaration not found"",
				  ""field"": ""$.transitDeclarations[1].transitDeclarationId""
				}]
			}
			";
			Factory.Save();
			ZDateTime testLocalTime = Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 07, 11, 10, 58, 12, DateTimeKind.Utc));

			ZString prettyHtml = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" +
				"<H3>GVMS Response</H3><p>" +
				"<strong>GMR ID:</strong>GMRA000002FK<br/>" +
				"<strong>Status:</strong>NOT_FINALISABLE - Not finalisable, requires amendment<br/>" +
				"<strong>Created Date/Time:</strong>" + testLocalTime + "<br/>" +
				"<strong>Updated Date/Time:</strong>" + testLocalTime + "<br/><strong>Version:</strong>2<br/>" +
				"<p><b>Rejection Details:</b><br/>" +
				"<strong>Description:</strong>Safety and security MRN not found<br/>" +
				"<strong>Field Name:</strong>customsDeclarations[3].sAndSMasterRefNum<br/>" +
				"<strong>Value:</strong>83736521<p><strong>Description:</strong>Transit declaration not found (008 from DB)<br/>" +
				"<strong>Field Name:</strong>transitDeclarations[1].transitDeclarationId<br/>" +
				"<p>";
			AssertEquals(prettyHtml, new GVMSEDIMessagePrettier(message).MakeHumanReadable());
		}

		public void TestGetCIDMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN0000001";

			var ehubId = new ZGuid("D171BC67-29C8-4B67-B8DE-A44D686FD3FB");
			var originalMessage = Factory.New<GVMSEDIMessage>();
			originalMessage.EM_MessageSubType = "CAN";
			originalMessage.EM_MessageText = @"{ ""direction"": ""UK_INBOUND"", ""isUnaccompanied"": false, ""plannedCrossing"": { ""routeId"": ""6"", ""localDateTimeOfDeparture"": ""2021-11-06T12:00"" } }";
			originalMessage.EM_MessageNum = "9";
			originalMessage.EM_SystemCreateTimeUtc = new ZDateTime(2021, 11, 15, 13, 14, 15);
			originalMessage.EM_LinkedObject = header;

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_SessionGUID = ehubId;
			outgoingInterchange.ContainedMessages.Add(originalMessage);

			var message = Factory.New<GVMSEDIMessage>();
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = GVMS.Constants.GVMSMessageSubTypes.NOTIFICATIONMESSAGEID;
			message.EM_ApplicationReference = "aaaaaaaabbbbccccddddeeeeeeeeeeee";
			message.EM_LinkedObject = header;

			var xmlEvent = CreateEvent(validXmlEvent, ehubId);

			var html = GVMSEDIMessagePrettier.GetCIDMessage(ehubId, originalMessage, message, "eHub", xmlEvent.EventTime.GetValueOrDefault().ToUtcZDateTime());

			CombineAssertions(() =>
			{
				AssertEquals("EQUALS", expectedCIDInterpretation, html);
				AssertContains("CONTAINS", expectedCIDInterpretation, html);
			});
		}

		UniversalEvent CreateEvent(ZString xmlMessage, ZGuid ehubId)
		{
			var eventDeserializer = new UniversalDataBuss.Management.EventProcessing.XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, xmlMessage, ehubId));

			return xmlEvent as UniversalEvent;
		}

		const string validXmlEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	  <Event>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>NctsHeader</Type>
			  <Key>MAN0000093</Key>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<EventTime>2021-11-15T01:14:14+11:00</EventTime>
		<EventType>MSN</EventType>
		<DataContext>
		  <DataSource>
			<DataProvider>GVMS</DataProvider>
		  </DataSource>
		</DataContext>
		<ContextCollection>
		  <Context>
			<Type>eHubTrackingID</Type>
			<Value>{0}</Value>
		  </Context>
		  <Context>
			<Type>NotificationMessageId</Type>
			<Value>40305125-1b5c-4162-bd8f-9bcd8d9572cd</Value>
		  </Context>
		</ContextCollection>
	  </Event>
	</UniversalEvent>";

		const string expectedCIDInterpretation = @"<style>
body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}
tr {
  height: 50px;
  vertical-align: center;
}
.Status {
padding-left: 40px;
padding-right: 10px;
}
.StepProgress-item {
  margin-top: 15px;
}
.StepProgress-item{
  content: '';
  width: 12px;
  height: 12px;
}
.StepProgress-item.is-done {
  font-size: 16px;
  color: green;
  text-align: center;
  font-weight: bold;
}
.StepProgress-item.current{
  font-size: 12px;
  text-align: center;
  color: grey;
}
.StepProgress-item.rejected {
  font-size: 16px;
  color: red;
  text-align: center;
  font-weight: bold;
}
</style><p><h4>Message 9 was uploaded to GVMS and received Tracking ID aaaaaaaabbbbccccddddeeeeeeeeeeee</h4></p><table>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Message created</td>
	<td><small>Message number = 9 at 15/11/2021 13:14 (UTC)</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to eHub</td>
	<td><small>eHub Tracking ID = d171bc67-29c8-4b67-b8de-a44d686fd3fb</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to GVMS</td>
	<td><small>Tracking ID = aaaaaaaabbbbccccddddeeeeeeeeeeee</small></td>
</tr>
</table><p/><table>
<tr style=""height: 20px;"">
<td>Status update time:</td>
<td><small>14/11/2021 14:14 (UTC)</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>MAN0000001</small></td>
</tr>
</table>";
	}
}
