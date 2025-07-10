using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	class ImportJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		#region TestCheckJE_GS_NKCusAgent
		public void TestCheckJE_GS_NKCusAgent()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "AXG";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			AssertHasMessageError(declaration.JE_GS_NKCusAgentInfo, ImportJobDeclarationValidation.WorkPhoneIsRequired);

			broker.GS_WorkPhone = "+86 025 1236 5987";
			declaration.Validation.ValidateJE_GS_NKCusAgent();

			AssertNoMessageError(declaration.JE_GS_NKCusAgentInfo, ImportJobDeclarationValidation.WorkPhoneIsRequired);

			AssertNoMessageErrorContaining("No message error", declaration.JE_GS_NKCusAgentInfo, "The following characters are not allowed in the full name: ");
			broker.GS_FullName = "赵四!|";
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasMessageErrorContaining("No message error", declaration.JE_GS_NKCusAgentInfo, "The following characters are not allowed in the full name: ");
			AssertHasMessageErrorContaining("No message error", declaration.JE_GS_NKCusAgentInfo, "The following characters are not allowed in the full name: '赵','四','!','|'");
		}
		#endregion

		#region TestCheckJE_OH_Importer

		public void TestCheckJE_OH_Importer()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_ImporterInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_OH_ImporterInfo);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_ImporterInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_OH_ImporterInfo);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_OH_ImporterInfo);

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "123ABC";
			declaration.JE_OH_Importer = importer.PK;
			AssertNoWarning(declaration.JE_OH_ImporterInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			importer.OH_FullName = "123ABC– ";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasWarning(declaration.JE_OH_ImporterInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		public void TestCheckJE_OH_Importer_EffectiveCFIAFeePaymentMethod()
		{
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAPaymentMethodNotSpecifiedErrorText);
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.No;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAPaymentMethodNotSpecifiedErrorText);
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;
			OrgImpAddInfo.Get(importer).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Importer;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAPaymentMethodNotSpecifiedErrorText);

			AssertHasMessageError(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);
			importer.SetCustomsCode(OrgCusCode.CACodeTypes.CFIAAccountNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "12345");
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);

			var proxy = Factory.New<OrgHeader>();
			declaration.Branch.GB_OH_OrgProxy = proxy.PK;
			OrgImpAddInfo.Get(importer).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Broker;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasWarning(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
			proxy.SetCustomsCode(OrgCusCode.CACodeTypes.CFIAAccountNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "23456");
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoWarning(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
		}

		public void TestImporterCanadianAddress()
		{
			const string msgError = "The Importer Documentary Address is not in Canada, and you have not specified a Canadian Purchaser or Consignee on the Invoice Header tab.";
			var importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			declaration.JE_OH_Importer = importer.PK;
			var invoice1 = declaration.Invoices.AddNew();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, msgError);
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, msgError);
			var buyer = Factory.New<OrgHeader>();
			buyer.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			invoice1.JZ_OH_Buyer = buyer.PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, msgError);
			buyer.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, msgError);
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OH_Buyer = ZGuid.Empty;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, msgError);
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			invoice2.JZ_OH_Consignee = consignee.PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, msgError);
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, msgError);
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, msgError);
		}

		public void TestImporterCanadianAddress_NoValidationsOnLVSJobs()
		{
			const string msgError = "The Importer Documentary Address is not in Canada, and you have not specified a Canadian Purchaser or Consignee on the Invoice Header tab.";
			var importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			declaration.JE_OH_Importer = importer.PK;
			var invoice1 = declaration.Invoices.AddNew();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, msgError);
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, msgError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, msgError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, msgError);
		}

		public void TestCheckJE_OH_Importer_BusinessNumberForImportExport()
		{
			var messageError = "This importer does not have a business number for import/export (BRM) or Business Number Importer Commercial (CAI) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
			var importer = Factory.New<OrgHeader>();

			declaration.JE_OH_Importer = importer.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			var cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RMASDF");
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			cusCode.OK_CustomsRegNo = "123456789RM0001";
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			importer.CustomsCodes.RemoveAndDelete(cusCode);
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);

			cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "12345678");
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);

			cusCode.OK_CustomsRegNo = "123456789";
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);
		}

		public void TestCheckJE_OH_Importer_BusinessNumberForImportExportWhenCADEnabled()
		{
			var messageError = "This importer does not have a Business Number Importer Commercial (CAI) or business number for import/export (BRM) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
			AssertCheckJE_OH_ImporterWhenCADEnabled(messageError,
				OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial,
				"123456789",
				CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat,
				false);

			messageError = "This importer does not have a Business Number Importer Non-Commercial (BNC) or business number for import/export (BRM) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
			AssertCheckJE_OH_ImporterWhenCADEnabled(messageError,
				OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial,
				"123456789RM0002",
				CanadianCustomsCodeValidator.Constants.BusinessNumberImporterNonCommercialFormat,
				true);
		}

		void AssertCheckJE_OH_ImporterWhenCADEnabled(string messageError, string codeType, string codeValue, string validationFormat, bool isCasual)
		{
			var importer = Factory.New<OrgHeader>();
			var importer1 = Factory.New<OrgHeader>();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				if (isCasual)
				{
					var header = declaration.Invoices.AddNew();
					var line = header.JobComInvoiceLines.AddNew();
					line.CA_IsCasualImport = true;
				}

				declaration.JE_OH_Importer = importer.PK;
				AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, validationFormat);

				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, validationFormat);

				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				var cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RMASDF");
				declaration.JE_OH_Importer = importer.PK;
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
				AssertHasMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

				cusCode.OK_CustomsRegNo = "123456789RM0001";
				declaration.JE_OH_Importer = importer.PK;
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

				cusCode = importer.CustomsCodes.AddNew(codeType, "12345678");
				declaration.JE_OH_Importer = importer.PK;
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
				AssertHasMessageError(declaration.JE_OH_ImporterInfo, validationFormat);

				cusCode.OK_CustomsRegNo = codeValue;
				declaration.JE_OH_Importer = importer.PK;
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, validationFormat);

				importer.CustomsCodes.RemoveAndDeleteAll();
				importer1.CustomsCodes.AddNew(codeType, codeValue);
				declaration.ImporterOfRecordAddress.OrganisationPK = importer1.PK;
				declaration.Validation.ValidateJE_OH_Importer();
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, validationFormat);
			}
		}

		public void TestCheckJE_OH_Importer_ValidateBondType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.NotOnPortal;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData1.PW_BondNumber = "00001";
			bondData1.PW_SuretyCode = "001";
			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.OnPortal;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData2.PW_BondNumber = "00002";
			bondData2.PW_SuretyCode = "002";

			var bondNotOnPortal = ImportAddInfoJobDeclarationValidation.ImporterIsNotOnCARMPortal;
			var bondWillBRequired = ImportAddInfoJobDeclarationValidation.ClinetIsInCARMPortalButNoBondOnFile;

			declaration.JE_OH_Importer = org.PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, bondNotOnPortal);
			AssertHasWarning(declaration.JE_OH_ImporterInfo, bondWillBRequired);

			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			Factory.Save();
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, bondNotOnPortal);
			AssertNoWarning(declaration.JE_OH_ImporterInfo, bondWillBRequired);
		}

		public void TestImporterAscPasswordNotSpecified()
		{
			declaration.Validation.ValidateJE_OH_Importer();
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			var orgAddInfo = declaration.ImporterAddInfo;
			orgAddInfo.ZO_AccountSecurityNumber = "1";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.AscPasswordNotSpecifiedErrorText);
			orgAddInfo.ZO_AccountSecirityPassword = "1";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.AscPasswordNotSpecifiedErrorText);
		}

		public void TestCheckImporterWithNoIOR()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "2701";

			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument1 = MakePoaDoc(importer);
				MakePoaDocAttr(poaDocument1, "DIRECTION", "EXP");

				var importer2 = Factory.New<OrgHeader>();
				var poaDocument2 = MakePoaDoc(importer2);
				MakePoaDocAttr(poaDocument2, "DIRECTION", "IMP");
				MakePoaDocAttr(poaDocument2, "PORT OF ENTRY", "1011");

				var importer3 = Factory.New<OrgHeader>();
				var poaDocument3 = MakePoaDoc(importer3);
				MakePoaDocAttr(poaDocument3, "DIRECTION", "IMP");
				MakePoaDocAttr(poaDocument3, "PORT OF ENTRY", "2701");

				var usCompany = Factory.NewWithValidTestData<GlbCompany>();
				usCompany.GC_Code = "~US";
				var importer4 = Factory.New<OrgHeader>();
				var poaDocument4 = MakePoaDoc(importer4);
				MakePoaDocAttr(poaDocument4, "COMPANY CODE", usCompany.GC_Code);

				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				var messageError = "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for 'IMP' Direction.";
				AssertHasMessageErrorContaining(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				declaration.JE_OH_Importer = importer.PK;
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

				messageError = "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for '2701' Port of Entry.";
				declaration.ClearAllNotifications();
				declaration.JE_OH_Importer = importer2.PK;
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

				declaration.ClearAllNotifications();
				declaration.JE_OH_Importer = importer3.PK;
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

				messageError = "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for 'EDI' Company.";
				declaration.ClearAllNotifications();
				declaration.JE_OH_Importer = importer4.PK;
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);
			}
		}

		static JobRequiredDocument MakePoaDoc(OrgHeader importer)
		{
			var poaDocument = importer.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddYears(-1);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			return poaDocument;
		}

		static JobRequiredDocAttrib MakePoaDocAttr(JobRequiredDocument document, ZString attribName, ZString attribValue)
		{
			var poaDocumentAttrib = document.Attributes.AddNew();
			poaDocumentAttrib.D0_AttribName = attribName;
			poaDocumentAttrib.D0_AttribDisplayValue = attribValue;
			return poaDocumentAttrib;
		}

		#endregion

		#region TestCheckJE_TotalNoOfPacks

		public void TestCheckJE_TotalNoOfPacks()
		{
			declaration.JE_TotalNoOfPacksPackType = "";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(declaration.JE_TotalNoOfPacksInfo, ValidateForMessageType.ACROSS, declaration);
			declaration.JE_TotalNoOfPacks = 0;
			AssertEquals("Enter the total number of outer packages for this shipment, as distinct from the number of units. The total number of outer packages is what the goods are packed into and the type of packaging in which the goods are contained or wrapped. Example: Drums, Barrels, Pallets etc.", DataBoundResourceStrings.GetDataForProperty(declaration.JE_TotalNoOfPacksInfo).FullDescription);
		}

		#endregion

		#region TestCheckJE_OH_ShippingLine

		public void TestCheckJE_OH_ShippingLine()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "12345";
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_OH_ShippingLineInfo);
			number.CE_EntryNum = "2ITN12345";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_ShippingLineInfo);
			ValidationTestHelper.ReSetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			number.CE_EntryNum = "2ITN12346";
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_OH_ShippingLineInfo);
		}

		#endregion

		#region TestCheckJE_MessageSubType

		public void TestCheckJE_MessageSubType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_MessageSubTypeInfo, "AA", B3EntryTypeList.Codes.Confirming);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(declaration.JE_MessageSubTypeInfo, ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			AssertHasMessageErrorContaining(declaration.JE_MessageSubTypeInfo, "Paper only entry type; electronic entry not valid.");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			AssertNoMessageErrorContaining(declaration.JE_MessageSubTypeInfo, "Paper only entry type; electronic entry not valid.");
			ValidationTestHelper.ReSetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			AssertNoMessageErrorContaining(declaration.JE_MessageSubTypeInfo, "Paper only entry type; electronic entry not valid.");

			declaration.NotesOfDeclarationOrShipment.RemoveAndDeleteAll();
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.CashD;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");

			var notes = declaration.NotesOfDeclarationOrShipment.AddNew();
			notes.ST_Description = "Message to Print on CAD Document";
			notes.ST_NoteText = "Blah";

			ValidationTestHelper.ReSetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.CashD;
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, "Please enter a message to print on the CAD (Notes).");

			var messageError = "The Proxy Organization of the branch of the Customs Broker specified on this job, or the current login branch, must have a Business Number for Low Value Shipments (BRL) defined. Please go to Config > Registration Numbers/Codes of the Organization specified on the appropriate branch.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, messageError);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, messageError);

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "123456789RL0001");
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, messageError);
		}

		public void TestCheckJE_MessageSubTypeForIM2()
		{
			var im2 = declaration.GetNewCopyToB2Declaration();
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, im2);
			im2.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			AssertNoMessageErrorContaining(im2.JE_MessageSubTypeInfo, "Paper only entry type; electronic entry not valid.");
			im2.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			AssertNoMessageErrorContaining(im2.JE_MessageSubTypeInfo, "Paper only entry type; electronic entry not valid.");
			im2.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, im2);
			im2.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			AssertHasMessageErrorContaining(im2.JE_MessageSubTypeInfo, "Paper only entry type; electronic entry not valid.");
		}

		public void TestCheckJE_MessageSubType_WHS()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B00000001", "00000001", B3EntryTypeList.Codes.Warehouse10, helper.Importer, helper.Warehouse);
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;
			declaration.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			Factory.Save();

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			AssertHasError(declaration.JE_MessageSubTypeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			AssertNoError(declaration.JE_MessageSubTypeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);

			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Factory.Save();

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			AssertHasError(declaration.JE_MessageSubTypeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			AssertNoError(declaration.JE_MessageSubTypeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			AssertNoError(declaration.JE_MessageSubTypeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);
		}

		#endregion

		#region TestCheckJE_EntryAuthorisationDate

		[TestDate(2023, 01, 21)]
		public void TestJE_EntryAuthorisationDate()
		{
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_EntryAuthorisationDateInfo);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_EntryAuthorisationDateInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);

			const string messageError = "Period should be the current, or previous month and year for LVS.";
			declaration.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2022, 11, 1);
			AssertHasMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2022, 12, 1);
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2023, 01, 1);
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2023, 02, 1);
			AssertHasMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			declaration.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_MessageStatus = "";
			declaration.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.CA_LVSCloseDate = ZDateTime.Now;
			declaration.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.CA_LVSCloseDate = ZDateTime.Invalid;
			declaration.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2023, 01, 1);
			AssertNoMessageError(declaration.JE_EntryAuthorisationDateInfo, messageError);

			declaration.DeclarationValidator.OverrideValidationType = ValidateForMessageType.ACROSS;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_EntryAuthorisationDateInfo);

			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvxJob.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			lvxJob.JE_EntryAuthorisationDate = new ZDateTime(2022, 11, 1);
			AssertHasMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			lvxJob.JE_EntryAuthorisationDate = new ZDateTime(2022, 12, 1);
			AssertNoMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			lvxJob.JE_EntryAuthorisationDate = ZDateTime.Now;
			AssertNoMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			lvxJob.JE_EntryAuthorisationDate = new ZDateTime(2023, 02, 1);
			AssertHasMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			lvxJob.LVXInvoiceHeader.AttachToAdditionalDeclaration(declaration);
			declaration.JE_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			lvxJob.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			declaration.JE_MessageStatus = "";
			lvxJob.Validation.ValidateJE_EntryAuthorisationDate();
			AssertHasMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			declaration.CA_LVSCloseDate = ZDateTime.Now;
			lvxJob.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			declaration.CA_LVSCloseDate = ZDateTime.Invalid;
			lvxJob.Validation.ValidateJE_EntryAuthorisationDate();
			AssertHasMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);

			lvxJob.JE_EntryAuthorisationDate = new ZDateTime(2023, 01, 1);
			AssertNoMessageError(lvxJob.JE_EntryAuthorisationDateInfo, messageError);
		}

		#endregion

		#region TestCheckJE_WarehouseReleaseDate

		public void TestCheckJE_WarehouseReleaseDate()
		{
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			ValidationTestHelper.AssertIfIsEnteredMessageErrorForValidationType(declaration.JE_WarehouseReleaseDateInfo, ValidateForMessageType.ACROSS, declaration);
		}

		#endregion

		#region TestChechJE_MergeBy

		public void TestChechJE_MergeBy_LVX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			declaration.JE_MergeBy = B3MergeByList.Codes.NotMerge;

			AssertNoErrors(declaration.JE_MergeByInfo);
		}

		#endregion

		#region TestCheckJE_RL_NKFinalDestination

		public void TestCheckJE_RL_NKFinalDestination()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "XX";
			AssertHasMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertNoMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);
			declaration.JE_RL_NKFinalDestination = "";
		}

		#endregion

		#region TestCheckJE_RL_NKOrigin

		public void TestCheckJE_RL_NKOrigin()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "XX";
			AssertHasMessageErrors(declaration.JE_RL_NKOriginInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoMessageErrors(declaration.JE_RL_NKOriginInfo);
			declaration.JE_RL_NKOrigin = "";
		}

		#endregion

		#region TestCheckJE_RL_NKPortOfArrival

		public void TestCheckJE_RL_NKPortOfArrival()
		{
			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			port.RL_IATA = ZString.Empty;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_RL_NKPortOfArrival = port.RL_Code;

			declaration.SuppressShipmentRelatedFields = true;
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.SuppressShipmentRelatedFields = false;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = port.RL_Code;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "XX";
			AssertHasMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.SuppressShipmentRelatedFields = true;
			AssertHasMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "";
		}

		#endregion

		#region TestCheckJE_RL_NKPortOfLoading

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_RL_NKPortOfLoading = "XX";

			declaration.SuppressShipmentRelatedFields = true;
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfLoadingInfo);

			declaration.SuppressShipmentRelatedFields = false;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfLoadingInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "XX";
			AssertHasMessageErrors(declaration.JE_RL_NKPortOfLoadingInfo);

			declaration.JE_RL_NKPortOfLoading = "";
		}

		public void TestCheckNoValidationWhenControlsHided()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_RL_NKPortOfLoading = "XX";
			declaration.JE_RL_NKPortOfArrival = "YY";
			declaration.SuppressShipmentRelatedFields = true;
			AssertNoMessageErrors("No Message error as Transport Mode Rail JE_RL_NKPortOfLoadingInfo.", declaration.JE_RL_NKPortOfLoadingInfo);
			AssertNoMessageErrors("No Message error as Transport Mode Rail JE_RL_NKPortOfArrivalInfo.", declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.SuppressShipmentRelatedFields = false;
			declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertHasMessageErrors("Should have message error", declaration.JE_RL_NKPortOfLoadingInfo);
			AssertHasMessageErrors("No Message error as Transport Mode Rail doesn't have Port Of Loading control on a form.", declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.SuppressShipmentRelatedFields = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertHasMessageErrors("Should have Message error as Transport Mode Sea", declaration.JE_RL_NKPortOfArrivalInfo);
		}

		#endregion

		#region TestCheckJE_TransportMode

		public void TestCheckJE_TransportMode()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "1234";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsRoad = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);

			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "2345";
			carrier2.ZZ4_Description = "Carrier Name";
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier2.ZZ4_IsSea = true;
			carrier2.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CarrierCode = "1234";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasWarning(declaration.JE_CarrierCodeInfo, "Carrier not allowed for mode of transport 'SEA'");

			declaration.JE_CarrierCode = "2345";
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoWarning(declaration.JE_CarrierCodeInfo, "Carrier not allowed for mode of transport 'SEA'");

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasWarning(declaration.JE_CarrierCodeInfo, "Carrier not allowed for mode of transport 'ROA'");

			declaration.JE_CarrierCode = "1234";
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoWarning(declaration.JE_CarrierCodeInfo, "Carrier not allowed for mode of transport 'ROA'");
		}

		#endregion

		#region TestCheckJE_TransportMode

		public void TestCheckJE_TotalNoOfPacksPackTypeIsAValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"PK", "Package", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var newFactory = new BusinessObjectFactory(); //Cached List is already loaded as blank from the setup
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_TotalNoOfPacksPackTypeInfo, "PKG", "PK");
		}

		#endregion

		#region TestCheckJE_LocationOfGoods

		public void TestCheckJE_LocationOfGoods()
		{
			var subLocation1 = CACSubLocationTest.CreateSubLocation(Factory, "XXX3", port: "00X1");
			var subLocation2 = CACSubLocationTest.CreateSubLocation(Factory, "XXX4", port: "00X2");
			var subLocation3 = CACSubLocationTest.CreateSubLocation(Factory, "4570", port: "0497");
			Factory.Save();

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_LocationOfGoodsInfo, "AAAA", "4570");
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertHasMessageError(declaration.JE_LocationOfGoodsInfo, ImportJobDeclarationValidation.LocationOfGoodsMustBeEntered);
			declaration.JE_LocationOfGoods = "XX";
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, ImportJobDeclarationValidation.LocationOfGoodsMustBeEntered);
			declaration.JE_LocationOfGoods = ZString.Empty;
			declaration.CA_SubLocationName = "XX";
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, ImportJobDeclarationValidation.LocationOfGoodsMustBeEntered);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_LocationOfGoodsInfo);

			declaration.JE_CustomsOffice = "0X2";
			declaration.JE_LocationOfGoods = "XXX3";
			AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, "The Customs Port of Clearance is not valid for this Sub-Location.");
			AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, "Either the Sub-Location code is incorrect or the Port of Clearance should be X1");
			declaration.JE_LocationOfGoods = "XXX4";
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, "The Customs Port of Clearance is not valid for this Sub-Location.");
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, "Either the Sub-Location code is incorrect or the Port of Clearance should be X1");
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_LocationOfGoods = "XXX3";
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, "The Customs Port of Clearance is not valid for this Sub-Location.");
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, "Either the Sub-Location code is incorrect or the Port of Clearance should be X1");
		}

		#endregion

		#region TestCheckJE_CustomsOffice

		public void TestCheckJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "X234", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsOfficeInfo, "BLA", "X234");
			declaration.JE_CustomsOffice = ZString.Empty;
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, "You have not entered a Port Of Clearance.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, "You have not entered a Reporting Port.");
		}

		#endregion

		#region TestValidateAdditionalConsignee

		public void TestValidateAdditionalConsignee()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var additionalConsignee = declaration.DocAddresses.AddNew();
			additionalConsignee.E2_AddressType = DocAddressTypes.Codes.AdditionalConsignee;
			Factory.Save();
			var validation = new ImportJobDeclarationValidation(declaration);
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});

			additionalConsignee.Delete();

			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});
		}

		#endregion

		#region TestValidateManufacturer

		public void TestValidateManufacturer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var manufacturer = declaration.DocAddresses.AddNew();
			manufacturer.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			Factory.Save();
			var validation = new ImportJobDeclarationValidation(declaration);
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});

			manufacturer.Delete();

			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});
		}

		#endregion

		#region TestValidateAdditionalDeliveryAddress

		public void TestValidateAdditionalDeliveryAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var additionalDeliveryAddress = declaration.DocAddresses.AddNew();
			additionalDeliveryAddress.E2_AddressType = DocAddressTypes.Codes.AdditionalDeliveryAddress;
			Factory.Save();
			var validation = new ImportJobDeclarationValidation(declaration);
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});

			additionalDeliveryAddress.Delete();

			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});
		}

		#endregion

		#region TestValidateOGDProcessInspectionLPCO

		public void TestValidateOGDProcessInspectionLPCO()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var ogdProcessInspectionLPCO = declaration.DocAddresses.AddNew();
			ogdProcessInspectionLPCO.E2_AddressType = DocAddressTypes.Codes.OGDProcessInspectionLPCO;
			Factory.Save();
			var validation = new ImportJobDeclarationValidation(declaration);
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});

			ogdProcessInspectionLPCO.Delete();

			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});
		}

		#endregion

		#region TestValidateCFIAPaymentParty

		public void TestValidateCFIAPaymentParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var cfiaPaymentParty = declaration.DocAddresses.AddNew();
			cfiaPaymentParty.E2_AddressType = DocAddressTypes.Codes.CFIAPaymentParty;
			Factory.Save();
			var validation = new ImportJobDeclarationValidation(declaration);
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});

			cfiaPaymentParty.Delete();

			AssertNoExceptionThrown(() =>
			{
				validation.ValidateAll();
			});
		}

		#endregion

		#region TestPackagesActualPackageCount
		public void TestPackagesActualPackageCount_NoPackageValidation()
		{
			const string errorMessage = "No packages have been associated with a bill of lading, which is required for IID filings";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.IsEnableACROSSValidation = true;

			declaration.Validation.ValidatePackagesActualPackageCount();
			AssertHasMessageError(declaration.PackagesActualPackageCountInfo, errorMessage);

			declaration.JE_HouseBill = "HWB1234";
			var packGroup = declaration.PackingGroups.AddNew();
			var package = packGroup.Packages.AddNew();
			package.CW_PackQty = 100;

			declaration.Validation.ValidatePackagesActualPackageCount();
			AssertNoMessageError(declaration.PackagesActualPackageCountInfo, errorMessage);
		}

		#endregion

		#region CheckJE_CarrierCode

		public void TestCheckJE_CarrierCodeWhenIID()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "1234";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsRoad = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(dec.JE_CarrierCodeInfo, "AAAA", "1234");
			dec.CargoControlNumbers.DeleteAll();

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, dec);
			ValidationTestHelper.SetUpHasUSPlaceOfExportInvoice(dec);
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_CarrierCode = ZString.Empty;
			dec.Validation.ValidateJE_CarrierCode();
			AssertHasMessageErrorContaining(dec.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.Validation.ValidateJE_CarrierCode();
			AssertHasMessageErrorContaining(dec.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_TransportMode = TransportTypeList.Codes.Road;
			dec.Validation.ValidateJE_CarrierCode();
			AssertNoMessageErrorContaining(dec.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var messageError = ImportJobDeclarationValidation.YouHaveEnteredMoreThan999CCNs;
			for (var i = 0; i < 999; i++)
			{
				dec.CargoControlNumbers.AddNew("C" + i);
			}
			dec.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(dec.JE_CarrierCodeInfo, messageError);
			dec.CargoControlNumbers.AddNew("C999");
			dec.Validation.ValidateJE_CarrierCode();
			AssertHasMessageError(dec.JE_CarrierCodeInfo, messageError);
			dec.CargoControlNumbers.RemoveAndDeleteAll();
			dec.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(dec.JE_CarrierCodeInfo, messageError);
		}

		public void TestCheckJE_CarrierCode()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "1234";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsRoad = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_CarrierCodeInfo, "AAAA", "1234");
			declaration.CargoControlNumbers.DeleteAll();

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.SetUpHasUSPlaceOfExportInvoice(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_CarrierCode = ZString.Empty;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ReleaseStatuses.Load();
			var messageError1 = ImportJobDeclarationValidation.AtLeastOneCCNMustBeEntered;
			var messageError2 = ImportJobDeclarationValidation.Only1CCNIsAllowedForPARS;
			var messageError3 = ImportJobDeclarationValidation.YouHaveEnteredMoreThan99CCNs;
			var messageError4 = "Carrier not allowed for mode of transport 'SEA'";
			var messageError5 = ImportJobDeclarationValidation.Only10CCNsMaximumAllowed;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError5);

			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "12345";
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError5);

			for (var i = 0; i < 10; i++)
			{
				declaration.CargoControlNumbers.AddNew("C" + i);
			}
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			AssertHasMessageError(declaration.JE_CarrierCodeInfo, messageError5);

			declaration.CargoControlNumbers.RemoveAndDeleteAll();
			declaration.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			declaration.CargoControlNumbers.AddNew("C1");
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "12345";
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError2);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageError(declaration.JE_CarrierCodeInfo, messageError2);
			declaration.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError2);
			number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "12345";
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageError(declaration.JE_CarrierCodeInfo, messageError2);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError2);
			for (var i = 1; i < 98; i++)
			{
				declaration.CargoControlNumbers.AddNew("C" + i);
			}
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError3);
			declaration.CargoControlNumbers.AddNew("C99");
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageError(declaration.JE_CarrierCodeInfo, messageError3);
			declaration.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError3);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.CargoControlNumbers.RemoveAndDeleteAll();
			declaration.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertHasMessageError(declaration.JE_CarrierCodeInfo, messageError1);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Voluntary;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, messageError1);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_CarrierCode = "1234";
			AssertHasWarning(declaration.JE_CarrierCodeInfo, messageError4);
			declaration.JE_CarrierCode = "XXXX";
			AssertNoWarning(declaration.JE_CarrierCodeInfo, messageError4);
			declaration.JE_CarrierCode = "";
			AssertNoWarning(declaration.JE_CarrierCodeInfo, messageError4);

			carrier.ZZ4_IsRoad = false;
			carrier.Attributes.AddNew(TransportTypeList.Codes.Road, TransportTypeList.Codes.Road);
			Factory.Save();

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_CarrierCode = "1234";
			AssertHasWarning(declaration.JE_CarrierCodeInfo, messageError4);
			declaration.JE_CarrierCode = "XXXX";
			AssertNoWarning(declaration.JE_CarrierCodeInfo, messageError4);
			declaration.JE_CarrierCode = "";
			AssertNoWarning(declaration.JE_CarrierCodeInfo, messageError4);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ReleaseStatuses.Load();
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		#endregion
	}
}
