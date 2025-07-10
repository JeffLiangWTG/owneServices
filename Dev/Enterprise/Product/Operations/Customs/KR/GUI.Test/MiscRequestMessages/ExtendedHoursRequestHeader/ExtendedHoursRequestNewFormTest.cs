using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ExtendedHoursRequestNewForm))]
	sealed class ExtendedHoursRequestNewFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new ExtendedHoursRequestNewForm(new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK)))
			{
				AssertEquals("(Exp) Application for Extended Office Hours", form.FormCaption);
			}

			using (var form = new ExtendedHoursRequestNewForm(new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK)))
			{
				AssertEquals("(Imp) Application for Extended Office Hours", form.FormCaption);
			}
		}

		[TestDate(2022, 07, 21)]
		public void TestSendButtionWhen5AC()
		{
			AssertSendButton_Click(ElectronicDocumentTypeList.Codes._5AC);
		}

		[TestDate(2022, 07, 21)]
		public void TestSendButtionWhen5GW()
		{
			AssertSendButton_Click(ElectronicDocumentTypeList.Codes._5GW);
		}

		void AssertSendButton_Click(string messageType)
		{
			SetupCusCodeList();

			var header = new ExtendedHoursRequestHeader(Factory, messageType, GlbCompany.CurrentCompany.PK);
			var line = header.ExtendedHoursRequestLines.AddNew();
			CusMiscRequestHeaderCollection cusMiscRequestHeaders = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			AssertEquals("CusMiscRequestHeader records count is zero.", 0, cusMiscRequestHeaders.Count);

			using (var form = new ExtendedHoursRequestNewForm(header))
			{
				form.Show();
				SetDefaultValue(header, line);
				form.AcceptButton.PerformClick();

				AssertEquals(true, header.HasErrors);
				AssertEquals("If header has error, CusMiscRequestHeader is not created.", 0, cusMiscRequestHeaders.Count);
				KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");

				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				form.Show();
				header.Department = ZString.Empty;
				form.AcceptButton.PerformClick();
				AssertEquals(false, header.HasErrors);
				AssertEquals(true, header.HasMessageErrors);
				AssertEquals("If header has message error and has no security to send, CusMiscRequestHeader is created.", 0, cusMiscRequestHeaders.Count);

				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				form.AcceptButton.PerformClick();
				AssertEquals(false, header.HasErrors);
				AssertEquals(true, header.HasMessageErrorsNotIncludingChildren);
				AssertEquals(false, line.HasMessageErrors);
				AssertEquals(true, header.HasMessageErrors);
				AssertEquals("If header has message error and has security to send but select 'No', CusMiscRequestHeader is created.", 0, cusMiscRequestHeaders.Count);

				form.Show();
				SetDefaultValue(header, line);
				line.UQ = ZString.Empty;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				form.AcceptButton.PerformClick();
				AssertEquals(false, header.HasErrors);
				AssertEquals(false, header.HasMessageErrorsNotIncludingChildren);
				AssertEquals(true, line.HasMessageErrors);
				AssertEquals(true, header.HasMessageErrors);
				AssertEquals("If header has message error and has security to send but select 'No', CusMiscRequestHeader is created.", 0, cusMiscRequestHeaders.Count);

				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				form.AcceptButton.PerformClick();
				AssertEquals(false, header.HasErrors);
				AssertEquals(true, header.HasMessageErrors);
				AssertEquals("If header has message error and has security to send and select 'Yes', CusMiscRequestHeader is created.", 1, cusMiscRequestHeaders.Count);

				var cusMiscRequestHeader = (CusMiscRequestHeader)cusMiscRequestHeaders[0];
				AssertEquals(header.CustomsOffice + header.Department, cusMiscRequestHeader.CMR_CustomsOffice);
				AssertEquals(header.MessageType, cusMiscRequestHeader.CMR_MessageType);
				AssertEquals(header.BranchPK, cusMiscRequestHeader.CMR_GB);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, cusMiscRequestHeader.CMR_GS_NKBroker);
				AssertEquals("MSC00000001", cusMiscRequestHeader.CMR_JobNumber);
				AssertEquals("2022-07-21", cusMiscRequestHeader.CMR_RequestDate.ToString(DateFormatType.DateKorean));
				AssertEquals("임시개청 시작일시 : 2022-07-21 18:00" +
							 "\r\n임시개청 종료일시 : 2022-07-21 18:30" +
							 "\r\n임시개청 사유 : 임시개청 사유입니다.", cusMiscRequestHeader.CMR_RequestDetails);
				AssertEquals("OST", cusMiscRequestHeader.CMR_Status);

				AssertEquals(1, cusMiscRequestHeader.Messages.Count);
				var message = cusMiscRequestHeader.Messages[0];

				AssertEquals("KRC", message.EM_ApplicationCode);
				AssertEquals(messageType, message.EM_MessageType);
				AssertEquals("TRX", message.EM_ReceiveTransmit);
				AssertEquals("QUE", message.EM_Status);
				AssertEquals(GlbBranch.CurrentBranch.PK, message.EM_GB);
				AssertEquals(GlbDepartment.CurrentDepartment.PK, message.EM_GE);
				AssertEquals(CusMiscRequestHeader.Schema.TableName, message.EM_LinkTable);
				AssertEquals(cusMiscRequestHeader.PK, message.EM_LinkUniqueID);

				AssertNotContains("ENTRY NUMBER PLACE HOLDER", message.EM_MessageText);
				AssertContains(cusMiscRequestHeader.CusEntryNumber.CE_EntryNum, message.EM_MessageText);
			}
		}

		void SetupCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "033", "양산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}

		[TestDate(2022, 07, 21)]
		public void TestSendButtionWhenNoEntryLine()
		{
			SetupCusCodeList();

			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			header.CustomsOffice = "033";
			header.Department = "10";
			header.StartDate = new ZDateTime(2022, 07, 21, 18, 00, 00);
			header.EndDate = new ZDateTime(2022, 07, 21, 18, 30, 00);
			header.BranchPK = GlbBranch.CurrentBranch.PK;
			header.Reason = "임시개청 사유입니다.";

			CusMiscRequestHeaderCollection cusMiscRequestHeaders = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			AssertEquals("CusMiscRequestHeader records count is zero.", 0, cusMiscRequestHeaders.Count);

			using (var form = new ExtendedHoursRequestNewFormForTest(header))
			{
				KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");

				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				form.AcceptButton.PerformClick();
				var cusMiscRequestHeader = (CusMiscRequestHeader)cusMiscRequestHeaders[0];
				AssertEquals(0, cusMiscRequestHeader.Messages.Count);
			}
		}

		class ExtendedHoursRequestNewFormForTest : ExtendedHoursRequestNewForm
		{
			public ExtendedHoursRequestNewFormForTest(ExtendedHoursRequestHeader header)
				: base(header)
			{
			}

			protected override CusMiscRequestHeaderCreator CreateCreator() => new CusMiscRequestHeaderCreatorForTest();
		}

		class CusMiscRequestHeaderCreatorForTest : CusMiscRequestHeaderCreator
		{
			protected override void SetEM_MessageTextOrDataSource<T>(EDIMessage message, T extendedHoursRequestHeader) => throw new Exception();
		}

		public void TestControls_RelatedEntriesArea()
		{
			Assert5ACControls_RelatedEntriesArea();
			Assert5GWControls_RelatedEntriesArea();
		}

		public void Assert5ACControls_RelatedEntriesArea()
		{
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);

			using (var form = new ExtendedHoursRequestNewForm(header))
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("ExtenedHoursRequestLinesBoundGrid");
				var index = 0;
				AssertEquals(grid.Columns[index++].ColumnName, nameof(ExtendedHoursRequestLine.FormattedReferenceNumber));
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.CustomsValue);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.PackageCount);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.TotalWeight);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.SupplierName);
			}
		}

		public void Assert5GWControls_RelatedEntriesArea()
		{
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);

			using (var form = new ExtendedHoursRequestNewForm(header))
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("ExtenedHoursRequestLinesBoundGrid");
				var index = 0;
				AssertEquals(grid.Columns[index++].ColumnName, nameof(ExtendedHoursRequestLine.FormattedReferenceNumber));
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.HSDescription);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.CustomsValue);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.PackageCount);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.TotalWeight);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.BondedAreaCode);
				AssertEquals(grid.Columns[index++].ColumnName, AutoExtendedHoursRequestLine.Schema.PayerCompanyName);
				AssertEquals(grid.Columns[index++].ColumnName, nameof(ExtendedHoursRequestLine.CustomsEntryType));
			}
		}

		void SetDefaultValue(ExtendedHoursRequestHeader header, ExtendedHoursRequestLine line)
		{
			header.CustomsOffice = "033";
			header.Department = "10";
			header.StartDate = new ZDateTime(2022, 07, 21, 18, 00, 00);
			header.EndDate = new ZDateTime(2022, 07, 21, 18, 30, 00);
			header.BranchPK = GlbBranch.CurrentBranch.PK;
			header.Reason = "임시개청 사유입니다.";

			line.ReferenceNumber = "6N002220000001U";
		}

		public void TestExportDeclarationSearchButton()
		{
			var header = new ExtendedHoursRequestHeader(Factory, "5AC", GlbCompany.CurrentCompany.PK);
			using (var form = new ExtendedHoursRequestNewForm(header))
			{
				form.Show();
				AssertNotNull(form.FindSingle<ZButton>("SearchButton"));
			}
		}

		public void TestImportDeclarationSearchButton()
		{
			var header = new ExtendedHoursRequestHeader(Factory, "5GW", GlbCompany.CurrentCompany.PK);
			using (var form = new ExtendedHoursRequestNewForm(header))
			{
				form.Show();
				AssertNotNull(form.FindSingle<ZButton>("SearchButton"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new ExtendedHoursRequestNewForm(new ExtendedHoursRequestHeader(Factory, "5AC", GlbCompany.CurrentCompany.PK));
			result.ControllerID = ControllerIDs.Customs.KR.MiscRequestMessages;
			return result;
		}
	}
}
