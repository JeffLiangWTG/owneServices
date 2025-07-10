using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMessageDecoderTest : TestCaseWithFactory
	{
		public void TestProcessBGM()
		{
			Assert("Produce Type is empty", testEXDOCMessageDecoder.ProduceType.IsEmpty);
			Assert("Request For Permit Number is empty", testEXDOCMessageDecoder.RequestIdentificationNumber.IsEmpty);
			BGMSegment bGM = message.BGM.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.Dairy[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "435345", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Dairy, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "435345", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.Eggs[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "9343927", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Eggs, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "9343927", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.Fish[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "9343928", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Fish, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "9343928", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.GrainsAndPlants[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "3435890", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.GrainsAndPlants, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "3435890", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.Horticulture[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "3298749", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Horticulture, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "3298749", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.InedibleMeat[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "12390880", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.InedibleMeat, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "12390880", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.Meat[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "12390879", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Meat, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "12390879", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.SkinsAndHides[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "874353", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.SkinsAndHides, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "874353", testEXDOCMessageDecoder.RequestIdentificationNumber);
			EXDOCMessageUtilities.PopulateBGM(bGM, DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodes.Codes.Wool[0].ToString()), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), "9", "8743592", MessageFunctionCodedList.Reissue, ResponseTypeCodedList.GetFromString(ZString.Empty));
			testEXDOCMessageDecoder.Process();
			AssertEquals("Produce Type", EXDOCCommodityCodes.Codes.Wool, testEXDOCMessageDecoder.ProduceType);
			AssertEquals("Request For Permit Number", "8743592", testEXDOCMessageDecoder.RequestIdentificationNumber);
		}

		public void TestProcessDTM()
		{
			testEXDOCMessageDecoder.Process();
			Assert("Pack Date is empty", testEXDOCMessageDecoder.PackDate.IsEmpty);
			EXDOCMessageUtilities.PopulateDTM(message.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.PackagingDate, "20051109", DateTimePeriodFormatQualifierList.Ccyymmdd);
			testEXDOCMessageDecoder.Process();
			AssertEquals("Pack Date", new ZDateTime(2005, 11, 9), testEXDOCMessageDecoder.PackDate);
		}

		public void TestProcessSTS()
		{
			testEXDOCMessageDecoder.Process();
			Assert("Compliance Status is empty", testEXDOCMessageDecoder.ComplianceStatus.IsEmpty);
			Assert("Transfer Status is empty", testEXDOCMessageDecoder.TransferStatus.IsEmpty);
			Assert("Embargo Status is empty", testEXDOCMessageDecoder.EmbargoStatus.IsEmpty);
			PopulateSTS(message.STS.InstantiateAChildAndAddItToChildrenCollection(), StatusTypeCodedList.GetFromString("COM"), StatusReasonCodedList.GetFromString("COM"), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), EXDOCComplianceStatusCodes.Codes.Cancelled);
			PopulateSTS(message.STS.InstantiateAChildAndAddItToChildrenCollection(), StatusTypeCodedList.GetFromString("TRN"), StatusReasonCodedList.GetFromString(EXDOCComplianceStatusCodes.Codes.Initial), CodeListResponsibleAgencyCodedList.GetFromString("AQ"), string.Empty);
			PopulateSTS(message.STS.InstantiateAChildAndAddItToChildrenCollection(), StatusTypeCodedList.GetFromString("EMB"), StatusReasonCodedList.GetFromString("NO"), CodeListResponsibleAgencyCodedList.GetFromString("95"), string.Empty);
			testEXDOCMessageDecoder.Process();
			AssertEquals("Compliance Status", EXDOCComplianceStatusCodes.Codes.Cancelled, testEXDOCMessageDecoder.ComplianceStatus);
			AssertEquals("Transfer Status", EXDOCComplianceStatusCodes.Codes.Initial, testEXDOCMessageDecoder.TransferStatus);
			AssertEquals("Embargo Status", "NO", testEXDOCMessageDecoder.EmbargoStatus);
		}

		public void TestProcessLOC()
		{
			testEXDOCMessageDecoder.Process();
			Assert("Border Inspection Port is empty", testEXDOCMessageDecoder.BorderInspectionPort.IsEmpty);
			Assert("Loading Port is empty", testEXDOCMessageDecoder.LoadingPort.IsEmpty);
			Assert("Discharge Port is empty", testEXDOCMessageDecoder.DischargePort.IsEmpty);
			Assert("Final Destination is empty", testEXDOCMessageDecoder.FinalDestination.IsEmpty);
			Assert("Product Source Country is empty", testEXDOCMessageDecoder.ProductSourceCountry.IsEmpty);
			Assert("Transit Country is empty", testEXDOCMessageDecoder.TransitCountry.IsEmpty);
			Assert("Certificate Required Location is empty", testEXDOCMessageDecoder.CertificateRequiredLocation.IsEmpty);
			Assert("Aqis Region is empty", testEXDOCMessageDecoder.AqisRegion.IsEmpty);
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.PlaceOfCustomsExamination, "AUSYD");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.PlacePortOfLoading, "MEL");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.PortOfDischarge, "USLAX");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.PlaceOfDestination, "Saint-Renan");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.CountryOfUltimateDestination, "FR");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.CountryOfSource, "TH");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.PlaceOfDocumentIssue, "SYD");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.CountryOfTransit, "NZ");
			EXDOCMessageUtilities.PopulateLOC(message.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.RegionOfProduction, "BNE");
			testEXDOCMessageDecoder.Process();
			AssertEquals("Border Inspection Port", "AUSYD", testEXDOCMessageDecoder.BorderInspectionPort);
			AssertEquals("Loading Port", "AUMEL", testEXDOCMessageDecoder.LoadingPort);
			AssertEquals("Discharge Port", "USLAX", testEXDOCMessageDecoder.DischargePort);
			AssertEquals("Final Destination", "FRSRE", testEXDOCMessageDecoder.FinalDestination);
			AssertEquals("Product Source Country", "TH", testEXDOCMessageDecoder.ProductSourceCountry);
			AssertEquals("Certificate Required Location", "SYD", testEXDOCMessageDecoder.CertificateRequiredLocation);
			AssertEquals("Transit Country", "NZ", testEXDOCMessageDecoder.TransitCountry);
			AssertEquals("AQIS Region", "BNE", testEXDOCMessageDecoder.AqisRegion);
		}

		public void TestProcessRFF()
		{
			testEXDOCMessageDecoder.Process();
			Assert("Exporter Reference is empty", testEXDOCMessageDecoder.ExporterReference.IsEmpty);
			Assert("Export Permit Number is empty", testEXDOCMessageDecoder.ExportPermitNumber.IsEmpty);
			Assert("Customs Authority Number is empty", testEXDOCMessageDecoder.CustomsAuthorityNumber.IsEmpty);
			EXDOCMessageUtilities.PopulateRFF(message.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.DeclarantsReferenceNumber, "B00000001");
			EXDOCMessageUtilities.PopulateRFF(message.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.ExportPermitNumber, "A234");
			EXDOCMessageUtilities.PopulateRFF(message.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.GoodsDeclarationNumber, "AAE34567");
			testEXDOCMessageDecoder.Process();
			AssertEquals("Exporter Reference", "B00000001", testEXDOCMessageDecoder.ExporterReference);
			AssertEquals("Export Permit Number", "A234", testEXDOCMessageDecoder.ExportPermitNumber);
			AssertEquals("Customs Authority Number", "AAE34567", testEXDOCMessageDecoder.CustomsAuthorityNumber);
		}

		public void TestProcessFTX()
		{
			testEXDOCMessageDecoder.Process();
			Assert("Exporter Declaration is empty", testEXDOCMessageDecoder.ExporterDeclaration.IsEmpty);
			Assert("Inspector Comments is empty", testEXDOCMessageDecoder.InspectorComments.IsEmpty);
			Assert("Notify Party is empty", testEXDOCMessageDecoder.NotifyPartyText.IsEmpty);
			Assert("Letter Of Credit Text is empty", testEXDOCMessageDecoder.LetterOfCreditText.IsEmpty);
			Assert("Additional Information is empty", testEXDOCMessageDecoder.AdditionalInformation.IsEmpty);
			Assert("Amendment Reason is empty", testEXDOCMessageDecoder.AmendmentReason.IsEmpty);
			Assert("Origin Catching Zone is empty", testEXDOCMessageDecoder.OriginCatchingZone.IsEmpty);
			Assert("Lot Number is empty", testEXDOCMessageDecoder.LotNumber.IsEmpty);
			Assert("Average Animal Age is empty", testEXDOCMessageDecoder.AvAnimalAge.IsEmpty);
			Assert("Embargo Message is empty", testEXDOCMessageDecoder.EmbargoMessage.IsEmpty);
			AssertEquals("Notices is empty", 0, testEXDOCMessageDecoder.Notices.Count);
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.Declaration, 210, RequestForPermitHeaderMessageBuilder.MaxElementLength, "This is a declaration to say that Scott is the greatest sex machine to have ever lived");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.CertificationStatements, 210, RequestForPermitHeaderMessageBuilder.MaxElementLength, "Inspect this biatch");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.PartyInstructions, 27225, 55, "Notify ya mumma");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.LetterOfCreditInformation, 750, RequestForPermitHeaderMessageBuilder.MaxElementLength, "Credit, ha you get nothin, I saw you kick me cat");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.AdditionalInformation, 34650, RequestForPermitHeaderMessageBuilder.MaxElementLength, "No more bloody tests please, I feel Tunnel Carpal Syndrome coming on");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.ChangeInformation, RequestForPermitHeaderMessageBuilder.MaxAmendmentReasonChars, RequestForPermitHeaderMessageBuilder.MaxAmendmentReasonElementChars, "Amendment Reason Text");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.GetFromString(RequestForPermitFishHeaderMessageBuilder.OriginCatchingZoneSubjectQualifer), 105, 35, "I like Bing Lee, I like Bing Lee");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.AdditionalMarksNumbersInformation, 70, RequestForPermitHeaderMessageBuilder.MaxElementLength, "Lets start the bid for Lot Number 23421 at $340");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.CustomsClearanceInstructions, 70, RequestForPermitHeaderMessageBuilder.MaxElementLength, "Don't know what an example of an embargo message is.");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.ErrorDescriptionFreeText, 70, RequestForPermitHeaderMessageBuilder.MaxElementLength, "Errors lots of them to many to put here");
			EXDOCMessageUtilities.PopulateFTX(message, TextSubjectQualifierList.AdditionalAttributeInformation, RequestForPermitHeaderMessageBuilder.MaxAvAnimalAgeChars, RequestForPermitHeaderMessageBuilder.MaxAvAnimalAgeChars, "AVANIMALAGE");
			testEXDOCMessageDecoder.Process();
			AssertEquals("Exporter Declaration", "This is a declaration to say that Scott is the greatest sex machine to have ever lived", testEXDOCMessageDecoder.ExporterDeclaration);
			AssertEquals("Inspector Comments", "Inspect this biatch", testEXDOCMessageDecoder.InspectorComments);
			AssertEquals("Notify Party Text", "Notify ya mumma", testEXDOCMessageDecoder.NotifyPartyText);
			AssertEquals("Letter of Credit Text", "Credit, ha you get nothin, I saw you kick me cat", testEXDOCMessageDecoder.LetterOfCreditText);
			AssertEquals("Additional Information", "No more bloody tests please, I feel Tunnel Carpal Syndrome coming on", testEXDOCMessageDecoder.AdditionalInformation);
			AssertEquals("Amendment Reason", "Amendment Reason Text", testEXDOCMessageDecoder.AmendmentReason);
			AssertEquals("Origin Catch Zone", "I like Bing Lee, I like Bing Lee", testEXDOCMessageDecoder.OriginCatchingZone);
			AssertEquals("Lot Number", "Lets start the bid for Lot Number 23421 at $340", testEXDOCMessageDecoder.LotNumber);
			AssertEquals("Average Animal Age", "AVANIMALAGE", testEXDOCMessageDecoder.AvAnimalAge);
			AssertEquals("Embargo Message", "Don't know what an example of an embargo message is.", testEXDOCMessageDecoder.EmbargoMessage);
			AssertEquals("Notices", 1, testEXDOCMessageDecoder.Notices.Count);
		}

		public void TestProcessMEA()
		{
			testEXDOCMessageDecoder.Process();
			Assert("Temperature Unit is empty", testEXDOCMessageDecoder.TemperatureUnit.IsEmpty);
			Assert("Absolute Temperature is empty", testEXDOCMessageDecoder.AbsoluteTemperature.IsEmpty);
			Assert("Minimum Temperature is empty", testEXDOCMessageDecoder.MinimumTemperature.IsEmpty);
			Assert("Maximum Temperature is empty", testEXDOCMessageDecoder.MaximumTemperature.IsEmpty);
			EXDOCMessageUtilities.PopulateMEA(message.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.Temperature, PropertyMeasuredCodedList.X_ShippingTolerance, EXDOCTemperatureUnitCodes.Codes.Celsius, "23.34", "45.56", "54.34");
			testEXDOCMessageDecoder.Process();
			AssertEquals("Termperature Unit", EXDOCTemperatureUnitCodes.Codes.Celsius, testEXDOCMessageDecoder.TemperatureUnit);
			AssertEquals("Absolute Temperature", 23.34m, testEXDOCMessageDecoder.AbsoluteTemperature);
			AssertEquals("Minimum Temperature", 45.56m, testEXDOCMessageDecoder.MinimumTemperature);
			AssertEquals("Maximum Temperature", 54.34m, testEXDOCMessageDecoder.MaximumTemperature);
		}

		public void TestProcessMOA()
		{
			testEXDOCMessageDecoder.Process();
			Assert("FOB Currency Unit is empty", testEXDOCMessageDecoder.FobCurrencyUnit.IsEmpty);
			EXDOCMessageUtilities.PopulateMOA(message.MOA.InstantiateAChildAndAddItToChildrenCollection(), MonetaryAmountTypeQualifierList.FobValue, "USD");
			testEXDOCMessageDecoder.Process();
			AssertEquals("FOB Currency Unit", "USD", Factory.Load<RefCurrency>(testEXDOCMessageDecoder.FobCurrencyUnit).RX_Code);
		}

		public void TestProcessGIS()
		{
			testEXDOCMessageDecoder.Process();
			Assert("Certificate Print Indicator is empty", testEXDOCMessageDecoder.CertificatePrintIndicator.IsEmpty);
			Assert("Separate Certificate Container Indicator is false", !testEXDOCMessageDecoder.SeparateCertificateContainerIndicator);
			Assert("Separate Certificate Marks Indicator is false", !testEXDOCMessageDecoder.SeparateCertificateMarksIndicator);
			Assert("Separate Certificate Packer Indicator is false", !testEXDOCMessageDecoder.SeparateCertificatePackerIndicator);
			Assert("Ships Stores Indicator is false", !testEXDOCMessageDecoder.ShipsStoresIndicator);
			Assert("Forward Status is empty", testEXDOCMessageDecoder.ForwardStatus.IsEmpty);
			Assert("AMLC Quota Indicator is false", !testEXDOCMessageDecoder.AMLCQuotaIndicator);
			Assert("Customs Agent Indicator is false", !testEXDOCMessageDecoder.CustomsAgentIndicator);
			AssertEquals("Declaration Estimate is empty", JobDeclaration.MessageSubType.NonConfirming, testEXDOCMessageDecoder.DeclarationEstimate);

			EXDOCMessageUtilities.PopulateGIS(message, EXDOCCertificatePrintCodes.Codes.CustomCertificate, RequestForPermitHeaderMessageBuilder.CertificatePrintIndicatorQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, "Y", RequestForPermitHeaderMessageBuilder.SeperateCertificateContainerIndicatorQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, "Y", RequestForPermitHeaderMessageBuilder.SeperateCertificateMarksIndicatorQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, "Y", RequestForPermitHeaderMessageBuilder.SeperateCertificatePackerIndicatorQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, "Y", RequestForPermitMeatHeaderMessageBuilder.ShipStoresIndicatorQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, EXDOCComplianceStatusCodes.Codes.Initial, RequestForPermitHeaderMessageBuilder.RFPForwardStatusQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, "Y", RequestForPermitHeaderMessageBuilder.AMLCQuotaIndicatorQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, "Y", RequestForPermitHeaderMessageBuilder.CustomsAgentIndicatorQualifier);
			EXDOCMessageUtilities.PopulateGIS(message, "M", RequestForPermitHeaderMessageBuilder.DeclarationEstimateIndicatorQualifier);

			testEXDOCMessageDecoder.Process();
			AssertEquals("Certificate Print Indicator", EXDOCCertificatePrintCodes.Codes.CustomCertificate, testEXDOCMessageDecoder.CertificatePrintIndicator);
			Assert("Separate Certificate Container Indicator", testEXDOCMessageDecoder.SeparateCertificateContainerIndicator);
			Assert("Separate Certificate Marks Indicator", testEXDOCMessageDecoder.SeparateCertificateMarksIndicator);
			Assert("Separate Certificate Packer Indicator", testEXDOCMessageDecoder.SeparateCertificatePackerIndicator);
			Assert("Ships Stores Indicator", testEXDOCMessageDecoder.ShipsStoresIndicator);
			AssertEquals("Forward Status", EXDOCComplianceStatusCodes.Codes.Initial, testEXDOCMessageDecoder.ForwardStatus);
			Assert("AMLC Quota Indicator", testEXDOCMessageDecoder.AMLCQuotaIndicator);
			Assert("Customs Agent Indicator", testEXDOCMessageDecoder.CustomsAgentIndicator);
			AssertEquals("Declaration Estimate", JobDeclaration.MessageSubType.Manual, testEXDOCMessageDecoder.DeclarationEstimate);
		}

		public void TestProcessPNA()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			OrgCusCode exportNum = supplier.CustomsCodes.AddNew();
			exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
			exportNum.OK_RN_NKCodeCountry = "AU";
			exportNum.OK_OH = supplier.PK;
			exportNum.OK_CustomsRegNo = "99999";
			Factory.Save();

			testEXDOCMessageDecoder.Process();
			Assert("Owner Exporter Number is empty", testEXDOCMessageDecoder.OwnerExporterNumber.IsEmpty);
			Assert("Forwardee EDI User Identifier is empty", testEXDOCMessageDecoder.ForwardeeEDIUserIdentifier.IsEmpty);
			Assert("Consignee Reference Number is empty", testEXDOCMessageDecoder.ConsigneeReferenceNumber.IsEmpty);
			Assert("Supplier is empty", testEXDOCMessageDecoder.Supplier.IsEmpty);
			Assert("Transferee EDI User Identifier is empty", testEXDOCMessageDecoder.TransfereeEDIUserIdentifier.IsEmpty);
			Assert("Importer is not To Order organisation", !testEXDOCMessageDecoder.IsToOrderImporter);

			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.Exporter, "99999");
			EXDOCMessageUtilities.PopulateCTA(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(), ContactFunctionCodedList.Agent, "88888");
			group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.Consignee, "77777", NameComponentQualifierList.WholeName, "TEST CRAP");
			group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.TransferTo, "666666");
			EXDOCMessageUtilities.PopulateCTA(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(), ContactFunctionCodedList.Agent, "55555");

			testEXDOCMessageDecoder.Process();
			AssertEquals("Owner Exporter Number", "99999", testEXDOCMessageDecoder.OwnerExporterNumber);
			AssertEquals("Forwardee EDI User Identifier", "88888", testEXDOCMessageDecoder.ForwardeeEDIUserIdentifier);
			AssertEquals("Consignee Reference Number", "77777", testEXDOCMessageDecoder.ConsigneeReferenceNumber);
			AssertEquals("Transferee EDI User Identifier", "55555", testEXDOCMessageDecoder.TransfereeEDIUserIdentifier);
			AssertEquals("Supplier is set", supplier.PK, testEXDOCMessageDecoder.Supplier);

			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.Consignee, "77777", NameComponentQualifierList.WholeName, "TO ORDER");
			testEXDOCMessageDecoder.Process();
			Assert("Importer is To Order organisation", testEXDOCMessageDecoder.IsToOrderImporter);
		}

		public void TestProcessGroup1()
		{
			testEXDOCMessageDecoder.Process();
			AssertEquals("Permits collection is empty", 0, testEXDOCMessageDecoder.Permits.Count);
			SegmentGroup1 firstGroup1 = message.Group1.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateDOC(firstGroup1.DOC.InstantiateAChildAndAddItToChildrenCollection(), DocumentMessageNameCodedList.ImportLicence, "TESTIMP1");
			EXDOCMessageUtilities.PopulateDTM(firstGroup1.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.DocumentMessageDateTime, "20061213", DateTimePeriodFormatQualifierList.Ccyymmdd);
			SegmentGroup1 secondGroup1 = message.Group1.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateDOC(secondGroup1.DOC.InstantiateAChildAndAddItToChildrenCollection(), DocumentMessageNameCodedList.ImportLicence, "TESTIMP2");
			EXDOCMessageUtilities.PopulateDTM(secondGroup1.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.DocumentMessageDateTime, "20061214", DateTimePeriodFormatQualifierList.Ccyymmdd);
			testEXDOCMessageDecoder.Process();
			AssertEquals("Permits collection contains 2", 2, testEXDOCMessageDecoder.Permits.Count);
			AssertEquals("First Permit Number", "TESTIMP1", testEXDOCMessageDecoder.Permits[0].importLicenseNumber);
			AssertEquals("First Permit Date", new ZDateTime(2006, 12, 13), testEXDOCMessageDecoder.Permits[0].importLicenseDate);
			AssertEquals("Second Permit Number", "TESTIMP2", testEXDOCMessageDecoder.Permits[1].importLicenseNumber);
			AssertEquals("Second Permit Date", new ZDateTime(2006, 12, 14), testEXDOCMessageDecoder.Permits[1].importLicenseDate);
		}

		public void TestImporterMatchingToCorrectOrgheader()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			//OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgMatchThresholds.Codes.Extreme);
			OrgHeader testConsignee = Factory.New<OrgHeader>();
			testConsignee.OH_Code = "ORLEXPORL";
			testConsignee.OH_FullName = "PT SURABAYA NOOR LEATHER";
			testConsignee.MainAddress.OA_Address1 = "JALAN RUNGKUT";
			testConsignee.MainAddress.OA_Address2 = "INDUSTRI IV NO.16";
			testConsignee.MainAddress.OA_City = "SURABAYA";
			testConsignee.MainAddress.OA_PostCode = "60293";
			testConsignee.MainAddress.OA_RL_NKRelatedPortCode = "IDSUB";
			Factory.Save();

			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.Consignee, "CCC6377669P", NameComponentQualifierList.WholeName, "PT SURABAYA NOOR LEATHER");
			EXDOCMessageUtilities.PopulateADR(group2.ADR.InstantiateAChildAndAddItToChildrenCollection(), AddressFormatCodedList.UnstructuredAddress, "JALAN RUNGKUT INDUSTRI IV NO.16", ZString.Empty, "SURABAYA", "60293     ", "ID", "ZString.Empty");
			testEXDOCMessageDecoder.Process();
			AssertEquals("Importer PK matches the TestConsignee's", testConsignee.PK, testEXDOCMessageDecoder.Importer);
		}

		public void TestImporterMatchingToTemporaryOrgheader()
		{
			AssertNull(Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "ORLANDO EXPORTS")));
			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.Consignee, ZString.Empty, NameComponentQualifierList.WholeName, "ORLANDO EXPORTS");
			EXDOCMessageUtilities.PopulateADR(group2.ADR.InstantiateAChildAndAddItToChildrenCollection(), AddressFormatCodedList.UnstructuredAddress, "45 EAST AVE", ZString.Empty, "ORLANDO", "76554", "US", "FLORIDA");
			testEXDOCMessageDecoder.Process();
			Assert("Importer PK is not empty (new temporary record)", !testEXDOCMessageDecoder.Importer.IsEmpty);
			OrgHeader temporaryOrganisation = Factory.Load<OrgHeader>(testEXDOCMessageDecoder.Importer);
			AssertNotNull("Temporary Organisation not null", temporaryOrganisation);
			Assert("Temporary Organisation is Temporary", temporaryOrganisation.OH_IsTempAccount);
			AssertEquals("Temporary Organisation Name", "ORLANDO EXPORTS", temporaryOrganisation.OH_FullName);
			AssertEquals("Temporary Organisation Code", "ORLEXP", temporaryOrganisation.OH_Code);
		}

		public void TestImporterMatchingToUnmatchedOrgheader()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.Consignee, ZString.Empty, NameComponentQualifierList.WholeName, "ORLANDO EXPORTS");
			EXDOCMessageUtilities.PopulateADR(group2.ADR.InstantiateAChildAndAddItToChildrenCollection(), AddressFormatCodedList.UnstructuredAddress, "45 EAST AVE", ZString.Empty, "ORLANDO", "76554", "US", "FLORIDA");
			testEXDOCMessageDecoder.Process();
			Assert("Importer PK is not empty (new temporary record)", !testEXDOCMessageDecoder.Importer.IsEmpty);
			OrgHeader temporaryOrganisation = Factory.Load<OrgHeader>(testEXDOCMessageDecoder.Importer);
			AssertNotNull("Temporary Organisation not null", temporaryOrganisation);
			AssertEquals("Temporary Organisation Code", "UNMATCHED", temporaryOrganisation.OH_Code);
			AssertMultilineASCIIEquals("Note Information for UNMATCHED", UnmatchedTestText, testEXDOCMessageDecoder.UnmatchedNoteInformation);
		}

		public const string UnmatchedTestText = "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Consignee</OrganisationType><OrganisationSubType>Consignee</OrganisationSubType><OwnerCode /><EDICode /><OrganisationName>ORLANDO EXPORTS (Threshold=MED)</OrganisationName><AddressLine1>45 EAST AVE</AddressLine1><AddressLine2 /><City>ORLANDO</City><PostCode>76554</PostCode><StateOrProvince>FLORIDA</StateOrProvince><Country>US</Country><DocAddressType /></UnmatchOrgRecord></UnmatchOrgRecords>";

		public void TestForwarderMatching()
		{
			var orgCarrier = Factory.New<OrgHeader>();
			orgCarrier.OH_Code = "ORGCTST";
			orgCarrier.OH_FullName = "TEST CARRIER";
			orgCarrier.OH_IsShippingProvider = true;
			orgCarrier.OH_IsDebtor = true;
			var orgForwarder = Factory.New<OrgHeader>();
			orgForwarder.OH_Code = "ORGFTST";
			orgForwarder.OH_FullName = "TEST FORWARDER";
			orgForwarder.OH_IsForwarder = true;
			orgForwarder.OH_IsDebtor = true;
			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.ConsigneesAgent, ZString.Empty, NameComponentQualifierList.WholeName, "TEST FORWARDER");
			testEXDOCMessageDecoder.Process();
			AssertEquals("Forwarder PK is correct", orgForwarder.PK, testEXDOCMessageDecoder.Forwarder);
		}

		public void TestForwarderMatchingToTemporaryOrganisation()
		{
			AssertNull(Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "TEST FORWARDER")));
			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.ConsigneesAgent, ZString.Empty, NameComponentQualifierList.WholeName, "TEST FORWARDER");
			testEXDOCMessageDecoder.Process();
			Assert("Forwarder PK is not empty (new temporary record)", !testEXDOCMessageDecoder.Forwarder.IsEmpty);
			OrgHeader temporaryOrganisation = Factory.Load<OrgHeader>(testEXDOCMessageDecoder.Forwarder);
			AssertNotNull("Temporary Organisation not null", temporaryOrganisation);
			Assert("Temporary Organisation is Temporary", temporaryOrganisation.OH_IsTempAccount);
			AssertEquals("Temporary Organisation Name", "TEST FORWARDER", temporaryOrganisation.OH_FullName);
			AssertEquals("Temporary Organisation Code", "TESFOR", temporaryOrganisation.OH_Code);
		}

		public void TestForwarderMatchingToUnmatched()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.ConsigneesAgent, ZString.Empty, NameComponentQualifierList.WholeName, "TEST FORWARDER");
			testEXDOCMessageDecoder.Process();
			Assert("Forwarder PK is not empty (new temporary record)", !testEXDOCMessageDecoder.Forwarder.IsEmpty);
			OrgHeader temporaryOrganisation = Factory.Load<OrgHeader>(testEXDOCMessageDecoder.Forwarder);
			AssertNotNull("Temporary Organisation not null", temporaryOrganisation);
			AssertEquals("Temporary Organisation Code", "UNMATCHED", temporaryOrganisation.OH_Code);
			AssertMultilineASCIIEquals("Note Information for UNMATCHED", UnmatchedForwarderTestText, testEXDOCMessageDecoder.UnmatchedNoteInformation);
		}

		const string UnmatchedForwarderTestText = "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Forwarder</OrganisationType><OrganisationSubType>Forwarder</OrganisationSubType><OwnerCode /><EDICode /><OrganisationName>TEST FORWARDER</OrganisationName><AddressLine1 /><AddressLine2 /><City /><PostCode /><StateOrProvince /><Country /><DocAddressType /></UnmatchOrgRecord></UnmatchOrgRecords>";

		public void TestCarrierMatching()
		{
			var orgCarrier = Factory.New<OrgHeader>();
			orgCarrier.OH_Code = "ORGCTST";
			orgCarrier.OH_FullName = "TEST CARRIER";
			orgCarrier.OH_IsShippingProvider = true;
			orgCarrier.OH_IsDebtor = true;
			var orgForwarder = Factory.New<OrgHeader>();
			orgForwarder.OH_Code = "ORGFTST";
			orgForwarder.OH_FullName = "TEST FORWARDER";
			orgForwarder.OH_IsForwarder = true;
			orgForwarder.OH_IsDebtor = true;
			SegmentGroup4 group4 = message.Group4.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateTDT(group4.TDT.InstantiateAChildAndAddItToChildrenCollection(), TransportStageQualifierList.AtDeparture, ZString.Empty, ZString.Empty, "TEST CARRIER", ZString.Empty);
			testEXDOCMessageDecoder.Process();
			AssertEquals("Carrier PK is correct", orgCarrier.PK, testEXDOCMessageDecoder.Carrier);
		}

		public void TestCarrierMatchingToTemporaryOrganisation()
		{
			AssertNull(Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "TEST CARRIER")));
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = false;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			SegmentGroup4 group4 = message.Group4.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateTDT(group4.TDT.InstantiateAChildAndAddItToChildrenCollection(), TransportStageQualifierList.AtDeparture, ZString.Empty, ZString.Empty, "TEST CARRIER", ZString.Empty);
			testEXDOCMessageDecoder.Process();
			Assert("Carrier PK is not empty (new temporary record)", !testEXDOCMessageDecoder.Carrier.IsEmpty);
			OrgHeader temporaryOrganisation = Factory.Load<OrgHeader>(testEXDOCMessageDecoder.Carrier);
			AssertNotNull("Temporary Organisation not null", temporaryOrganisation);
			Assert("Temporary Organisation is Temporary", temporaryOrganisation.OH_IsTempAccount);
			AssertEquals("Temporary Organisation Name", "TEST CARRIER", temporaryOrganisation.OH_FullName);
			AssertEquals("Temporary Organisation Code", "TESCAR", temporaryOrganisation.OH_Code);
		}

		public void TestCarrierMatchingToUnmatched()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			SegmentGroup4 group4 = message.Group4.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateTDT(group4.TDT.InstantiateAChildAndAddItToChildrenCollection(), TransportStageQualifierList.AtDeparture, ZString.Empty, ZString.Empty, "TEST CARRIER", ZString.Empty);
			testEXDOCMessageDecoder.Process();
			Assert("Carrier PK is not empty (new temporary record)", !testEXDOCMessageDecoder.Carrier.IsEmpty);
			OrgHeader temporaryOrganisation = Factory.Load<OrgHeader>(testEXDOCMessageDecoder.Carrier);
			AssertNotNull("Temporary Organisation not null", temporaryOrganisation);
			AssertEquals("Temporary Organisation Code", "UNMATCHED", temporaryOrganisation.OH_Code);
			AssertMultilineASCIIEquals("Note Information for UNMATCHED", UnmatchedCarrierTestText, testEXDOCMessageDecoder.UnmatchedNoteInformation);
		}

		const string UnmatchedCarrierTestText = "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Carrier</OrganisationType><OrganisationSubType>Carrier</OrganisationSubType><OwnerCode /><EDICode /><OrganisationName>TEST CARRIER</OrganisationName><AddressLine1 /><AddressLine2 /><City /><PostCode /><StateOrProvince /><Country /><DocAddressType /></UnmatchOrgRecord></UnmatchOrgRecords>";

		protected override void SetUp()
		{
			base.SetUp();
			message = new SANCRTMessage();
			testEXDOCMessageDecoder = new EXDOCMessageDecoder(message, Factory);
		}

		void PopulateSTS(STSSegment sTS, StatusTypeCodedList statusType, StatusReasonCodedList statusReason, CodeListResponsibleAgencyCodedList codeListResponsibleAgency, string statusReasonText)
		{
			sTS.StatusType.StatusTypeCoded = statusType;
			sTS.StatusType.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.GetFromString("AQ");
			sTS.StatusReason1.StatusReasonCoded = statusReason;
			sTS.StatusReason1.CodeListResponsibleAgencyCoded = codeListResponsibleAgency;
			sTS.StatusReason1.StatusReason = statusReasonText;
		}

		EXDOCMessageDecoder testEXDOCMessageDecoder;
		SANCRTMessage message;
	}
}
