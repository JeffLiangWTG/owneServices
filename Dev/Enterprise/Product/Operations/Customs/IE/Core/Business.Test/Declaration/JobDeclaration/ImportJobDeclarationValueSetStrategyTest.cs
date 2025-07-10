using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportJobDeclarationValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestDefaultImporterChanged_PopulateJE_OH_DutyPayer()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMP1";
			Factory.Save();

			var customsCode = importer.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.IrelandCodeTypes.VatFreeAuthorisation;
			customsCode.SecuredCustomsRegNo = "VatFreeAuthorisation";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			declaration.JE_OH_Importer = importer.PK;
			Assert("Non UCC6 & No TRA CustomsCode", declaration.JE_OH_DutyPayer.IsEmpty);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_OH_Importer = importer.PK;
			Assert("UCC5 & No TRA CustomsCode", declaration.JE_OH_DutyPayer.IsEmpty);

			customsCode = importer.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.IrelandCodeTypes.TraderAccountNumber;
			customsCode.SecuredCustomsRegNo = "TraderAccountNumber";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("UCC5 & Has TRA CustomsCode", importer.PK, declaration.JE_OH_DutyPayer);

			customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber;
			customsCode.SecuredCustomsRegNo = "DefermentApprovalNumber";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;

			var orgImpAddInfo = RegionOrgImpAddInfo.Get(importer, declaration.CountryCode);
			orgImpAddInfo.Deserialise();
			orgImpAddInfo.ZO_OtherDeferType = "E";
			Factory.Save();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_PaymentMethod = "E";
			declaration.JE_DefermentAccountNumber = "123456";
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			CombineAssertions("Test for UCC5", () =>
			{
				entryInstruction.CEI_Style = "H1";
				AssertEquals("UCC5 & H1/H5/I1 & Payment Conditions Met", importer.PK, declaration.JE_OH_DutyPayer);
				entryInstruction.CEI_Style = "H5";
				AssertEquals("UCC5 & H1/H5/I1 & Payment Conditions Met", importer.PK, declaration.JE_OH_DutyPayer);
				entryInstruction.CEI_Style = "I1";
				AssertEquals("UCC5 & H1/H5/I1 & Payment Conditions Met", importer.PK, declaration.JE_OH_DutyPayer);
			});

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			CombineAssertions("Test for UCC6", () =>
			{
				entryInstruction.CEI_Style = "H1";
				AssertEquals("UCC6 & H1/H5/I1 & Payment Conditions Met", importer.PK, declaration.JE_OH_DutyPayer);
				entryInstruction.CEI_Style = "H5";
				AssertEquals("UCC6 & H1/H5/I1 & Payment Conditions Met", importer.PK, declaration.JE_OH_DutyPayer);
				entryInstruction.CEI_Style = "I1";
				AssertEquals("UCC6 & H1/H5/I1 & Payment Conditions Met", importer.PK, declaration.JE_OH_DutyPayer);
			});
		}

		public void TestPopulateJE_DefermentAccountNumber()
		{
			var (declaration, customsCode) = InitializeDataForTestPopulateJE_DefermentAccountNumber(ImportDeclarationApplicationCodeList.Codes.V1, "H1", "123324");
			
			CombineAssertions("Test for UCC5", () =>
			{
				AssertEquals("UCC5 & H1/H5/I1 & Payment Conditions Met", customsCode.SecuredCustomsRegNo, declaration.JE_DefermentAccountNumber);

				(declaration, customsCode) = InitializeDataForTestPopulateJE_DefermentAccountNumber(ImportDeclarationApplicationCodeList.Codes.V1, "H5", "575688");
				AssertEquals("UCC5 & H1/H5/I1 & Payment Conditions Met", customsCode.SecuredCustomsRegNo, declaration.JE_DefermentAccountNumber);

				(declaration, customsCode) = InitializeDataForTestPopulateJE_DefermentAccountNumber(ImportDeclarationApplicationCodeList.Codes.V1, "I1", "987058");
				AssertEquals("UCC5 & H1/H5/I1 & Payment Conditions Met", customsCode.SecuredCustomsRegNo, declaration.JE_DefermentAccountNumber);
			});

			CombineAssertions("Test for UCC6", () =>
			{
				(declaration, customsCode) = InitializeDataForTestPopulateJE_DefermentAccountNumber(ImportDeclarationApplicationCodeList.Codes.V2, "H1", "547354");
				AssertEquals("UCC6 & H1/H5/I1 & Payment Conditions Met", customsCode.SecuredCustomsRegNo, declaration.JE_DefermentAccountNumber);

				(declaration, customsCode) = InitializeDataForTestPopulateJE_DefermentAccountNumber(ImportDeclarationApplicationCodeList.Codes.V2, "H5", "685887");
				AssertEquals("UCC6 & H1/H5/I1 & Payment Conditions Met", customsCode.SecuredCustomsRegNo, declaration.JE_DefermentAccountNumber);

				(declaration, customsCode) = InitializeDataForTestPopulateJE_DefermentAccountNumber(ImportDeclarationApplicationCodeList.Codes.V2, "I1", "347657");
				AssertEquals("UCC6 & H1/H5/I1 & Payment Conditions Met", customsCode.SecuredCustomsRegNo, declaration.JE_DefermentAccountNumber);
			});
		}

		public void TestDefaultImporterChanged_Populate1A05()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var host = Factory.New<OrgCountryData>();
			host.OV_OH_OrgHeader = orgHeader.PK;
			host.OV_RN_NKClientCountryRelation = "IE";

			var economicGroupAddInfoInfo = new EUOrgImpAddInfo((ZPropertyInfoString)host.OV_CustomsEconomicGroupAddInfoInfo);
			economicGroupAddInfoInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var (declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Import, Constants.SupportingDocumentCodes._1A05, Constants.SupportingDocumentCodes._1A01, Constants.SupportingDocumentReferenceReferenceNumbers.IEPOSTPONED);
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for Import", () =>
			{
				AssertEquals("Should create new document", 2, entryInstruction1.SupportingDocuments.Count);
			Assert("Create document is Code 1A05 and ReferenceNumber IEPOSTPONED",
					entryInstruction1.SupportingDocuments.Find(p => p.CSI_Code == "1A05" && p.CSI_ReferenceNumber == "IEPOSTPONED" && p.CSI_AdditionalDescription == "System added supporting document") is not null);

				AssertEquals("Should create new document", 2, entryInstruction2.SupportingDocuments.Count);
				Assert("Create document is Code 1A05 and ReferenceNumber IEPOSTPONED",
					entryInstruction2.SupportingDocuments.Find(p => p.CSI_Code == "1A05" && p.CSI_ReferenceNumber == "IEPOSTPONED" && p.CSI_AdditionalDescription == "System added supporting document") is not null);

				var supportingDocument = entryInstruction3.SupportingDocuments.First();
				AssertEquals("Existed document is Code 1A05 ", "1A05", supportingDocument.CSI_Code);
				AssertEquals("Existed document is ReferenceNumber IEPOSTPONED", "IEPOSTPONED", supportingDocument.CSI_ReferenceNumber);
			});

			declaration.JE_OH_Importer = ZGuid.Empty;

			CombineAssertions("Test for Import after removing fiscal representation", () =>
			{
				AssertEquals("Should remove system added document", 1, entryInstruction1.SupportingDocuments.Count);
				Assert("System-added 1A05 should be removed from entryInstruction1",
					!entryInstruction1.SupportingDocuments.Cast<SupportingDocument>().Any(p => p.CSI_Code == "1A05" && p.CSI_ReferenceNumber == "IEPOSTPONED" && p.CSI_AdditionalDescription == "System added supporting document"));

				AssertEquals("Should remove system added document", 1, entryInstruction2.SupportingDocuments.Count);
				Assert("System-added 1A05 should be removed from entryInstruction2",
					!entryInstruction2.SupportingDocuments.Cast<SupportingDocument>().Any(p => p.CSI_Code == "1A05" && p.CSI_ReferenceNumber == "IEPOSTPONED" && p.CSI_AdditionalDescription == "System added supporting document"));
				
				AssertEquals("Should not remove manually added document", 1, entryInstruction3.SupportingDocuments.Count);
			});

			(declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Export, Constants.SupportingDocumentCodes._1A05, Constants.SupportingDocumentCodes._1A01, Constants.SupportingDocumentReferenceReferenceNumbers.IEPOSTPONED);
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for Export", () =>
			{
				AssertEquals("Should not create new document", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction3.SupportingDocuments.Count);
			});
		}

		public void TestDefaultImporterChanged_Populate1A01()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var expectedReferenceNumber = "Vat Free Authorisation";
			var (declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Import, Constants.SupportingDocumentCodes._1A01, Constants.SupportingDocumentCodes._1A05, expectedReferenceNumber);
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for Invalid Importer", () =>
			{
				AssertEquals("Should not create new document", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction3.SupportingDocuments.Count);
			});

			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.IrelandCodeTypes.VatFreeAuthorisation;
			customsCode.SecuredCustomsRegNo = expectedReferenceNumber;

			(declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Import, Constants.SupportingDocumentCodes._1A01, Constants.SupportingDocumentCodes._1A05, expectedReferenceNumber);
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for UCC6", () =>
			{
				AssertEquals("Should not create new document", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction3.SupportingDocuments.Count);
			});

			(declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Import, Constants.SupportingDocumentCodes._1A01, Constants.SupportingDocumentCodes._1A05, expectedReferenceNumber);
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			entryInstruction1.CEI_Style = "H1";
			entryInstruction2.CEI_Style = "H6";
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for UCC5", () =>
			{
				AssertEquals("Should create new document", 2, entryInstruction1.SupportingDocuments.Count);
				Assert("Create document is Code 1A01 and ReferenceNumber Vat Free Authorisation", entryInstruction1.SupportingDocuments.Find(p => p.CSI_Code == "1A01" && p.CSI_ReferenceNumber == expectedReferenceNumber) is not null);

				AssertEquals("Should not create new document for H6", 1, entryInstruction2.SupportingDocuments.Count);

				AssertEquals("Should not create new document because already existed", 1, entryInstruction3.SupportingDocuments.Count);
				var supportingDocument = entryInstruction3.SupportingDocuments.First();
				AssertEquals("Existed document is Code 1A01 ", "1A01", supportingDocument.CSI_Code);
				AssertEquals("Existed document is ReferenceNumber Vat Free Authorisation", expectedReferenceNumber, supportingDocument.CSI_ReferenceNumber);
			});

			declaration.JE_OH_Importer = ZGuid.Empty;

			CombineAssertions("Test for Importer without VAT Free Authorisation", () =>
			{
				AssertEquals("Should remove system added document", 1, entryInstruction1.SupportingDocuments.Count);
				Assert("System-added 1A01 should be removed from entryInstruction1",
					!entryInstruction1.SupportingDocuments.Cast<SupportingDocument>().Any(p => p.CSI_Code == "1A01" && p.CSI_ReferenceNumber == expectedReferenceNumber && p.CSI_AdditionalDescription == "System added supporting document"));

				AssertEquals("No document is removed for H6", 1, entryInstruction2.SupportingDocuments.Count);

				AssertEquals("User-added document should not be removed", 1, entryInstruction3.SupportingDocuments.Count);
			});
		}

		public void TestDefaultImporterChanged_Populate1A03()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var expectedReferenceNumber = "Zero-Rated VAT Authorisation";
			var (declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Import, Constants.SupportingDocumentCodes._1A03, Constants.SupportingDocumentCodes._1A05, expectedReferenceNumber);
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for Invalid Importer", () =>
			{
				AssertEquals("Should not create new document", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction3.SupportingDocuments.Count);
			});

			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.IrelandCodeTypes.VatZeroRatedAct2010;
			customsCode.SecuredCustomsRegNo = expectedReferenceNumber;

			(declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Import, Constants.SupportingDocumentCodes._1A03, Constants.SupportingDocumentCodes._1A05, expectedReferenceNumber);
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for UCC6", () =>
			{
				AssertEquals("Should not create new document", 1, entryInstruction1.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction2.SupportingDocuments.Count);
				AssertEquals("Should not create new document", 1, entryInstruction3.SupportingDocuments.Count);
			});

			(declaration, entryInstruction1, entryInstruction2, entryInstruction3) = InitializeDataForTestDefaultImporterChanged(IEJobMessageTypeList.Codes.Import, Constants.SupportingDocumentCodes._1A03, Constants.SupportingDocumentCodes._1A05, expectedReferenceNumber);
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			entryInstruction1.CEI_Style = "H1";
			entryInstruction2.CEI_Style = "H6";
			declaration.JE_OH_Importer = orgHeader.PK;

			CombineAssertions("Test for UCC5", () =>
			{
				AssertEquals("Should create new document", 2, entryInstruction1.SupportingDocuments.Count);
				Assert("Create document is Code 1A03 and ReferenceNumber Zero-Rated VAT Authorisation", entryInstruction1.SupportingDocuments.Find(p => p.CSI_Code == "1A03" && p.CSI_ReferenceNumber == expectedReferenceNumber) is not null);

				AssertEquals("Should not create new document for H6", 1, entryInstruction2.SupportingDocuments.Count);

				AssertEquals("Should not create new document because already existed", 1, entryInstruction3.SupportingDocuments.Count);
				var supportingDocument = entryInstruction3.SupportingDocuments.First();
				AssertEquals("Existed document is Code 1A03 ", "1A03", supportingDocument.CSI_Code);
				AssertEquals("Existed document is ReferenceNumber Zero-Rated VAT Authorisation", expectedReferenceNumber, supportingDocument.CSI_ReferenceNumber);
			});

			declaration.JE_OH_Importer = ZGuid.Empty;

			CombineAssertions("Test for Importer without Zero-Rated VAT Authorisation", () =>
			{
				AssertEquals("Should remove system added document", 1, entryInstruction1.SupportingDocuments.Count);
				Assert("System-added 1A03 should be removed from entryInstruction1",
					!entryInstruction1.SupportingDocuments.Cast<SupportingDocument>().Any(p => p.CSI_Code == "1A03" && p.CSI_ReferenceNumber == expectedReferenceNumber && p.CSI_AdditionalDescription == "System added supporting document"));

				AssertEquals("No document is removed for H6", 1, entryInstruction2.SupportingDocuments.Count);

				AssertEquals("User-added document should not be removed", 1, entryInstruction3.SupportingDocuments.Count);
			});
		}

		(JobDeclaration, CusEntryInstruction, CusEntryInstruction, CusEntryInstruction) InitializeDataForTestDefaultImporterChanged(string messageType, string expectedCode, string otherCode, string expectedReferenceNumber)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var supportingDocument1 = entryInstruction1.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = expectedCode;
			supportingDocument1.CSI_ReferenceNumber = "Other Reference";

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var supportingDocument2 = entryInstruction2.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = otherCode;
			supportingDocument2.CSI_ReferenceNumber = expectedReferenceNumber;

			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			var supportingDocument3 = entryInstruction3.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = expectedCode;
			supportingDocument3.CSI_ReferenceNumber = expectedReferenceNumber;

			return (declaration, entryInstruction1, entryInstruction2, entryInstruction3);
		}

		(JobDeclaration, OrgCusCode) InitializeDataForTestPopulateJE_DefermentAccountNumber(string applicationCode, string ceiStyle, string actualCode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = applicationCode;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			var customsCode = importer.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber;
			customsCode.SecuredCustomsRegNo = actualCode;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_PaymentMethod = "E";
			declaration.JE_OH_DutyPayer = ZGuid.Empty;
			entryInstruction.CEI_Style = ceiStyle;
			declaration.JE_OH_DutyPayer = declaration.JE_OA_DeclarantAddress;

			return (declaration, customsCode);
		}
	}
}
