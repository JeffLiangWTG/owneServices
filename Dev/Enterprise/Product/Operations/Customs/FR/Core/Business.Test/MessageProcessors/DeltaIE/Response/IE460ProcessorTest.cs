using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE460;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IE460ProcessorTest : DeltaIEBaseProcessorTest<CC460BType, IE460Processor>
	{
		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE460ResponseMessage.json");

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE460ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE460ResponseMessageWithoutLRN.json");

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.UnderControlNotification;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK; // to be improved

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 2;

		protected override ZString GetExpectedMessageInterpretation() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>Status: </strong>STA<br><strong>Status Date: </strong>2023-04-19T23:28:57<br><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>MRN: </strong>MRN099999999<br><strong>Notification Date: </strong>2021-05-01T12:34:56Z<br><strong>Notification Type: </strong>1 - Demande de documents supplémentaires<br><strong>Anticipated Control Date: </strong>2021-05-02T12:34:56Z<br><strong>Text: </strong>Text</p><strong><p style=\"font-size: 120%\">Types Of Control</p></strong><p style=\"font-size: 120%\"><strong>Sequence : </strong>1<br><strong>Type: </strong>43<br><strong>Description: </strong>Quality control / Partial or total<br><strong>Remarks: </strong>The Declaration is under Customs control.1</p><p style=\"font-size: 120%\"><strong>Sequence : </strong>2<br><strong>Type: </strong>40<br><strong>Description: </strong>Physical controls<br><strong>Remarks: </strong>The Declaration is under Customs control.2</p><p style=\"font-size: 120%\"><strong>Sequence : </strong>3<br><strong>Type: </strong>99<br><strong>Remarks: </strong>The Declaration is under Customs control.3</p><strong><p style=\"font-size: 120%\">Requested Documents</p></strong><p style=\"font-size: 120%\"><strong>Sequence: </strong>1<br><strong>Type: </strong>N001<br><strong>Reference Number: </strong>Reference001<br><strong>Description: </strong>Descriptipon Text1</p><p style=\"font-size: 120%\"><strong>Sequence: </strong>2<br><strong>Type: </strong>N002<br><strong>Reference Number: </strong>Reference002<br><strong>Description: </strong>Descriptipon Text2</p>");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>Status: </strong>STA<br><strong>Status Date: </strong>2023-04-19T23:28:57<br><strong>MRN: </strong>MRN099999999<br><strong>Notification Date: </strong>2021-05-01T12:34:56Z<br><strong>Notification Type: </strong>1 - Demande de documents supplémentaires<br><strong>Anticipated Control Date: </strong>2021-05-02T12:34:56Z<br><strong>Text: </strong>Text</p><strong><p style=\"font-size: 120%\">Types Of Control</p></strong><p style=\"font-size: 120%\"><strong>Sequence : </strong>1<br><strong>Type: </strong>43<br><strong>Description: </strong>Quality control / Partial or total<br><strong>Remarks: </strong>The Declaration is under Customs control.1</p><p style=\"font-size: 120%\"><strong>Sequence : </strong>2<br><strong>Type: </strong>40<br><strong>Description: </strong>Physical controls<br><strong>Remarks: </strong>The Declaration is under Customs control.2</p><strong><p style=\"font-size: 120%\">Requested Documents</p></strong><p style=\"font-size: 120%\"><strong>Sequence: </strong>1<br><strong>Type: </strong>N001<br><strong>Reference Number: </strong>Reference001<br><strong>Description: </strong>Descriptipon Text1</p><p style=\"font-size: 120%\"><strong>Sequence: </strong>2<br><strong>Type: </strong>N002<br><strong>Reference Number: </strong>Reference002<br><strong>Description: </strong>Descriptipon Text2</p>");

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => "CRN099999999";

		protected override ZString GetExpectedMRN() => "MRN099999999";

		protected override ZString GetMessageTextForFees() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.UnderControl;

		protected override ZString GetExpectedCESLogInfo() => "CES 2023-04-19 23:28:57";

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NotificationType, "CL384");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NotificationType, "1", "Demande de documents supplémentaires", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "CL716");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "40", "Physical controls", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "43", "Quality control / Partial or total", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE460ResponseMessageForMissingField.json");
	}
}
