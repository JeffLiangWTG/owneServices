using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AEODocumentManagerTest : TestCaseWithFactory
	{
		#region Y025

		public void TestSupportingDocument_Y025_Import_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y025_Import_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, Y025 document is not added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, Y025 document is not added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, Y025 document is not added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				instruction.CEI_Style = "X";

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation, 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation, 1 Y025 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation, 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation, Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation, Y025 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation, Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);

				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When there are documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When there are documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, all Y025 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When there are documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y025_Import_WithDeclarant_WithT2LT2CLines()
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

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true (T2L)", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true (T2C)", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

				entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
				entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
				AssertEquals("When the entryInstruction is T2L and no documents in entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
				AssertEquals("When the entryInstruction is T2C and no documents in entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

				SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
				SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y025 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are not updated to have correct data entryInstruction 2 (T2L)", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are not updated to have correct data entryInstruction 4 (T2C)", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
			});
		}

		public void TestSupportingDocument_Y025_Import_DeclarantSameAsImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				var declarant = Factory.New<OrgHeader>();
				declarant.OH_Code = "AA";
				declaration.Declarant.OA_OH = declarant.PK;
				declaration.JE_OH_Importer = declarant.PK;
				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is the same as the importer and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y025_Export_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y025_ExportUcc6_EmptyDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

				CombineAssertions(() =>
				{
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y025 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("Added a Y025 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
				});
			}
		}

		public void TestSupportingDocument_Y025_Export_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
			});
		}

		public void TestSupportingDocument_Y025_ExportUcc6_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

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
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

					entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2L/T2C and no documents in entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 5", entryInstruction5, docType);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y025 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y025 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in entryInstructions Y025 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
				});
			}
		}

		public void TestSupportingDocument_Y025_ExportUcc6_WithDeclarant_WithT2LT2CLines()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

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
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y025 doc in entryInstructions but there is no AEO authorisation, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

					entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2L/T2C and no documents in entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y025 document is added to entryInstruction 5", entryInstruction5, docType);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y025 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y025 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y025 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y025 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in entryInstructions Y025 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y025 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
				});
			}
		}

		public void TestSupportingDocument_Y025_Export_DeclarantSameAsSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				var declarant = Factory.New<OrgHeader>();
				declarant.OH_Code = "AA";
				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation);
				declaration.Declarant.OA_OH = declarant.PK;
				declaration.JE_OH_Supplier = declarant.PK;
				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y025_ExportUcc6_DeclarantSameAsSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

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
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y025 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y025 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is the same as the supplier and there is at least 1 Y025 doc in entryInstructions, all Y025 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
				});
			}
		}

		#endregion

		#region Y022

		public void TestSupportingDocument_Y022_Import_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y022_Import_WithSupplier()
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

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				AssertEquals("When supplier is not empty in declaration (no invoice header associated) and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 4 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				AssertEquals("When supplier is not empty in declaration (no invoice header associated) and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 4 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				header3.JZ_OH_Supplier = ZGuid.Empty;

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and supplier is declared and has AEO authorisation and there is at least one H2, Y022 document is not added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and supplier is declared and has AEO authorisation and there is at least one H2, Y022 document is not added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and supplier is empty and there is at least one H2, Y022 document is not added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				AssertEquals("When no documents in entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation and there is at least one H2, Y022 document is not added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));

				instruction.CEI_Style = "X";

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation, 1 Y022 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation, 1 Y022 document is added to entryInstruction 2", entryInstruction2, docType);
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation, 1 Y022 document is added to entryInstruction 4", entryInstruction4, docType);

				var supDoc = entryInstruction3.SupportingDocuments.AddNew();
				supDoc.CSI_Code = docType;
				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y022 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y022 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y022 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y022 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation, Y022 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation, Y022 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier empty no document is updated in entryInstruction 3", entryInstruction3, docType, SupDocTestHelper.initialDocRef, SupDocTestHelper.initialDocDate);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation, Y022 documents are updated to have correct data entryInstruction 4", entryInstruction4, docType);

				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When there are documents in entryInstructions and supplier is declared and has AEO authorisation and there is at least one H2, all Y022 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When there are documents in entryInstructions and supplier is declared and has AEO authorisation and there is at least one H2, all Y022 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When there are documents in entryInstructions and supplier is empty and there is at least one H2, Y022 document is not added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				AssertEquals("When there are documents in entryInstructions and supplier is declared in declaration (no invoice header associated) and has AEO authorisation and there is at least one H2, all Y022 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
			});
		}

		public void TestSupportingDocument_Y022_Export_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and supplier is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y022_ExportUcc6_EmptySupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

				CombineAssertions(() =>
				{
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y022 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("Added a Y022 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y022 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is empty and there is at least 1 Y022 doc in entryInstructions, all Y022 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
				});
			}
		}

		public void TestSupportingDocument_Y022_Export_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y022 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y022 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y022 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
			});
		}

		public void TestSupportingDocument_Y022_ExportUcc6_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

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
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y022 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y022 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and supplier is not empty and there is at least 1 Y022 doc in entryInstructions but there is no AEO authorisation, all Y022 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

					entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2L/T2C and no documents in entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and supplier is declared and has AEO authorisation 1 Y022 document is added to entryInstruction 5", entryInstruction5, docType);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y022 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y022 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y022 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y022 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in entryInstructions Y022 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and supplier is declared and has AEO authorisation Y022 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
				});
			}
		}

		#endregion

		#region Y023

		public void TestSupportingDocument_Y023_EmptyImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and importer is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and importer is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and importer is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y023_ExportUcc6_EmptyImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

				CombineAssertions(() =>
				{
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("Added a Y023 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y023 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions, all Y023 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
				});
			}
		}

		public void TestSupportingDocument_Y023_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When importer is not empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, importer.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, Y023 document is not added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, Y023 document is not added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, Y023 document is not added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y023 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y023 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y023 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, Y023 document is removed from entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, Y023 document is removed from entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, Y023 document is removed from entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				instruction.CEI_Style = "X";

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and importer is declared and has AEO authorisation, 1 Y023 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and importer is declared and has AEO authorisation, 1 Y023 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and importer is declared and has AEO authorisation, 1 Y023 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y023 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y023 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y023 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and importer is declared and has AEO authorisation, Y023 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and importer is declared and has AEO authorisation, Y023 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and importer is declared and has AEO authorisation, Y023 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);

				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When there are documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, all Y023 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When there are documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, all Y023 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When there are documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, all Y023 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y023_ExportUcc6_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

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
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y023 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y023 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and importer is empty and there is at least 1 Y023 doc in entryInstructions but there is no AEO authorisation, all Y023 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

					entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, importer.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and importer is declared and has AEO authorisation, 1 Y023 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2L/T2C and no documents in entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and importer is declared and has AEO authorisation, 1 Y023 document is added to entryInstruction 5", entryInstruction5, docType);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y023 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y023 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y023 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y023 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and importer is declared and has AEO authorisation, Y023 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in entryInstructions Y023 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and importer is declared and has AEO authorisation, Y023 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
				});
			}
		}

		#endregion

		#region Y024

		public void TestSupportingDocument_Y024_Import_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y024_Import_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, Y024 document is not added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, Y024 document is not added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, Y024 document is not added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				instruction.CEI_Style = "X";

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation, 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation, 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation, 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation, Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation, Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation, Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);

				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When there are documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When there are documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When there are documents in entryInstructions and representative is declared and has AEO authorisation and there is at least one H2, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y024_Import_WithImporter()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When declarant and importer are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation and there is at least one H2, Y024 document is not added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation and there is at least one H2, Y024 document is not added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation and there is at least one H2, Y024 document is not added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				instruction.CEI_Style = "X";

				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation, 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation, 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and importer are the same and has AEO authorisation, 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and importer are the same and has AEO authorisation, Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and importer are the same and has AEO authorisation, Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and importer are the same and has AEO authorisation, Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);

				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When there are documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When there are documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When there are documents in entryInstructions and importer is declared and has AEO authorisation and there is at least one H2, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y024_Export_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When no documents in entryInstructions and representative is empty no document is added to entryInstruction 3", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
				AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
			});
		}

		public void TestSupportingDocument_Y024_ExportUcc6_EmptyRepresentative()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

				CombineAssertions(() =>
				{
					SupDocTestHelper.AddSuportingDocumentsToEntryInstructions(declaration, docType);
					AssertEquals("Added a Y024 supporting document to entryInstruction 1", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 2", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 3", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 4", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("Added a Y024 supporting document to entryInstruction 5", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y024 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is empty and there is at least 1 Y024 doc in entryInstructions, all Y024 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));
				});
			}
		}

		public void TestSupportingDocument_Y024_Export_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
			});
		}

		public void TestSupportingDocument_Y024_ExportUcc6_WithDeclarant()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

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
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y024 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and representative is not empty and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

					entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2L/T2C and no documents in entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and representative is declared and has AEO authorisation 1 Y024 document is added to entryInstruction 5", entryInstruction5, docType);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y024 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y024 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C there are documents in entryInstructions Y024 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
				});
			}
		}

		public void TestSupportingDocument_Y024_Export_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.Factory.Save();

			var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(false));

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
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are not removed with MessageBoxProvider false", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
				docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 1 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
				AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 2 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
				AssertEquals("When declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));

				SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);

				SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
				AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
				docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 1", entryInstruction1, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 2", entryInstruction2, docType);
				SupDocTestHelper.AssertAEODocuments("When there are documents in entryInstructions and declarant and supplier are the same and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
			});
		}

		public void TestSupportingDocument_Y024_ExportUcc6_WithSupplier()
		{
			var docType = AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo;
			var popupMessage = "Do you want to remove '" + AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo + "' documents from Inv.Headers?";

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();
				(var entryInstruction1, var entryInstruction2, var entryInstruction3, var entryInstruction4, var entryInstruction5) = SetUpEntryInstructionsForUcc6(declaration);

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
					var docManager = new AEODocumentManager(declaration, GetMessageBoxProvider(true));
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 1 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z, all Y024 documents in entryInstruction 2 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 3 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction3, docType));
					AssertEquals("When the entryInstruction is T2L/T2C, all Y024 documents in entryInstruction 4 are not removed with MessageBoxProvider true", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					AssertEquals("When the entryInstruction is not A/B/C/X/Y/Z and declarant and supplier are the same and there is at least 1 Y024 doc in entryInstructions but there is no AEO authorisation, all Y024 documents in entryInstruction 5 are removed with MessageBoxProvider true", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction5, docType));

					entryInstruction1.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction2.SupportingDocuments.RemoveAndDeleteAll();
					entryInstruction4.SupportingDocuments.RemoveAndDeleteAll();
					SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 1", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction1, docType));
					AssertEquals("When the entryInstruction is A/B/C/X/Y/Z and no documents in entryInstruction no document is added to entryInstruction 2", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction2, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 3", entryInstruction3, docType);
					AssertEquals("When the entryInstruction is T2L/T2C and no documents in entryInstruction no document is added to entryInstruction 4", 0, SupDocTestHelper.GetAEODocumentCount(entryInstruction4, docType));
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and no documents in entryInstructions and declarant and supplier are the same and has AEO authorisation 1 Y024 document is added to entryInstruction 5", entryInstruction5, docType);

					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction1, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction2, docType);
					SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction4, docType);
					SupDocTestHelper.EditSuportingDocumentsToEntryInstructions(declaration);
					AssertEquals("Y024 supporting documents modified for entryInstruction 1", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction1.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 2", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction2.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 3", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction3.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 4", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction4.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					AssertEquals("Y024 supporting documents modified for entryInstruction 5", SupDocTestHelper.initialDocRef, ((SupportingDocument)entryInstruction5.SupportingDocuments.FirstOrDefault()).CSI_ReferenceNumber);
					docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y024 documents are not updated entryInstruction 1", entryInstruction1, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is A/B/C/X/Y/Z and there are documents in entryInstructions Y024 documents are not updated entryInstruction 2", entryInstruction2, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 3", entryInstruction3, docType);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is T2L/T2C and there are documents in entryInstructions Y024 documents are not updated entryInstruction 4", entryInstruction4, docType, docRef: SupDocTestHelper.initialDocRef, docDate: SupDocTestHelper.initialDocDate);
					SupDocTestHelper.AssertAEODocuments("When the entryInstruction is not A/B/C/X/Y/Z and there are documents in entryInstructions and representative is declared and has AEO authorisation Y024 documents are updated to have correct data entryInstruction 5", entryInstruction5, docType);
				});
			}
		}

		#endregion

		IAEODocumentMessageBoxProvider GetMessageBoxProvider(bool response)
		{
			var mockProvider = new Mock<IAEODocumentMessageBoxProvider>();
			mockProvider.Setup(m => m.AskIfShouldRemoveAEODocument(It.IsAny<ZString>())).Returns(response);
			return mockProvider.Object;
		}

		(CusEntryInstruction entryInstruction1, CusEntryInstruction entryInstruction2, CusEntryInstruction entryInstruction3, CusEntryInstruction entryInstruction4, CusEntryInstruction entryInstruction5) SetUpEntryInstructionsForUcc6(JobDeclaration declaration)
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
	}
}
