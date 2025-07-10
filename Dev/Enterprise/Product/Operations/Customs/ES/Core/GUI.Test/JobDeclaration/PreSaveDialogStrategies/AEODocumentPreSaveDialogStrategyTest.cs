using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class AEODocumentPreSaveDialogStrategyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new AEODocumentPreSaveDialogStrategy(null));
				AssertNoExceptionThrown(() => new AEODocumentPreSaveDialogStrategy(Factory.New<JobDeclaration>()));
			});
		}

		public void TestRunPreSaveActionDoesNothing()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			CombineAssertions(() =>
			{
				ShowPreSaveDialog(declaration);
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, entryInstruction3.SupportingDocuments.Count);
				AssertNull("When no documents in entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRunPreSaveActionDoesChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			CombineAssertions(() =>
			{
				var declarant = Factory.New<OrgHeader>();
				declarant.OH_Code = "AA";
				declaration.Declarant.OA_OH = declarant.PK;
				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				ShowPreSaveDialog(declaration);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 type document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 type document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 type document is added to entryInstruction 3", entryInstruction3, docType);
				AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Type supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Type supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Type supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				ShowPreSaveDialog(declaration);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation type documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation type documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation type documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
				AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRunPreSaveActionShouldNotAsk()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			CombineAssertions(() =>
			{
				var declarant = Factory.New<OrgHeader>();
				declarant.OH_Code = "AA";
				declaration.Declarant.OA_OH = declarant.PK;
				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				ShowPreSaveDialog(declaration);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 type document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 type document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 type document is added to entryInstruction 3", entryInstruction3, docType);
				AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Type supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Type supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Type supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				ShowPreSaveDialog(declaration);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation type documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation type documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation type documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
				AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRunPreSaveActionShouldAskAnswerNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			CombineAssertions(() =>
			{
				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a type supporting document to entryInstruction 1", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 2", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 3", 1, entryInstruction3.SupportingDocuments.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ShowPreSaveDialog(declaration);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions, all type documents in entryInstruction 1 are not removed with dialog NO", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions, all type documents in entryInstruction 2 are not removed with dialog NO", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions, all type documents in entryInstruction 3 are not removed with dialog NO", 1, entryInstruction3.SupportingDocuments.Count);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?", UnitTestUserNotification.Instance.LastMessage.Text);

				var declarant = Factory.New<OrgHeader>();
				declarant.OH_Code = "AA";
				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
				declaration.Declarant.OA_OH = declarant.PK;
				AssertEquals("Added a type supporting document to entryInstruction 1", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 2", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 3", 1, entryInstruction3.SupportingDocuments.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ShowPreSaveDialog(declaration);
				AssertEquals("When representative is not empty and there is at least 1 type doc in entryInstructions but there is no AEO authorisation, all type documents in entryInstruction 1 are not removed with dialog NO", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("When representative is not empty and there is at least 1 type doc in entryInstructions but there is no AEO authorisation, all type documents in entryInstruction 2 are not removed with dialog NO", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("When representative is not empty and there is at least 1 type doc in entryInstructions but there is no AEO authorisation, all type documents in entryInstruction 3 are not removed with dialog NO", 1, entryInstruction3.SupportingDocuments.Count);
				AssertEquals("When representative is not empty and there is at least 1 type doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRunPreSaveActionShouldAskAnswerYes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			CombineAssertions(() =>
			{
				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a type supporting document to entryInstruction 1", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 2", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 3", 1, entryInstruction3.SupportingDocuments.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions, all type documents in entryInstruction 1 are removed with dialog YES", 0, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions, all type documents in entryInstruction 2 are removed with dialog YES", 0, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions, all type documents in entryInstruction 3 are removed with dialog YES", 0, entryInstruction3.SupportingDocuments.Count);

				var declarant = Factory.New<OrgHeader>();
				declarant.OH_Code = "AA";
				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
				declaration.Declarant.OA_OH = declarant.PK;
				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a type supporting document to entryInstruction 1", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 2", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Added a type supporting document to entryInstruction 3", 1, entryInstruction3.SupportingDocuments.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions but there is no AEO authorisation, all type documents in entryInstruction 1 are removed with dialog YES", 0, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions but there is no AEO authorisation, all type documents in entryInstruction 2 are removed with dialog YES", 0, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("When representative is empty and there is at least 1 type doc in entryInstructions but there is no AEO authorisation, all type documents in entryInstruction 3 are removed with dialog YES", 0, entryInstruction3.SupportingDocuments.Count);
			});
		}

		void ShowPreSaveDialog(JobDeclaration declaration) => new AEODocumentPreSaveDialogStrategy(declaration).ShowPreSaveDialogs(ContinueWithSave.Yes);

		readonly string docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
	}
}
