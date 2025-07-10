using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(RFIMessageInterpreter))]
sealed class RFIMessageInterpreterTest : MessageInterpreterTestCase<RFIMessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>MRN:</b></td><td><i>MRN123</i></td></tr>");
			sb.Append("<tr><td><b>Functional Reference ID:</b></td><td><i>TestReferenceRFI</i></td></tr>");
			sb.Append("<tr><td style='padding-left:1em;'><b>Control Date:</b></td><td><i>20220207</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Control Type:</b></td><td><i>10: Documents Control</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Applies to Entry Line:</b></td><td><i>6</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Control Statement:</b></td><td><i>Documents not complete</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Corrected Value:</b></td><td><i>Value</i></td></tr>");
			sb.Append("<tr><td style='padding-left:3em;'><b>Name Path:</b></td><td><i>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[6]/PreviousDocument</i></td></tr>");
			sb.Append("<tr><td style='padding-left:3em;'><b>Control Statement:</b></td><td><i>Provide all available documents</i></td></tr>");
			sb.Append("<tr><td><b></b></td><td><i>&nbsp;</i></td></tr>");
			sb.Append("<tr><td style='padding-left:1em;'><b>Control Date:</b></td><td><i>20220207</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Control Type:</b></td><td><i>10: Documents Control</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Applies to Entry Line:</b></td><td><i>7</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Control Statement:</b></td><td><i>Documents not complete</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Corrected Value:</b></td><td><i>Value2</i></td></tr>");
			sb.Append("<tr><td style='padding-left:3em;'><b>Name Path:</b></td><td><i>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[7]/PreviousDocument</i></td></tr>");
			sb.Append("<tr><td style='padding-left:3em;'><b>Control Statement:</b></td><td><i>Provide all available documents</i></td></tr>");
			sb.Append("<tr><td style='padding-left:2em;'><b>Corrected Value:</b></td><td><i>TEST</i></td></tr>");
			sb.Append("<tr><td style='padding-left:3em;'><b>Name Path:</b></td><td><i>Declaration/GoodsShipment/Consignment/Consignee</i></td></tr>");
			sb.Append("<tr><td style='padding-left:3em;'><b>Control Statement:</b></td><td><i>Provide phone number and e-mail address(es) of Consignee</i></td></tr>");
			sb.Append("<tr><td><b></b></td><td><i>&nbsp;</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		var controlResult = DMSResponseMessageTestHelper.MockResponseControlResult(goodsItemNumeric: 6);
		var controlResultControl = DMSResponseMessageTestHelper.MockResponseControlResultControl(new DateTime(2022, 02, 07), "10", "Documents not complete");
		var controlResultControlControlDetail = DMSResponseMessageTestHelper.MockResponseControlResultControlControlDetail
			("Value", "Declaration/GoodsShipment/GovernmentAgencyGoodsItem[6]/PreviousDocument", "Provide all available documents").Object;
		controlResultControl.Setup(x => x.ControlDetails).Returns(new IDMSControlDetail[] { controlResultControlControlDetail });
		controlResult.Setup(x => x.Controls).Returns(new IDMSControl[] { controlResultControl.Object });

		var controlResult2 = DMSResponseMessageTestHelper.MockResponseControlResult(goodsItemNumeric: 7);
		var controlResultControl2 = DMSResponseMessageTestHelper.MockResponseControlResultControl(new DateTime(2022, 02, 07), "10", "Documents not complete");
		var controlResultControlControlDetail2 = DMSResponseMessageTestHelper.MockResponseControlResultControlControlDetail
			("Value2", "Declaration/GoodsShipment/GovernmentAgencyGoodsItem[7]/PreviousDocument", "Provide all available documents").Object;
		var controlResultControlControlDetail3 = DMSResponseMessageTestHelper.MockResponseControlResultControlControlDetail
			("TEST", "Declaration/GoodsShipment/Consignment/Consignee", "Provide phone number and e-mail address(es) of Consignee").Object;
		controlResultControl2.Setup(x => x.ControlDetails).Returns(new IDMSControlDetail[] { controlResultControlControlDetail2, controlResultControlControlDetail3 });
		controlResult2.Setup(x => x.Controls).Returns(new IDMSControl[] { controlResultControl2.Object });
		DataProviderMock.Setup(x => x.ControlResults).Returns(new IDMSControlResult[] { controlResult.Object, controlResult2.Object });

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceRFI";
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN123";
		MessageMock.Object.EM_LinkedObject = entryHeader;
	}
}
