using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class BrokeragePlugInTest : EU.GUI.Testing.BrokeragePlugInTest
	{
		#region AEO Supporting Documents

		#region Y025

		public void TestSupportingDocument_Y025_Import_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y025_Import_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_Y025_Import_WithDeclarant_WithT2LT2CLines()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
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

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 4 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog YES (T2L)", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 4 are not removed with dialog YES (T2C)", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
					AssertEquals("When the entryInstruction is T2L and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2C and no documents in invoice entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 4", "AAAA", ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are not updated to have correct data entryInstruction 2 (T2L)", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are not updated to have correct data entryInstruction 4 (T2C)", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_Y025_Import_DeclarantSameAsImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y025_Export_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y025_Export_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_Y025_Export_WithDeclarant_WithT2LT2CLines()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
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

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 4 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 5 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with dialog YES", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with dialog YES", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and rhen representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C all Y025 documents in entryInstruction 4 are not removed with dialog YES", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and rhen representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 5 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

					entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in invoice entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2L/T2C and no documents in invoice entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 5", entryInstruction5, docType);
					AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 4", "AAAA", ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 5", "AAAA", ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in invoice entryInstructions Y025 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
					AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_Y025_Export_DeclarantSameAsSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		#endregion

		#region Y022

		public void TestSupportingDocument_Y022_Import_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in entryInstructions and supplier is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y022_Import_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
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

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is not empty in declaration (no invoice header associated) and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 4 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is not empty in declaration (no invoice header associated) and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 4 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					header3.JZ_OH_Supplier = ZGuid.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 2", entryInstruction2, docType);
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation 1 Y022 document is added to entryInstruction 4", entryInstruction4, docType);
					AssertNull("When no documents in entryInstructions and supplier is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					var supDoc = entryInstruction3.SupportingDocuments.AddNew();
					supDoc.CSI_Code = docType;
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y022 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 4", "AAAA", ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier empty no document is updated in entryInstruction 3", entryInstruction3, docType, SupDocTestHelper.initialDocRef, SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 4", entryInstruction4, docType);
					AssertEquals("When supplier is empty for at least one invoice entryInstruction and there is at least 1 Y022 doc in that invoice entryInstruction a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
				});
			}
		}

		public void TestSupportingDocument_Y022_Export_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in entryInstructions and supplier is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y022_Export_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and supplier is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y022 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and supplier is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		#endregion

		#region Y023

		public void TestSupportingDocument_Y023_EmptyImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in entryInstructions and importer is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in entryInstructions and importer is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and importer is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in entryInstructions and importer is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y023_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, importer.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and importer is declared and has AEO authorisation 1 Y023 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and importer is declared and has AEO authorisation 1 Y023 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and importer is declared and has AEO authorisation 1 Y023 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and importer is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y023 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and importer is declared and has AEO authorisation Y023 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and importer is declared and has AEO authorisation Y023 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and importer is declared and has AEO authorisation Y023 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and importer is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		#endregion

		#region Y024

		public void TestSupportingDocument_Y024_Import_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y024_Import_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_Y024_Import_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and importer are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and importer are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and importer are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and rdeclarant and importer are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_Y024_Export_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertNull("When no documents in entryInstructions and representative is empty no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				});
			}
		}

		public void TestSupportingDocument_Y024_Export_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and representative is declared and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_Y024_Export_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with dialog NO", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions a pop up is shown to ask if we want to remove the existing documents", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with dialog YES", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertNull("When no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", "AAAA", ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", "AAAA", ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", "AAAA", ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
					SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					AssertNull("When there are documents in entryInstructions and declarant and supplier are the same and has AEO authorisation no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		#endregion

		#endregion

		#region Import 9015 Supporting Documents

		public void TestSupportingDocument_9015_ConditionsMet()
		{
			var docType = SupportingDocumentType.VATReductionCode;

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
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

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When no documents in declaration/invoice header/invoice line and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
					AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in declaration and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
					declaration.SupportingDocuments.RemoveAndDeleteAll();
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in invoice header and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
					declaration.SupportingDocuments.RemoveAndDeleteAll();
					invoice.SupportingDocuments.RemoveAndDeleteAll();
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					AssertEquals("Invoice line has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in invoice line and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSupportingDocument_9015_ConditionsNotMet()
		{
			var docType = SupportingDocumentType.VATReductionCode;
			string popUpMessage = "there is a document 9015 in Supporting Documents to request a 50% VAT guaranteed amount reduction, but this declaration doesn't match criteria for this request. Do you want to remove 9015 documents?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
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

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When no documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

					SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
					AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));

					SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice header and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice header and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));

					SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
					AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					AssertEquals("Invoice line has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice line and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in invoice line and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
					AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertNull("When there are documents in declaration/invoice header/invoice line, conditions are not met but declaration is export no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When the pop up is not shown the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				});
			}
		}

		public void TestSupportingDocument_9015_MultipleEntries()
		{
			var docType = SupportingDocumentType.VATReductionCode;
			string popUpMessage = "there is a document 9015 in Supporting Documents to request a 50% VAT guaranteed amount reduction, but this declaration doesn't match criteria for this request. Do you want to remove 9015 documents?";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
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

			const string bondNumber = "Guarantee";
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction1.PK, bondNumber), (entryInstruction2.PK, bondNumber));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.ZG_MethodOfPayment = "R";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.ZG_MethodOfPayment = "A";

			declaration.DoMerge();

			using (var plugin = new BrokeragePlugIn(shipment))
			{
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
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
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction in one of the entries a pop up is shown to ask if we want to remove the existing documents for the entry", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Since for one entryHeader the conditions for a VAT reduction are met, documents are removed from declaration after merge", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
					AssertEquals("Since for one entryHeader the conditions for a VAT reduction are met, documents are removed from invoice header after merge", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
					AssertEquals("Since for the entryHeader or this entryLine the conditions for a VAT reduction are met, documents are removed from invoice line 1 after merge", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
					AssertEquals("When dialog answer is NO, the documents are not removed from invoice line 2", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
					AssertContains("When there are documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction in one of the entries a pop up is shown to ask if we want to remove the existing documents for the entry", popUpMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("When dialog answer is YES, the documents are removed from invoice line 2", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
				});
			}
		}

		#endregion

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
		protected override BaseJobDeclaration GetDeclaration() => Factory.New<JobDeclaration>();
		protected override Type ExpectedTopLevelMenuType => typeof(EDIMenu);
	}
}
