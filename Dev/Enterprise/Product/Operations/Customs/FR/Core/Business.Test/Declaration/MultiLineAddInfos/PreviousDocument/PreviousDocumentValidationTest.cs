using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class PreviousDocumentValidationTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentValidationTest
	{
		public override void TestClass()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Prerequisite: UCC6 must be false", false, declaration.IsUCC6);
			AssertClassValidationResult(previousDocument, true);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Prerequisite: UCC6 must be true", true, declaration.IsUCC6);
			AssertClassValidationResult(previousDocument, false);
		}

		void AssertClassValidationResult(PreviousDocument previousDocument, bool hasErrorIfEmpty)
		{
			previousDocument.CSI_SubType = ZString.Empty;
			if (hasErrorIfEmpty)
			{
				AssertHasMessageError(previousDocument.CSI_SubTypeInfo, cSI_SubTypeEmpty);
			}
			else
			{
				AssertNoMessageError(previousDocument.CSI_SubTypeInfo, cSI_SubTypeEmpty);
			}

			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			AssertNoMessageError(previousDocument.CSI_SubTypeInfo, cSI_SubTypeEmpty);

			previousDocument.CSI_SubType = "A";
			AssertHasMessageError(previousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_PackType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.Invoices.AddNew().InvoiceLines.AddNew().PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				previousDocument.CSI_PackType = ZString.Empty;
				AssertNoMessageErrors(previousDocument.CSI_PackTypeInfo);

				previousDocument.CSI_PackType = "XX";
				AssertHasMessageError(previousDocument.CSI_PackTypeInfo, ListValidation.InvalidCodeMessageError);

				previousDocument.CSI_PackType = "VG";
				AssertNoMessageErrors(previousDocument.CSI_PackTypeInfo);
			});
		}

		public void TestCheckCSI_UnitOfQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ", "test UQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				previousDocument.CSI_UnitOfQuantity = ZString.Empty;
				AssertNoMessageErrors(previousDocument.CSI_UnitOfQuantityInfo);

				previousDocument.CSI_UnitOfQuantity = "XX";
				AssertHasMessageError(previousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

				previousDocument.CSI_UnitOfQuantity = "UQ";
				AssertNoMessageErrors(previousDocument.CSI_UnitOfQuantityInfo);
			});
		}

		public void TestCheckCSI_Reference_ShouldShowMessageError_WhenReferenceCannotIndexRegTempHeader()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ000046";
			var cusEntryNum = CusEntryNumber.New(ist, CusEntryNumberTypes.France.DDT, Core.Constants.CountryCodes.France);
			cusEntryNum.CE_EntryNum = "A000024";

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			previousDocument.CSI_ReferenceNumber = "FRJ000045";
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, "Could not find matching temporary storage register using reference FRJ000045.");
			previousDocument.CSI_ReferenceNumber = "FRJ000046";
			AssertNoMessageErrors(previousDocument.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_Reference()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Code = ZString.Empty;
			previousDocument.CSI_SubType = ZString.Empty;
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = "TEST";
			previousDocument.CSI_SubType = ZString.Empty;
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = ZString.Empty;
			previousDocument.CSI_SubType = "TEST";
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = "TEST";
			previousDocument.CSI_SubType = "TEST";
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = "TEST";
			previousDocument.CSI_SubType = "TEST";
			previousDocument.CSI_ReferenceNumber = "TEST";
			AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public new void TestTypeCode()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Code = ZString.Empty;
			previousDocument.Validation.ValidateCSI_Code();
			AssertHasNotifications("You have not entered a Type.", previousDocument.CSI_CodeInfo);
			AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			previousDocument.CSI_Code = "AAA";
			AssertNoMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			previousDocument.CSI_Code = "ZZZ";
			AssertNoMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_Reference_N377()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REGISTER HEADER";

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register but document type is not N337.", previousDocument.CSI_ReferenceNumberInfo, "Could not find matching temporary storage register using reference OTHER REGISTER HEADER.");

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateAll();
			AssertHasMessageErrorContaining("A message error is expected when there is no matching register and document type is N337.", previousDocument.CSI_ReferenceNumberInfo, "Could not find matching temporary storage register using reference OTHER REGISTER HEADER.");

			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			AssertNoMessageErrors("No message error is expected when a matching register header is found.", previousDocument.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_ItemNumber_N377()
		{
			var testedMessage = "The entered Goods Item Identifier does not exist for this IST.";

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REGISTER HEADER";

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 1;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line but document type is not N337.", previousDocument.CSI_ItemNumberInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateAll();
			AssertHasMessageErrorContaining("A message error is expected when there is no matching register line and document type is N337.", previousDocument.CSI_ItemNumberInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 99;
			AssertNoMessageErrorContaining("No message error is expected when there is a matching register line.", previousDocument.CSI_ItemNumberInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 1;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_ItemNumberInfo, testedMessage);
		}

		public void TestCheckCSI_PackType_N377()
		{
			var testedMessage = "The entered package type does not equal the package type for this line on the IST.";

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REGISTER HEADER";

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;
			regLine.SRL_PackageType = "PKG";

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_PackType = "CTN";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line package type doesn't match the document pack type but document type is not N337.", previousDocument.CSI_PackTypeInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateAll();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line package type doesn't match the document pack type and document type is N337.", previousDocument.CSI_PackTypeInfo, testedMessage);

			previousDocument.CSI_PackType = "PKG";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line package type matches the document pack type.", previousDocument.CSI_PackTypeInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_PackType = "CTN";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_PackTypeInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_PackType = "PKG";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_PackTypeInfo, testedMessage);
		}

		public void TestCheckCSI_PackQty_N377()
		{
			var testedMessage = "The entered package quantity should be less than or equal to the corresponding line in the IST.";

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REGISTER HEADER";

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;
			regLine.SRL_PackagesRemaining = 9;

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_PackQty = 10;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has not enough quantity but document type is not N337.", previousDocument.CSI_PackQtyInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateAll();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line has not enough quantity and document type is N337.", previousDocument.CSI_PackQtyInfo, testedMessage);

			previousDocument.CSI_PackQty = 9;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has enough quantity.", previousDocument.CSI_PackQtyInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_PackQty = 10;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_PackQtyInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_PackQty = 10;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_PackQtyInfo, testedMessage);
		}

		public void TestCheckCSI_Quantity_N377()
		{
			var testedMessage = "The entered quantity should be less than or equal to the corresponding line in the IST.";

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REGISTER HEADER";

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;

			var transactionLine = regLine.CusTempStorageRegLineTransactions.AddNew();
			transactionLine.SRT_TransactionStatus = "CON";
			transactionLine.SRT_GrossWeight = 999;
			AssertEquals("Prerequisite", 999m, regLine.GrossWeightRemainingCalculated);

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_Quantity = 1000;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has not enough weight but document type is not N337.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateAll();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line has not enough weight and document type is N337.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_Quantity = 999;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has enough weight.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_Quantity = 1000;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_Quantity = 100;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_QuantityInfo, testedMessage);
		}

		public void TestCheckCSI_UnitOfQuantity_N377()
		{
			var testedMessage = "The entered measurement unit does not equal the unit for this line on the IST.";

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REGISTER HEADER";

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;
			regLine.SRL_GrossWeightUQ = "KGM";

			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_UnitOfQuantity = "HLT";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line doesn't have matching unit but document type is not N337.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateAll();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line doesn't have matching unit and document type is N337.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_UnitOfQuantity = "KGM";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has matching unit.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_UnitOfQuantity = "HLT";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_UnitOfQuantity = "HLT";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_QuantityInfo, testedMessage);
		}

		public void TestCheckCSI_DateOfIssue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var additionalInfo = declaration.AdditionalInfos.AddNew();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			var nat_130Message = "[NAT_130] When declaration has E0001 Additional Information, the NMRN document must have a Date of Issue.";
			var nat_259Message = "[NAT_259] When declaration has E0001 Additional Information, the NMRN is document Date of Issue (i.e the status date of IE429 response).";

			using (var context = new PreviousDocumentValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(x => x.IsRuleNAT_130Active);
				context.EnableRule(x => x.ISRuleNAT_259Active);
				previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
				additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
				previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
				AssertHasMessageError(previousDocument.CSI_DateOfIssueInfo, nat_130Message);
				previousDocument.CSI_DateOfIssue = ZDateTime.UtcNow.AddDays(1);
				AssertHasMessageError(previousDocument.CSI_DateOfIssueInfo, nat_259Message);

				previousDocument.CSI_DateOfIssue = ZDateTime.UtcNow.AddDays(-1);
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_130Message);
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_259Message);

				previousDocument.CSI_Code = "NOTNMRN";
				previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_130Message);
				previousDocument.CSI_DateOfIssue = ZDateTime.UtcNow.AddDays(1);
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_259Message);

				previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
				additionalInfo.CSI_Code = "NOTE0001";
				previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_130Message);
				previousDocument.CSI_DateOfIssue = ZDateTime.UtcNow.AddDays(1);
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_259Message);

				additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
				context.DisableRule(x => x.IsRuleNAT_130Active);
				previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_130Message);

				context.DisableRule(x => x.ISRuleNAT_259Active);
				previousDocument.CSI_DateOfIssue = ZDateTime.UtcNow.AddDays(1);
				AssertNoMessageError(previousDocument.CSI_DateOfIssueInfo, nat_259Message);
			}
		}

		public void TestCheckSingleNMRNInDeclarationLevelOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var additionalInfo = declaration.AdditionalInfos.AddNew();
			var previousDocument1 = declaration.PreviousDocuments.AddNew();
			var previousDocument2 = declaration.PreviousDocuments.AddNew();
			var nat_088Message1 = "[NAT_088] There can be only one NMRN document when declaration has E0001 Additional Information.";
			var nat_088Message2 = "[NAT_088] NMRN is only allowed at Declaration level when Declaration has E0001 Additional Information.";

			using (var context = new PreviousDocumentValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(x => x.IsRuleNAT_088Active);
				additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
				previousDocument1.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
				AssertNoMessageError(previousDocument1.CSI_CodeInfo, nat_088Message1);

				previousDocument2.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
				AssertHasMessageError(previousDocument2.CSI_CodeInfo, nat_088Message1);

				additionalInfo.CSI_Code = "NOTE0001";
				previousDocument2.Validation.ValidateCSI_Code();
				AssertNoMessageError(previousDocument2.CSI_CodeInfo, nat_088Message1);

				additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
				context.DisableRule(x => x.IsRuleNAT_088Active);
				previousDocument2.Validation.ValidateCSI_Code();
				AssertNoMessageError(previousDocument2.CSI_CodeInfo, nat_088Message1);

				context.EnableRule(x => x.IsRuleNAT_088Active);
				var invoiceHeader = declaration.Invoices.AddNew();
				var previousDocumentInInoviceLevel = invoiceHeader.PreviousDocuments.AddNew();
				previousDocumentInInoviceLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
				AssertHasMessageError(previousDocumentInInoviceLevel.CSI_CodeInfo, nat_088Message2);
				previousDocumentInInoviceLevel.CSI_Code = "NOTNMRN";
				AssertNoMessageError(previousDocumentInInoviceLevel.CSI_CodeInfo, nat_088Message2);

				context.DisableRule(x => x.IsRuleNAT_088Active);
				previousDocumentInInoviceLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
				AssertNoMessageError(previousDocumentInInoviceLevel.CSI_CodeInfo, nat_088Message2);
			}
		}
	}
}
