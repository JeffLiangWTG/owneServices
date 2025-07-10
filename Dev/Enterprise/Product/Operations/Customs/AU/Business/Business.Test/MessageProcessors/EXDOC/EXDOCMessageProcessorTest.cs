using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMessageProcessorTest : TestCaseWithFactory
	{
		public void TestImporterNotAutoCreateForAQS()
		{
			SetupTestDeclaration();
			var messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("RFPTestWithOutImporter.edi")).Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = messageText;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			AssertEquals("Job Declaration JE_OH_Importer is empty", ZGuid.Empty, exDocHeader.Declaration.JE_OH_Importer);
			Assert("Job Declaration JE_ToOrder is true", exDocHeader.Declaration.JE_ToOrder);
			AssertEquals("Job Declaration ToOrder Comment", "TESTCITY", exDocHeader.Declaration.JE_ToOrderComment);

			messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("RFPTestWithImporter.edi")).Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message = Factory.New<EDIMessage>();
			message.EM_MessageText = messageText;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			AssertNotEquals("Job Declaration JE_OH_Importer is not empty", ZGuid.Empty, exDocHeader.Declaration.JE_OH_Importer);
			Assert("Job Declaration JE_ToOrder is false", !exDocHeader.Declaration.JE_ToOrder);
			AssertEquals("Job Declaration ToOrder Comment", ZString.Empty, exDocHeader.Declaration.JE_ToOrderComment);
		}

		public void TestReissueMessage()
		{
			SetupTestDeclaration();
			var invoiceHeader = exDocHeader.Declaration.Invoices[0];
			invoiceHeader.JZ_InvoiceNumber = "1234";

			EDIMessage message = Factory.New<EDIMessage>();
			string messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("Reissue.edi"));
			message.EM_MessageText = new EdifactDelimiterConverter(EdifactDelimiterConverter.StandardDelimiters, EdifactDelimiterConverter.ExDocDelimiters).Convert(messageText.Replace("\r\n", ""));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			AssertEquals("Invoice number no reset", "1234", invoiceHeader.JZ_InvoiceNumber);
			AssertEquals("Message is acknowledged", EDIMessage.Status.Acknowledged, message.EM_MessageType);
			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, exDocHeader.Declaration.JE_MessageStatus);
			AssertEquals("Export Permit Number", "PIM1039211", exDocHeader.QH_ExportPermitNumber);
			AssertEquals("Request For Permit Number", "2077447", exDocHeader.QH_RequestForPermitNumber);
			AssertEquals("Request for Permit Compliance Isnpected", "INS", exDocHeader.RequestForPermitStatus);
			AssertEquals("Authorising Officer Identifier", "KYNANB", exDocHeader.QH_AuthorisingOfficerID);
			Assert("Start Hold Seal is empty", exDocHeader.QH_StartHoldSeal.IsEmpty);
			Assert("End Hold Seal is empty", exDocHeader.QH_EndHoldSeal.IsEmpty);
			AssertEquals("Declaration of Compliance", "YES", exDocHeader.QH_DecOfCompliance);
			AssertEquals("Imported Product Flag", "NO", exDocHeader.QH_ImportedProductFlag);
			AssertEquals("True And Complete Indicator", "YES", exDocHeader.QH_TrueAndCompleteIndicator);
			AssertEquals("Average Age of Animals", "MORE THAN 1 YEAR", exDocHeader.QH_AvAnimalAge);
			AssertEquals("Approved Certifier", "H1234", exDocHeader.QH_ApprovedCertifier);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertEquals("Containers against declaration", 1, exDocHeader.Declaration.CusContainers.Count);
			AssertEquals("Container Number is correct", "ABOC8457924", exDocHeader.Declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Container Seal is correct", ZString.Empty, exDocHeader.Declaration.CusContainers[0].CO_Seal);
			foreach (JobComInvoiceLine invoiceLine in exDocHeader.InvoiceHeader.JobComInvoiceLines)
			{
				AssertEquals("Health Certificate Description", ZString.Empty, invoiceLine.QuarantineExDocLine.QL_HealthCertificateDescription);
				AssertEquals("Allocated Certificate Format", "E188/506", invoiceLine.QuarantineExDocLine.QL_HCFormatAllocated);
				Assert("Container is linked", invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("ABOC8457924").IsForInvoiceLine);
				AssertEquals("Dominant Product", "DPCODE", invoiceLine.QuarantineExDocLine.QL_DominantProduct);
				AssertEquals("Additional Products", "APCODES", invoiceLine.QuarantineExDocLine.QL_AdditionalProducts);
			}

			AssertEquals("Response Email Subject", "RFP Accepted for B9999999", messageProcessor.ResponseEmail.Subject);
		}

		public void TestTransferMessageDairyAndCatch()
		{
			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_RL_NKHomePort = "AUWAZ";
			auBranch.GB_Code = "WAZ";
			AUCustomsDataRegistry.Instance.DefaultBranchForTransferIn.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, auBranch.PK.ToGuid());

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			supplier.MiscServ.OM_RX_NKEXDefCurrency = JobDeclaration.LocalCurrencyConstantCode;
			OrgCusCode exportNum = supplier.CustomsCodes.AddNew();
			exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
			exportNum.OK_RN_NKCodeCountry = "AU";
			exportNum.OK_OH = supplier.PK;
			exportNum.OK_CustomsRegNo = "99999";

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			importer.OH_FullName = "CONSIGNEE NAME MAX OF 35 CHARACTERS";
			importer.OH_RL_NKClosestPort = "THBKK";
			importer.MainAddress.OA_Address1 = "1 LINE ADDRESS MAX OF 35 CAHRACTERS";
			importer.MainAddress.OA_City = "CITY ADDRESS A MAX";
			importer.MainAddress.OA_PostCode = "1234567890";
			importer.MainAddress.OA_State = "STATE ADDRESS AMAXI";

			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			var messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("Transfer.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_OwnerRef, "TRANS ATT G");
			JobDeclaration declaration = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull("Declaration isn't null", declaration);
			AssertEquals("Correct branch", auBranch.PK, declaration.Branch.PK);
			AssertEquals("Supplier Guid", supplier.PK, declaration.JE_OH_Supplier);
			AssertEquals("Importer Guid", importer.PK, declaration.JE_OH_Importer);
			Assert("Forwarder Guid", declaration.JE_OH_Forwarder.IsEmpty);
			AssertEquals("Transport Mode", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals("Voyage Flight Number", "666666", declaration.JE_VoyageFlightNo);
			AssertEquals("Vessel Name", "ACT 5 GON ZAALLEEES TO BE MAXIMUM29", declaration.JE_VesselName);
			Assert("Shipping Line Guid", declaration.JE_OH_ShippingLine.IsEmpty);
			AssertEquals("Incoterm is FOB", Core.Constants.IncoTerms.FreeOnBoard, declaration.JE_ShipmentIncoTerm);
			AssertEquals("Port of Loading", "AUMEL", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port of Origin", "AUMEL", declaration.JE_RL_NKOrigin);
			AssertEquals("Port of Arrival", "THBKK", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Final Destination", "THBKK", declaration.JE_RL_NKFinalDestination);
			Assert("Port of First Arrival is empty", declaration.JE_RL_NKPortOfFirstArrival.IsEmpty);
			Assert("CAN", declaration.DeclarationNumber.IsEmpty);
			AssertEquals("Message Type", Declaration.Business.JobMessageTypeList.Codes.Quarantine, declaration.JE_MessageType);
			StmNote[] note = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCNotifyText.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOC Notify Party", note[0]);
			AssertEquals("EXDOC Notify Party Text", "REPEAT THE ABOVE STEPS WITH THIS ADDITIONAL INFORAMTI 6REPEAT THE ABOVE STEPS WITH THIS ADDITIONAL INFORAMTI 7REPEAT THE ABOVE STEPS WITH THIS ADDITIONAL INFORAMTI 8", note[0].ST_NoteDataAsText);
			AssertEquals("Exdoc Letter Of Credit", 0, declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description).Length);
			AssertEquals("Exdoc Additional Information", 0, declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description).Length);
			AssertEquals("Exdoc Amendment Reason", 0, declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description).Length);
			AssertEquals("There is only one Invoice Header", 1, declaration.Invoices.Count);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices[0];
			AssertEquals("Invoice Header Supplier is set", invoiceHeader.JZ_OH_Supplier, supplier.PK);
			AssertEquals("Containers against declaration", 1, invoiceHeader.JobDeclaration.CusContainers.Count);
			AssertEquals("Container Number is correct", "TWENTYABOC45217", invoiceHeader.JobDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Container Seal is correct", ZString.Empty, invoiceHeader.JobDeclaration.CusContainers[0].CO_Seal);
			AssertEquals("Invoice Number", "1", invoiceHeader.JZ_InvoiceNumber);
			AssertEquals("Quarantine Produce Type", EXDOCCommodityCodes.Codes.Dairy, invoiceHeader.QuarantineExDocHeader.QH_ProduceType);
			AssertEquals("Quarantine Permit Number", ZString.Empty, invoiceHeader.QuarantineExDocHeader.QH_ExportPermitNumber);
			AssertEquals("RFP Number", "2077308", invoiceHeader.QuarantineExDocHeader.QH_RequestForPermitNumber);
			AssertEquals("RFP Compliance Status", "FIN", invoiceHeader.QuarantineExDocHeader.RequestForPermitStatus);
			AssertEquals("Actual Export Date", new ZDateTime(2006, 12, 17), invoiceHeader.JobDeclaration.JE_ExportDate);
			Assert("Border Inspection Port", invoiceHeader.QuarantineExDocHeader.QH_RL_NKBorderInspectionPort.IsEmpty);
			AssertEquals("Product Source Country", "AU", invoiceHeader.QuarantineExDocHeader.QH_RN_NKOriginCountry);
			AssertEquals("Certificate Required Location", "CBR", invoiceHeader.QuarantineExDocHeader.QH_CertificateRequiredLocation);
			AssertEquals("AQIS Region", "CBR", invoiceHeader.QuarantineExDocHeader.QH_AQISRegion);
			Assert("Exporter Declaration", invoiceHeader.QuarantineExDocHeader.QH_ExporterDeclaration.IsEmpty);
			Assert("Inspector Comments", invoiceHeader.QuarantineExDocHeader.QH_InspectorComments.IsEmpty);
			Assert("Origin Catch Zone", invoiceHeader.QuarantineExDocHeader.QH_OriginCatchZone.IsEmpty);
			Assert("Lot Number", invoiceHeader.QuarantineExDocHeader.QH_LotNumber.IsEmpty);
			Assert("Temperature Unit", invoiceHeader.QuarantineExDocHeader.QH_TemperatureUM.IsEmpty);
			Assert("Absolute Temperature", invoiceHeader.QuarantineExDocHeader.QH_AbsoluteTemperature.IsEmpty);
			Assert("Min Temperature", invoiceHeader.QuarantineExDocHeader.QH_MinimumTemperature.IsEmpty);
			Assert("Max Temperature", invoiceHeader.QuarantineExDocHeader.QH_MaximumTemperature.IsEmpty);
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals("Invoice Currency", currency.RX_Code, Core.Constants.CurrencyCodes.Australia);
			AssertEquals("Certificate Print Indicator", EXDOCCertificatePrintCodes.Codes.Manual, invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator);
			Assert("Split Health Certificate by Container", !invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByContainer);
			Assert("Split Health Certificate by Marks", !invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByMarks);
			Assert("Split Health Certificate by Packer", !invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByPacker);
			Assert("Forward Status", invoiceHeader.QuarantineExDocHeader.QH_ForwardStatus.IsEmpty);
			Assert("AMLC Quota Indicator", !invoiceHeader.QuarantineExDocHeader.QH_AMLCQuota);
			Assert("Get customs CAN", !invoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit);
			AssertEquals("Customs Declaration Estimate", JobDeclaration.MessageSubType.NonConfirming, invoiceHeader.JobDeclaration.JE_MessageSubType);
			Assert("Declaration Of Compliance", invoiceHeader.QuarantineExDocHeader.QH_DecOfCompliance.IsEmpty);
			AssertEquals("Imported Product Flag", "NO", invoiceHeader.QuarantineExDocHeader.QH_ImportedProductFlag);
			Assert("Forwardee User Identifier", invoiceHeader.QuarantineExDocHeader.QH_ForwardeeEDIUserIdentifier.IsEmpty);
			Assert("Start Hold Seal", invoiceHeader.QuarantineExDocHeader.QH_StartHoldSeal.IsEmpty);
			Assert("End Hold Seal", invoiceHeader.QuarantineExDocHeader.QH_EndHoldSeal.IsEmpty);
			Assert("Inspection Requested Date", invoiceHeader.QuarantineExDocHeader.QH_InspectionRequestedDate.IsEmpty);
			Assert("Authorised Start Date", invoiceHeader.QuarantineExDocHeader.QH_AuthorisedStartDate.IsEmpty);
			Assert("Authorised End Dtae", invoiceHeader.QuarantineExDocHeader.QH_AuthorisedEndDate.IsEmpty);
			AssertEquals("Authorisation Establishment", "88", invoiceHeader.QuarantineExDocHeader.QH_AuthorisationEstablishment);
			Assert("Authorising Officer ID", invoiceHeader.QuarantineExDocHeader.QH_AuthorisingOfficerID.IsEmpty);
			Assert("Storage Establishment", invoiceHeader.QuarantineExDocHeader.QH_StorageEstablishment.IsEmpty);
			AssertEquals("Ships Compartments", 0, invoiceHeader.QuarantineExDocHeader.Compartments.Count);
			AssertEquals("2 Invoice Lines", 2, invoiceHeader.JobComInvoiceLines.Count);
			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines[0];
			AssertNotNull("Invoice Line 1", line1);
			AssertEquals("Customs Quantity", 1234567.89m, line1.JI_CustomsQuantity);
			AssertEquals("Quarantine Net Quantity", 1234567.89m, line1.QuarantineExDocLine.QL_NetQuantity);
			AssertEquals("Customs Quantity Unit", "KG", line1.JI_CustomsUnitQty);
			AssertEquals("Quarantine Net Quantity Unit", "KGM", line1.QuarantineExDocLine.QL_NetQuantityUnit);
			Assert("Imperial Net Weight", line1.QuarantineExDocLine.QL_ImperialNetWeight.IsEmpty);
			Assert("Imperial Net Weight Unit", line1.QuarantineExDocLine.QL_ImperialNetWeightUnit.IsEmpty);
			Assert("Line Weight", line1.JI_Weight.IsEmpty);
			Assert("Gross Metric Weight", line1.QuarantineExDocLine.QL_GrossMetricWeight.IsEmpty);
			Assert("Line Weight Unit", line1.JI_WeightUQ.IsEmpty);
			Assert("Gross Metric Weight Unit", line1.QuarantineExDocLine.QL_GrossMetricWeightUnit.IsEmpty);
			Assert("Drained Weight", line1.QuarantineExDocLine.QL_DrainedWeight.IsEmpty);
			Assert("Customs Weight Unit", line1.QuarantineExDocLine.QL_AqisCustomsWeightUQ.IsEmpty);
			Assert("Customs Weight", line1.QuarantineExDocLine.QL_AqisCustomsWeight.IsEmpty);
			Assert("Drained Weight Unit", line1.QuarantineExDocLine.QL_DrainedWeightUnit.IsEmpty);
			Assert("Percentage of Milk Protein", line1.QuarantineExDocLine.QL_PercentOfMilkProtein.IsEmpty);
			Assert("Percentage of Milk fat", line1.QuarantineExDocLine.QL_PercentOfMilkFat.IsEmpty);
			Assert("Total Weight of Milk Protein", line1.QuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures.IsEmpty);
			Assert("Total Weight of Milk Fat", line1.QuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures.IsEmpty);
			Assert("Beef/Veal Weight", line1.QuarantineExDocLine.QL_BeefVealWeightAmount.IsEmpty);
			Assert("Chemical Lean Percentage", line1.QuarantineExDocLine.QL_ChemicalLeanPercentage.IsEmpty);
			AssertEquals("Preservation Type", EXDOCPreservationTypeCodes.Codes.Chilled, line1.QuarantineExDocLine.QL_PreservationType);
			AssertEquals("Product Type", "DES", line1.QuarantineExDocLine.QL_ProductType);
			AssertEquals("Pack Type", EXDOCPackTypeCodes.Codes.Cartons, line1.QuarantineExDocLine.QL_PackType);
			Assert("Supplementary Code", line1.QuarantineExDocLine.QL_SupplimentaryCode.IsEmpty);
			AssertEquals("Cut Code", "DC0368", line1.QuarantineExDocLine.QL_CutCode);
			Assert("Aheec Code", line1.JI_Tariff.IsEmpty);
			AssertEquals("Exporter Defined Product Description", "CHILLED DAIRY DESSERT CARTON", line1.JI_Description);
			Assert("Additional Product Description", line1.QuarantineExDocLine.QL_AddtionalProductDescription.IsEmpty);
			Assert("Commercial Product Description", line1.QuarantineExDocLine.QL_CommercialProductDescription.IsEmpty);
			Assert("Product Description Quality Qualifier", line1.QuarantineExDocLine.QL_ProductDescriptionQualityQualifier.IsEmpty);
			Assert("Product Description Location Qualifier", line1.QuarantineExDocLine.QL_ProductDescriptionLocationQualifier.IsEmpty);
			Assert("Label Approval Number", line1.QuarantineExDocLine.QL_LabelApprovalNumber.IsEmpty);
			Assert("Duty Drawback", !line1.JI_Drawback);
			Assert("Motor Vehicle Plan", !line1.JI_MotorVehiclePlan);
			Assert("Texco", !line1.JI_Texco);
			Assert("Quota Approval Reference", line1.QuarantineExDocLine.QL_QuotaApprovalRef.IsEmpty);
			AssertEquals("Use by start", new ZDateTime(2006, 1, 1), line1.QuarantineExDocLine.QL_UseByStart);
			AssertEquals("Use by end", new ZDateTime(2007, 12, 1), line1.QuarantineExDocLine.QL_UseByEnd);
			Assert("Salting Date", line1.QuarantineExDocLine.QL_SaltingDate.IsEmpty);
			Assert("Product Source State", line1.JI_AUState.IsEmpty);
			Assert("Additional Declaration Comments", line1.QuarantineExDocLine.QL_AddtionalDeclarationComments.IsEmpty);
			Assert("Coded Statement 1", line1.QuarantineExDocLine.QL_StatementNumber1.IsEmpty);
			Assert("Coded Statement 2", line1.QuarantineExDocLine.QL_StatementNumber2.IsEmpty);
			Assert("Coded Statement 3", line1.QuarantineExDocLine.QL_StatementNumber3.IsEmpty);
			Assert("Coded Statement 4", line1.QuarantineExDocLine.QL_StatementNumber4.IsEmpty);
			Assert("Coded Statement 5", line1.QuarantineExDocLine.QL_StatementNumber5.IsEmpty);
			Assert("Statement Text", line1.QuarantineExDocLine.QL_StatementText.IsEmpty);
			Assert("Grower Number", line1.QuarantineExDocLine.QL_GrowerNumber.IsEmpty);
			Assert("Import Authority Code", line1.QuarantineExDocLine.QL_ImportAuthorityCode.IsEmpty);
			Assert("Nature Of Commodity", line1.QuarantineExDocLine.QL_NatureOfCommodity.IsEmpty);
			Assert("Treatment Type", line1.QuarantineExDocLine.QL_TreatmentType.IsEmpty);
			Assert("Label Approval Indicator", !line1.QuarantineExDocLine.QL_LabelApprovalIndicator);
			Assert("Ungraded Product Indicator", !line1.QuarantineExDocLine.QL_UngradedProductIndicator);
			Assert("Final Consumer Indicator", !line1.QuarantineExDocLine.QL_FinalConsumer);
			Assert("Client Line Item ID", line1.QuarantineExDocLine.QL_ClientLineItemID.IsEmpty);
			Assert("FOB Amount", line1.JI_LinePrice.IsEmpty);
			Assert("Health Certificate Requested Format", line1.QuarantineExDocLine.QL_HCFormatRequested.IsEmpty);
			AssertEquals("Extra Health Certificate Format", "ZX01,ZX02", line1.QuarantineExDocLine.QL_ExtraCertificate);
			Assert("Health Certificate Description", line1.QuarantineExDocLine.QL_HealthCertificateDescription.IsEmpty);
			AssertEquals("Format Allocated", "ZD035/422,ZD036,ZD037", line1.QuarantineExDocLine.QL_HCFormatAllocated);
			AssertEquals("Certificate Numbers", "123,456,789", line1.QuarantineExDocLine.QL_HCNumber);
			Assert("Related Export Permit Number", line1.JI_RelatedExportPermitNumber.IsEmpty);
			Assert("Releated Export Permit Authority", line1.JI_RelatedExportPermitAuthority.IsEmpty);
			Assert("Related Export Permit Date", line1.JI_RelatedExportPermitDate.IsEmpty);
			AssertEquals("Invoice Quantity", 123456m, line1.JI_InvoiceQuantity);
			AssertEquals("Quarantine Outer Pack Count", 123456, line1.QuarantineExDocLine.QL_OuterPackCount);
			AssertEquals("Invoice Quantity Unit", "CTN", line1.JI_InvoiceUQ);
			AssertEquals("Quarantine Outer Pack Type", EXDOCPacakgeTypeCodes.Codes.Cartons, line1.QuarantineExDocLine.QL_OuterPackType);
			AssertEquals("Outer Pack Accuracy", EXDOCPackAccuracyCodes.Codes.EqualTo, line1.QuarantineExDocLine.QL_OuterPackAccuracy);
			AssertEquals("Shipping Marks", "MARKSTOEQUAL17171171", line1.QuarantineExDocLine.QL_ShippingMarks);
			AssertEquals("Outer Pack Weight", 5000m, line1.QuarantineExDocLine.QL_OuterPackWeight);
			AssertEquals("OUter Pack Weight Unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, line1.QuarantineExDocLine.QL_OuterPackWeightUnit);
			Assert("Intermediate Pack Count", line1.QuarantineExDocLine.QL_IntermediatePackCount.IsEmpty);
			Assert("Intermediate Pack Type", line1.QuarantineExDocLine.QL_IntermediatePackType.IsEmpty);
			Assert("Intermediate Pack Accuracy", line1.QuarantineExDocLine.QL_IntermediatePackAccuracy.IsEmpty);
			Assert("Intermediate Pack Weight", line1.QuarantineExDocLine.QL_IntermediatePackWeight.IsEmpty);
			Assert("Intermediate Pack Weight Unit", line1.QuarantineExDocLine.QL_IntermediatePackWeightUnit.IsEmpty);
			Assert("Inner Pack Count", line1.QuarantineExDocLine.QL_InnerPackCount.IsEmpty);
			Assert("Inner Pack Type", line1.QuarantineExDocLine.QL_InnerPackType.IsEmpty);
			Assert("Inner Pack Accuracy", line1.QuarantineExDocLine.QL_InnerPackAccuracy.IsEmpty);
			Assert("Inner Pack Weight", line1.QuarantineExDocLine.QL_InnerPackWeight.IsEmpty);
			Assert("Inner Pack Weight Unit", line1.QuarantineExDocLine.QL_InnerPackWeightUnit.IsEmpty);
			AssertEquals("Permit Number", "111117777711111ATOF", line1.JI_TempImportNum);
			AssertEquals("Permit Date", new ZDateTime(2006, 12, 9), line1.JI_TempImportDate);
			Assert("Container Linked to Line", line1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("TWENTYABOC45217").IsForInvoiceLine);
			AssertEquals("Process should not be created or updated anymore by incoming message", 0, line1.QuarantineExDocLine.Processes.Count);
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines[1];
			AssertNotNull("Invoice Line 2", line2);
			AssertEquals("Customs Quantity", 9976m, line2.JI_CustomsQuantity);
			AssertEquals("Quarantine Net Quantity", 9976m, line2.QuarantineExDocLine.QL_NetQuantity);
			AssertEquals("Customs Quantity Unit", "KG", line2.JI_CustomsUnitQty);
			AssertEquals("Quarantine Net Quantity Unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, line2.QuarantineExDocLine.QL_NetQuantityUnit);
			Assert("Imperial Net Weight", line2.QuarantineExDocLine.QL_ImperialNetWeight.IsEmpty);
			Assert("Imperial Net Weight Unit", line2.QuarantineExDocLine.QL_ImperialNetWeightUnit.IsEmpty);
			Assert("Gross Weight", line2.JI_Weight.IsEmpty);
			Assert("Quarantine Gross Weight", line2.QuarantineExDocLine.QL_GrossMetricWeight.IsEmpty);
			Assert("Gross Weight Unit", line2.JI_WeightUQ.IsEmpty);
			Assert("Quarantine Gross Weight Unit", line2.QuarantineExDocLine.QL_GrossMetricWeightUnit.IsEmpty);
			Assert("Drained Weight", line2.QuarantineExDocLine.QL_DrainedWeight.IsEmpty);
			Assert("Drained Weight Unit", line2.QuarantineExDocLine.QL_DrainedWeightUnit.IsEmpty);
			Assert("Percent Of Milk Protein", line2.QuarantineExDocLine.QL_PercentOfMilkProtein.IsEmpty);
			Assert("Percent Of Milk Fat", line2.QuarantineExDocLine.QL_PercentOfMilkFat.IsEmpty);
			Assert("Total Weight Of Milk Protein", line2.QuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures.IsEmpty);
			Assert("Total Weight Of Milk Fat", line2.QuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures.IsEmpty);
			Assert("Beef Veal Weight Amount", line2.QuarantineExDocLine.QL_BeefVealWeightAmount.IsEmpty);
			Assert("Chemical Lean Percentage", line2.QuarantineExDocLine.QL_ChemicalLeanPercentage.IsEmpty);
			AssertEquals("Preservation Type", EXDOCPreservationTypeCodes.Codes.Chilled, line2.QuarantineExDocLine.QL_PreservationType);
			AssertEquals("Product Type", "BUT", line2.QuarantineExDocLine.QL_ProductType);
			AssertEquals("Pack Type", EXDOCPackTypeCodes.Codes.BagInABox, line2.QuarantineExDocLine.QL_PackType);
			Assert("Supplimentary Code", line2.QuarantineExDocLine.QL_SupplimentaryCode.IsEmpty);
			AssertEquals("Cut Code", "DC0147", line2.QuarantineExDocLine.QL_CutCode);
			Assert("Ahecc", line2.JI_Tariff.IsEmpty);
			AssertEquals("Exporter Defined Product Description", "CHILLED BUTTER IN BULK PACK", line2.JI_Description);
			Assert("Additional Product Description", line2.QuarantineExDocLine.QL_AddtionalProductDescription.IsEmpty);
			Assert("Commercial Product Description", line2.QuarantineExDocLine.QL_CommercialProductDescription.IsEmpty);
			Assert("Product Description Quality Qualifier", line2.QuarantineExDocLine.QL_ProductDescriptionQualityQualifier.IsEmpty);
			Assert("Product Description Location Qualifier", line2.QuarantineExDocLine.QL_ProductDescriptionLocationQualifier.IsEmpty);
			Assert("Label Approval Number", line2.QuarantineExDocLine.QL_LabelApprovalNumber.IsEmpty);
			Assert("Duty Drawback", !line2.JI_Drawback);
			Assert("Motor Vehicle Plan", !line2.JI_MotorVehiclePlan);
			Assert("Texco", !line2.JI_Texco);
			Assert("Quota Approval Reference", line2.QuarantineExDocLine.QL_QuotaApprovalRef.IsEmpty);
			AssertEquals("Use by start", new ZDateTime(2006, 1, 1), line2.QuarantineExDocLine.QL_UseByStart);
			AssertEquals("Use by end", new ZDateTime(2007, 12, 1), line2.QuarantineExDocLine.QL_UseByEnd);
			Assert("Salting Date", line2.QuarantineExDocLine.QL_SaltingDate.IsEmpty);
			Assert("Product Source State", line2.JI_AUState.IsEmpty);
			Assert("Additional Declaration Comments", line2.QuarantineExDocLine.QL_AddtionalDeclarationComments.IsEmpty);
			Assert("Coded Statement 1", line2.QuarantineExDocLine.QL_StatementNumber1.IsEmpty);
			Assert("Coded Statement 2", line2.QuarantineExDocLine.QL_StatementNumber2.IsEmpty);
			Assert("Coded Statement 3", line2.QuarantineExDocLine.QL_StatementNumber3.IsEmpty);
			Assert("Coded Statement 4", line2.QuarantineExDocLine.QL_StatementNumber4.IsEmpty);
			Assert("Coded Statement 5", line2.QuarantineExDocLine.QL_StatementNumber5.IsEmpty);
			Assert("Statement Text", line2.QuarantineExDocLine.QL_StatementText.IsEmpty);
			Assert("Grower Number", line2.QuarantineExDocLine.QL_GrowerNumber.IsEmpty);
			Assert("Import Authority Code", line2.QuarantineExDocLine.QL_ImportAuthorityCode.IsEmpty);
			AssertEquals("Nature Of Commodity", "CT", line2.QuarantineExDocLine.QL_NatureOfCommodity);
			AssertEquals("Treatment Type", "BO", line2.QuarantineExDocLine.QL_TreatmentType);
			Assert("Label Approval Indicator", line2.QuarantineExDocLine.QL_LabelApprovalIndicator);
			Assert("Ungraded Product Indicator", line2.QuarantineExDocLine.QL_UngradedProductIndicator);
			Assert("Final Consumer Indicator", line2.QuarantineExDocLine.QL_FinalConsumer);
			AssertEquals("Client Line Item ID", "LINE1LINE2LINE3LINE4", line2.QuarantineExDocLine.QL_ClientLineItemID);
			Assert("FOB Amount", line2.JI_LinePrice.IsEmpty);
			Assert("Health Certificate Requested Format", line2.QuarantineExDocLine.QL_HCFormatRequested.IsEmpty);
			Assert("Extra Health Certificate Format", line2.QuarantineExDocLine.QL_ExtraCertificate.IsEmpty);
			Assert("Health Certificate Description", line2.QuarantineExDocLine.QL_HealthCertificateDescription.IsEmpty);
			AssertEquals("Format Allocated", "ZD035P/422", line2.QuarantineExDocLine.QL_HCFormatAllocated);
			Assert("Related Export Permit Number", line2.JI_RelatedExportPermitNumber.IsEmpty);
			Assert("Releated Export Permit Authority", line2.JI_RelatedExportPermitAuthority.IsEmpty);
			Assert("Related Export Permit Date", line2.JI_RelatedExportPermitDate.IsEmpty);
			AssertEquals("Invoice Quantity", 789012m, line2.JI_InvoiceQuantity);
			AssertEquals("Quarantine Outer Pack Count", 789012, line2.QuarantineExDocLine.QL_OuterPackCount);
			AssertEquals("Invoice Quantity Unit", "CTN", line2.JI_InvoiceUQ);
			AssertEquals("Quarantine Outer Pack Type", EXDOCPacakgeTypeCodes.Codes.Cartons, line2.QuarantineExDocLine.QL_OuterPackType);
			AssertEquals("Outer Pack Accuracy", EXDOCPackAccuracyCodes.Codes.EqualTo, line2.QuarantineExDocLine.QL_OuterPackAccuracy);
			AssertEquals("Shipping Marks", "MARKSTOEQUAL28282828", line2.QuarantineExDocLine.QL_ShippingMarks);
			AssertEquals("Outer Pack Weight", 6000m, line2.QuarantineExDocLine.QL_OuterPackWeight);
			AssertEquals("OUter Pack Weight Unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, line2.QuarantineExDocLine.QL_OuterPackWeightUnit);
			Assert("Intermediate Pack Count", line2.QuarantineExDocLine.QL_IntermediatePackCount.IsEmpty);
			Assert("Intermediate Pack Type", line2.QuarantineExDocLine.QL_IntermediatePackType.IsEmpty);
			Assert("Intermediate Pack Accuracy", line2.QuarantineExDocLine.QL_IntermediatePackAccuracy.IsEmpty);
			Assert("Intermediate Pack Weight", line2.QuarantineExDocLine.QL_IntermediatePackWeight.IsEmpty);
			Assert("Intermediate Pack Weight Unit", line2.QuarantineExDocLine.QL_IntermediatePackWeightUnit.IsEmpty);
			Assert("Inner Pack Count", line2.QuarantineExDocLine.QL_InnerPackCount.IsEmpty);
			Assert("Inner Pack Type", line2.QuarantineExDocLine.QL_InnerPackType.IsEmpty);
			Assert("Inner Pack Accuracy", line2.QuarantineExDocLine.QL_InnerPackAccuracy.IsEmpty);
			Assert("Inner Pack Weight", line2.QuarantineExDocLine.QL_InnerPackWeight.IsEmpty);
			Assert("Inner Pack Weight Unit", line2.QuarantineExDocLine.QL_InnerPackWeightUnit.IsEmpty);
			Assert("Permit Number is empty", line2.JI_TempImportNum.IsEmpty);
			Assert("Permit Date", line2.JI_TempImportDate.IsEmpty);
			Assert("Container Linked to Line", line2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("TWENTYABOC45217").IsForInvoiceLine);
			AssertEquals("Process is no longer created or updated by messaage processing", 0, line2.QuarantineExDocLine.Processes.Count);
			AssertEquals("Response Email Subject", "RFP Accepted for B00001000", messageProcessor.ResponseEmail.Subject);
		}

		public void TestFinalConsumer()
		{
			SetupTestDeclaration();

			var message = Factory.New<EDIMessage>();
			var messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("Transfer.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_OwnerRef, "TRANS ATT G");
			JobDeclaration declaration = Factory.LoadTop1<JobDeclaration>(query);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices[0];
			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines[0];
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines[1];
			AssertEquals("Final Consumer", false, line1.QuarantineExDocLine.QL_FinalConsumer);
			AssertEquals("Final Consumer set", true, line2.QuarantineExDocLine.QL_FinalConsumer);
		}

		public void TestCompletedWithCertificateNumber()
		{
			SetupTestDeclaration();

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CompletedWithCertificateNumber.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("Message is acknowledged", EDIMessage.Status.Acknowledged, message.EM_MessageType);
			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, exDocHeader.Declaration.JE_MessageStatus);
			AssertEquals("Export Permit Number", "PIM1039211", exDocHeader.QH_ExportPermitNumber);
			AssertEquals("Request For Permit Number", "2077447", exDocHeader.QH_RequestForPermitNumber);
			AssertEquals("Request for Permit Compliance Isnpected", "COM", exDocHeader.RequestForPermitStatus);
			AssertEquals("Authorising Officer Identifier", "KYNANB", exDocHeader.QH_AuthorisingOfficerID);
			Assert("Start Hold Seal is empty", exDocHeader.QH_StartHoldSeal.IsEmpty);
			Assert("End Hold Seal is empty", exDocHeader.QH_EndHoldSeal.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertEquals("Containers against declaration", 1, exDocHeader.Declaration.CusContainers.Count);
			AssertEquals("Container Number is correct", "ABOC8457924", exDocHeader.Declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Container Seal is correct", ZString.Empty, exDocHeader.Declaration.CusContainers[0].CO_Seal);
			foreach (JobComInvoiceLine invoiceLine in exDocHeader.InvoiceHeader.JobComInvoiceLines)
			{
				AssertEquals("Health Certificate Description", ZString.Empty, invoiceLine.QuarantineExDocLine.QL_HealthCertificateDescription);
				AssertEquals("Allocated Certificate Format", "E188/506", invoiceLine.QuarantineExDocLine.QL_HCFormatAllocated);
				AssertEquals("Health Certificate Number", "2073124", invoiceLine.QuarantineExDocLine.QL_HCNumber);
				Assert("Container is linked", invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("ABOC8457924").IsForInvoiceLine);
			}

			AssertEquals("Response Email Subject", "RFP Accepted for B9999999", messageProcessor.ResponseEmail.Subject);
		}

		public void TestFinalWithLongHCDescription()
		{
			SetupTestDeclaration();

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("FinalWithLongHCDescription.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("Message is acknowledged", EDIMessage.Status.Acknowledged, message.EM_MessageType);
			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, exDocHeader.Declaration.JE_MessageStatus);
			Assert("Export Permit Number", exDocHeader.QH_ExportPermitNumber.IsEmpty);
			AssertEquals("Request For Permit Number", "2078036", exDocHeader.QH_RequestForPermitNumber);
			AssertEquals("Request for Permit Compliance Isnpected", "FIN", exDocHeader.RequestForPermitStatus);
			Assert("Authorising Officer Identifier", exDocHeader.QH_AuthorisingOfficerID.IsEmpty);
			Assert("Authorising Comments", exDocHeader.QH_InspectorComments.IsEmpty);
			Assert("Start Hold Seal is empty", exDocHeader.QH_StartHoldSeal.IsEmpty);
			Assert("End Hold Seal is empty", exDocHeader.QH_EndHoldSeal.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertEquals("Containers against declaration", 0, exDocHeader.Declaration.CusContainers.Count);
			AssertEquals("Number of Invoice Lines", 2, exDocHeader.InvoiceHeader.JobComInvoiceLines.Count);
			AssertEquals("Health Certificate Description", "FROZEN BONELESS GRAIN FED YOUNG PRIME STEER CHUCK 5 RIB STEAK BULK PACK", exDocHeader.InvoiceHeader.JobComInvoiceLines[0].QuarantineExDocLine.QL_HealthCertificateDescription);
			AssertEquals("Allocated Certificate Format", "E171/5", exDocHeader.InvoiceHeader.JobComInvoiceLines[0].QuarantineExDocLine.QL_HCFormatAllocated);
			Assert("Health Certificate Number", exDocHeader.InvoiceHeader.JobComInvoiceLines[0].QuarantineExDocLine.QL_HCNumber.IsEmpty);
			AssertEquals("Health Certificate Description", "FROZEN BONELESS GRAIN FED YOUNG PRIME STEER SHORT RIB PLATE STEAK BULK PACK", exDocHeader.InvoiceHeader.JobComInvoiceLines[1].QuarantineExDocLine.QL_HealthCertificateDescription);
			AssertEquals("Allocated Certificate Format", "E171/5", exDocHeader.InvoiceHeader.JobComInvoiceLines[1].QuarantineExDocLine.QL_HCFormatAllocated);
			Assert("Health Certificate Number", exDocHeader.InvoiceHeader.JobComInvoiceLines[1].QuarantineExDocLine.QL_HCNumber.IsEmpty);

			AssertEquals("Response Email Subject", "RFP Accepted for B9999999", messageProcessor.ResponseEmail.Subject);
		}

		public void TestSuspendedWithInspectorComments()
		{
			SetupTestDeclaration();

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("SuspendedWithInspectorComments.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("Message is acknowledged", EDIMessage.Status.Acknowledged, message.EM_MessageType);
			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, exDocHeader.Declaration.JE_MessageStatus);
			AssertEquals("Export Permit Number", "PIM1039300", exDocHeader.QH_ExportPermitNumber);
			AssertEquals("Request For Permit Number", "2078037", exDocHeader.QH_RequestForPermitNumber);
			AssertEquals("Request for Permit Compliance Isnpected", "SUS", exDocHeader.RequestForPermitStatus);
			AssertEquals("Authorising Officer Identifier", "KYLIEBL", exDocHeader.QH_AuthorisingOfficerID);
			AssertEquals("Authorising Comments", "POOLS OF BLOOD IN CONTAINER", exDocHeader.QH_InspectorComments);
			Assert("Start Hold Seal is empty", exDocHeader.QH_StartHoldSeal.IsEmpty);
			Assert("End Hold Seal is empty", exDocHeader.QH_EndHoldSeal.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertEquals("Containers against declaration", 1, exDocHeader.Declaration.CusContainers.Count);
			AssertEquals("Container Number is correct", "TWENTYABOC45217", exDocHeader.Declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Container Seal is correct", ZString.Empty, exDocHeader.Declaration.CusContainers[0].CO_Seal);
			AssertEquals("Number of Invoice Lines", 2, exDocHeader.InvoiceHeader.JobComInvoiceLines.Count);
			AssertEquals("Health Certificate Description", "FROZEN BONELESS GRAIN FED YOUNG PRIME STEER CHUCK 5 RIB STEAK BULK PACK", exDocHeader.InvoiceHeader.JobComInvoiceLines[0].QuarantineExDocLine.QL_HealthCertificateDescription);
			AssertEquals("Allocated Certificate Format", "E171/5", exDocHeader.InvoiceHeader.JobComInvoiceLines[0].QuarantineExDocLine.QL_HCFormatAllocated);
			Assert("Health Certificate Number", exDocHeader.InvoiceHeader.JobComInvoiceLines[0].QuarantineExDocLine.QL_HCNumber.IsEmpty);
			Assert("Container is linked", exDocHeader.InvoiceHeader.JobComInvoiceLines[0].ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("TWENTYABOC45217").IsForInvoiceLine);
			AssertEquals("Health Certificate Description", "FROZEN BONELESS GRAIN FED YOUNG PRIME STEER SHORT RIB PLATE STEAK BULK PACK", exDocHeader.InvoiceHeader.JobComInvoiceLines[1].QuarantineExDocLine.QL_HealthCertificateDescription);
			AssertEquals("Allocated Certificate Format", "E171/5", exDocHeader.InvoiceHeader.JobComInvoiceLines[1].QuarantineExDocLine.QL_HCFormatAllocated);
			Assert("Health Certificate Number", exDocHeader.InvoiceHeader.JobComInvoiceLines[1].QuarantineExDocLine.QL_HCNumber.IsEmpty);
			Assert("Container is linked", exDocHeader.InvoiceHeader.JobComInvoiceLines[1].ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("TWENTYABOC45217").IsForInvoiceLine);

			AssertEquals("Response Email Subject", "RFP Accepted for B9999999", messageProcessor.ResponseEmail.Subject);
		}

		public void TestAcceptTransferUsingRFPNumber()
		{
			SetupTestDeclaration();
			exDocHeader.QH_RequestForPermitNumber = "2078036";
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("AcceptTransferTestWithDifferentReference.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("Message is acknowledged", EDIMessage.Status.Acknowledged, message.EM_MessageType);
			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, exDocHeader.Declaration.JE_MessageStatus);
			Assert("Export Permit Number", exDocHeader.QH_ExportPermitNumber.IsEmpty);
			AssertEquals("Request For Permit Number", "2078036", exDocHeader.QH_RequestForPermitNumber);
			AssertEquals("Request for Permit Compliance Isnpected", "FIN", exDocHeader.RequestForPermitStatus);
			Assert("Authorising Officer Identifier", exDocHeader.QH_AuthorisingOfficerID.IsEmpty);
			Assert("Start Hold Seal is empty", exDocHeader.QH_StartHoldSeal.IsEmpty);
			Assert("End Hold Seal is empty", exDocHeader.QH_EndHoldSeal.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertEquals("Containers against declaration", 0, exDocHeader.Declaration.CusContainers.Count);
			foreach (JobComInvoiceLine invoiceLine in exDocHeader.InvoiceHeader.JobComInvoiceLines)
			{
				Assert("Health Certificate Description", invoiceLine.QuarantineExDocLine.QL_HealthCertificateDescription.IsEmpty);
				Assert("Allocated Certificate Format", invoiceLine.QuarantineExDocLine.QL_HCFormatAllocated.IsEmpty);
			}

			AssertEquals("Response Email Subject", "RFP Accepted for B9999999", messageProcessor.ResponseEmail.Subject);
		}

		public void TestNotesAddedToDeclaration()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			SetupTestDeclaration();
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			var messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NotesAddTest.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("There are 5 notes on the declaration", 5, exDocHeader.Declaration.Notes.GetAllNotes().Count);
			StmNote[] note = exDocHeader.Declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCNotifyText.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOC Notify Party", note[0]);
			AssertEquals("EXDOC Notify Party Text", "NOTIFY TEXT", note[0].ST_NoteDataAsText);
			note = exDocHeader.Declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOCLetterOfCredit", note[0]);
			AssertEquals("EXDOCLetterOfCredit Text", "LETTER OF CREDIT TEXT", note[0].ST_NoteDataAsText);
			note = exDocHeader.Declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOCAdditionalInformation", note[0]);
			AssertEquals("EXDOCAdditionalInformation Text", "ADDITIONAL TEXT", note[0].ST_NoteDataAsText);
			note = exDocHeader.Declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOCAmendmentReason", note[0]);
			AssertEquals("EXDOCAmendmentReason Text", "AMENDMENT REASON TEXT", note[0].ST_NoteDataAsText);
			note = exDocHeader.Declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			Assert("One unmatched org note on declaration", note.Length == 1);
		}

		public void TestNotesUpdatedOnDeclaration()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			SetupTestDeclaration();
			var notifyNote = exDocHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCNotifyText.Description, "OLD NOTIFY TEXT");
			var letterOfCreditNote = exDocHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description, "OLD LETTER OF CREDIT TEXT");
			var additionalInfoNote = exDocHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description, "OLD ADDITIONAL INFO TEXT");
			var amendmentReasonNote = exDocHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description, "OLD AMENDMENT REASON TEXT");
			var unmatchedNote = exDocHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, "OLD UNMATCHED ORG TEXT");
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			var messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NotesAddTest.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("There are 4o notes on the declaration", 5, exDocHeader.Declaration.Notes.GetAllNotes().Count);
			AssertEquals("EXDOC Notify Party Text", "NOTIFY TEXT", notifyNote.ST_NoteDataAsText);
			AssertEquals("EXDOCLetterOfCredit Text", "LETTER OF CREDIT TEXT", letterOfCreditNote.ST_NoteDataAsText);
			AssertEquals("EXDOCAdditionalInformation Text", "ADDITIONAL TEXT", additionalInfoNote.ST_NoteDataAsText);
			AssertEquals("EXDOCAmendmentReason Text", "AMENDMENT REASON TEXT", amendmentReasonNote.ST_NoteDataAsText);
			var note = exDocHeader.Declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			Assert("Still only one unmatched org note on declaration", note.Length == 1);
			AssertNotEquals("Text updated", "OLD UNMATCHED ORG TEXT", unmatchedNote.ST_NoteDataAsText);
		}

		public void TestNotesAddedToShipment()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			SetupTestDeclaration();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = exDocHeader.Declaration.JE_DeclarationReference;
			exDocHeader.Declaration.JE_JS = shipment.PK;
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			var messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NotesAddTest.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("There are no notes on the declaration", 0, exDocHeader.Declaration.Notes.GetAllNotes().Count);
			StmNote[] note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCNotifyText.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOC Notify Party", note[0]);
			AssertEquals("EXDOC Notify Party Text", "NOTIFY TEXT", note[0].ST_NoteDataAsText);
			note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOCLetterOfCredit", note[0]);
			AssertEquals("EXDOCLetterOfCredit Text", "LETTER OF CREDIT TEXT", note[0].ST_NoteDataAsText);
			note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOCAdditionalInformation", note[0]);
			AssertEquals("EXDOCAdditionalInformation Text", "ADDITIONAL TEXT", note[0].ST_NoteDataAsText);
			note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description);
			AssertEquals("There is only one", 1, note.Length);
			AssertNotNull("EXDOCAmendmentReason", note[0]);
			AssertEquals("EXDOCAmendmentReason Text", "AMENDMENT REASON TEXT", note[0].ST_NoteDataAsText);
			note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			Assert("One unmatched org note on shipment", note.Length == 1);
		}

		public void TestNotesUpdatedOnShipment()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			SetupTestDeclaration();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = exDocHeader.Declaration.JE_DeclarationReference;
			exDocHeader.Declaration.JE_JS = shipment.PK;
			var notifyNote = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCNotifyText.Description, "OLD NOTIFY TEXT");
			var letterOfCreditNote = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description, "OLD LETTER OF CREDIT TEXT");
			var additionalInfoNote = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description, "OLD ADDITIONAL INFO TEXT");
			var amendmentReasonNote = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description, "OLD AMENDMENT REASON TEXT");
			var unmatchedNote = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, "OLD UNMATCHED ORG TEXT");
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			var messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("NotesAddTest.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("There are no notes on the declaration", 0, exDocHeader.Declaration.Notes.GetAllNotes().Count);
			AssertEquals("EXDOC Notify Party Text", "NOTIFY TEXT", notifyNote.ST_NoteDataAsText);
			AssertEquals("EXDOCLetterOfCredit Text", "LETTER OF CREDIT TEXT", letterOfCreditNote.ST_NoteDataAsText);
			AssertEquals("EXDOCAdditionalInformation Text", "ADDITIONAL TEXT", additionalInfoNote.ST_NoteDataAsText);
			AssertEquals("EXDOCAmendmentReason Text", "AMENDMENT REASON TEXT", amendmentReasonNote.ST_NoteDataAsText);
			var note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			Assert("Still only one unmatched org note on shipment", note.Length == 1);
			AssertNotEquals("Text updated", "OLD UNMATCHED ORG TEXT", unmatchedNote.ST_NoteDataAsText);
		}

		public void TestTransferWithLargeGrossWeightInKGM()
		{
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TransferWithLargeGrossWeightInKG.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			ZQuery query = new ZQuery(JobDeclarationSchema.JE_OwnerRef, "MEAT TRAN 7002 EDN 2");
			JobDeclaration declaration = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull("Declaration isn't null", declaration);
			AssertEquals("Customs EDN", "AAACERCTC", declaration.DeclarationNumber);
			AssertEquals("Customs EDN Status is clear", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Entry status is clear", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			var statusLogs = new LogsForNominatedEvent(declaration.Logs, Events.StatusChange);
			AssertEquals("log posted", 1, statusLogs.Count);
			AssertEquals("AQSDec#: AAACERCTC", statusLogs[0].SL_Reference);
			QuarantineExDocHeader header = declaration.Invoices[0].QuarantineExDocHeader;
			AssertEquals("Request For Permit Number", "2078293", header.QH_RequestForPermitNumber);
			AssertEquals("Request for Permit Compliance Isnpected", "HCR", header.RequestForPermitStatus);
			JobComInvoiceLine line1 = header.InvoiceHeader.JobComInvoiceLines[0];
			AssertEquals("Invoice Line 1 Gross Weight has been converted to Tonnes", 1234.568m, line1.JI_Weight);
			AssertEquals("Invoice Line 1 Gross Weight UQ is Tonnes", Core.Constants.Weight.Tonnes, line1.JI_WeightUQ);
			AssertEquals("Quarantine Line 1 Gross Metric Weight is in Kilograms", 1234.568m, line1.QuarantineExDocLine.QL_GrossMetricWeight);
			AssertEquals("Quarantine Line 1 Gross Metric Weight Unit is Kilograms", EXDOCMetricWeightUnitCodes.Codes.Kilogram, line1.QuarantineExDocLine.QL_GrossMetricWeightUnit);
			JobComInvoiceLine line2 = header.InvoiceHeader.JobComInvoiceLines[1];
			AssertEquals("Invoice Line 2 Gross Weight has been converted to Tonnes", 9876.543m, line2.JI_Weight);
			AssertEquals("Invoice Line 2 Gross Weight UQ is Tonnes", Core.Constants.Weight.Tonnes, line2.JI_WeightUQ);
			AssertEquals("Quarantine Line 2 Gross Metric Weight is in Kilograms", 9876.543m, line2.QuarantineExDocLine.QL_GrossMetricWeight);
			AssertEquals("Quarantine Line 2 Gross Metric Weight Unit is Kilograms", EXDOCMetricWeightUnitCodes.Codes.Kilogram, line2.QuarantineExDocLine.QL_GrossMetricWeightUnit);
			AssertEquals("Response Email Subject", "RFP Accepted for B00001000", messageProcessor.ResponseEmail.Subject);
		}

		public void TestUnmatchedOrganisationNote()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TransferWithLargeGrossWeightInKG.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_OwnerRef, "MEAT TRAN 7002 EDN 2");
			JobDeclaration declaration = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull("Declaration isn't null", declaration);
			AssertNotNull("Importer not null", declaration.Importer);
			AssertEquals("Organisation is Unmatched", "UNMATCHED", declaration.Importer.OH_Code);
			StmNote[] unmatchedOrganisationNotes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			Assert("Length is 1", unmatchedOrganisationNotes.Length == 1);
			AssertMultilineASCIIEquals("Note has been added to the Declaration", EXDOCMessageDecoderTest.UnmatchedTestText, unmatchedOrganisationNotes[0].GetUnSerializeNoteText());
		}

		public void TestUnmatchedRFPCreatesNewDeclaration()
		{
			var aQISAcknowledgementsGroup = Factory.New<GlbGroup>();
			aQISAcknowledgementsGroup.GG_Code = "G1";
			aQISAcknowledgementsGroup.Staff.AddNew();
			aQISAcknowledgementsGroup.Staff[0].GS_EmailAddress = "blah@blah.com";
			AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aQISAcknowledgementsGroup.PK.ToGuid());

			var org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TransferWithLargeGrossWeightInKG.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);

			var query = new ZQuery(JobDeclarationSchema.JE_OwnerRef, "MEAT TRAN 7002 EDN 2");
			var declaration = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull("Declaration isn't null", declaration);
			AssertNotNull("Importer not null", declaration.Importer);
			AssertEquals("Organisation is Unmatched", "UNMATCHED", declaration.Importer.OH_Code);

			ZQuery mailItemQuery = new ZQuery();
			var emailGenerated = Factory.LoadTop1<MailItem>(mailItemQuery);
			AssertNotNull("Unmatched RFP message processed & new declaration generated", emailGenerated);
			AssertEquals("Email generated after creating a new declaration from an unmatched third party AQUIS RFP", "RFP Accepted for B00001000", emailGenerated.MI_Subject);
			AssertEquals("Email details", true, emailGenerated.MI_Body.Contains("Job Number : B00001000"));
			AssertEquals("Email details", true, emailGenerated.MI_Body.Contains("RFP Number : 2078293"));
			AssertEquals("Email details", true, emailGenerated.MI_Body.Contains("RFP Status : HCRD - Health Certificate Ready"));
			AssertEquals("Email details", true, emailGenerated.MI_Body.Contains("Export Permit Number : PIM1039444"));
			AssertEquals("Email details", true, emailGenerated.MI_Body.Contains("GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH."));

			AssertEquals("Response Email Subject", "RFP Accepted for B00001000", messageProcessor.ResponseEmail.Subject);
		}

		public void TestWithdrawalResponse()
		{
			SetupTestDeclaration();
			exDocHeader.QH_RequestForPermitNumber = "2078036";
			exDocHeader.RequestForPermitStatus = "COM";
			exDocHeader.QH_ExportPermitNumber = "PIM1039211";
			exDocHeader.Declaration.DeclarationNumber = "AAACERCTC";
			exDocHeader.Declaration.ExportEntryNumber.CE_EntryStatus = "CLR";
			exDocHeader.Declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("WithdrawalResponse.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			Assert("RFP Number cleared", exDocHeader.QH_RequestForPermitNumber.IsEmpty);
			Assert("RFP Status is empty", exDocHeader.RequestForPermitStatus.IsEmpty);
			Assert("Permit Number is cleared", exDocHeader.QH_ExportPermitNumber.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertNull("Declaration Status is empty", exDocHeader.Declaration.ExportEntryNumber);
			Assert("Entry status is empty", exDocHeader.Declaration.JE_EntryStatus.IsEmpty);
		}

		public void TestAcknowledgementRejectionResponse()
		{
			SetupTestDeclaration();
			exDocHeader.QH_RequestForPermitNumber = "2079173";
			exDocHeader.RequestForPermitStatus = "FIN";
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("AcknowledgementRejection.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			AssertEquals("RFP Number cleared", "2079173", exDocHeader.QH_RequestForPermitNumber);
			AssertEquals("RFP Status is empty", "FIN", exDocHeader.RequestForPermitStatus);
			Assert("Permit Number is empty", exDocHeader.QH_ExportPermitNumber.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertNull("Declaration Status is empty", exDocHeader.Declaration.ExportEntryNumber);
			Assert("Entry status is empty", exDocHeader.Declaration.JE_EntryStatus.IsEmpty);
		}

		public void TestRejectionResponse()
		{
			SetupTestDeclaration();
			exDocHeader.QH_RequestForPermitNumber = ZString.Empty;
			exDocHeader.RequestForPermitStatus = ZString.Empty;
			exDocHeader.Declaration.JE_MessageStatus = "AWT";
			Factory.Save();

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("RejectionResponse.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			Assert("RFP Number cleared", exDocHeader.QH_RequestForPermitNumber.IsEmpty);
			Assert("RFP Status is empty", exDocHeader.RequestForPermitStatus.IsEmpty);
			Assert("Permit Number is empty", exDocHeader.QH_ExportPermitNumber.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertNull("Declaration Status is empty", exDocHeader.Declaration.ExportEntryNumber);
			Assert("Entry status is empty", exDocHeader.Declaration.JE_EntryStatus.IsEmpty);
			AssertEquals("Message status is Rejected", "REJ", exDocHeader.Declaration.JE_MessageStatus);
		}

		public void TestCertReqRejectionResponse()
		{
			SetupTestDeclaration();
			exDocHeader.QH_RequestForPermitNumber = ZString.Empty;
			exDocHeader.RequestForPermitStatus = ZString.Empty;
			exDocHeader.Declaration.JE_MessageStatus = "AWT";
			Factory.Save();
			EDIMessage message = Factory.New<EDIMessage>();
			string messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("RejectedCertReqResponse.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			Assert("RFP Number cleared", exDocHeader.QH_RequestForPermitNumber.IsEmpty);
			Assert("RFP Status is empty", exDocHeader.RequestForPermitStatus.IsEmpty);
			Assert("Cert Request Number cleared", exDocHeader.CertificateRequestNumber.IsEmpty);
			Assert("Cert Request Status is empty", exDocHeader.CertificateStatus.IsEmpty);
			Assert("Permit Number is empty", exDocHeader.QH_ExportPermitNumber.IsEmpty);
			Assert("Declaration Number is empty", exDocHeader.Declaration.DeclarationNumber.IsEmpty);
			AssertNull("Declaration Status is empty", exDocHeader.Declaration.ExportEntryNumber);
			Assert("Entry status is empty", exDocHeader.Declaration.JE_EntryStatus.IsEmpty);
			AssertEquals("Message status is Rejected", "REJ", exDocHeader.Declaration.JE_MessageStatus);
			AssertEquals("Response Email Subject", "Certificate Request Rejected for B9999999", messageProcessor.ResponseEmail.Subject);
			Assert("Response Email Body doesn't contain RFP", !messageProcessor.ResponseEmail.Body.Contains("RFP"));
			Assert("Response Email Body contains Certificate Request Number", messageProcessor.ResponseEmail.Body.Contains("Certificate Request Number"));
			Assert("Response Email Body contains Certificate Request Status", messageProcessor.ResponseEmail.Body.Contains("Certificate Request Status"));
			Assert("Response Email Body does not contains missing line data", !messageProcessor.ResponseEmail.Body.Contains("data for lines not found on this declaration"));
		}

		public void TestMatchAQSDeclarationWithOwnerRef()
		{
			var org = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = org.Branches.AddNew();
			branch1.FillWithValidTestData();
			var branch2 = org.Branches.AddNew();
			branch2.FillWithValidTestData();

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Export;
			dec1.JE_GB = branch1.PK;
			dec1.JE_OwnerRef = "B9999999";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			dec2.JE_GB = branch2.PK;
			dec2.JE_OwnerRef = "B9999999";
			var invoice = dec2.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			dec2.JE_MessageStatus = "AWT";

			Factory.Save();

			var message = Factory.New<EDIMessage>();
			string messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("AcceptedCertReqResponse.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);

			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, dec2.JE_MessageStatus);
		}

		public void TestMatchAQSDeclrationWithDecReference()
		{
			var org = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = org.Branches.AddNew();
			branch1.FillWithValidTestData();
			var branch2 = org.Branches.AddNew();
			branch2.FillWithValidTestData();

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Export;
			dec1.JE_GB = branch1.PK;
			dec1.JE_DeclarationReference = "B9999998";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			dec2.JE_GB = branch2.PK;
			dec2.JE_DeclarationReference = "B9999999";
			var invoice = dec2.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			dec2.JE_MessageStatus = "AWT";

			Factory.Save();

			var message = Factory.New<EDIMessage>();
			string messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("AcceptedCertReqResponse.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);

			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, dec2.JE_MessageStatus);
		}

		public void TestCertReqAcceptedResponse()
		{
			SetupTestDeclaration();
			exDocHeader.QH_RequestForPermitNumber = ZString.Empty;
			exDocHeader.RequestForPermitStatus = ZString.Empty;
			exDocHeader.Declaration.JE_MessageStatus = "AWT";
			Factory.Save();
			EDIMessage message = Factory.New<EDIMessage>();
			string messageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("AcceptedCertReqResponse.edi"));
			message.EM_MessageText = messageText.Replace("\r\n", "").Replace('+', '\x1d').Replace(':', '\x1f').Replace('|', '\x1c');
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageProcessor.ProcessMessage(message);
			AssertEquals("Message is acknowledged", EDIMessage.Status.Acknowledged, message.EM_MessageType);
			AssertEquals("Job Declaration message status is acknowledged", EDIMessage.Status.Acknowledged, exDocHeader.Declaration.JE_MessageStatus);
			AssertEquals("Cert Request Number", "62943261", exDocHeader.CertificateRequestNumber);
			AssertEquals("Cert Request Status", "A", exDocHeader.CertificateStatus);
			AssertEquals("Response Email Subject", "Certificate Request Accepted for B9999999", messageProcessor.ResponseEmail.Subject);
			Assert("Response Email Body doesn't contain RFP", !messageProcessor.ResponseEmail.Body.Contains("RFP"));
			Assert("Response Email Body contains Certificate Request Number", messageProcessor.ResponseEmail.Body.Contains("Certificate Request Number"));
			Assert("Response Email Body contains Certificate Request Status", messageProcessor.ResponseEmail.Body.Contains("Certificate Request Status"));
			Assert("Response Email Body contains missing line data", messageProcessor.ResponseEmail.Body.Replace("\r", "").Replace("\n", "").Contains(
@"<br />The response contains the following data for lines not found on this declaration. Were these lines deleted after sending the message to Quarantine?
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Ln #</th><th>Description</th><th>Allocated 
format</th><th>HC Number</th><th>Extra Cert</th></tr></thead><tr><td>3</td><td>WHEAT</td><td>E16,E17</td><td>132,134</td><td>X01,X02</td></tr></table>
<br />".Replace("\r", "").Replace("\n", "")));

			foreach (JobComInvoiceLine line in exDocHeader.InvoiceHeader.JobComInvoiceLines)
			{
				AssertEquals("WHEAT", line.QuarantineExDocLine.QL_HealthCertificateDescription);
				AssertEquals("E16,E17", line.QuarantineExDocLine.QL_HCFormatAllocated);
				AssertEquals("132,134", line.QuarantineExDocLine.QL_HCNumber);
				AssertEquals("X01,X02", line.QuarantineExDocLine.QL_ExtraCertificate);
			}
		}

		[ExpectNoExceptions]
		public void TestDeclarationWithoutInvoices_ThrowNoException()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			declaration.JE_OwnerRef = "B9999999";
			declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Factory.Save();

			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("FinalWithLongHCDescription.edi"));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			AssertNoExceptionThrown("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", () => messageProcessor.ProcessMessage(message));
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageProcessor = new EXDOCMessageProcessorForTesting(new LoggingInformation());
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;
		QuarantineExDocHeader exDocHeader;
		EXDOCMessageProcessorForTesting messageProcessor;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.EXDOC.TestFiles." + fileName;

		void SetupTestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			declaration.JE_DeclarationReference = "B9999999";
			declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			var invoiceHeader = declaration.Invoices.AddNew();
			exDocHeader = invoiceHeader.QuarantineExDocHeader;
			invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
		}

		sealed class EXDOCMessageProcessorForTesting : EXDOCMessageProcessor
		{
			public EXDOCMessageProcessorForTesting(LoggingInformation logger)
				: base(logger)
			{
			}

			public EmailDef ResponseEmail => responseEmail;
		}
	}
}
