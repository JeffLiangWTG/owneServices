using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AUCOLSDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDirectionsResetButton() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = declaration.CreateCOLSHeaderIfRequired();
			var direction1 = colsHeader.Directions.AddNew();
			var direction2 = colsHeader.Directions.AddNew();

			using (ZForm form = new ZForm(colsHeader))
			using (var userControl = new AUCOLSDetailsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var resetButton = userControl.DirectionsResetButton;
				AssertEquals("Caption", "Reset", resetButton.CaptionResourceString.Caption);
				AssertEquals("Button placed inside DirectionsGroupBox", true, userControl.DirectionsGroupBox.Contains(resetButton));

				AssertEquals("Prerequisite - there are 2 directions", 2, colsHeader.Directions.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				resetButton.PerformClick();
				var dialog = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Dialog Caption", "Reset Directions", dialog.Caption);
				AssertEquals("Dialog Text", "This will delete all Directions in the grid. Do you want to continue?", dialog.Text);
				AssertEquals("Answering No to the question, directions are not deleted", 2, colsHeader.Directions.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				resetButton.PerformClick();
				AssertEquals("After button is clicked, all directions are deleted", 0, colsHeader.Directions.Count);
			}
		});

		public void TestControlReadOnly()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "IMP1234";
			_ = declaration.CreateCOLSHeaderIfRequired();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.COLSTabPage;
				var colsTabControl = testForm.CustomsBrokerageUserControl.cOLSUserControl.Controls.Find("AUCOLSTabControl", true).FirstOrDefault() as ZTabControl;
				var colsDetailsControl = colsTabControl.Controls.Find("DetailsUserControl", true).FirstOrDefault() as AUCOLSDetailsUserControl;
				AssertEquals("LRNTextBox is readOnly", true, colsDetailsControl.Controls.Find("LRNTextBox", true).FirstOrDefault().GetReadOnly());
				AssertEquals("LRNStatusTextBox is readOnly", true, colsDetailsControl.Controls.Find("LRNStatusTextBox", true).FirstOrDefault().GetReadOnly());
				AssertEquals("MessageStatusTextBox is readOnly", true, colsDetailsControl.Controls.Find("MessageStatusTextBox", true).FirstOrDefault().GetReadOnly());
				AssertEquals("LodgementStatusTextBox is readOnly", true, colsDetailsControl.Controls.Find("LodgementStatusTextBox", true).FirstOrDefault().GetReadOnly());
				AssertEquals("LodgementResultMessageTextBox is readOnly", true, colsDetailsControl.Controls.Find("LodgementResultMessageTextBox", true).FirstOrDefault().GetReadOnly());
			}
		}

		public void TestReasonUserControl()
		{
			using (var userControl = new AUCOLSDetailsUserControl())
			{
				AssertEquals("BindingMember", nameof(QuarantineColsHeader.QCH_LateLodgementReason), userControl.ReasonUserControl.GetBindingMember());
			}
		}

		public void TestDirectionsGrid()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "IMP1234";
			_ = declaration.CreateCOLSHeaderIfRequired();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.COLSTabPage;
				var colsTabControl = testForm.CustomsBrokerageUserControl.cOLSUserControl.FindSingle<ZTabControl>("AUCOLSTabControl");
				var colsDetailsControl = colsTabControl.FindSingle<AUCOLSDetailsUserControl>("DetailsUserControl");
				var colsDirectionsGridControl = colsDetailsControl.FindSingle<ZGrid>("DirectionsGrid");

				AssertEquals("Container column header", "Container", colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_CO_Container)].ColumnStyle.HeaderText);
				AssertEquals("Entry Line column header", "Entry Line", colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_CL_CusEntryLine)].ColumnStyle.HeaderText);
				AssertEquals("Direction column header", "Direction", colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_Direction)].ColumnStyle.HeaderText);
				AssertEquals("Treatment Type column header", "Treatment Type", colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_TreatmentType)].ColumnStyle.HeaderText);
				AssertEquals("(AA) ID column header", "(AA) ID", colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.AAId)].ColumnStyle.HeaderText);
				AssertEquals("(AA) Location column header", "(AA) Location", colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.AALocation)].ColumnStyle.HeaderText);

				AssertType<ZGuidDropEditColumnStyle>(colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_CO_Container)].ColumnStyle);
				AssertType<ZGuidDropEditColumnStyle>(colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_CL_CusEntryLine)].ColumnStyle);
				AssertType<ZDropEditColumnStyle>(colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_Direction)].ColumnStyle);
				AssertType<ZCodeFindBoxColumnStyle>(colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.QCD_TreatmentType)].ColumnStyle);
				AssertType<ZDropEditColumnStyle>(colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.AAId)].ColumnStyle);
				AssertType<ZTextBoxColumnStyle>(colsDirectionsGridControl.Columns[nameof(QuarantineColsDirection.AALocation)].ColumnStyle);
			}
		}

		public void TestControlBindingMember()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP12345";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.COLSTabPage;
				var colsTabControl = testForm.CustomsBrokerageUserControl.cOLSUserControl.Controls.Find("AUCOLSTabControl", true).FirstOrDefault() as ZTabControl;
				var colsDetailsControl = colsTabControl.Controls.Find("DetailsUserControl", true).FirstOrDefault() as AUCOLSDetailsUserControl;

				AssertEquals(nameof(colsHeader.LRNStatus), colsDetailsControl.Controls.Find("LRNStatusTextBox", true).FirstOrDefault().GetBindingMember());
				AssertEquals(nameof(colsHeader.HeaderStatus), colsDetailsControl.Controls.Find("MessageStatusTextBox", true).FirstOrDefault().GetBindingMember());
				AssertEquals(nameof(colsHeader.LodgementStatus), colsDetailsControl.Controls.Find("LodgementStatusTextBox", true).FirstOrDefault().GetBindingMember());
				AssertEquals(nameof(colsHeader.LodgementResultMessage), colsDetailsControl.Controls.Find("LodgementResultMessageTextBox", true).FirstOrDefault().GetBindingMember());
			}
		}
	}
}
