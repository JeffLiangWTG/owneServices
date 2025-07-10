using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestEarliestCustomsIssueDateEdit()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var declarationForm = new JobDeclarationForm(declaration))
			using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControl)
			{
				declarationUserControl.JobDeclaration = declaration;
				var dateEdit = (ZDateEdit)(declarationUserControl.Controls.Find("EarliestCustomsIssueDateEdit", true).Single());
				declarationForm.Show();

				AssertEquals("EarliestCustomsIssueDateEdit is Visible", true, dateEdit.Visible);

				AssertEquals("EarliestCustomsIssueDateEdit is bind to LatestEntryAcceptanceDate", "LatestEntryAcceptanceDate", dateEdit.BindTo);
			}
		}

		public void TestGetCustomsOfficesUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var customsOfficesUserControl = (ZDynamicControlCreationUserControl)control.Controls.Find("CustomsOfficesUserControl", true).FirstOrDefault();
				AssertEquals(typeof(CustomsOfficesUserControl), customsOfficesUserControl.UserControlType);
			}
		}

		public void TestAddOrChangeReferenceTransportDocumentsAfterJE_HouseBillChange_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.T2L);
			declaration.Factory.Save();

			AssertAddOrChangeReferenceTransportDocumentsAfterJE_HouseBillChange(declaration);
		}

		public void TestAddOrChangeReferenceTransportDocumentsAfterJE_HouseBillChange_ImportT2LorT2C()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, EntrySubStyleList.Codes.T2C);
			declaration.Factory.Save();

			AssertAddOrChangeReferenceTransportDocumentsAfterJE_HouseBillChange(declaration);
		}

		public void TestAddOrChangeReferenceTransportDocumentsAfterJE_HouseBillChange_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B);
			declaration.Factory.Save();

			using (var form = new ZForm(declaration))
			{
				form.Controls.Add(new JobDeclarationUserControl());
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					declaration.JE_HouseBill = "Transport A";
					AssertEquals("Change JE_HouseBill without merge should not add document in the list", 0, declaration.SupportingDocuments.Count);
					declaration.DoMerge();
					declaration.JE_HouseBill = "Transport B";
					AssertEquals("New document added to the list after change JE_HouseBill cause there is not a transport document in the list", 1, declaration.SupportingDocuments.Count);
					AssertEquals("No new additional info added to the list", 0, declaration.AdditionalInfos.Count);
					var document = declaration.SupportingDocuments.FirstOrDefault();
					AssertEquals("New document Code after change JE_HouseBill", "N705", document.CSI_Code);
					AssertEquals("New document Reference after change JE_HouseBill", "Transport B", document.CSI_ReferenceNumber);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();
					declaration.JE_HouseBill = "Transport C";
					var expectedDialogText = @"A Transport Document (N705) already exists and its Reference Number ""Transport B"" does not match the House of Bill ""Transport C"" specified.
Do you want to update the existing Transport Document with the new House Bill?";
					AssertContains("Dialog text shows Transport information", expectedDialogText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Transport Document changes with dialog YES", "Transport C", declaration.SupportingDocuments.FirstOrDefault().CSI_ReferenceNumber);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.FireSaveButton();
					declaration.JE_HouseBill = "Transport D";
					AssertEquals("Transport Document doesn't change with dialog NO", "Transport C", declaration.SupportingDocuments.FirstOrDefault().CSI_ReferenceNumber);
				});
			}
		}

		public void TestAddOrChangeReferenceTransportDocumentsAfterJE_HouseBillChange_ImportT2LAndNoT2L()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.T2L);
			declaration.Factory.Save();

			using (var form = new ZForm(declaration))
			{
				form.Controls.Add(new JobDeclarationUserControl());
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					declaration.JE_HouseBill = "Transport A";
					AssertEquals("AdditionalDoc: Change JE_HouseBill without merge should not add document in the list", 0, declaration.AdditionalInfos.Count);
					AssertEquals("SupportingDoc: Change JE_HouseBill without merge should not add document in the list", 0, declaration.SupportingDocuments.Count);
					declaration.DoMerge();
					declaration.JE_HouseBill = "Transport B";

					AssertEquals("AdditionalDoc: Change JE_HouseBill after merge should not add document in the list", 0, declaration.SupportingDocuments.Count);
					AssertEquals("SupportingDoc: Change JE_HouseBill after merge should not add document in the list", 0, declaration.AdditionalInfos.Count);
				});
			}
		}

		void AssertAddOrChangeReferenceTransportDocumentsAfterJE_HouseBillChange(JobDeclaration declaration)
		{
			using (var form = new ZForm(declaration))
			{
				form.Controls.Add(new JobDeclarationUserControl());
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					declaration.JE_HouseBill = "Transport A";
					AssertEquals("Change JE_HouseBill without merge should not add document in the list", 0, declaration.AdditionalInfos.Count);
					declaration.DoMerge();
					declaration.JE_HouseBill = "Transport B";
					AssertEquals("New document added to the list after change JE_HouseBill cause there is not a transport document in the list", 1, declaration.AdditionalInfos.Count);
					AssertEquals("no new supporting document added to the list", 0, declaration.SupportingDocuments.Count);
					var document = declaration.AdditionalInfos.FirstOrDefault();
					AssertEquals("New document Code after change JE_HouseBill", "N705", document.CSI_Code);
					AssertEquals("New document Reference after change JE_HouseBill", "Transport B", document.CSI_ReferenceNumber);
					AssertEquals("New document SubType after change JE_HouseBill", "TRA", document.CSI_SubType);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();
					declaration.JE_HouseBill = "Transport C";
					var expectedDialogText = @"A Transport Document (N705) already exists and its Reference Number ""Transport B"" does not match the House of Bill ""Transport C"" specified.
Do you want to update the existing Transport Document with the new House Bill?";
					AssertContains("Dialog text shows Transport information", expectedDialogText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Transport Document changes with dialog YES", "Transport C", declaration.AdditionalInfos.FirstOrDefault().CSI_ReferenceNumber);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.FireSaveButton();
					declaration.JE_HouseBill = "Transport D";
					AssertEquals("Transport Document doesn't change with dialog NO", "Transport C", declaration.AdditionalInfos.FirstOrDefault().CSI_ReferenceNumber);
				});
			}
		}
	}
}
