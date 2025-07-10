using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class JobDeclarationMessageSendingObjectValidationTest : Customs.Business.Testing.MessageSendingObjectValidationTest
	{
		public void TestMessageTypeError()
		{
			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
			{
				MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration
			};

			CombineAssertions(() =>
			{
				testItem.ShouldSend = false;
				AssertNoErrors("No errors when Should Send is not ticked", testItem.MessageTypeInfo);

				testItem.ShouldSend = true;
				testItem.MessageType = ZString.Empty;
				AssertHasErrorContaining("There is an error when Should Send is ticked and MessageType is empty", testItem.MessageTypeInfo, "Declaration cannot be sent with empty Message Type");

				testItem.MessageType = "AAA";
				AssertHasErrorContaining("There is an error when Should Send is ticked and MessageType is not correct", testItem.MessageTypeInfo, ListValidation.InvalidCodeError);

				testItem.MessageType = "PDC";
				AssertNoErrors("No errors when Should Send is ticked and MessageType is in list", testItem.MessageTypeInfo);

				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Cancellation
				};
				testItem.ShouldSend = true;
				testItem.MessageType = ZString.Empty;
				AssertHasErrorContaining("There is an error when Should Send is ticked and MessageType is empty", testItem.MessageTypeInfo, "Declaration cannot be sent with empty Message Type");

				testItem.MessageType = "PCN";
				AssertNoErrors("No errors when Should Send is ticked, MessageType is assigned and MessageTypeList is empty", testItem.MessageTypeInfo);
			});
		}

		public void TestCheckComplementaryExportUndeclaredInvoiceDocument()
		{
			var warningMessage = "There are no undeclared invoice documents for this entry";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("N740", "NoInvoice1"));
			declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("N380", "Invoice1"));
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.SupportingDocuments.Add(Factory.CreateSupportingDocument("N935", "Invoice2"));
			invoiceHeader.SupportingDocuments.Add(Factory.CreateSupportingDocument("N705", "NoInvoice2"));
			invoiceHeader.InvoiceLines.AddNew();

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.AddEntryLineDocument<SupportingDocument>("N740", "NoInvoice");
			entryLine.AddEntryLineDocument<SupportingDocument>("N935", "Invoice2");

			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
			{
				MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration
			};

			CombineAssertions(() =>
			{
				testItem.ShouldSend = false;
				AssertNoWarnings("No warnings when Should Send is not ticked", testItem.MessageTypeInfo);

				testItem.ShouldSend = true;
				testItem.MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;
				AssertNoWarnings("No warnings when Message Type is not Complementary Export", testItem.MessageTypeInfo);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
				testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;

				AssertNoWarnings("No warnings when Message Type is Complementary Export and there is an invoice document undeclared", testItem.MessageTypeInfo);

				entryLine.AddEntryLineDocument<SupportingDocument>("N380", "Invoice1");
				testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;

				AssertHasWarning("There is warning when Message Type is Complementary Export and has the same invoice already declared", testItem.MessageTypeInfo, warningMessage);
			});
		}

		public void TestDJPDocumentsExistValidation_DeclarationDocument()
		{
			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9001";
			var supdoc2 = declaration.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "9002";
			var supdoc3 = declaration.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not DJP", testItem);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered a Procedure (DJP) for any document");

				supdoc1.CSI_Procedure = "A";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is DJP and at least one document has procedure", testItem);
			});
		}

		public void TestDJPDocumentsExistValidation_InvoiceHeaderDocument()
		{
			var supdoc1 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9001";
			var supdoc2 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "9002";
			var supdoc3 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not DJP", testItem);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered a Procedure (DJP) for any document");

				supdoc1.CSI_Procedure = "A";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is DJP and at least one document has procedure", testItem);
			});
		}

		public void TestDJPDocumentsExistValidation_InvoiceLineDocument()
		{
			var supdoc1 = invoiceLine.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9001";
			var supdoc2 = invoiceLine.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "9002";
			var supdoc3 = invoiceLine.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not DJP", testItem);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered a Procedure (DJP) for any document");

				supdoc1.CSI_Procedure = "A";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is DJP and at least one document has procedure", testItem);
			});
		}

		public void TestValidatePreviousDocExistsForC40Declaration_DeclarationDocument()
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not C40", testItem);

				testItem.MessageType = "C40";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered a Previous Document");

				var prevdoc = declaration.PreviousDocuments.AddNew();
				prevdoc.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment,
					ShouldSend = true
				};
				testItem.MessageType = "C40";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is C40 and a previous document is declared", testItem);
			});
		}

		public void TestValidatePreviousDocExistsForC40Declaration_InvoiceHeaderDocument()
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not C40", testItem);

				testItem.MessageType = "C40";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered a Previous Document");

				var prevdoc = invoiceHeader.PreviousDocuments.AddNew();
				prevdoc.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment,
					ShouldSend = true
				};
				testItem.MessageType = "C40";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is C40 and a previous document is declared", testItem);
			});
		}

		public void TestValidatePreviousDocExistsForC40Declaration_InvoiceLineDocument()
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not C40", testItem);

				testItem.MessageType = "C40";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered a Previous Document");

				var prevdoc = invoiceLine.PreviousDocuments.AddNew();
				prevdoc.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment,
					ShouldSend = true
				};
				testItem.MessageType = "C40";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is C40 and a previous document is declared", testItem);
			});
		}

		public void TestValidateSupportingDocExistsToSendForC44Declaration_DeclarationDocument()
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not C44", testItem);

				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered Supporting Documents");

				var supdoc = declaration.SupportingDocuments.AddNew();
				supdoc.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is C44 and at least a supporting document is declared", testItem);

				var entryLine = entryHeader.MergedLines[0];
				var supdoc2 = Factory.New<SupportingDocument>();
				supdoc2.CSI_ParentID = entryLine.PK;
				supdoc2.CSI_ParentTableCode = entryLine.TablePrefix;
				supdoc2.CSI_Status = "ACC";
				supdoc2.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered Supporting Documents");
			});
		}

		public void TestValidateSupportingDocExistsToSendForC44Declaration_InvoiceHeaderDocument()
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not C44", testItem);

				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered Supporting Documents");

				var supdoc = invoiceHeader.SupportingDocuments.AddNew();
				supdoc.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is C44 and at least a supporting document is declared", testItem);

				var entryLine = entryHeader.MergedLines[0];
				var supdoc2 = Factory.New<SupportingDocument>();
				supdoc2.CSI_ParentID = entryLine.PK;
				supdoc2.CSI_ParentTableCode = entryLine.TablePrefix;
				supdoc2.CSI_Status = "ACC";
				supdoc2.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered Supporting Documents");
			});
		}

		public void TestValidateSupportingDocExistsToSendForC44Declaration_InvoiceLineDocument()
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration
				};

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is not C44", testItem);

				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered Supporting Documents");

				var supdoc = invoiceLine.SupportingDocuments.AddNew();
				supdoc.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Message Type is C44 and at least a supporting document is declared", testItem);

				var entryLine = entryHeader.MergedLines[0];
				var supdoc2 = Factory.New<SupportingDocument>();
				supdoc2.CSI_ParentID = entryLine.PK;
				supdoc2.CSI_ParentTableCode = entryLine.TablePrefix;
				supdoc2.CSI_Status = "ACC";
				supdoc2.CSI_Code = "AA";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
					ShouldSend = true
				};
				testItem.MessageType = "C44";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, "You have not entered Supporting Documents");
			});
		}

		public void TestValidateImportDescriptionLength()
		{
			var errorMessage = "All Goods Descriptions must be at least 5 characters long";

			invoiceLine.JI_Description = ZString.Empty;

			CombineAssertions(() =>
			{
				var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration
				};
				testItem.ShouldSend = true;
				testItem.MessageType = "PDC";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, errorMessage);
				testItem.ClearRowNotifications();

				invoiceLine.JI_Description = "AAAAAAA";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when description has at least 5 characters for PDC", testItem);

				invoiceLine.JI_Description = "A";
				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDI";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, errorMessage);
				testItem.ClearRowNotifications();

				invoiceLine.JI_Description = "AAAAA";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when description has at least 5 characters for PDI", testItem);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
				invoiceLine.JI_Description = "A";
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration
				};
				testItem.ShouldSend = true;
				AssertEquals("Message Type is DJP", "DJP", testItem.MessageType);
				testItem.Validation.ValidateAll();
				AssertNoRowErrorContaining(testItem, errorMessage);

				entryHeader.CH_EntryStatus = ZString.Empty;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
				{
					MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration
				};
				testItem.ShouldSend = true;
				testItem.MessageType = "PDS";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, errorMessage);
				testItem.ClearRowNotifications();

				invoiceLine.JI_Description = "AAAAA";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when description has at least 5 characters for PDS", testItem);

				invoiceLine.JI_Description = ZString.Empty;
				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when Should Send is not ticked", testItem);

				testItem.ShouldSend = true;
				testItem.MessageType = "PDI";
				testItem.Validation.ValidateAll();
				AssertHasRowErrorContaining(testItem, errorMessage);
				testItem.ClearRowNotifications();

				invoiceLine.JI_Description = "AAAAAAA";
				testItem.Validation.ValidateAll();
				AssertNoRowErrors("No errors when description has at least 5 characters for PDI", testItem);
			});
		}

		public void TestValidateSecurityFlag()
		{
			var errorText = "You have not entered a Security flag";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.ZG_UCC6Version = 1;

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is not ticked and ZG_UCC6Version > 0", testItem.SecurityFlagInfo);

				testItem.ShouldSend = true;
				testItem.MessageType = ZString.Empty;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is empty", testItem.SecurityFlagInfo);

				testItem.MessageType = "EDP";
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is EDP", testItem.SecurityFlagInfo, errorText);

				testItem.MessageType = "EDC";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is EDC (not EDP, PDE or EDM)", testItem.SecurityFlagInfo);

				testItem.MessageType = "PDE";
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is PDE", testItem.SecurityFlagInfo, errorText);

				testItem.MessageType = "EDX";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is EDX (not EDP, PDE or EDM)", testItem.SecurityFlagInfo);

				testItem.MessageType = "EDM";
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is EDM", testItem.SecurityFlagInfo, errorText);

				declaration.JE_MessageSubType = "CO";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is EDM but declaration messageSubType is CO", testItem.SecurityFlagInfo);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = "IM";
				testItem.MessageType = "EDM";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0 and MessageType is EDM but declaration is Import", testItem.SecurityFlagInfo);

				entryHeader.ZG_UCC6Version = 0;

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is not ticked and ZG_UCC6Version = 0", testItem.SecurityFlagInfo);

				testItem.ShouldSend = true;
				testItem.MessageType = ZString.Empty;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0 and MessageType is empty", testItem.SecurityFlagInfo);

				testItem.MessageType = "EDP";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0 and MessageType is EDP", testItem.SecurityFlagInfo);

				testItem.MessageType = "EDC";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0 and MessageType is EDC (not EDP, PDE or EDM)", testItem.SecurityFlagInfo);

				testItem.MessageType = "PDE";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0 and MessageType is PDE", testItem.SecurityFlagInfo);

				testItem.MessageType = "EDX";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0 and MessageType is EDX (not EDP, PDE or EDM)", testItem.SecurityFlagInfo);

				testItem.MessageType = "EDM";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0 and MessageType is EDM", testItem.SecurityFlagInfo);
			});
		}

		public void TestValidateRequestDispatch()
		{
			var errorText = "You have not entered a Request Dispatch";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.ZG_UCC6Version = 1;
				entryHeader.MovementReferenceNumber = "MRN-TEST";

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);

				testItem.ShouldSend = false;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is not ticked and ZG_UCC6Version > 0", testItem.RequestDispatchInfo);

				testItem.ShouldSend = true;
				testItem.MessageType = ZString.Empty;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is empty", testItem.RequestDispatchInfo);

				testItem.MessageType = "EDA";
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, ZG_UCC6Version > 0mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA", testItem.RequestDispatchInfo, errorText);

				entryHeader.ZG_UCC6Version = 0;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA", testItem.RequestDispatchInfo);

				entryHeader.ZG_UCC6Version = 1;
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA", testItem.RequestDispatchInfo, errorText);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, CEI_SubStyle is T2C, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA", testItem.RequestDispatchInfo);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, CEI_SubStyle is B, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA", testItem.RequestDispatchInfo, errorText);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version = 0, mrn is empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA", testItem.RequestDispatchInfo);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, ZG_UCC6Version > 0, mrn is not empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA", testItem.RequestDispatchInfo, errorText);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				testItem.MessageType = "EDA";
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0, mrn is declared, there is a sendable annex and MessageType is EDA but declaration is Import", testItem.RequestDispatchInfo);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var message = Factory.New<ESEDIMessage>();
				entryHeader.Messages.Add(message);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Sent;
				var messagePivot = Factory.New<GenPivot>();
				messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot.XX_Relation1ID = docPivot.PK;
				messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
				messagePivot.XX_Relation2ID = message.PK;
				messagePivot.XX_Relation2TableCode = message.TablePrefix;
				Factory.Save();
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response and MessageType is EDA", testItem.RequestDispatchInfo);

				message.EM_Status = EDIMessage.Status.Rejected;
				Factory.Save();
				testItem.Validation.ValidateAll();
				AssertHasErrorContaining("There is an error when Should Send is ticked, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response and MessageType is EDA", testItem.RequestDispatchInfo, errorText);

				message.EM_Status = EDIMessage.Status.Received;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Factory.Save();
				testItem.Validation.ValidateAll();
				AssertNoErrors("No errors when Should Send is ticked, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV and MessageType is EDA", testItem.RequestDispatchInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			invoiceHeader = declaration.Invoices.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "description";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders[0];
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		JobComInvoiceLine invoiceLine;
		Declaration.CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;
	}
}
