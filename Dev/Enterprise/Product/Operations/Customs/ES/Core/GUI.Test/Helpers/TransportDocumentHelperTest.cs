using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class TransportDocumentHelperTest : TestCaseWithFactory
	{
		public void TestAddOrCopyTransportDocumentToMisc_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport A", Common.Shared.SharedJobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.T2L);
			declaration.Factory.Save();

			AssertAddOrCopyTransportDocumentToMiscAdditionalInfos(declaration);
		}

		public void TestAddOrCopyTransportDocumentToMisc_ImportT2LorT2C()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport A", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, EntrySubStyleList.Codes.T2C);
			declaration.Factory.Save();

			AssertAddOrCopyTransportDocumentToMiscAdditionalInfos(declaration);
		}

		public void TestAddOrCopyTransportDocumentToMisc_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport A", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B);
			declaration.Factory.Save();

			using (var form = new ZForm(declaration))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					AssertEquals("No documents in the list", 0, declaration.SupportingDocuments.Count);

					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					AssertEquals("New document added to the list", 1, declaration.SupportingDocuments.Count);
					AssertEquals("No new additional info added to the list", 0, declaration.AdditionalInfos.Count);
					var document = declaration.SupportingDocuments.FirstOrDefault();
					AssertEquals("New document Code", "N705", document.CSI_Code);
					AssertEquals("New document Reference", "Transport A", document.CSI_ReferenceNumber);

					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					AssertEquals("No document added to the list cause there is already a document with the same reference", 1, declaration.SupportingDocuments.Count);

					declaration.SupportingDocuments.FirstOrDefault().CSI_ReferenceNumber = "Transport B";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					var expectedDialogText = @"A Transport Document (N705) already exists and its Reference Number ""Transport B"" does not match the House of Bill ""Transport A"" specified.
Do you want to update the existing Transport Document with the new House Bill?";
					AssertContains("Dialog text shows Transport information", expectedDialogText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Transport Document doesn't change with dialog NO", "Transport B", declaration.SupportingDocuments.FirstOrDefault().CSI_ReferenceNumber);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					AssertEquals("Transport Document changes with dialog YES", "Transport A", declaration.SupportingDocuments.FirstOrDefault().CSI_ReferenceNumber);
				});
			}
		}

		public void TestAddOrCopyTransportDocumentToMisc_ImportT2LAndNoT2L()
		{
			var declaration = Factory.New<JobDeclaration>();
			BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport A", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.T2L);
			declaration.Factory.Save();

			using (var form = new ZForm(declaration))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					AssertEquals("AdditionalDoc: No documents in the list", 0, declaration.SupportingDocuments.Count);
					AssertEquals("SupportingDoc: No documents in the list", 0, declaration.AdditionalInfos.Count);

					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					AssertEquals("AdditionalDoc: No new documents added to the list", 0, declaration.SupportingDocuments.Count);
					AssertEquals("SupportingDoc: No new documents added to the list", 0, declaration.AdditionalInfos.Count);
				});
			}
		}

		void AssertAddOrCopyTransportDocumentToMiscAdditionalInfos(JobDeclaration declaration)
		{
			using (var form = new ZForm(declaration))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					AssertEquals("No documents in the list", 0, declaration.AdditionalInfos.Count);

					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					AssertEquals("New document added to the list", 1, declaration.AdditionalInfos.Count);
					AssertEquals("no new supporting document added to the list", 0, declaration.SupportingDocuments.Count);
					var document = declaration.AdditionalInfos.FirstOrDefault();
					AssertEquals("New document Code", "N705", document.CSI_Code);
					AssertEquals("New document Reference", "Transport A", document.CSI_ReferenceNumber);
					AssertEquals("New document SubType", "TRA", document.CSI_SubType);

					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					AssertEquals("No document added to the list cause there is already a document with the same reference", 1, declaration.AdditionalInfos.Count);

					declaration.AdditionalInfos.FirstOrDefault().CSI_ReferenceNumber = "Transport B";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					var expectedDialogText = @"A Transport Document (N705) already exists and its Reference Number ""Transport B"" does not match the House of Bill ""Transport A"" specified.
Do you want to update the existing Transport Document with the new House Bill?";
					AssertContains("Dialog text shows Transport information", expectedDialogText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Transport Document doesn't change with dialog NO", "Transport B", declaration.AdditionalInfos.FirstOrDefault().CSI_ReferenceNumber);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(declaration);
					AssertEquals("Transport Document changes with dialog YES", "Transport A", declaration.AdditionalInfos.FirstOrDefault().CSI_ReferenceNumber);
				});
			}
		}
	}
}
