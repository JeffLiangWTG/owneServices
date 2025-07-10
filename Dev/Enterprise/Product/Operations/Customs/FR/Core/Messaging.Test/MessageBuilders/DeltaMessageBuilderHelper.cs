using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Messaging.Testing
{
	public class DeltaMessageBuilderHelper : TestCaseWithFactory
	{
		public CusEntryHeader CreateEntryDeclarationForTest(bool isDeltaC) => CreateEntryDeclarationForTest(isDeltaC, true);

		public CusEntryHeader CreateEntryDeclarationForTest(bool isDeltaC, bool isImport, bool hasEORI = false)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "INC", "INC desc", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "IsC", "IsC desc", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0003", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "GHI", "Anser fabalis/Bean goose", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "DEF", "DDezful", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var france = Factory.Load<RefCountry>(Core.Constants.CountryGuids.France);

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.SetCustomsCode(OrgCusCode.FranceCodeTypes.Siret, france, "FR33159700500064");
			declarant.SetCustomsCode(OrgCusCode.CodeTypes.BrokerageRegistration, france, "FR33159700500064");

			var declarantAddress = Factory.New<OrgAddress>();
			declarantAddress.OA_OH = declarant.PK;
			declarantAddress.OA_Address1 = "Eugene Leroy Street ";

			var importerAddress = Factory.New<OrgAddress>();
			importerAddress.OA_Address1 = "Eugene Leroy Street ";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_RL_NKClosestPort = isImport ? "FRPAR" : "AUSYD";
			importerAddress.OA_OH = importer.PK;
			importer.SetCustomsCode(OrgCusCode.FranceCodeTypes.TVA, france, "FR30159700500061");
			if (hasEORI)
			{
				importer.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, france, "32159700500065");
			}

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_RL_NKClosestPort = isImport ? "AUSYD" : "FRPAR";

			var declaration = Factory.New<JobDeclaration>();
			var deltaMode = isDeltaC ? OrgCusAccountDeltaGTypeList.Codes.G1 : OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.JE_MessageType = isImport ? "IMP" : "EXP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MasterBill = "UnitTest";
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MainAddress.OA_PostCode = "24130";
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_DeltaMode = deltaMode;

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 49.66m;
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_ConcessionOrder = "Concession test";

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.FillWithValidTestData();

			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.FillWithValidTestData();
			supportingDocument.CSI_Code = "0001";
			supportingDocument.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocument.CSI_Quantity3 = 10;
			supportingDocument.CSI_Value = 1;

			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.FillWithValidTestData();
			supportingDocument2.CSI_Code = "0003";
			supportingDocument2.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocument2.CSI_Quantity3 = 30;

			var supportingDocument3 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument3.FillWithValidTestData();
			supportingDocument3.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocument3.CSI_Code = "INC";
			supportingDocument3.CSI_IsDTP = true;
			supportingDocument3.CSI_Quantity3 = 30;

			var supportingDocument4 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument4.FillWithValidTestData();
			supportingDocument4.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocument4.CSI_Code = "IsC";
			supportingDocument4.CSI_IsDTP = true;
			supportingDocument4.CSI_Quantity3 = 40;

			var customOffice = declaration.CustomsOffices.AddNew();
			customOffice.CY_Code = "ENT";
			customOffice.CY_Type = "EUO";
			customOffice.CY_Data = "FR000130";

			var shutUp = new SendsMessagesToCustomsShutterUpperer();

			var mergeResult = declaration.DoMerge(shutUp);

			Assert("Merge should not fail.", mergeResult);

			Factory.Save();

			return declaration.CustomsEntryHeaders[0];
		}

		public ZString GetMessage(ZString messageType, CusEntryHeader entry)
		{
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = messageType;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			return messageBuilder.GetMessage();
		}

		public ZString GetFlatMessage(ZString messageType, CusEntryHeader entry)
		{
			return Extensions.GetFlatXml(GetMessage(messageType, entry));
		}

		public string[] GetMessageErrorsForTest(bool isDeltaC, bool isImport)
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(isDeltaC, isImport);
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);

			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);

			messageBuilder.GetMessage();
			return errCollector.GetErrors().ToArray();
		}
	}
}
