using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FinalPriceExtensionRequestNewForm))]
	sealed class FinalPriceExtensionRequestNewFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new FinalPriceExtensionRequestNewForm(new FinalPriceReportByDateExtensionHeader(Factory)))
			{
				AssertEquals("(Imp) Final Price Period Extension Application", form.FormCaption);
			}
		}

		[TestDate(2023, 12, 26)]
		public void TestSendButtonClick()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "033", "양산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var header = new FinalPriceReportByDateExtensionHeader(Factory);
			var line = header.FinalPriceReportByDateExtensionLines.AddNew();
			CusMiscRequestHeaderCollection cusMiscRequestHeaders = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			AssertEquals("CusMiscRequestHeader records count is zero.", 0, cusMiscRequestHeaders.Count);

			using (var form = new FinalPriceExtensionRequestNewForm(header))
			{
				form.Show();

				header.CustomsOffice = "030";
				header.GB_Branch = GlbBranch.CurrentBranch.PK;
				line.ImportDeclarationNumber = "XXXXXXXXXXXXXXX";
				line.ExtensionDate = new DateTime(2023, 12, 27);
				line.ApplicationReason = "테스트 중입니다.";

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				form.AcceptButton.PerformClick();
				AssertEquals(1, cusMiscRequestHeaders.Count);

				var cusMiscRequestHeader = (CusMiscRequestHeader)cusMiscRequestHeaders[0];
				AssertEquals(ElectronicDocumentTypeList.Codes._5SG, cusMiscRequestHeader.CMR_MessageType);
				AssertEquals(header.CustomsOffice, cusMiscRequestHeader.CMR_CustomsOffice);
				AssertEquals(header.GB_Branch, cusMiscRequestHeader.CMR_GB);
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, cusMiscRequestHeader.CMR_Status);

				var cusMiscRequestLine = cusMiscRequestHeader.RequestLines[0];
				AssertEquals(line.ImportDeclarationNumber, cusMiscRequestLine.CML_EntryNumber);

				AssertEquals("(2023-12-27) 테스트 중입니다.", cusMiscRequestLine.CML_Remarks);
				AssertContains(line.ExtensionDate.ToString(DateFormatType.DateKorean), cusMiscRequestLine.CML_Remarks);
				AssertContains(line.ApplicationReason, cusMiscRequestLine.CML_Remarks);

				AssertEquals(1, cusMiscRequestHeader.Messages.Count);
				var message = cusMiscRequestHeader.Messages[0];
				AssertEquals("KRC", message.EM_ApplicationCode);
				AssertEquals(ElectronicDocumentTypeList.Codes._5SG, message.EM_MessageType);
				AssertEquals("TRX", message.EM_ReceiveTransmit);
				AssertEquals("QUE", message.EM_Status);
				AssertEquals(GlbBranch.CurrentBranch.PK, message.EM_GB);
				AssertEquals(GlbDepartment.CurrentDepartment.PK, message.EM_GE);
				AssertEquals(CusMiscRequestHeader.Schema.TableName, message.EM_LinkTable);
				AssertEquals(cusMiscRequestHeader.PK, message.EM_LinkUniqueID);
			}
		}

		public void TestRelatedEntriesAtea()
		{
			var header = new FinalPriceReportByDateExtensionHeader(Factory);
			using (var form = new FinalPriceExtensionRequestNewForm(header))
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("FinalPriceExtensionRequestLinesBoundGrid");
				var index = 0;
				AssertEquals(grid.Columns[index++].ColumnName, FinalPriceReportByDateExtensionLine.Schema.ImportDeclarationNumber);
				AssertEquals(grid.Columns[index++].ColumnName, FinalPriceReportByDateExtensionLine.Schema.ExtensionDate);
				AssertEquals(grid.Columns[index++].ColumnName, FinalPriceReportByDateExtensionLine.Schema.ApplicationReason);
			}
		}

		public void TestImportDeclarationSearchButton()
		{
			var header = new FinalPriceReportByDateExtensionHeader(Factory);
			using (var form = new FinalPriceExtensionRequestNewForm(header))
			{
				form.Show();
				AssertNotNull(form.FindSingle<ZButton>("SearchButton"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new FinalPriceExtensionRequestNewForm(new FinalPriceReportByDateExtensionHeader(Factory));
			result.ControllerID = ControllerIDs.Customs.KR.MiscRequestMessages;
			return result;
		}
	}
}
