using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class BuilderHelperTest
	{
		public static GlbStaff GetStaffAccount(this BusinessObjectFactory factory)
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			staff.StaffPlainTextPassword = "1234";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = CertificateName;
			cert.GP_Certificate = GetCertificateBytes();
			cert.CurrentDecryptedCertificatePassphrase = CertificatePassword;
			cert.GP_UserID = "CertThumbPrint";

			return staff;
		}

		public static ZBlob GetCertificateBytes()
		{
			using (var certificateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.ES.Business.Testing.TestFiles.ESCertificate_password.pfx"))
			using (var memoryStream = new MemoryStream())
			{
				certificateStream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}

		public static ZString CertificatePassword => "password";

		public static SupportingDocument CreateSupportingDocument(this BusinessObjectFactory factory, ZString code, ZString refNumber)
		{
			var supDoc = factory.New<SupportingDocument>();
			supDoc.SuspendValidation();

			supDoc.CSI_Code = code;
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_Status = ZString.Empty;

			return supDoc;
		}

		public static void AddEntryLineDocument<T>(this CusEntryLine entryLine, ZString code, ZString reference, string subType = "", string status = "", decimal quantity = 0m, string unitOfQuantity = "", decimal amount = 0, string currency = "")
			where T : Customs.Business.CusSupportingInfo
		{
			var doc = entryLine.Factory.New<T>();
			doc.CSI_Code = code;
			doc.CSI_Quantity = quantity;
			doc.CSI_UnitOfQuantity = unitOfQuantity;
			doc.CSI_ReferenceNumber = reference;
			doc.CSI_ParentTableCode = entryLine.TablePrefix;
			doc.CSI_ParentID = entryLine.PK;
			doc.CSI_SubType = subType;
			doc.CSI_Status = status;
			doc.CSI_Value = amount;
			doc.CSI_RX_NKCurrency = currency;
		}

		public static void AddEntryHeaderDocument<T>(this CusEntryHeader entryHeader, ZString code, ZString reference, string subType = "", string status = "", decimal quantity = 0m, string unitOfQuantity = "")
			where T : Customs.Business.CusSupportingInfo
		{
			var doc = entryHeader.Factory.New<T>();
			doc.CSI_Code = code;
			doc.CSI_Quantity = quantity;
			doc.CSI_UnitOfQuantity = unitOfQuantity;
			doc.CSI_ReferenceNumber = reference;
			doc.CSI_ParentTableCode = entryHeader.TablePrefix;
			doc.CSI_ParentID = entryHeader.PK;
			doc.CSI_SubType = subType;
			doc.CSI_Status = status;
		}

		public static void AddInterchangeToMessage(this BusinessObjectFactory factory, EDIMessage message, string status, string interchangeNum = "1")
		{
			var interchange = factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_InterchangeNum = interchangeNum;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			message.EM_EI = interchange.PK;
		}

		public static void AddBillDataToDeclaration(JobDeclaration declaration, ZDateTime expectedDate, ZString houseBill, string messageType, string entrySubStyle1, string entrySubStyle2)
		{
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_HouseBill = houseBill;
			var bill = declaration.Bills.Cast<Customs.Business.Bill>().FirstOrDefault(x => x.CU_BillType == Customs.Business.BillTypeList.Codes.HouseBill);
			bill.CU_IssueDate = expectedDate;
			var header = declaration.Invoices.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = entrySubStyle1;
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = entrySubStyle2;
			var invoiceLine2 = header.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
		}

		public static OrgHeader CreateOrgHeader(this BusinessObjectFactory factory, string id, string type, string code, string name, string address, string city, string postCode, string country)
		{
			var org = factory.New<OrgHeader>();
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

		public static OrgHeader GetNewDeclarant(this BusinessObjectFactory factory)
		{
			var declarant = factory.CreateOrgHeader(DeclarantData.Id, DeclarantData.IdType, DeclarantData.Code, DeclarantData.Name, DeclarantData.Address, DeclarantData.City, DeclarantData.PostCode, DeclarantData.Country);
			return declarant;
		}

		public static OrgHeader GetNewImporter(this BusinessObjectFactory factory)
		{
			var importer = factory.CreateOrgHeader(ImporterData.Id, ImporterData.IdType, ImporterData.Code, ImporterData.Name, ImporterData.Address, ImporterData.City, ImporterData.PostCode, ImporterData.Country);
			return importer;
		}

		public static OrgHeader GetNewSupplier(this BusinessObjectFactory factory)
		{
			var supplier = factory.CreateOrgHeader(SupplierData.Id, SupplierData.IdType, SupplierData.Code, SupplierData.Name, SupplierData.Address, SupplierData.City, SupplierData.PostCode, SupplierData.Country);
			supplier.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			return supplier;
		}

		public static JobDeclaration GetNewJobDeclaration(this BusinessObjectFactory factory, GlbStaff staff, OrgHeader supplier = null, OrgHeader importer = null, OrgHeader declarant = null, bool createSecondEntry = false, string messageType = Header.MessageType, string entryInstructionSubStyle1 = Header.EntryInstructionSubStyle1, string entryInstructionSubStyle2 = Header.EntryInstructionSubStyle2, bool createInvLinesInDifferentInvHeaders = false, bool createThirdEntry = false, string entryInstructionStyle1 = "", string entryInstructionStyle2 = "")
		{
			var helper = new ESUniversalReferenceTestDataHelper(factory);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Spain,
				"", EntryLine1.ProcedurePart1, EntryLine1.ProcedurePart2, EntryLine1.ProcedureConcessionPart,
				"AB DESC 1", "EXP", group: "EFD");
			factory.Save();

			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, CanaryIslandCode, "Test 61");

			#region JobDeclaration
			var declaration = factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.ZG_IsTrainingDeclaration = Header.IsTest;

			declaration.JE_UCR = Header.ReferenceNumber;
			declaration.JE_OH_Supplier = supplier == null ? ZGuid.Empty : supplier.PK;
			declaration.JE_OH_Importer = importer == null ? ZGuid.Empty : importer.PK;
			declaration.JE_MessageSubType = Header.MessageSubType;

			if (declarant != null)
			{
				declaration.Declarant.OA_OH = declarant.PK;
				declaration.JE_DeclarantType = DeclarantData.DeclarantType;
				declaration.ZG_OtherEmailAddr = DeclarantData.OtherEmail;
				declaration.ZG_AuthPerDeclaration = true;
			}

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			#region Certificate

			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.JE_CustomsProfile = CertificateName;

			#endregion

			#region InvoiceLine1
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";

			GetNewInvoiceLine(declaration, entryInstructionSubStyle1, invoiceHeader1, entryInstructionStyle1);

			#endregion

			#region InvoiceHeader2

			if (createSecondEntry)
			{
				#region InvoiceLine2

				if (createInvLinesInDifferentInvHeaders)
				{
					var invoiceHeader2 = declaration.Invoices.AddNew();
					invoiceHeader2.JZ_InvoiceNumber = "2";
					GetNewInvoiceLine(declaration, entryInstructionSubStyle2, invoiceHeader2, entryInstructionStyle2);
				}
				else
				{
					GetNewInvoiceLine(declaration, entryInstructionSubStyle2, invoiceHeader1, entryInstructionStyle2);
				}

				#endregion
			}

			#region InvoiceHeader3

			if (createThirdEntry)
			{
				#region InvoiceLine3

				if (createInvLinesInDifferentInvHeaders)
				{
					var invoiceHeader3 = declaration.Invoices.AddNew();
					GetNewInvoiceLine(declaration, Header.EntryInstructionSubStyle3, invoiceHeader3);
				}
				else
				{
					GetNewInvoiceLine(declaration, Header.EntryInstructionSubStyle3, invoiceHeader1);
				}

				#endregion
			}

			#endregion

			#endregion

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			factory.Save();

			return declaration;

			#endregion
		}

		public static JobComInvoiceLine GetNewInvoiceLine(JobDeclaration declaration, string entryInstructionSubStyle, JobComInvoiceHeader invoiceHeader, string entryInstructionStyle = "")
		{
			#region EntryInstruction

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = entryInstructionSubStyle;
			entryInstruction.CEI_Style = entryInstructionStyle;

			#endregion

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			invoiceLine.JI_Tariff = EntryLine1.Tariff;
			invoiceLine.JI_SupplementaryCode1 = EntryLine1.SuppCode1;
			invoiceLine.JI_SupplementaryCode2 = EntryLine1.SuppCode2;
			invoiceLine.JI_Procedure = EntryLine1.Procedure;
			invoiceLine.JI_Description = EntryLine1.GoodsDescription;

			return invoiceLine;
		}

		public static JobDeclaration GetNewDeclarationForExitControlGeneration(this BusinessObjectFactory factory, GlbStaff staff, ZBool createSecondEntry, bool createThirdEntry = false, bool thirdEntryShouldBeAccepted = false,
																				string declarationReference = "ES00001", bool shouldEntriesBeUcc6 = false)
		{
			var declaration = factory.GetNewJobDeclaration(staff, createSecondEntry: createSecondEntry, createThirdEntry: createThirdEntry);
			declaration.JE_DeclarationReference = declarationReference;

			var entryHeader1 = declaration.CustomsEntryHeaders[0];
			entryHeader1.CH_EntryStatus = "CLR";
			entryHeader1.MovementReferenceNumber = "refNum1";
			entryHeader1.CH_BGMReference = "ES00001";
			entryHeader1.ZG_UCC6Version = shouldEntriesBeUcc6 ? UCC6VersionCodes.UCC6 : UCC6VersionCodes.NoUCC6;

			if (createSecondEntry)
			{
				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader2.CH_EntryStatus = "CDA";
				entryHeader2.MovementReferenceNumber = "refNum2";
				entryHeader2.CH_BGMReference = "ES00002";
				entryHeader2.ZG_UCC6Version = shouldEntriesBeUcc6 ? UCC6VersionCodes.UCC6 : UCC6VersionCodes.NoUCC6;
			}

			if (createThirdEntry)
			{
				var entryHeader3 = declaration.CustomsEntryHeaders[2];
				entryHeader3.CH_EntryStatus = thirdEntryShouldBeAccepted ? "CLP" : "AAA";
				entryHeader3.MovementReferenceNumber = "refNum3";
				entryHeader3.CH_BGMReference = "ES00003";
				entryHeader3.ZG_UCC6Version = shouldEntriesBeUcc6 ? UCC6VersionCodes.UCC6 : UCC6VersionCodes.NoUCC6;
			}

			return declaration;
		}

		struct Header
		{
			public const string ReferenceNumber = "Reference";
			public const string MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			public const string MessageSubType = "EX";
			public const string EntryInstructionSubStyle1 = EntrySubStyleList.Codes.A;
			public const string EntryInstructionSubStyle2 = EntrySubStyleList.Codes.B;
			public const string EntryInstructionSubStyle3 = EntrySubStyleList.Codes.C;
			public const bool IsTest = true;
		}

		struct SupplierData
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
		struct ImporterData
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
		struct DeclarantData
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

		struct EntryLine1
		{
			public const int LineNum = 1;
			public const string Tariff = "2203001010";
			public const string SuppCode1 = "First";
			public const string SuppCode2 = "Second";
			public const string Procedure = "1234001";
			public const string ProcedurePart1 = "12";
			public const string ProcedurePart2 = "34";
			public const string ProcedureConcessionPart = "001";
			public const string GoodsDescription = "Description1";
		}

		public const string CertificateName = "TestCert1";
		public const string CanaryIslandCode = "61";
	}
}
