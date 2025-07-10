using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CommercialInvoiceHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestManufacturerIsExported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var manufacturer = Factory.New<OrgHeader>();
			var address = manufacturer.Addresses.MainAddress;
			address.Address1 = "100 QUEEN STREET";
			address.OA_City = "Sydney";
			address.OA_RN_NKCountryCode = "AU";

			invoiceLine.JI_OA_ManufacturerAddress = address.PK;

			DeclarationDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			Shipment shipment = writer.GetDataObject(declaration);
			var invoiceLineData = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];

			AssertEquals("Manufacturer is exported only once", 1, invoiceLineData.OrganizationAddressCollection.Count);

			var orgAddress = invoiceLineData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "Manufacturer");
			AssertEquals("100 QUEEN STREET", orgAddress.Address1);
		}

		public void TestExportContactInformation()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "ADDRESS LINE 1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.AQISEUContactPerson.E2_OA_Address = org.MainAddress.PK;
			var quarantineHeader = invoice.QuarantineExDocHeader;
			quarantineHeader.QH_EUComments = "COMMENT TEST";
			quarantineHeader.QH_EUTestResultRequired = true;

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);
			var aecAddress = shipment.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "AQISEUContactPerson");
			CombineAssertions(delegate
			{
				AssertEquals("ADDRESS LINE 1", aecAddress.Address1);
				AssertEquals("COMMENT TEST", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.EUComments).Value);
				AssertEquals("Y", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.EUTestResultRequired).Value);
			});
		}

		public void TestAQISLoadingEstablishment_SelectedAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "My Company Pty Ltd";
			org.MainAddress.OA_Address1 = "ADDRESS LINE 1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.AQISLoadingEstablishmentLocation.E2_OA_Address = org.MainAddress.PK;

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			var loadingEstablishmentAddress = invoiceData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == "AQISLoadingEstablishment");
			AssertEquals("CompanyName", "My Company Pty Ltd", loadingEstablishmentAddress.CompanyName);
			AssertEquals("Address1", "ADDRESS LINE 1", loadingEstablishmentAddress.Address1);
			AssertEquals("AddressOverride", false, loadingEstablishmentAddress.AddressOverride);
		}

		public void TestAQISLoadingEstablishment_OverrideAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.AQISLoadingEstablishmentLocation.EXDOCEstablishmentNumber = "123";
			invoice.AQISLoadingEstablishmentLocation.CompanyName = "Some Company";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			var loadingEstablishmentAddress = invoiceData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == "AQISLoadingEstablishment");
			AssertEquals("CompanyName", "Some Company", loadingEstablishmentAddress.CompanyName);
			AssertNullOrEmpty("Address1", loadingEstablishmentAddress.Address1);
			AssertEquals("AddressOverride", true, loadingEstablishmentAddress.AddressOverride);
		}

		public void TestEmptyRFPNumberAndEPNNumberNotExported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			var quarantineHeader = invoice.QuarantineExDocHeader;

			DeclarationDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			Shipment shipment = writer.GetDataObject(declaration);
			CommercialInvoiceHeader invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];

			var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);

			AssertEquals("Empty CustomsReference shouln't be exported.", null, qHAddInfoGroup.CustomsReferenceCollection);
		}

		public void TestAmendmentReasonExported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.ManualAmendmentReasonForMessaging = "Amendment Reason";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);

			AssertEquals("Y", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.SubmitAmendmentRequest).Value);
			AssertEquals("Amendment Reason", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.RequestAmendReason).Value);
		}

		public void TestReissueCertificateNameAndReasonExported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.ReissueCertificateNameForMessaging = "AU1234567";
			invoice.QuarantineExDocHeader.ReissueCertificateReasonForMessaging = "Reissue Reason";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);

			AssertEquals("AU1234567", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ReissueCertificateName).Value);
			AssertEquals("Reissue Reason", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ReissueCertificateReason).Value);
		}

		public void TestAuthorisationFlagExported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.QuarantineExDocHeader.QH_AuthorisationFlag = false;
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);
			AssertEquals("N", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.AuthorisationFlag).Value);

			invoice.QuarantineExDocHeader.QH_AuthorisationFlag = true;
			var writer2 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment2 = writer2.GetDataObject(declaration);
			var invoiceData2 = shipment2.CommercialInfo.CommercialInvoiceCollection[0];
			var qHAddInfoGroup2 = invoiceData2.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);
			AssertEquals("Y", qHAddInfoGroup2.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.AuthorisationFlag).Value);
		}

		public void TestExportQuarantineExDocLine_FinalConsumerAndCombinedNomenclature()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var quarantineHeader = invoice.QuarantineExDocHeader;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;

			quarantineExDocLine.QL_FinalConsumer = true;
			quarantineExDocLine.QL_CombinedNomenclature = "90132654";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceLineData = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
			var qLAddInfoGroup = invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceLine.Codes.QL);

			AssertEquals("Y", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.FinalConsumer).Value);
			AssertEquals("90132654", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.CombinedNomenclature).Value);
		}

		[TestDate(2019, 3, 6)]
		public void TestExportQuarantineExDocHeader()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string natyp = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSAttachmentType;
			helper.CreateNewOrGetExistingCusCodeType(natyp, "NATYP Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY1", "Doc Type DESC 1", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY2", "Doc Type DESC 2", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY3", "Doc Type DESC 3", date1, date2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "F1", description: "F1 Desc");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.pdf", "F2", description: "F2 Desc");
			declaration.DocManagerInfo.Save();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			var quarantineHeader = invoice.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_ProductUseIndicator = "H";
			quarantineHeader.QH_ObtainExportCustomsPermit = true;
			quarantineHeader.QH_ConsigneeAgentName = "TEST CONSIGNEE AGENT";
			quarantineHeader.QH_ExporterDeclaration = "test declaration";
			quarantineHeader.QH_CertificatePrintIndicator = "C";
			quarantineHeader.QH_AQISRegion = "AD2";
			quarantineHeader.QH_SplitHealthCertByContainer = true;
			quarantineHeader.QH_AMLCQuotaYear = "2018";
			quarantineHeader.QH_CertificateRequiredLocation = "LOCATIO";
			quarantineHeader.QH_CustomsConsigneeName = "Consignee";
			quarantineHeader.QH_ExemptionCode = "Exemption";
			quarantineHeader.QH_PrintLocation = "ORGANISATION";
			quarantineHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia = "NO";
			quarantineHeader.QH_LegallyImportedFlag = "NO";
			quarantineHeader.QH_AuthorisationDate = new ZDate(2019, 2, 20);
			quarantineHeader.QH_AuthorisationComments = "Comments";
			quarantineHeader.QH_QuotaType = "XXX";
			quarantineHeader.QH_ApprovalNumber = "DDD-334455";
			quarantineHeader.QH_TransitLocationType = "C";
			quarantineHeader.QH_LastAmendDateTime = new ZDateTimeOffset(2019, 8, 29, 9, 26, 0, 1, TimeSpan.FromHours(10));
			quarantineHeader.QH_AuthorisationFlag = true;

			var pivot1Invoice = invoice.EDocPivotCollection.AddNew();
			pivot1Invoice.CSD_DocType = "TY1";
			pivot1Invoice.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot1Invoice.CSD_Description = "File 1";
			var pivot2Invoice = invoice.EDocPivotCollection.AddNew();
			pivot2Invoice.CSD_DocType = "TY2";
			pivot2Invoice.CSD_StorageDocReference = eDoc2.UniqueKey;
			pivot2Invoice.CSD_Description = "File 2";

			var supportingInfo = quarantineHeader.SupportingInfos.AddNew();
			supportingInfo.CSI_LineNo = 1;
			supportingInfo.CSI_Description = "DEC CODE 1";

			var acknowledgement4 = quarantineHeader.Acknowledgements.AddNew();
			acknowledgement4.CY_Data = "Ack 4";

			var acknowledgement3 = quarantineHeader.Acknowledgements.AddNew();
			acknowledgement3.CY_Data = "Ack 3";

			var acknowledgement2 = quarantineHeader.Acknowledgements.AddNew();
			acknowledgement2.CY_Data = "Ack 2";

			var acknowledgement = quarantineHeader.Acknowledgements.AddNew();
			acknowledgement.CY_Data = "Ack 1";

			var catchZone1 = quarantineHeader.NexDocCatchZones.AddNew();
			catchZone1.CY_Data = "catch zone 1";

			var catchZone2 = quarantineHeader.NexDocCatchZones.AddNew();
			catchZone2.CY_Data = "catch zone 2";

			var catchZone3 = quarantineHeader.NexDocCatchZones.AddNew();
			catchZone3.CY_Data = "catch zone 3";

			var letter = quarantineHeader.RecommendationLetters.AddNew();
			letter.ZA_LetterNumber = "1";
			letter.ZA_LetterDate = new ZDateTime(2018, 01, 30);

			var compartment = quarantineHeader.Compartments.AddNew();
			compartment.QC_Compartments = "test";
			compartment.QC_RL_NKInspectionPort = "ADALV";
			compartment.QC_InspectionDate = new ZDateTime(2018, 01, 30);

			quarantineHeader.QH_RequestForPermitNumber = "111";
			quarantineHeader.RequestForPermitStatus = "AAA";

			quarantineHeader.QH_ExportPermitNumber = "222";
			quarantineHeader.ExportPermitStatus = "BBB";

			Factory.Save();

			CombineAssertions(delegate
			{
				using (AssertDbHitsForAllFactories(message: ZString.Empty,
					expectedHitCounts: expectedHits(),
					hitTolerance: 0,
					stackTraceToIgnore: System.Environment.StackTrace.SplitByLine().Last(),
					tablesToCollectQueriesFor: tablesToCollectQueriesFor(),
					useOnlyNewFactories: false,
					tablesToIgnore: new[] { StmUniversalCopySchema.Constants.TableName, RefCurrencySchema.Constants.TableName },
					acceptableVariance: 0))
				{
					var newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactory" };
					((IExternalFetchHintSupporter)newFactory).SetupCreator();

					var dec = newFactory.Load<JobDeclaration>(declaration.PK);
					var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, dec)));
					var shipment = writer.GetDataObject(dec);
					var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];

					AssertEquals(3, invoiceData.AddInfoGroupCollection.Count);
					var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);
					AssertEquals(EXDOCCommodityCodes.Codes.Dairy, qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ProduceType).Value);
					AssertEquals("H", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ProductUse).Value);
					AssertEquals("Y", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ObtainExportCustomsPermit).Value);
					AssertEquals("TEST CONSIGNEE AGENT", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ConsigneeAgentName).Value);
					AssertEquals("test declaration", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ExporterDeclaration).Value);
					AssertEquals("C", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.CertificatePrintIndicator).Value);
					AssertEquals("AD2", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ProductionRegion).Value);
					AssertEquals("Y", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.SplitHealthCertByContainer).Value);
					AssertEquals("2018", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.AMLCQuotaYear).Value);
					AssertEquals("XXX", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.QuotaType).Value);
					AssertEquals("LOCATIO", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.CertificateRequiredLocation).Value);
					AssertEquals("Consignee", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.CustomsConsigneeName).Value);
					AssertEquals("Exemption", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ExemptionCode).Value);
					AssertEquals("ORGANISATION", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.PrintLocation).Value);
					AssertEquals("NO", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ManufacturedTreatedPackagedLabelledInAustralia).Value);
					AssertEquals("NO", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.LegallyImportedFlag).Value);
					AssertEquals("2019-02-20", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.AuthorisationDate).Value);
					AssertEquals("Comments", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.AuthorisationComments).Value);
					AssertEquals("DDD-334455", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ApprovalNumber).Value);
					AssertEquals("C", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.TransitLocationType).Value);
					AssertEquals("2019-08-29T09:26:00.001+10:00", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.LastAmendDateTime).Value);
					AssertEquals("Y", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.AuthorisationFlag).Value);

					AssertEquals(2, qHAddInfoGroup.AddInfoGroupCollection.Count);
					var nRLAddInfoGroup = qHAddInfoGroup.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.NRL);
					AssertEquals("1", nRLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.RecommendationLetterNumber).Value);
					AssertEquals("2018-01-30 00:00:00.000", nRLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.RecommendationLetterDate).Value);

					var nSIAddInfoGroup = qHAddInfoGroup.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.NSI);
					AssertEquals("test", nSIAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.Compartments).Value);
					AssertEquals("ADALV", nSIAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.InspectionPort).Value);
					AssertEquals("2018-01-30 00:00:00.000", nSIAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "InspectionDate").Value);

					AssertEquals(10, qHAddInfoGroup.CustomsReferenceCollection.Count);
					var rFSReference = qHAddInfoGroup.CustomsReferenceCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == CusEntryNumber.EntryType.RequestForPermitStatus);
					AssertEquals("111", rFSReference.Reference.Value);
					AssertEquals("AAA", rFSReference.ReferencedEntityDescription.Value);

					var ePNReference = qHAddInfoGroup.CustomsReferenceCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == CusEntryNumber.EntryType.ExdocPermitNumber);
					AssertEquals("222", ePNReference.Reference.Value);
					AssertEquals("BBB", ePNReference.ReferencedEntityDescription.Value);

					var decReference = qHAddInfoGroup.CustomsReferenceCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == QuarantineSupportingInfoCollection.DeclarationConstant);
					AssertEquals(1, decReference.Order.Value);
					AssertEquals("DEC CODE 1", decReference.Reference.Value);

					var ackReferences = qHAddInfoGroup.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == QuarantineExDocRexAcknowledgement.AcknowledgementCode).OrderBy(x => x.Reference.Value);
					AssertArrayEqualsByElements("Acknowledgements should be orderd alphabetically by CY_Data.", new ZString[] { "Ack 1", "Ack 2", "Ack 3", "Ack 4", }, ackReferences.Select(x => x.Reference.Value).ToArray());

					var catchZoneReferences = qHAddInfoGroup.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == CusCodeDataTypeList.Codes.NEXDOCSCatchZone).OrderBy(x => x.Reference.Value);
					AssertArrayEqualsByElements(new ZString[] { "catch zone 1", "catch zone 2", "catch zone 3", }, catchZoneReferences.Select(x => x.Reference.Value).ToArray());

					AssertNull("SubmitAmendmentRequest", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.SubmitAmendmentRequest));
					AssertNull("RequestAmendReason", qHAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.RequestAmendReason));

					var qhas = invoiceData.AddInfoGroupCollection
						.Where(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QHA)
						.ToArray();
					var qha1AddInfoGroup = qhas[0];
					AssertEquals("Invoice1.pdf", qha1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "FileName").Value);
					AssertEquals("application/pdf", qha1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "mimeType").Value);
					AssertEquals("TY1", qha1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "AttachmentType").Value);
					AssertEquals("File 1", qha1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "Description").Value);

					var qha2AddInfoGroup = qhas[1];
					AssertEquals("Invoice2.pdf", qha2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "FileName").Value);
					AssertEquals("application/pdf", qha2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "mimeType").Value);
					AssertEquals("TY2", qha2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "AttachmentType").Value);
					AssertEquals("File 2", qha2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "Description").Value);
				}
			});

			Dictionary<string, int> expectedHits() => new Dictionary<string, int>
			{
				{ CusAddInfoSchema.Constants.TableName, 1 },
				{ CusCodeDataSchema.Constants.TableName, 2 },
				{ CusContainerSchema.Constants.TableName, 1 },
				{ CusDecHouseBillSchema.Constants.TableName, 1 },
				{ CusEntryHeaderSchema.Constants.TableName, 1 },
				{ CusEntryNumSchema.Constants.TableName, 4 },
				{ CusSupportingInfoSchema.Constants.TableName, 1 },
				{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
				{ GenCustomColumnDefinitionSchema.Constants.TableName, 1 },
				{ JobComInvHeaderChargeSchema.Constants.TableName, 1 },
				{ JobComInvoiceHeaderSchema.Constants.TableName, 4 },
				{ JobComInvoiceLineSchema.Constants.TableName, 1 },
				{ JobConsolTransportSchema.Constants.TableName, 1 },
				{ JobDeclarationSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobDocsAndCartageSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ JobOrderHeaderSchema.Constants.TableName, 1 },
				{ JobOrderItemSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ QuarantineExDocHeaderSchema.Constants.TableName, 1 },
				{ QuarantineExDocShipsCompartmentSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ RefServiceLevelSchema.Constants.TableName, 1 },
				{ RefVesselSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ CusStorageDocPivotSchema.Constants.TableName, 1 },
				{ StorageMainSchema.Constants.TableName, 1 },
				{ StorageDocsSchema.Constants.TableName, 1 },
				{ EDIMessageSchema.Constants.TableName, 2 }
			};

			string[] tablesToCollectQueriesFor() => new string[]
			{
				JobDeclarationSchema.Constants.TableName,
				JobComInvoiceHeaderSchema.Constants.TableName,
				CusCodeDataSchema.Constants.TableName,
				CusSupportingInfoSchema.Constants.TableName,
				CusAddInfoSchema.Constants.TableName,
				QuarantineExDocShipsCompartmentSchema.Constants.TableName
			};
		}

		public void TestExportQuarantineExDocHeader_LoadingEstablishmentIdOverridden()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.AQISLoadingEstablishmentLocation.EXDOCEstablishmentNumber = "92";
			var quarantineHeader = invoice.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			quarantineHeader.QH_LoadingDate = new ZDate(2024, 08, 21);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactory" };
			((IExternalFetchHintSupporter)newFactory).SetupCreator();

			var dec = newFactory.Load<JobDeclaration>(declaration.PK);
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, dec)));
			var shipment = writer.GetDataObject(dec);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];

			var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.Single(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);
			AssertEquals(EXDOCCommodityCodes.Codes.SkinsAndHides, qHAddInfoGroup.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ProduceType).Value);

			AssertEquals("92", qHAddInfoGroup.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.LoadingEstablishment).Value);
			AssertEquals("2024-08-21", qHAddInfoGroup.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.LoadingDate).Value);
		}

		public void TestExportQuarantineExDocHeader_LoadingEstablishmentIdFromOrgAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ01";
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, "295", "AU");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.AQISLoadingEstablishmentLocation.E2_OA_Address = org.MainAddress.PK;
			var quarantineHeader = invoice.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			quarantineHeader.QH_LoadingDate = new ZDate(2024, 08, 21);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactory" };
			((IExternalFetchHintSupporter)newFactory).SetupCreator();

			var dec = newFactory.Load<JobDeclaration>(declaration.PK);
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, dec)));
			var shipment = writer.GetDataObject(dec);
			var invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];

			var qHAddInfoGroup = invoiceData.AddInfoGroupCollection.Single(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceHeader.Codes.QH);
			AssertEquals(EXDOCCommodityCodes.Codes.SkinsAndHides, qHAddInfoGroup.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.ProduceType).Value);

			AssertEquals("295", qHAddInfoGroup.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.LoadingEstablishment).Value);
			AssertEquals("2024-08-21", qHAddInfoGroup.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == Constants.InvoiceHeader.Keys.LoadingDate).Value);
		}

		[TestDate(2019, 3, 6)]
		public void TestExportQuarantineExDocLine()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string natyp = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSAttachmentType;
			helper.CreateNewOrGetExistingCusCodeType(natyp, "NATYP Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY1", "Doc Type DESC 1", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY2", "Doc Type DESC 2", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY3", "Doc Type DESC 3", date1, date2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Quarantine;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "F1", description: "F1 Desc");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.pdf", "F2", description: "F2 Desc");
			declaration.DocManagerInfo.Save();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			var quarantineHeader = invoice.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_ProductUseIndicator = "H";
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_NetQuantity = 2m;
			quarantineExDocLine.QL_NetQuantityUnit = "BIL";
			quarantineExDocLine.QL_ImperialNetWeight = 3m;
			quarantineExDocLine.QL_ImperialNetWeightUnit = "ONZ";
			quarantineExDocLine.QL_GrossMetricWeight = 4m;
			quarantineExDocLine.QL_GrossMetricWeightUnit = "KGM";
			quarantineExDocLine.QL_ShippingMarks = "MARKS";
			quarantineExDocLine.QL_BatchCode = "123";
			quarantineExDocLine.QL_Category = "TEST";
			quarantineExDocLine.QL_AddtionalDeclarationComments = "ADD DEC";
			quarantineExDocLine.QL_AddtionalProductDescription = "ADD DESC";
			quarantineExDocLine.QL_AdditionalProducts = "EMU,GOAT,CAMEL";
			quarantineExDocLine.QL_IMA1ProductDesciption = "DESC";
			quarantineExDocLine.QL_FarmCode = "FARMCODE";
			quarantineExDocLine.QL_FarmType = "FARMTYPE";
			quarantineExDocLine.QL_FishWaterIndicator = "F";
			quarantineExDocLine.QL_CatchStartDate = new ZDateTime(2018, 02, 01);
			quarantineExDocLine.QL_CatchEndDate = new ZDateTime(2018, 02, 15);

			invoiceLine.JI_RelatedExportPermitNumber = "456";
			invoiceLine.JI_RelatedExportPermitAuthority = "AHC";
			invoiceLine.JI_RelatedExportPermitDate = new ZDateTime(2018, 1, 30);
			invoiceLine.JI_Drawback = true;

			var pivot1InvoiceLine = invoiceLine.EDocPivotCollection.AddNew();
			pivot1InvoiceLine.CSD_DocType = "TY1";
			pivot1InvoiceLine.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot1InvoiceLine.CSD_Description = "File 1";
			var pivot2InvoiceLine = invoiceLine.EDocPivotCollection.AddNew();
			pivot2InvoiceLine.CSD_DocType = "TY2";
			pivot2InvoiceLine.CSD_StorageDocReference = eDoc2.UniqueKey;
			pivot2InvoiceLine.CSD_Description = "File 2";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_AddressType = "APE";
			docAddress.E2_CompanyName = "Company";
			docAddress.E2_ParentID = declaration.PK;
			docAddress.E2_ParentTableCode = declaration.TablePrefix;

			var process = quarantineExDocLine.Processes.AddNew();
			process.EE_ProcessingType = "CT";
			process.EE_E2_Address = docAddress.PK;
			process.EE_StartDate = new ZDateTime(2018, 01, 30);
			process.EE_EstablishmentIndicator = "IN";
			process.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;

			var number = invoiceLine.RFPNumbers.AddNew();
			number.ZA_RFPNumber = "111";
			number.ZA_RFPLine = 3;
			number.ZA_RFPNetQuantity = 4;
			number.ZA_RFPQtyUM = "GLD";
			number.ZA_RFPPackCount = 5;
			number.ZA_RFPPackType = "BL";

			Factory.Save();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var invoiceLineData = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];

			CombineAssertions(delegate
			{
				AssertEquals(4, invoiceLineData.AddInfoGroupCollection.Count);
				var qLAddInfoGroup = invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceLine.Codes.QL);
				AssertEquals("2", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.NetQuantity).Value);
				AssertEquals("BIL", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.NetQuantityUnit).Value);
				AssertEquals("3", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.ImperialNetWeight).Value);
				AssertEquals("ONZ", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.ImperialNetWeightUnit).Value);
				AssertEquals("4", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.GrossMetricWeight).Value);
				AssertEquals("KGM", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.GrossMetricWeightUnit).Value);
				AssertEquals("MARKS", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.ShippingMarks).Value);
				AssertEquals("123", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.BatchCode).Value);
				AssertEquals("TEST", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.Category).Value);
				AssertEquals("ADD DEC", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.AdditionalDeclarationComments).Value);
				AssertEquals("ADD DESC", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.AdditionalProductDescription).Value);
				AssertEquals("DESC", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.IMA1ProductDescription).Value);
				AssertEquals("FARMCODE", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.FarmCode).Value);
				AssertEquals("FARMTYPE", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.FarmType).Value);
				AssertEquals("F", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.FishWaterIndicator).Value);
				AssertEquals("2018-02-01 00:00:00.000", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.CatchStartDate).Value);
				AssertEquals("2018-02-15 00:00:00.000", qLAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.CatchEndDate).Value);

				AssertEquals("456", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.RelatedExportPermitNumber).Value);
				AssertEquals("AHC", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.RelatedExportPermitAuthority).Value);
				AssertEquals("2018-01-30 00:00:00.000", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.RelatedExportPermitDate).Value);
				AssertEquals("EMU,GOAT,CAMEL", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "AQISAdditionalProducts_Hidden").Value);

				var nPDAddInfoGroup = qLAddInfoGroup.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceLine.Codes.NPD);
				AssertEquals("CT", nPDAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.ProcessingType).Value);
				AssertEquals("2018-01-30 00:00:00.000", nPDAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.StartDate).Value);
				AssertEquals("Company", nPDAddInfoGroup.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "AQISProcessingEstablishment").CompanyName.Value);
				AssertEquals("IN", nPDAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.EstablishmentIndicator).Value);
				AssertEquals("true", nPDAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.InvoiceLine.Keys.RemoveEntry).Value);

				var rFPAddInfoGroup = invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == "RFP");
				AssertEquals("111", rFPAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "RFPNumber").Value);
				AssertEquals("3", rFPAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "RFPLine").Value);
				AssertEquals("4", rFPAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "RFPNetQuantity").Value);
				AssertEquals("GLD", rFPAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "RFPQtyUM").Value);
				AssertEquals("5", rFPAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "RFPPackCount").Value);
				AssertEquals("BL", rFPAddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "RFPPackType").Value);

				var qlas = invoiceLineData.AddInfoGroupCollection
					.Where(x => x.Type.Code.GetValueOrDefault() == Constants.InvoiceLine.Codes.QLA)
					.ToArray();
				var qla1AddInfoGroup = qlas[0];
				AssertEquals("Invoice1.pdf", qla1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "FileName").Value);
				AssertEquals("application/pdf", qla1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "mimeType").Value);
				AssertEquals("TY1", qla1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "AttachmentType").Value);
				AssertEquals("File 1", qla1AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "Description").Value);

				var qla2AddInfoGroup = qlas[1];
				AssertEquals("Invoice2.pdf", qla2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "FileName").Value);
				AssertEquals("application/pdf", qla2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "mimeType").Value);
				AssertEquals("TY2", qla2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "AttachmentType").Value);
				AssertEquals("File 2", qla2AddInfoGroup.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "Description").Value);
			});
		}

		public void TestBondedWarehouseDetailsIsExported()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 10m;
			invoiceLine1.JI_InvoiceUQ = "PK";
			invoiceLine1.JI_IsPackToBondForLine = true;
			invoiceLine1.UseBondedWarehouseAutomation = false;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = "PK";
			invoiceLine2.JI_IsPackToBondForLine = false;
			invoiceLine2.UseBondedWarehouseAutomation = true;

			DeclarationDataObjectWriter writer = null;
			Shipment shipment = null;
			CommercialInvoiceHeader invoiceData = null;
			CommercialInvoiceLine invoiceLine1Data = null;
			CommercialInvoiceLine invoiceLine2Data = null;
			foreach (var messageType in new[] { Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Import, Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker })
			{
				declaration.SetSupportsBondedWarehousingForTesting(true);
				declaration.JE_MessageType = messageType;
				writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
				shipment = writer.GetDataObject(declaration);
				invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
				invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
				invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
				AssertEquals("invoiceLine1Data.BondedWarehouseQuantity", 10m, invoiceLine1Data.BondedWarehouseQuantity.GetValueOrDefault());
				AssertEquals("invoiceLine1Data.BondedWarehouseQuantityUnit.Code", "PK", invoiceLine1Data.BondedWarehouseQuantityUnit.GetCodeAsUpperCase());
				AssertNull("invoiceLine2Data.BondedWarehouseQuantity", invoiceLine2Data.BondedWarehouseQuantity);
				AssertNull("invoiceLine2Data.BondedWarehouseQuantityUnit", invoiceLine2Data.BondedWarehouseQuantityUnit);

				declaration.SetSupportsBondedWarehousingForTesting(false);
				writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
				shipment = writer.GetDataObject(declaration);
				invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
				invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
				invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
				AssertNull("invoiceLine1Data.BondedWarehouseQuantity", invoiceLine1Data.BondedWarehouseQuantity);
				AssertNull("invoiceLine1Data.BondedWarehouseQuantityUnit", invoiceLine1Data.BondedWarehouseQuantityUnit);
				AssertNull("invoiceLine2Data.BondedWarehouseQuantity", invoiceLine2Data.BondedWarehouseQuantity);
				AssertNull("invoiceLine2Data.BondedWarehouseQuantityUnit", invoiceLine2Data.BondedWarehouseQuantityUnit);
			}

			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.WarehousedByExternalAgent;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			shipment = writer.GetDataObject(declaration);
			invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
			invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
			AssertEquals("invoiceLine1Data.BondedWarehouseQuantity", 10m, invoiceLine1Data.BondedWarehouseQuantity.GetValueOrDefault());
			AssertEquals("invoiceLine1Data.BondedWarehouseQuantityUnit.Code", "PK", invoiceLine1Data.BondedWarehouseQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("invoiceLine2Data.BondedWarehouseQuantity", 10m, invoiceLine2Data.BondedWarehouseQuantity.GetValueOrDefault());
			AssertEquals("invoiceLine2Data.BondedWarehouseQuantityUnit.Code", "PK", invoiceLine2Data.BondedWarehouseQuantityUnit.GetCodeAsUpperCase());

			declaration.SetSupportsBondedWarehousingForTesting(false);
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			shipment = writer.GetDataObject(declaration);
			invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
			invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
			AssertNull("invoiceLine1Data.BondedWarehouseQuantity", invoiceLine1Data.BondedWarehouseQuantity);
			AssertNull("invoiceLine1Data.BondedWarehouseQuantityUnit", invoiceLine1Data.BondedWarehouseQuantityUnit);
			AssertNull("invoiceLine2Data.BondedWarehouseQuantity", invoiceLine2Data.BondedWarehouseQuantity);
			AssertNull("invoiceLine2Data.BondedWarehouseQuantityUnit", invoiceLine2Data.BondedWarehouseQuantityUnit);

			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.ExWarehouse;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			shipment = writer.GetDataObject(declaration);
			invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
			invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
			AssertEquals("invoiceLine1Data.BondedWarehouseQuantity", 10m, invoiceLine1Data.BondedWarehouseQuantity.GetValueOrDefault());
			AssertEquals("invoiceLine1Data.BondedWarehouseQuantityUnit.Code", "PK", invoiceLine1Data.BondedWarehouseQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("invoiceLine2Data.BondedWarehouseQuantity", 10m, invoiceLine2Data.BondedWarehouseQuantity.GetValueOrDefault());
			AssertEquals("invoiceLine2Data.BondedWarehouseQuantityUnit.Code", "PK", invoiceLine2Data.BondedWarehouseQuantityUnit.GetCodeAsUpperCase());

			declaration.SetSupportsBondedWarehousingForTesting(false);
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			shipment = writer.GetDataObject(declaration);
			invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
			invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
			AssertNull("invoiceLine1Data.BondedWarehouseQuantity", invoiceLine1Data.BondedWarehouseQuantity);
			AssertNull("invoiceLine1Data.BondedWarehouseQuantityUnit", invoiceLine1Data.BondedWarehouseQuantityUnit);
			AssertNull("invoiceLine2Data.BondedWarehouseQuantity", invoiceLine2Data.BondedWarehouseQuantity);
			AssertNull("invoiceLine2Data.BondedWarehouseQuantityUnit", invoiceLine2Data.BondedWarehouseQuantityUnit);

			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Export;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			shipment = writer.GetDataObject(declaration);
			invoiceData = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
			invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
			AssertNull("invoiceLine1Data.BondedWarehouseQuantity", invoiceLine1Data.BondedWarehouseQuantity);
			AssertNull("invoiceLine1Data.BondedWarehouseQuantityUnit", invoiceLine1Data.BondedWarehouseQuantityUnit);
			AssertNull("invoiceLine2Data.BondedWarehouseQuantity", invoiceLine2Data.BondedWarehouseQuantity);
			AssertNull("invoiceLine2Data.BondedWarehouseQuantityUnit", invoiceLine2Data.BondedWarehouseQuantityUnit);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
