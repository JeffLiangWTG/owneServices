using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
	{
		public void TestTopLevelMenu()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();
				AssertType<EDIMenu>(testForm.TopLevelMenu);
			}
		}

		#region AEO Supporting Documents

		#region Y025

		public void TestSupportingDocumentWhenSaving_Y025_Import_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in invoice entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_Import_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.Declarant.OA_OH = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_Import_WithDeclarant_WithT2LT2CLines()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var header1 = declaration.Invoices.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var invoiceLine1 = header1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var header2 = declaration.Invoices.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			var invoiceLine2 = header2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			var header3 = declaration.Invoices.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine3 = header3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction3.PK;

			var header4 = declaration.Invoices.AddNew();
			var entryInstruction4 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction4.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			var invoiceLine4 = header4.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction4.PK;

			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.Declarant.OA_OH = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 4 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog YES (T2L)", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 4 are not removed with dialog YES (T2C)", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
					AssertEquals("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation but there is at least one T2L/T2C invoice line in the entryInstruction, no Y025 document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation but there is at least one T2L/T2C invoice line in the entryInstruction, no Y025 document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertNull("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are not updated to have correct data entryInstruction 2 (T2C)", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are not updated to have correct data entryInstruction 4 (T2C)", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					AssertNull("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_Import_DeclarantSameAsImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					declaration.Declarant.OA_OH = declarant.PK;
					declaration.JE_OH_Importer = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_Export_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in invoice entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_ExportUcc6_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					});
				}
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_Export_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.Declarant.OA_OH = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_ExportUcc6_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						var declarant = Factory.New<OrgHeader>();
						declarant.OH_Code = "AA";
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
						declaration.Declarant.OA_OH = declarant.PK;
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

						entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
						AssertEquals("hen the entryInstruction is T2L/T2C invoice line and no documents in invoice entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 5", entryInstruction5, docType);

						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
						SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
						AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
					});
				}
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_ExportUcc6_WithDeclarant_WithT2LT2CLines()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						var declarant = Factory.New<OrgHeader>();
						declarant.OH_Code = "AA";
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
						declaration.Declarant.OA_OH = declarant.PK;
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

						entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
						AssertEquals("When there is at least one T2L/T2C invoice line in the entryInstruction and no documents in invoice entryInstructions and representative is declared and has AEO authorisation, no Y025 document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 5", entryInstruction5, docType);

						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
						SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
						AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y025 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
					});
				}
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_Export_DeclarantSameAsSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.Declarant.OA_OH = declarant.PK;
					declaration.JE_OH_Supplier = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y025_ExportUcc6_DeclarantSameAsSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						var declarant = Factory.New<OrgHeader>();
						declarant.OH_Code = "AA";
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
						declaration.Declarant.OA_OH = declarant.PK;
						declaration.JE_OH_Supplier = declarant.PK;
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y025 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					});
				}
			}
		}

		#endregion

		#region Y022

		public void TestSupportingDocumentWhenSaving_Y022_Import_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in invoice entryInstructions and supplier is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y022_Import_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var header1 = declaration.Invoices.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = header1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var header2 = declaration.Invoices.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = header2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			var header3 = declaration.Invoices.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine3 = header3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction3.PK;

			var entryInstruction4 = declaration.CustomsEntryInstructions.AddNew();

			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					header1.JZ_OH_Supplier = declarant.PK;
					header2.JZ_OH_Supplier = declarant.PK;
					declaration.JE_OH_Supplier = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					entryInstruction3.SupportingDocuments.RemoveAndDeleteAll();
					AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("No supporting document in entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is not empty in declaration (no invoice header associated) and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 4 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and supplier is not empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is not empty in declaration (no invoice header associated) and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 4 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 2", entryInstruction2, docType);
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation 1 Y022 document is added to entryInstruction 4", entryInstruction4, docType);
					AssertNull("When no documents in invoice entryInstructions and supplier is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					var supDoc = entryInstruction3.SupportingDocuments.AddNew();
					supDoc.CSI_Code = docType;
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y022 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and supplier empty no document is updated in entryInstruction 3", entryInstruction3, docType, SupDocTestHelper.initialDocRef, SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 4", entryInstruction4, docType);
					AssertEquals("When supplier is empty for at least one invoice entryInstruction and there is at least 1 Y022 doc in that invoice entryInstruction a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y022_Export_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in invoice entryInstructions and supplier is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y022_ExportUcc6_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y022 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					});
				}
			}
		}

		public void TestSupportingDocumentWhenSaving_Y022_Export_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.JE_OH_Supplier = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and supplier is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y022 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y022_ExportUcc6_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						var declarant = Factory.New<OrgHeader>();
						declarant.OH_Code = "AA";
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
						declaration.JE_OH_Supplier = declarant.PK;
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y022 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y022 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

						entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 3", entryInstruction3, docType);
						AssertEquals("When the entryInstruction is T2L/T2C and no documents in invoice entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 5", entryInstruction5, docType);

						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
						SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
						AssertEquals("Y022 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y022 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y022 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y022 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y022 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y022 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y022 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in invoice entryInstructions Y022 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
					});
				}
			}
		}

		#endregion

		#region Y023

		public void TestSupportingDocumentWhenSaving_Y023_EmptyImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in invoice entryInstructions and importer is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in invoice entryInstructions and importer is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and importer is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in invoice entryInstructions and importer is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y023_ExportUcc6_EmptyImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y023 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					});
				}
			}
		}

		public void TestSupportingDocumentWhenSaving_Y023_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var importer = Factory.New<OrgHeader>();
					importer.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, importer.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.JE_OH_Importer = importer.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, importer.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and importer is declared and has AEO authorisation 1 Y023 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and importer is declared and has AEO authorisation 1 Y023 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and importer is declared and has AEO authorisation 1 Y023 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and importer is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y023 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and importer is declared and has AEO authorisation Y023 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and importer is declared and has AEO authorisation Y023 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and importer is declared and has AEO authorisation Y023 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and importer is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y023_ExportUcc6_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						var importer = Factory.New<OrgHeader>();
						importer.OH_Code = "AA";
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, importer.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
						declaration.JE_OH_Importer = importer.PK;
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y023 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y023 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

						entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, importer.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and importer is declared and has AEO authorisation, 1 Y023 document is added to entryInstruction 3", entryInstruction3, docType);
						AssertEquals("When the entryInstruction is T2L/T2C and no documents in invoice entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and importer is declared and has AEO authorisation, 1 Y023 document is added to entryInstruction 5", entryInstruction5, docType);

						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
						SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
						AssertEquals("Y023 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y023 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y023 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y023 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y023 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y023 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y023 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and importer is declared and has AEO authorisation, Y023 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in invoice entryInstructions Y023 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and importer is declared and has AEO authorisation, Y023 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
					});
				}
			}
		}

		#endregion

		#region Y024

		public void TestSupportingDocumentWhenSaving_Y024_Import_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in invoice entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_Import_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.Declarant.OA_OH = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_Import_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.JE_OH_Importer = declarant.PK;
					declaration.Declarant.OA_OH = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and declarant and importer are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and declarant and importer are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and declarant and importer are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and declarant and importer are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and declarant and importer are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and declarant and importer are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and declarant and importer are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and rdeclarant and importer are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_Export_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in invoice entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in invoice entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_ExportUcc6_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y024 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					});
				}
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_Export_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.Declarant.OA_OH = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_ExportUcc6_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						var declarant = Factory.New<OrgHeader>();
						declarant.OH_Code = "AA";
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
						declaration.Declarant.OA_OH = declarant.PK;
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y024 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

						entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
						AssertEquals("When the entryInstruction is T2L/T2C and no documents in invoice entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 5", entryInstruction5, docType);

						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
						SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
						AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y024 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y024 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in invoice entryInstructions Y024 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
					});
				}
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_Export_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					var declarant = Factory.New<OrgHeader>();
					declarant.OH_Code = "AA";
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
					declaration.Declarant.OA_OH = declarant.PK;
					declaration.JE_OH_Supplier = declarant.PK;
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocumentWhenSaving_Y024_ExportUcc6_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6ForSupDocsY02X(declaration);

				using (var form = new JobDeclarationFormForTest(declaration))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = false;

					CombineAssertions(() =>
					{
						var declarant = Factory.New<OrgHeader>();
						declarant.OH_Code = "AA";
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
						declaration.Declarant.OA_OH = declarant.PK;
						declaration.JE_OH_Supplier = declarant.PK;
						SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
						AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("Added a Y024 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
						AssertEquals("When the entryInstruction is T2L/T2C, all Y024 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

						entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
						entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
						SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
						AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
						AssertEquals("When the entryInstruction is T2L/T2C and no documents in invoice entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in invoice entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 5", entryInstruction5, docType);

						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
						SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
						SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
						AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						AssertEquals("Y024 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
						(form as IShowPreSaveDialog).ShowPreSaveDialogs();
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y024 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y024 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in invoice entryInstructions Y024 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
						SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in invoice entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
					});
				}
			}
		}

		#endregion

		(CusEntryInstruction entryInstruction1, CusEntryInstruction entryInstruction2, CusEntryInstruction entryInstruction3, CusEntryInstruction entryInstruction4, CusEntryInstruction entryInstruction5) SetUpEntryInstructionsForUcc6ForSupDocsY02X(JobDeclaration declaration)
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var header1 = declaration.Invoices.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var invoiceLine1 = header1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var header2 = declaration.Invoices.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;
			var invoiceLine2 = header2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			var header3 = declaration.Invoices.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine3 = header3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction3.PK;

			var header4 = declaration.Invoices.AddNew();
			var entryInstruction4 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction4.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			var invoiceLine4 = header4.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction4.PK;

			var header5 = declaration.Invoices.AddNew();
			var entryInstruction5 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction5.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			var invoiceLine5 = header5.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction5.PK;

			declaration.Factory.Save();

			return (entryInstruction1, entryInstruction2, entryInstruction3, entryInstruction4, entryInstruction5);
		}

		#endregion

		#region Import 9015 Supporting Documents

		public void TestSupportingDocument_9015_ConditionsMet()
		{
			var docType = SupportingDocumentType.VATReductionCode;

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = "DIR";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "AA";
			declaration.Declarant.OA_OH = declarant.PK;
			var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "Guarantee"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ZG_MethodOfPayment = "R";

			declaration.DoMerge();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When no documents in declaration/invoice header/invoice line and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
					AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in declaration and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
					declaration.SupportingDocuments.RemoveAndDeleteAll();
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in invoice header and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
					declaration.SupportingDocuments.RemoveAndDeleteAll();
					invoice.SupportingDocuments.RemoveAndDeleteAll();
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					AssertEquals("Invoice line has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in invoice line and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_9015_ConditionsNotMet()
		{
			var docType = SupportingDocumentType.VATReductionCode;
			string popUpMessage = "there is a document 9015 in Supporting Documents to request a 50% VAT guaranteed amount reduction, but this declaration doesn't match criteria for this request. Do you want to remove 9015 documents?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.DoMerge();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When no documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
					AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));

					SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice header and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice header and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));

					SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					AssertEquals("Invoice line has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice line and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice line and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
					AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in declaration/invoice header/invoice line, conditions are not met but declaration is export no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When the pop up is not shown the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				});
			}
		}

		public void TestSupportingDocument_9015_MultipleEntries()
		{
			var docType = SupportingDocumentType.VATReductionCode;
			string popUpMessage = "there is a document 9015 in Supporting Documents to request a 50% VAT guaranteed amount reduction, but this declaration doesn't match criteria for this request. Do you want to remove 9015 documents?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = "DIR";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "AA";
			declaration.Declarant.OA_OH = declarant.PK;
			var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction1.PK, "Guarantee"), (entryInstruction2.PK, "Guarantee"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.ZG_MethodOfPayment = "R";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.ZG_MethodOfPayment = "A";

			declaration.DoMerge();

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When no documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
					SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
					SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine1, docType);
					SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine2, docType);
					AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					AssertEquals("Invoice line 1 has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
					AssertEquals("Invoice line 2 has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction in one of the entries a pop up is shown to ask if we want to remove the existing documents for the entry", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Since for one entryHeader the conditions for a VAT reduction are met, documents are removed from declaration after merge", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Since for one entryHeader the conditions for a VAT reduction are met, documents are removed from invoice header after merge", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					AssertEquals("Since for the entryHeader or this entryLine the conditions for a VAT reduction are met, documents are removed from invoice line 1 after merge", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
					AssertEquals("When dialog answer is NO, the documents are not removed from invoice line 2", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(form as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction in one of the entries a pop up is shown to ask if we want to remove the existing documents for the entry", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed from invoice line 2", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
				});
			}
		}

		#endregion

		public void TestHandleSaveException_ShouldReportDeveloperException_ForOrgHeaderSaveException()
		{
			var factory1 = new BusinessObjectFactory();
			var declarationSupplier = OrgHeader.New(factory1);
			declarationSupplier.OH_Code = "AAA";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";
			factory1.Save();

			using var form = (BaseJobDeclarationForm)GetFormToBash();
			var dec = (JobDeclaration)form.Declaration;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = dec.PK;
			var dep = Factory.NewWithValidTestData<GlbDepartment>();
			job.JH_GE = dep.PK;
			dec.JE_OH_Supplier = declarationSupplier.PK;
			dec.ZG_OtherEmailAddr = "test@example.com";

			form.Show();

			declarationSupplier.Delete();
			factory1.Save();

			form.FireSaveButton();
			CombineAssertions(() =>
			{
				AssertEquals(declarationSupplier.PK, dec.JE_OH_Supplier);
				Assert("Save should fail", !form.LastSaveSucceeded);

				var lastMessage = ErrorReporter.LastMessageReported;
				AssertContains("Declaration.Supplier:", lastMessage);
				AssertContains("JE_OH_Supplier from a new factory:", lastMessage);
				AssertContains("Declaration.Importer:", lastMessage);
				AssertContains("JE_OH_Importer from a new factory:", lastMessage);
				AssertContains("Current registry settings:", lastMessage);
				ErrorReporter.Clear();
			});
		}

		public void TestSetLocationOfGoodsUserControlReadOnly()
		{
			SetUpPremises();
			var declarationImport = SetUpDeclaration(MessageTypeList.Codes.Import);
			var declarationExport = SetUpDeclaration(MessageTypeList.Codes.Export);
			var declarationEXS = SetUpDeclaration(MessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS);
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;

			ZFormModaliser.ShowDialogsInTest = false;

			CombineAssertions(() =>
			{
				AssertSetLocationOfGoodsUserControlReadOnly(declarationImport, true, "Import");
				AssertSetLocationOfGoodsUserControlReadOnly(declarationExport, false, "Export");
				AssertSetLocationOfGoodsUserControlReadOnly(declarationEXS, true, "EXS");
			});

			void AssertSetLocationOfGoodsUserControlReadOnly(JobDeclaration declaration, bool shouldBeReadOnly, string assertMessage)
			{
				var goodsLocation = declaration.CustomsEntryInstructions[0].GoodsLocation;
				goodsLocation.Address.AuthorisationNumber = "9999000002";

				using (var form = new JobDeclarationFormForTest(declaration))
				using (var control = new LocationOfGoodsUserControl())
				{
					control.SetDataBinding(declaration.CustomsEntryInstructions[0], "");
					form.Controls.Add(control);
					form.Show();

					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						form.SetLocationOfGoodsUserControlReadOnly();
						AssertEquals($"{assertMessage}: when Temporary Storage not enable, GoodsLocation should not be readOnly", false, goodsLocation.ReadOnly);
					}

					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						form.SetLocationOfGoodsUserControlReadOnly();
						AssertEquals($"{assertMessage}: when Temporary Storage enable and Location manage in premises, GoodsLocation should be {shouldBeReadOnly}", shouldBeReadOnly, goodsLocation.ReadOnly);

						goodsLocation.Address.AuthorisationNumber = "9999000000";
						form.SetLocationOfGoodsUserControlReadOnly();
						AssertEquals($"{assertMessage}: when Temporary Storage enable but Location not manage in premises, GoodsLocation should not be readOnly", false, goodsLocation.ReadOnly);
					}
				}
			}

			void SetUpPremises()
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "AAA";
				var orgAddress = Factory.New<OrgAddress>();
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Address1 = "Address";

				var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
				premises.SRP_Type = "ADT";
				premises.SRP_CustomsLocation = "9999000002";
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;
			}

			JobDeclaration SetUpDeclaration(CargoWise.Types.ZString messageType, string entrySubStyle = EntrySubStyleList.Codes.A)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = entrySubStyle;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;

				return declaration;
			}
		}

		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.CusContainers.AddNew();
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.GoodsLocation.Address.City = "M";
			//declaration.SupportingDocuments.AddNew();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			//invoiceHeader.SupportingDocuments.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			//invoiceLine.SupportingDocuments.AddNew();
			jobDeclaration.Bills.AddNew();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.MergedLines.AddNew();
			return jobDeclaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		sealed class JobDeclarationFormForTest : JobDeclarationForm
		{
			public JobDeclarationFormForTest(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public new ZMenuItem TopLevelMenu => base.TopLevelMenu;
		}
	}
}
