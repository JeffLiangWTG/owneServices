using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLResponseEDIMessagePrettier))]
sealed class NLResponseEDIMessagePrettierTest : MessageInterpreterTestCase<NLResponseEDIMessagePrettier, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>MRN:</b></td><td><i>MRN123</i></td></tr>");
			sb.Append("<tr><td><b>Functional Reference ID:</b></td><td><i>TestReferenceABC</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>Enquiry information code</i></td></tr>");
			sb.Append("<tr><td><b>Control Remarks:</b></td><td><i>Control Result DESC</i></td></tr>");
			sb.Append("<tr><td><b>Control Remarks:</b></td><td><i>Statement DESC</i></td></tr>");
			sb.Append("<tr><td><b>Control Remarks:</b></td><td><i>Statement DESC2</i></td></tr>");
			sb.Append("<tr><td><b><li>Control Date:</b></td><td><i>20220112</i></td></tr>");
			sb.Append("<tr><td style='padding-left:1em;'><b>Applies to Entry Line:</b></td><td><i>1</i></td></tr>");
			sb.Append("<tr><td style='padding-left:1em;'><b>Control Type:</b></td><td><i>45: 345 DESC</i></td></tr>");
			sb.Append("<tr><td style='padding-left:1em;'><b>Control Statement:</b></td><td><i>Additional Info Statement DESC</i></td></tr>");
			sb.Append("<tr><td><b></b></td><td><i>&nbsp;</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs status");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "345", "345 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "456", "456 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var additionalInformation = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(ResponseStatementTypes.Codes.EnquiryInformationCode, "Statement DESC").Object;
		var additionalInformation2 = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(ResponseStatementTypes.Codes.ExaminationResultComment, "Statement DESC2").Object;
		DataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation, additionalInformation2 });
		var control = DMSResponseMessageTestHelper.MockResponseControl(controlResultDescription: "Control Result DESC").Object;
		DataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control });

		var controlResult = DMSResponseMessageTestHelper.MockResponseControlResult(goodsItemNumeric: 1);
		var controlResultControl = DMSResponseMessageTestHelper.MockResponseControlResultControl(new DateTime(2022, 01, 12, 14, 34, 32), "45", "Additional Info Statement DESC").Object;
		controlResult.Setup(x => x.Controls).Returns(new IDMSControl[] { controlResultControl });
		DataProviderMock.Setup(x => x.ControlResults).Returns(new IDMSControlResult[] { controlResult.Object });

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN123";
		MessageMock.Object.EM_LinkedObject = entryHeader;
	}
}
