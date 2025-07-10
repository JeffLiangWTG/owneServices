using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ESMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestNewMessageBuilderOriginalEXS()
		{
			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeForEXS(DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
				var exsMessageText = messageBuilder.GetSignedMessageText();
				AssertNotContains("DocOpeHEA of Original exist", "<DocOpeHEA>", exsMessageText);
			});
		}

		public void TestNewMessageBuilderCancelEXS()
		{
			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeForEXS(DeclarationMessageSubTypeList.Codes.Cancellation);
				var exsMessageText = messageBuilder.GetSignedMessageText();
				AssertContains("DocOpeHEA of Original is not A", "<DocOpeHEA>A</DocOpeHEA>", exsMessageText);
			});
		}

		public void TestNewMessageBuilderAmendmentEXS()
		{
			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeForEXS(DeclarationMessageSubTypeList.Codes.Amendment);
				var exsMessageText = messageBuilder.GetSignedMessageText();
				AssertContains("DocOpeHEA of Original is not M", "<DocOpeHEA>M</DocOpeHEA>", exsMessageText);
			});
		}

		EXSMessageBuilder AssertMessageBuilderTypeForEXS(ZString messageSubType)
		{
			var declaration = CreateMessageObjectsForTest();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageSubType);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<EXSMessageBuilder>("NewMessageBuilder is EXS Subtype " + messageSubType, messageBuilder);
			return (EXSMessageBuilder)messageBuilder;
		}

		public void TestNewMessageBuilderDUAExport()
		{
			AssertMessageBuilderType<DUAExportMessageBuilder>(DeclarationMessageTypeList.Codes.Export, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderAmendmentDUAExport()
		{
			AssertMessageBuilderType<DUAExportMessageBuilder>(DeclarationMessageTypeList.Codes.ExportAmendment, DeclarationMessageSubTypeList.Codes.Amendment);
		}

		public void TestNewMessageBuilderComplXExport()
		{
			AssertMessageBuilderType<ComplXExportMessageBuilder>(DeclarationMessageTypeList.Codes.TypeXExport, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderAmendmentCompXExport()
		{
			var declaration = CreateMessageObjectsForTest();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryHeader.CH_EntryStatus = "CLR";
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ExportAmendment, DeclarationMessageSubTypeList.Codes.Amendment);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<DUAExportMessageBuilder>("NewMessageBuilder is DUAExport type (Compl X Amendment uses the same builder with different wrapper)", messageBuilder);
		}

		public void TestNewMessageBuilderPreDUAIncompleteImport()
		{
			AssertMessageBuilderType<PreDUAIncompleteImportMessageBuilder>(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderCANPreDUAImport()
		{
			AssertMessageBuilderType<CANPreDUAImportMessageBuilder>(DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderDUACompleteImport()
		{
			AssertMessageBuilderType<DUACompleteImportMessageBuilder>(DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderBox44Import()
		{
			AssertMessageBuilderType<Box44ImportMessageBuilder>(DeclarationMessageTypeList.Codes.Box44Documents, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderBox40AmendmentImport()
		{
			AssertMessageBuilderType<Box40AmendmentImportMessageBuilder>(DeclarationMessageTypeList.Codes.ImportAmendmentBox40, DeclarationMessageSubTypeList.Codes.Amendment);
		}

		public void TestNewMessageBuilderT2LExpedition()
		{
			AssertMessageBuilderType<ExpeditionMessageBuilder>(DeclarationMessageTypeList.Codes.T2lExpedition, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderT2LExpeditionAmendment()
		{
			AssertMessageBuilderType<ExpeditionAmendmentMessageBuilder>(DeclarationMessageTypeList.Codes.T2lExpeditionAmendment, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderT2LReception()
		{
			AssertMessageBuilderType<ReceptionMessageBuilder>(DeclarationMessageTypeList.Codes.T2lReception, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderT2LReceptionAmendment()
		{
			AssertMessageBuilderType<ReceptionAmendmentMessageBuilder>(DeclarationMessageTypeList.Codes.T2lReceptionAmendment, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderT2LClearance()
		{
			AssertMessageBuilderType<ClearanceMessageBuilder>(DeclarationMessageTypeList.Codes.T2lClearance, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderDUASimplifiedImport()
		{
			AssertMessageBuilderType<DUASimplifiedImportMessageBuilder>(DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderDVDQuery()
		{
			CreateMessageObjectsForTest(style: IMPDeclarationTypeList.Codes.H2);
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.DvdH2Query, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewDVDQueryBuilder();
			AssertType<QueryDVDMessageBuilder>("NewMessageBuilder is DVDQuery type", messageBuilder);
			AssertNotContains("When not canary islands flag is not present", "IndicadorDatosATC", messageBuilder.GetSignedMessageText());
		}

		public void TestNewMessageBuilderDVDQueryCanaryIslands()
		{
			CreateMessageObjectsForTest(style: IMPDeclarationTypeList.Codes.H2);
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.DvdH2Query, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewDVDQueryBuilderCanaryIslands();
			AssertType<QueryDVDMessageBuilder>("NewMessageBuilder is DVDQuery type", messageBuilder);
			AssertContains("When canary islands flag is present", "IndicadorDatosATC", messageBuilder.GetSignedMessageText());
		}

		public void TestNewMessageBuilderImportQuery()
		{
			CreateMessageObjectsForTest();
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ImportQuery, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewImportQueryBuilder();
			AssertType<DUAImportQueryMessageBuilder>("NewMessageBuilder is ImportQuery type", messageBuilder);
			AssertNotContains("When not canary islands flag is not present", "DatosEnATC", messageBuilder.GetSignedMessageText());
		}

		public void TestNewMessageBuilderImportQueryCanaryIslands()
		{
			CreateMessageObjectsForTest();
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ImportQuery, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewImportQueryBuilderCanaryIslands();
			AssertType<DUAImportQueryMessageBuilder>("NewMessageBuilder is ImportQuery type", messageBuilder);
			AssertContains("When canary islands flag is present", "DatosEnATC", messageBuilder.GetSignedMessageText());
		}

		public void TestNewMessageBuilderQueryImportH1()
		{
			CreateMessageObjectsForTest();
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ImportH1Query, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewQueryImportH1Builder();
			AssertType<QueryImportH1MessageBuilder>("NewMessageBuilder is ImportH1Query type", messageBuilder);
			AssertNotContains("When not canary islands flag is not present", "ATC", messageBuilder.GetSignedMessageText());
		}

		public void TestNewMessageBuilderQueryImportH1CanaryIslands()
		{
			var declaration = CreateMessageObjectsForTest();
			declaration.JE_CustomsOffice = "ES003800";
			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ImportH1Query, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewQueryImportH1Builder(declaration.IsCustomOfficeCanaryIsland);
			AssertType<QueryImportH1MessageBuilder>("NewMessageBuilder is ImportH1Query type", messageBuilder);
			AssertContains("When canary islands flag is present", "ATC", messageBuilder.GetSignedMessageText());
		}

		public void TestNewMessageBuilderDJPImport()
		{
			AssertMessageBuilderType<DJPImportMessageBuilder>(DeclarationMessageTypeList.Codes.PendingSupportingDocuments, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		}

		public void TestNewMessageBuilderNotImplementedException()
		{
			CreateMessageObjectsForTest();
			var messageBuilderManager = GetBuilderManager("AAA", DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertExceptionThrown<NotImplementedException>("NewMessageBuilder throws an exception for any other DeclarationMessageType not yet supported", () => messageBuilderManager.NewMessageBuilder());
		}

		public void TestNewT2LAnnexMessageBuilder()
		{
			var declaration = CreateMessageObjectsForTest();

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.T2lAnnex, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewT2LAnnexMessageBuilder(pivot, true);
			AssertType<AnnexMessageBuilder>("NewMessageBuilder is T2lAnnex type", messageBuilder);
		}

		public void TestNewMessageBuilderUCC6Export()
		{
			AssertMessageBuilderTypeWithSendingObject<DeclarationAESMessageBuilder>(DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderUCC6ExportAmendment()
		{
			AssertMessageBuilderTypeWithSendingObject<AmendmentAESMessageBuilder>(DeclarationMessageTypeList.Codes.ExportAmendmentUcc6, DeclarationMessageSubTypeList.Codes.Amendment);
		}

		public void TestNewMessageBuilderUCC6Annex()
		{
			var declaration = CreateMessageObjectsForTest();

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			var docPivotList = new List<CusStorageDocPivot>() { pivot };

			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ExportAnnexes, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewAESAnnexMessageBuilder(docPivotList, "S");
			AssertType<AnnexAESMessageBuilder>("NewMessageBuilder is ExportAnnexes type", messageBuilder);
		}

		public void TestNewMessageBuilderUCC6ExportQuery()
		{
			AssertMessageBuilderTypeWithSendingObject<QueryAESMessageBuilder>(DeclarationMessageTypeList.Codes.ExportQuery, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderUCC6ExportPreDeclaration()
		{
			var messageBuilder = AssertMessageBuilderTypeWithSendingObject<DeclarationAESMessageBuilder>(DeclarationMessageTypeList.Codes.ExportPreDeclaration, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertContains("messageText contains the mapper of the EntrySubStyle", $"<{XMLTestFileConstants.XmlElementNamespace}additionalDeclarationType>D</{XMLTestFileConstants.XmlElementNamespace}additionalDeclarationType>", messageBuilder.GetSignedMessageText());
		}

		public void TestNewMessageBuilderUCC6ExportNotification()
		{
			AssertMessageBuilderTypeWithSendingObject<GoodsNotificationAESMessageBuilder>(DeclarationMessageTypeList.Codes.ExportNotification, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderUCC6ExportCancel()
		{
			AssertMessageBuilderTypeWithSendingObject<CancelAESMessageBuilder>(DeclarationMessageTypeList.Codes.ExportCancellation, DeclarationMessageSubTypeList.Codes.Cancellation);
		}

		public void TestNewMessageBuilderUCC6ExportComplX()
		{
			AssertMessageBuilderTypeWithSendingObject<ComplXAESMessageBuilder>(DeclarationMessageTypeList.Codes.TypeXExportUcc6, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		}

		public void TestNewMessageBuilderDeclarationDVD()
		{
			AssertMessageBuilderType<DeclarationDVDMessageBuilder>(DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderCancelDVD()
		{
			AssertMessageBuilderType<CancelDVDMessageBuilder>(DeclarationMessageTypeList.Codes.DvdH2Cancellation, DeclarationMessageSubTypeList.Codes.Cancellation);
		}

		public void TestNewMessageBuilderCompXDVD()
		{
			AssertMessageBuilderType<ComplXDVDMessageBuilder>(DeclarationMessageTypeList.Codes.TypeXDvdH2, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		}

		public void TestNewMessageBuilderCommonAnnex()
		{
			var declaration = CreateMessageObjectsForTest();

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.T2lDocumentationPous, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageBuilder = messageBuilderManager.NewCommonAnnexMessageBuilder(pivot, "S");
			AssertType<CommonAnnexMessageBuilder>("NewMessageBuilder is CommonAnnex type", messageBuilder);
		}

		public void TestNewMessageBuilderT2lRequestPous()
		{
			AssertMessageBuilderType<RequestT2LMessageBuilder>(DeclarationMessageTypeList.Codes.T2lRequestPous, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderT2lPresentationPous()
		{
			AssertMessageBuilderType<PresentationT2LMessageBuilder>(DeclarationMessageTypeList.Codes.T2lPresentationPous, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderT2lQueryPous()
		{
			AssertMessageBuilderType<QueryT2LMessageBuilder>(DeclarationMessageTypeList.Codes.T2lQueryPous, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderT2lReceptionPous()
		{
			AssertMessageBuilderType<ReceptionT2LMessageBuilder>(DeclarationMessageTypeList.Codes.T2lReceptionPous, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		public void TestNewMessageBuilderIncompleteImportH1()
		{
			AssertMessageBuilderType<IncompleteImportH1MessageBuilder>(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestGetCompleteMessageDUAExport()
		{
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(DeclarantData.Email))
			{
				CreateMessageObjectsForTest();
				var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.Export, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
				var messageBuilder = messageBuilderManager.NewMessageBuilder();

				AssertMessageBuilder(messageBuilder, DeclarationMessageTypeList.Codes.Export, ExpectedStringDUAExport, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestGetCompleteMessageDUAExportAmendment()
		{
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(DeclarantData.Email))
			{
				CreateMessageObjectsForTest();
				var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ExportAmendment, DeclarationMessageSubTypeList.Codes.Amendment);
				var messageBuilder = messageBuilderManager.NewMessageBuilder();

				AssertMessageBuilder(messageBuilder, DeclarationMessageTypeList.Codes.ExportAmendment, ExpectedStringDUAExportAmendment, DeclarationMessageSubTypeList.Codes.Amendment);
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestGetCompleteMessageComplXExportAmendment()
		{
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(DeclarantData.Email))
			{
				var declaration = CreateMessageObjectsForTest();
				var entryInstruction = declaration.CustomsEntryInstructions[0];
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryHeader.CH_EntryStatus = "CLR";
				var messageBuilderManager = GetBuilderManager(DeclarationMessageTypeList.Codes.ExportAmendment, DeclarationMessageSubTypeList.Codes.Amendment);
				var messageBuilder = messageBuilderManager.NewMessageBuilder();

				AssertMessageBuilder(messageBuilder, DeclarationMessageTypeList.Codes.ExportAmendment, ExpectedStringComplXExportAmendment, DeclarationMessageSubTypeList.Codes.Amendment);
			}
		}

		T AssertMessageBuilderType<T>(ZString messageType, ZString messageSubType)
			where T : IMessageBuilderBase
		{
			CreateMessageObjectsForTest();
			var messageBuilderManager = GetBuilderManager(messageType, messageSubType);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<T>("NewMessageBuilder is " + messageType + " type", messageBuilder);
			return (T)messageBuilder;
		}

		T AssertMessageBuilderTypeWithSendingObject<T>(ZString messageType, ZString messageSubType)
			where T : IMessageBuilderBase
		{
			CreateMessageObjectsForTest();

			var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			sendingObject.MessageSubType = messageSubType;
			sendingObject.SecurityFlag = "2";
			sendingObject.MessageType = messageType;

			var messageBuilderManager = new ESMessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<T>("NewMessageBuilder is " + messageType + " type", messageBuilder);
			return (T)messageBuilder;
		}

		void AssertMessageBuilder(IMessageBuilderBase messageBuilder, ZString messageType, ZString expectedMessageText, ZString messageSubType)
		{
			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", messageType, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", messageSubType, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider.IsTest", Header.IsTest, messageBuilder.Provider.IsTest);
				AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
				AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
				AssertEquals("messageBuilder.Provider.BusinessObjectReference", entryHeader.CH_BGMReference, messageBuilder.Provider.BusinessObjectReference);
				AssertMultilineASCIIEquals("messageBuilder.CompleteMessageText", expectedMessageText, messageBuilder.GetSignedMessageText().Replace("'", "'\n"));
			});
		}

		ESMessageBuilderManager GetBuilderManager(ZString messageType, ZString messageSubType) => new ESMessageBuilderManager(messageType, messageSubType, entryHeader, certificate);

		protected JobDeclaration CreateMessageObjectsForTest(string style = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Spain,
				"", EntryLine1.ProcedurePart1, EntryLine1.ProcedurePart2, EntryLine1.ProcedureConcessionPart,
				"AB DESC 1", "EXP", group: "EFD");

			#region JobDeclaration
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = Header.MessageType;
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.ZG_IsTrainingDeclaration = Header.IsTest;

			declaration.JE_UCR = Header.ReferenceNumber;
			declaration.JE_OH_Supplier = Supplier.PK;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageSubType = Header.MessageSubType;

			#region EntryInstruction

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Header.EntryInstructionSubStyle;
			entryInstruction.CEI_Style = style;

			#endregion

			declaration.JE_CustomsOffice = Header.CustomsOffice;
			var officeOfExit = declaration.CustomsOffices.AddNew();
			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;

			#region Declarant

			declaration.Declarant.OA_OH = Declarant.PK;
			declaration.JE_DeclarantType = DeclarantData.DeclarantType;
			declaration.ZG_OtherEmailAddr = DeclarantData.OtherEmail;
			declaration.ZG_AuthPerDeclaration = true;
			declaration.ZG_AgreedPlaceCode = "";

			#endregion

			#region InvoiceHeader

			var invoice = declaration.Invoices.AddNew();

			#region InvoiceLines

			#region InvoiceLine1

			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;

			invLine1.JI_Tariff = EntryLine1.Tariff;
			invLine1.JI_SupplementaryCode1 = EntryLine1.SuppCode1;
			invLine1.JI_SupplementaryCode2 = EntryLine1.SuppCode2;
			invLine1.JI_Procedure = EntryLine1.Procedure;
			invLine1.JI_Description = EntryLine1.GoodsDescription;

			#endregion

			#endregion

			#endregion

			#region Certificate

			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			Factory.Save();

			#endregion

			#endregion

			Factory.Save();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert("Merge done", mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];
			Factory.Save();
			entryHeader.CH_BGMReference = "123412341234";

			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;
		}

		CusEntryHeader entryHeader;
		CertificateProviderTestClass certificate;
		GlbStaff staff;

		public OrgHeader Declarant
		{
			get
			{
				if (declarant == null)
				{
					declarant = CreateOrgHeader(DeclarantData.Id, DeclarantData.IdType, DeclarantData.Code, DeclarantData.Name, DeclarantData.Address, DeclarantData.City, DeclarantData.PostCode, DeclarantData.Country);
				}
				return declarant;
			}
		}
		OrgHeader declarant;

		public OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = CreateOrgHeader(ImporterData.Id, ImporterData.IdType, ImporterData.Code, ImporterData.Name, ImporterData.Address, ImporterData.City, ImporterData.PostCode, ImporterData.Country);
				}
				return importer;
			}
		}
		OrgHeader importer;

		public OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = CreateOrgHeader(SupplierData.Id, SupplierData.IdType, SupplierData.Code, SupplierData.Name, SupplierData.Address, SupplierData.City, SupplierData.PostCode, SupplierData.Country);
					supplier.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				}
				return supplier;
			}
		}
		OrgHeader supplier;

		OrgHeader CreateOrgHeader(string id, string type, string code, string name, string address, string city, string postCode, string country)
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(type, id);
			org.OH_Code = code;
			org.OH_FullName = name;

			org.MainAddress.OA_OH = org.PK;
			org.MainAddress.OA_Address1 = address;
			org.MainAddress.OA_City = city;
			org.MainAddress.OA_PostCode = postCode;
			org.MainAddress.OA_RN_NKCountryCode = country;

			return org;
		}

		public struct Header
		{
			public const string ReferenceNumber = "Reference";
			public const string MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			public const string MessageSubType = "EX";
			public const string EntryInstructionSubStyle = EntrySubStyleList.Codes.A;
			public const string CustomsOffice = "ES009999";
			public const bool IsTest = true;
		}

		public struct SupplierData
		{
			public const string Id = "SUP22222222";
			public const string IdType = OrgCusCode.SpainCodeTypes.NIF;
			public const string Code = "SUPPLIERTEST";
			public const string Name = "Supplier Test Org";
			public const string Address = "1234 Test Street";
			public const string City = "Barcelona";
			public const string PostCode = "98765";
			public const string Country = "ES";
		}
		public struct ImporterData
		{
			public const string Id = "IMP11111111";
			public const string IdType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			public const string Code = "IMPORTERTEST";
			public const string Name = "Importer Test Org";
			public const string Address = "Test Street 1234";
			public const string City = "Madrid";
			public const string PostCode = "12345";
			public const string Country = "ES";
		}
		public struct DeclarantData
		{
			public const string Id = "DEC33333333";
			public const string IdType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			public const string Code = "DECTEST";
			public const string Name = "Declarant Test Org";
			public const string Address = "Declarant Test Street";
			public const string City = "Valencia";
			public const string PostCode = "45612";
			public const string Country = "ES";

			public const string DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			public const string Email = "mail.mail@mail.com";
			public const string OtherEmail = "other.mail@mail.com";
		}

		public struct EntryLine1
		{
			public const int LineNum = 1;
			public const string Tariff = "2203001010";
			public const string SuppCode1 = "First";
			public const string SuppCode2 = "Second";
			public const string Procedure = "12349VA";
			public const string ProcedurePart1 = "12";
			public const string ProcedurePart2 = "34";
			public const string ProcedureConcessionPart = "9VA";
			public const string GoodsDescription = "Description1";
		}

		const string ExpectedStringDUAExport = @"UNB+UNOA:1+ESDEC33333333:ZZ+AEATADUE:ZZ+200109:1513+<<MSGNO PLACEHOLDER>>++&EE++++1'
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:1:921:UN:ECS003'
BGM+830+123412341234+9'
CST++EX:104:141+A:105:141++:112:141+9999:113:148'
LOC+42+ES::141:009999'
GIS+0:109:141'
NAD+EX+SUP22222222:P:148++Supplier Test Org+1234 Test Street+Barcelona++98765+ES'
NAD+CN+ESIMP11111111::148++Importer Test Org+Test Street 1234+Madrid++12345+ES'
NAD+2+ESDEC33333333::148+mail.mail@mail.com+Declarant Test Org:::::O'
TOD+++FOB:106'
MOA+ZZZ::EUR'
UNS+D'
CST+1+2203001010FirstSecond:122:148+12.34:117:141+9VA:117:148'
FTX+AAA+++Description1'
UNS+S'
CNT+5:1'
CNT+11:0'
UNT+17+<<MSGNO PLACEHOLDER>>'
UNZ+1+<<MSGNO PLACEHOLDER>>'";

		const string ExpectedStringDUAExportAmendment = @"UNB+UNOA:1+ESDEC33333333:ZZ+AEATADUE:ZZ+200109:1513+<<MSGNO PLACEHOLDER>>++&EE++++1'
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:1:921:UN:ECS003'
BGM+830+<<EXPORT_AMENDMENT_LOCAL_REF_NUMBER_PLACE_HOLDER>>+33'
CST++EX:104:141+A:105:141++:112:141+:113:148'
LOC+42+ES::141:009999'
GIS+0:109:141'
NAD+EX+SUP22222222:P:148++Supplier Test Org+1234 Test Street+Barcelona++98765+ES'
NAD+CN+ESIMP11111111::148++Importer Test Org+Test Street 1234+Madrid++12345+ES'
NAD+2+ESDEC33333333::148+mail.mail@mail.com+Declarant Test Org:::::O'
TOD+++FOB:106'
MOA+ZZZ::EUR'
UNS+D'
CST+1+2203001010FirstSecond:122:148+12.34:117:141+9VA:117:148'
FTX+AAA+++Description1'
UNS+S'
CNT+5:1'
CNT+11:0'
UNT+17+<<MSGNO PLACEHOLDER>>'
UNZ+1+<<MSGNO PLACEHOLDER>>'";

		const string ExpectedStringComplXExportAmendment = @"UNB+UNOA:1+ESDEC33333333:ZZ+AEATADUE:ZZ+200109:1513+<<MSGNO PLACEHOLDER>>++&EE++++1'
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:1:921:UN:ECS003'
BGM+830+<<EXPORT_AMENDMENT_LOCAL_REF_NUMBER_PLACE_HOLDER>>+33'
CST++EX:104:141+X:105:141++:112:141+:113:148'
LOC+42+ES::141:009999'
GIS+0:109:141'
NAD+EX+SUP22222222:P:148++Supplier Test Org+1234 Test Street+Barcelona++98765+ES'
NAD+CN+ESIMP11111111::148++Importer Test Org+Test Street 1234+Madrid++12345+ES'
NAD+2+ESDEC33333333::148+mail.mail@mail.com+Declarant Test Org:::::O'
TOD+++FOB:106'
MOA+ZZZ::EUR'
UNS+D'
CST+1+2203001010FirstSecond:122:148+12.34:117:141'
FTX+AAA+++Description1'
UNS+S'
CNT+5:1'
CNT+11:0'
UNT+17+<<MSGNO PLACEHOLDER>>'
UNZ+1+<<MSGNO PLACEHOLDER>>'";
	}
}
