using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC228C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC228CProcessor))]
	class CC228CProcessorTest : NCTSGuaranteeMessageProcessorAbstractTest<CC228CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC228CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE228;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC228CText(grn1, grn2, invalidDate, identificationNumber);

		protected override ZString MessageFriendlyName => "CC228C: COMPREHENSIVE GUARANTEE CANCELLATION LIABILITY LIBERATION";

		protected override CC228CProcessor Processor => new CC228CProcessor(logger, typeof(Cc228CType));

		[TestDate(2023, 06, 28)]
		public void TestUpdateMessageAttachee()
		{
			var permitHolder = Factory.New<OrgHeader>();
			permitHolder.OH_Code = "TEST";
			permitHolder.OH_FullName = "TEST";
			var cusCode = permitHolder.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "REG001";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_OH = permitHolder.PK;

			var guarantee1 = CreateCusGuaranteeHeader(permitHolder.PK, CountryCodes.Ireland, grn1, ZDate.BrettsBirthday, ZDate.BrettsBirthday);
			var guarantee2 = CreateCusGuaranteeHeader(permitHolder.PK, CountryCodes.UnitedKingdom, grn2, ZDate.BrettsBirthday, ZDate.BrettsBirthday);
			var guarantee3 = CreateCusGuaranteeHeader(permitHolder.PK, CountryCodes.Ireland, "other_grn", ZDate.BrettsBirthday, ZDate.BrettsBirthday);
			var guarantee4 = CreateCusGuaranteeHeader(permitHolder.PK, CountryCodes.Latvia, grn1, ZDate.BrettsBirthday, ZDate.BrettsBirthday);

			var testData = CreateSetupData();
			testData.outgoingMessage.EM_SystemCreateUser = "ST1";
			var incomingMessage = testData.incomingMessage;
			incomingMessage.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE228;
			var cc228Text = InterchangeProcessorTestHelper.GetStandardCC228CText(grn1, grn2, invalidDate, identificationNumber);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc228Text, includeResponseWrap: false);
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_GB = Branch.PK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Ireland - Referenced GRN", ZDate.Today, guarantee1.CPH_EndDate);
				AssertEquals("UK - Referenced GRN", ZDate.Today, guarantee2.CPH_EndDate);
				AssertEquals("Ireland - Not Referenced GRN", ZDate.BrettsBirthday, guarantee3.CPH_EndDate);
				AssertEquals("Latvia - Referenced GRN", ZDate.BrettsBirthday, guarantee4.CPH_EndDate);
			});
		}

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, @"A Comprehensive Guarantee Cancellation Liability Liberation Message (IE228) message has been received.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
</table>
<br />
<br />
Guarantee Information:<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<tr>
<td>
Guarantor’s. identification number</td>
<td>
ID001</td>
</tr>
<tr>
<td>
Guarantor’s. Name</td>
<td>
Bond</td>
</tr>
<tr>
<td>
Guarantor’s. Address </td>
<td>
MI5 London GB</td>
</tr>
<tr>
<td>
GRN</td>
<td>
12GRNCC055C012345A678901</td>
</tr>
<tr>
<td>
Currency</td>
<td>
EUR</td>
</tr>
<tr>
<td>
Guarantee amount</td>
<td>
123.45</td>
</tr>
<tr>
<td>
Invalidity date</td>
<td>
28-Jun-23</td>
</tr>
<tr>
<td>
Liability liberation date </td>
<td>
&nbsp;</td>
</tr>
<tr>
<td>
Customs Office Of Guarantee</td>
<td>
RNCC229A</td>
</tr>
</table>
<br />
<br />
Guarantee Information:<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<tr>
<td>
Guarantor’s. identification number</td>
<td>
ID001</td>
</tr>
<tr>
<td>
Guarantor’s. Name</td>
<td>
Bond</td>
</tr>
<tr>
<td>
Guarantor’s. Address </td>
<td>
MI5 London GB</td>
</tr>
<tr>
<td>
GRN</td>
<td>
12GRNCC055C012345A678902</td>
</tr>
<tr>
<td>
Currency</td>
<td>
EUR</td>
</tr>
<tr>
<td>
Guarantee amount</td>
<td>
123.45</td>
</tr>
<tr>
<td>
Invalidity date</td>
<td>
28-Jun-23</td>
</tr>
<tr>
<td>
Liability liberation date </td>
<td>
&nbsp;</td>
</tr>
<tr>
<td>
Customs Office Of Guarantee</td>
<td>
RNCC229B</td>
</tr>
</table>
");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Comprehensive Guarantee Cancellation Liability Liberation Message (IE228) message has been received." },
				new string[] { "staff1@where.com" });
		}

		CusGuaranteeHeader CreateCusGuaranteeHeader(ZGuid permitHolder, ZString countryCode, ZString grn, ZDate startDate, ZDate endDate)
		{
			var header = Factory.New<CusGuaranteeHeader>();
			header.CPH_RN_NKCountryCode = countryCode;
			header.CPH_Number = grn;
			header.CPH_StartDate = startDate;
			header.CPH_EndDate = endDate;
			header.CPH_IsActive = true;
			header.CPH_Type = "NCT";
			header.CPH_OH_PermitHolder = permitHolder;
			return header;
		}

		const string identificationNumber = "ID001";
		const string grn1 = "12GRNCC055C012345A678901";
		const string grn2 = "12GRNCC055C012345A678902";
		readonly DateTime invalidDate = new DateTime(2023, 06, 28);
	}
}
