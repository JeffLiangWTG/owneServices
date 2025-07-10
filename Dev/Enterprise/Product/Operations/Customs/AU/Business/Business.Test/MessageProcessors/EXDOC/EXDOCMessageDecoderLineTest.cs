using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMessageDecoderLineTest : TestCaseWithFactory
	{
		public void TestProcessMEA()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Net Quantity is empty", testEXDOCMessageDecoderLine.NetQuantity.IsEmpty);
			Assert("Net Quantity Unit is empty", testEXDOCMessageDecoderLine.NetQuantityUnit.IsEmpty);
			Assert("Imperial Net Weight is empty", testEXDOCMessageDecoderLine.ImperialNetWeight.IsEmpty);
			Assert("Imperial Net Weight Unit is empty", testEXDOCMessageDecoderLine.ImperialNetWeightUnit.IsEmpty);
			Assert("Metric Gross Weight is empty", testEXDOCMessageDecoderLine.MetricGrossWeight.IsEmpty);
			Assert("Metric Gross Weight Unit is empty", testEXDOCMessageDecoderLine.MetricGrossWeightUnit.IsEmpty);
			Assert("Drained Weight is empty", testEXDOCMessageDecoderLine.DrainedWeight.IsEmpty);
			Assert("Drained Weight Unit is empty", testEXDOCMessageDecoderLine.DrainedWeightUnit.IsEmpty);
			Assert("Percentage Of Milk Protein is empty", testEXDOCMessageDecoderLine.PercentageOfMilkProtein.IsEmpty);
			Assert("Percentage Of Milk Fat is empty", testEXDOCMessageDecoderLine.PercentageOfMilkFat.IsEmpty);
			Assert("Total Weight Of Milk Protein In Mixtures is empty", testEXDOCMessageDecoderLine.TotalWeightOfMilkProteinInMixtures.IsEmpty);
			Assert("Total Weight Of Milk Fat In Mixtures is empty", testEXDOCMessageDecoderLine.TotalWeightOfMilkFatInMixtures.IsEmpty);
			Assert("Beef Veal Weight is empty", testEXDOCMessageDecoderLine.BeefVealWeight.IsEmpty);
			Assert("Checimal Lean Percentage", testEXDOCMessageDecoderLine.ChecimalLeanPercentage.IsEmpty);
			Assert("Customs weight is empty", testEXDOCMessageDecoderLine.CustomsWeight.IsEmpty);
			Assert("Customs weight Q is empty", testEXDOCMessageDecoderLine.CustomsWeightUQ.IsEmpty);
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.LineItemMeasurement, PropertyMeasuredCodedList.ShippedQuantity, EXDOCMetricWeightUnitCodes.Codes.Kilogram, "1456.00");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.ItemWeight, PropertyMeasuredCodedList.NetWeight, EXDOCImperialWeightUnitCodes.Codes.TonUkOrLongtonUs, "100");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.ItemWeight, PropertyMeasuredCodedList.ItemGrossWeight, EXDOCMetricWeightUnitCodes.Codes.MetricTonne, "120");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.ItemWeight, PropertyMeasuredCodedList.NetNetWeight, EXDOCMetricWeightUnitCodes.Codes.Kilogram, "34.5");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkProtein), RequestForPermitLineMessageBuilder.Percentage, "5");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkFat), RequestForPermitLineMessageBuilder.Percentage, "2");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkProtein), PropertyMeasuredCodedList.NetWeight, EXDOCMetricWeightUnitCodes.Codes.Kilogram, "4000");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkFat), PropertyMeasuredCodedList.NetWeight, EXDOCMetricWeightUnitCodes.Codes.Kilogram, "1600");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.ItemWeight, PropertyMeasuredCodedList.ChargeableWeight, EXDOCMetricWeightUnitCodes.Codes.Kilogram, "12.34");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.Chemistry, RequestForPermitLineMessageBuilder.Percentage, "23");
			EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.CustomsLineItemMeasurement, PropertyMeasuredCodedList.ShippedQuantity, "KGM", "69.70");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Net Quantity", 1456.00m, testEXDOCMessageDecoderLine.NetQuantity);
			AssertEquals("Net Quantity Unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, testEXDOCMessageDecoderLine.NetQuantityUnit);
			AssertEquals("Imperial Net Weight", 100m, testEXDOCMessageDecoderLine.ImperialNetWeight);
			AssertEquals("Imperial Net Weight Unit", EXDOCImperialWeightUnitCodes.Codes.TonUkOrLongtonUs, testEXDOCMessageDecoderLine.ImperialNetWeightUnit);
			AssertEquals("Metric Gross Weight", 120m, testEXDOCMessageDecoderLine.MetricGrossWeight);
			AssertEquals("Metric Gross Weight Unit", EXDOCMetricWeightUnitCodes.Codes.MetricTonne, testEXDOCMessageDecoderLine.MetricGrossWeightUnit);
			AssertEquals("Drained Weight", 34.5m, testEXDOCMessageDecoderLine.DrainedWeight);
			AssertEquals("Drained Weight Unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, testEXDOCMessageDecoderLine.DrainedWeightUnit);
			AssertEquals("Percentage Of Milk Protein", 5m, testEXDOCMessageDecoderLine.PercentageOfMilkProtein);
			AssertEquals("Percentage Of Milk Fat", 2m, testEXDOCMessageDecoderLine.PercentageOfMilkFat);
			AssertEquals("Total Weight Of Milk Protein In Mixtures", 4000m, testEXDOCMessageDecoderLine.TotalWeightOfMilkProteinInMixtures);
			AssertEquals("Total Weight Of Milk Fat In Mixtures", 1600m, testEXDOCMessageDecoderLine.TotalWeightOfMilkFatInMixtures);
			AssertEquals("Beef Veal Weight", 12.34m, testEXDOCMessageDecoderLine.BeefVealWeight);
			AssertEquals("Checimal Lean Percentage", 23, testEXDOCMessageDecoderLine.ChecimalLeanPercentage);
			AssertEquals("Customs weight", 69.70m, testEXDOCMessageDecoderLine.CustomsWeight);
			AssertEquals("Customs weight UQ", "KGM", testEXDOCMessageDecoderLine.CustomsWeightUQ);
		}

		public void TestProcessPIA()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Preservation Type is empty", testEXDOCMessageDecoderLine.PreservationType.IsEmpty);
			Assert("Product Type is empty", testEXDOCMessageDecoderLine.ProductType.IsEmpty);
			Assert("Pack Type is empty", testEXDOCMessageDecoderLine.PackType.IsEmpty);
			Assert("Supplimentary Code is empty", testEXDOCMessageDecoderLine.SupplimentaryCode.IsEmpty);
			Assert("Cut Code is empty", testEXDOCMessageDecoderLine.CutCode.IsEmpty);
			Assert("EAN 13 Number is empty", testEXDOCMessageDecoderLine.EAN13Number.IsEmpty);
			Assert("Ahecc Code is empty", testEXDOCMessageDecoderLine.AheccCode.IsEmpty);
			PIASegment productCodePIA = group11.PIA.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePIA(productCodePIA, ProductIdFunctionQualifierList.ProductIdentification, "UCASBG", ItemNumberTypeCodedList.IndustryCommodityCode);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Preservation Type", "U", testEXDOCMessageDecoderLine.PreservationType);
			AssertEquals("Product Type", "CAS", testEXDOCMessageDecoderLine.ProductType);
			AssertEquals("Pack Type", "BG", testEXDOCMessageDecoderLine.PackType);
			AssertEquals("Supplimentary Code", ZString.Empty, testEXDOCMessageDecoderLine.SupplimentaryCode);
			EXDOCMessageUtilities.PopulatePIA(productCodePIA, ProductIdFunctionQualifierList.ProductIdentification, "XWHTBGGC", ItemNumberTypeCodedList.IndustryCommodityCode);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Preservation Type", ZString.Empty, testEXDOCMessageDecoderLine.PreservationType);
			AssertEquals("Product Type", "WHT", testEXDOCMessageDecoderLine.ProductType);
			AssertEquals("Pack Type", "BG", testEXDOCMessageDecoderLine.PackType);
			AssertEquals("Supplimentary Code", "GC", testEXDOCMessageDecoderLine.SupplimentaryCode);
			EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(), ProductIdFunctionQualifierList.ProductIdentification, "W00400", ItemNumberTypeCodedList.BuyersPartNumber);
			EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(), ProductIdFunctionQualifierList.AdditionalIdentification, "EAN1234567890", ItemNumberTypeCodedList.IndustryCommodityCode);
			EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(), ProductIdFunctionQualifierList.ProductIdentification, "35011020", ItemNumberTypeCodedList.HarmonisedSystem);
			EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(), ProductIdFunctionQualifierList.ProductIdentification, "DPCODE", ItemNumberTypeCodedList.CommodityGrouping);
			EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(), ProductIdFunctionQualifierList.ProductIdentification, "APCODES", ItemNumberTypeCodedList.StandardGroupOfProductsMixedAssortment);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Cut Code", "W00400", testEXDOCMessageDecoderLine.CutCode);
			AssertEquals("EAN 13 Number", "EAN1234567890", testEXDOCMessageDecoderLine.EAN13Number);
			AssertEquals("AHECC Code", "35011020", testEXDOCMessageDecoderLine.AheccCode);
			AssertEquals("Dominant Product", "DPCODE", testEXDOCMessageDecoderLine.DominantProduct);
			AssertEquals("Additional Products", "APCODES", testEXDOCMessageDecoderLine.AdditionalProducts);
		}

		public void TestProcessIMD()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Line Item Description is empty", testEXDOCMessageDecoderLine.LineItemDescription.IsEmpty);
			Assert("Exporter Defined Product Description is empty", testEXDOCMessageDecoderLine.ExporterDefinedProductDescription.IsEmpty);
			Assert("Generated Product Description is empty", testEXDOCMessageDecoderLine.GeneratedProductDescription.IsEmpty);
			Assert("Additional Product Description is empty", testEXDOCMessageDecoderLine.AdditionalProductDescription.IsEmpty);
			Assert("EAN Description is empty", testEXDOCMessageDecoderLine.EANDescription.IsEmpty);
			Assert("Commercial Product Description is empty", testEXDOCMessageDecoderLine.CommercialProductDescription.IsEmpty);
			Assert("Product Quality Qualification is empty", testEXDOCMessageDecoderLine.ProductQualityQualification.IsEmpty);
			Assert("Product Location Qualification is empty", testEXDOCMessageDecoderLine.ProductLocationQualification.IsEmpty);
			Assert("Nature of Commodity", testEXDOCMessageDecoderLine.NatureOfCommodity.IsEmpty);
			Assert("Treatment Type", testEXDOCMessageDecoderLine.TreatmentType.IsEmpty);
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitMeatLineMessageBuilder.Inspection, "Geoff says that straight pubic hair is weird");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitLineMessageBuilder.UserDefinedCertificate, "DESCRIPTIVE CRAP ABOUT GEOFF'S BIG WILLY");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), "GHC1", "FROZEN FRESH MILK (<1%FAT) CARTONS 104 X 20 KGM BAGS");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitLineMessageBuilder.Additional, "WOW I WISH I HAD SOMETHING RELEVANT TO TYPE BUT I AM SO OVER WRITING FRIGGING TESTS THAT I AM GOING TO POKE AN EYE OUT");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), "ED", "THE COWS ARE ALL DEAD SO BUGGER OFF");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitGrainsAndPlantsLineMessageBuilder.Commercial, "No more free text I am so out of crap to say it isn't funny, got nothin");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitLineMessageBuilder.QualityQualifier, "NOW I AM Just Copying text");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitLineMessageBuilder.LocationQualifier, EXDOCLocationQualifier.Codes.Australian);
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitLineMessageBuilder.NatureOfCommodity, "CT");
			EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitLineMessageBuilder.TreatmentType, "BO");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Line Item Description", "Geoff says that straight pubic hair is weird", testEXDOCMessageDecoderLine.LineItemDescription);
			AssertEquals("Exporter Defined Description", "DESCRIPTIVE CRAP ABOUT GEOFF'S BIG WILLY", testEXDOCMessageDecoderLine.ExporterDefinedProductDescription);
			AssertEquals("Generated Product Description", "FROZEN FRESH MILK (<1%FAT) CARTONS 104 X 20 KGM BAGS", testEXDOCMessageDecoderLine.GeneratedProductDescription);
			AssertEquals("Additional Product Description", "WOW I WISH I HAD SOMETHING RELEVANT TO TYPE BUT I AM SO OVER WRITING FRIGGING TESTS THAT I AM GOING TO POKE AN EYE OUT", testEXDOCMessageDecoderLine.AdditionalProductDescription);
			AssertEquals("EAN Description", "THE COWS ARE ALL DEAD SO BUGGER OFF", testEXDOCMessageDecoderLine.EANDescription);
			AssertEquals("Commercial Product Description", "No more free text I am so out of crap to say it isn't funny, got nothin", testEXDOCMessageDecoderLine.CommercialProductDescription);
			AssertEquals("Product Quality Qualification", "NOW I AM Just Copying text", testEXDOCMessageDecoderLine.ProductQualityQualification);
			AssertEquals("Product Location Qualification", EXDOCLocationQualifier.Codes.Australian, testEXDOCMessageDecoderLine.ProductLocationQualification);
			AssertEquals("Nature of Commodity", "CT", testEXDOCMessageDecoderLine.NatureOfCommodity);
			AssertEquals("Treatment Type", "BO", testEXDOCMessageDecoderLine.TreatmentType);
		}

		public void TestProcessRFF()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Label Approval Number is empty", testEXDOCMessageDecoderLine.LabelApprovalNumber.IsEmpty);
			Assert("Duty Draw back is empty", !testEXDOCMessageDecoderLine.DutyDrawback);
			Assert("Motor Vehicle Plan is empty", !testEXDOCMessageDecoderLine.MotorVehiclePlan);
			Assert("Texaco is empty", !testEXDOCMessageDecoderLine.Texaco);
			Assert("AMLC Quota Approval is empty", testEXDOCMessageDecoderLine.AMLCQuotaApproval.IsEmpty);
			EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.MarkingLabelReference, "LAB1234");
			EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.SchemePlanNumber, "DRB");
			EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.SchemePlanNumber, "MVP");
			EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.SchemePlanNumber, "TX");
			EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceQualifierList.ReleaseNumber, "No more please no more tests I am going nuts");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Label Approval Number", "LAB1234", testEXDOCMessageDecoderLine.LabelApprovalNumber);
			Assert("Duty Draw Back", testEXDOCMessageDecoderLine.DutyDrawback);
			Assert("Motor Vehicle Plan", testEXDOCMessageDecoderLine.MotorVehiclePlan);
			Assert("Texaco", testEXDOCMessageDecoderLine.Texaco);
			AssertEquals("AMLC Quota Approval", "No more please no more tests I am going nuts", testEXDOCMessageDecoderLine.AMLCQuotaApproval);
		}

		public void TestProcessATTForLAI()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Label Approval Indicator is OFF", !testEXDOCMessageDecoderLine.LabelApprovalIndicator);
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.LabelApprovalIndicatorQualifier);
			testEXDOCMessageDecoderLine.Process();
			Assert("Label Approval Indicator is ON", testEXDOCMessageDecoderLine.LabelApprovalIndicator);
		}

		public void TestProcessATTForUPI()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Ungraded Product Indicator is OFF", !testEXDOCMessageDecoderLine.UngradedProductIndicator);
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.UngradedProductIndicatorQualifier);
			testEXDOCMessageDecoderLine.Process();
			Assert("Ungraded Product Indicator is ON", testEXDOCMessageDecoderLine.UngradedProductIndicator);
		}

		public void TestProcessATTForHPI()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Halal Product Indicator is OFF", !testEXDOCMessageDecoderLine.HalalProductIndicator);
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.HalalProductIndicatorQualifier);
			testEXDOCMessageDecoderLine.Process();
			Assert("Halal Product Indicator is ON", testEXDOCMessageDecoderLine.HalalProductIndicator);
		}

		public void TestProcessGIN()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Batch Number is empty", testEXDOCMessageDecoderLine.BatchNumber.IsEmpty);
			EXDOCMessageUtilities.PopulateGIN(group11.GIN.InstantiateAChildAndAddItToChildrenCollection(), IdentityNumberQualifierList.BatchNumber, "3453");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Batch Number", "3453", testEXDOCMessageDecoderLine.BatchNumber);
		}

		public void TestProcessDTM()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Durability Start Date is empty", testEXDOCMessageDecoderLine.DurabilityStartDate.IsEmpty);
			Assert("Durability End Date is empty", testEXDOCMessageDecoderLine.DurabilityEndDate.IsEmpty);
			Assert("Empty Container Inspection Date is empty", testEXDOCMessageDecoderLine.EmptyContainerInspectionDate.IsEmpty);
			Assert("Salting Date is empty", testEXDOCMessageDecoderLine.SaltingDate.IsEmpty);
			EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.StartDateTime, "20060506", DateTimePeriodFormatQualifierList.Ccyymmdd);
			EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.EndDateTime, "20060507", DateTimePeriodFormatQualifierList.Ccyymmdd);
			EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.ContainerSafetyConventionCscInspectionDate, "20060301", DateTimePeriodFormatQualifierList.Ccyymmdd);
			EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.ProcessingDateTime, "20060908", DateTimePeriodFormatQualifierList.Ccyymmdd);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Durability Start Date", new ZDateTime(2006, 5, 6), testEXDOCMessageDecoderLine.DurabilityStartDate);
			AssertEquals("Durability End Date", new ZDateTime(2006, 5, 7), testEXDOCMessageDecoderLine.DurabilityEndDate);
			AssertEquals("Empty Container Inspection Date", new ZDateTime(2006, 3, 1), testEXDOCMessageDecoderLine.EmptyContainerInspectionDate);
			AssertEquals("Salting Date", new ZDateTime(2006, 9, 8), testEXDOCMessageDecoderLine.SaltingDate);
		}

		public void TestProcessLOC()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Product Source State is empty", testEXDOCMessageDecoderLine.ProductSourceState.IsEmpty);
			EXDOCMessageUtilities.PopulateLOC(group11.LOC.InstantiateAChildAndAddItToChildrenCollection(), PlaceLocationQualifierList.MutuallyDefined, "NSW");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Product Source State", "NSW", testEXDOCMessageDecoderLine.ProductSourceState);
		}

		public void TestProcessFTX()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Additional Declaration Text is empty", testEXDOCMessageDecoderLine.AdditionalDeclarationText.IsEmpty);
			Assert("Coded Statement 1 is empty", testEXDOCMessageDecoderLine.CodedStatement1.IsEmpty);
			Assert("Coded Statement 2 is empty", testEXDOCMessageDecoderLine.CodedStatement2.IsEmpty);
			Assert("Coded Statement 3 is empty", testEXDOCMessageDecoderLine.CodedStatement3.IsEmpty);
			Assert("Coded Statement 4 is empty", testEXDOCMessageDecoderLine.CodedStatement4.IsEmpty);
			Assert("Coded Statement 5 is empty", testEXDOCMessageDecoderLine.CodedStatement5.IsEmpty);
			Assert("Free Text Statement is empty", testEXDOCMessageDecoderLine.FreeTextStatement.IsEmpty);
			Assert("Grower Number is empty", testEXDOCMessageDecoderLine.GrowerNumber.IsEmpty);
			Assert("Import Authority Code is empty", testEXDOCMessageDecoderLine.ImportAuthorityCode.IsEmpty);
			Assert("Client Line Item ID is empty", testEXDOCMessageDecoderLine.ClientLineItemID.IsEmpty);
			EXDOCMessageUtilities.PopulateFTX(group11, TextSubjectQualifierList.AdditionalExportInformation, 34650, 70, "THE GRAIN IS FROM A CROP THAT HAS BEEN INSPECTED DURING THE GROWING SEASON ACCORDING TO APPROPRIATE PROCEDURES AND NO TILLETIA CONTRAVERSA WAS DETECTED.");
			EXDOCMessageUtilities.PopulateFTX(group11, TextSubjectQualifierList.MutuallyDefined, "123", "456", "789", "012", "345");
			EXDOCMessageUtilities.PopulateFTX(group11, TextSubjectQualifierList.CertificationStatements, 350, 70, "Why me I didn't hurt anyone, no more monkey tests jeez this is repetitive.");
			EXDOCMessageUtilities.PopulateFTX(group11, TextSubjectQualifierList.AdditionalMarksNumbersInformation, 25, 25, "34543");
			EXDOCMessageUtilities.PopulateFTX(group11, TextSubjectQualifierList.GovernmentInformation, 350, 70, "Import Authority Codexx");
			EXDOCMessageUtilities.PopulateFTX(group11, TextSubjectQualifierList.LineItem, 250, 70, "Client Line item IDxx");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Additional Declaration Text", "THE GRAIN IS FROM A CROP THAT HAS BEEN INSPECTED DURING THE GROWING SEASON ACCORDING TO APPROPRIATE PROCEDURES AND NO TILLETIA CONTRAVERSA WAS DETECTED.", testEXDOCMessageDecoderLine.AdditionalDeclarationText);
			AssertEquals("Coded Statement 1", new ZShort("123"), testEXDOCMessageDecoderLine.CodedStatement1);
			AssertEquals("Coded Statement 2", new ZShort("456"), testEXDOCMessageDecoderLine.CodedStatement2);
			AssertEquals("Coded Statement 3", new ZShort("789"), testEXDOCMessageDecoderLine.CodedStatement3);
			AssertEquals("Coded Statement 4", new ZShort("012"), testEXDOCMessageDecoderLine.CodedStatement4);
			AssertEquals("Coded Statement 5", new ZShort("345"), testEXDOCMessageDecoderLine.CodedStatement5);
			AssertEquals("Free Text Statement", "Why me I didn't hurt anyone, no more monkey tests jeez this is repetitive.", testEXDOCMessageDecoderLine.FreeTextStatement);
			AssertEquals("Grower Number", "34543", testEXDOCMessageDecoderLine.GrowerNumber);
			AssertEquals("Import Authority Code", "Import Authority Codexx", testEXDOCMessageDecoderLine.ImportAuthorityCode);
			AssertEquals("Client Line item ID", "Client Line item IDxx", testEXDOCMessageDecoderLine.ClientLineItemID);
		}

		public void TestFinalConsumer()
		{
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Final Consumer - default value", false, testEXDOCMessageDecoderLine.FinalConsumerIndicator);
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.FinalConsumerIndicator);
			testEXDOCMessageDecoderLine.Process();
			Assert("Final Consumer is Indicated", testEXDOCMessageDecoderLine.FinalConsumerIndicator);
		}

		public void TestProcessMOA()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("FOB Amount is empty", testEXDOCMessageDecoderLine.FOBAmount.IsEmpty);
			EXDOCMessageUtilities.PopulateMOA(group11.MOA.InstantiateAChildAndAddItToChildrenCollection(), MonetaryAmountTypeQualifierList.FobValue, 234.21m);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("FOB Amount", 234.21m, testEXDOCMessageDecoderLine.FOBAmount);
		}

		public void TestProcessDOC()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Extra Certificate is empty", testEXDOCMessageDecoderLine.ExtraCertificates.IsEmpty);
			Assert("Assigned Certificate Numbers is empty", testEXDOCMessageDecoderLine.AssignedCertificateNumbers.IsEmpty);
			Assert("Assigned Certificate Templates is empty", testEXDOCMessageDecoderLine.AssignedCertificateTemplates.IsEmpty);
			Assert("Export Permit Number is empty", testEXDOCMessageDecoderLine.ExportPermitNumber.IsEmpty);
			Assert("Export Permit Authority is empty", testEXDOCMessageDecoderLine.ExportPermitAuthority.IsEmpty);
			SegmentGroup12 group12 = group11.Group12.InstantiateAChildAndAddItToChildrenCollection();
			PopulateDOC(group12.DOC.InstantiateAChildAndAddItToChildrenCollection(), DocumentMessageNameCodedList.SanitaryCertificate, "H2342", DocumentMessageStatusCodedList.ToBePrinted);
			PopulateDOC(group12.DOC.InstantiateAChildAndAddItToChildrenCollection(), DocumentMessageNameCodedList.SanitaryCertificate, "H4534", "9999999", DocumentMessageStatusCodedList.Accepted);
			PopulateDOC(group12.DOC.InstantiateAChildAndAddItToChildrenCollection(), DocumentMessageNameCodedList.CertificateOfQuality, "J4534", "8888888", DocumentMessageStatusCodedList.Accepted);
			EXDOCMessageUtilities.PopulateDOC(group12.DOC.InstantiateAChildAndAddItToChildrenCollection(), DocumentMessageNameCodedList.GoodsControlCertificate, "34434534", EXDOCPermitTypeCodes.Codes.AustralianFisheriesManagementAuthority);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Extra Certificate", "H2342", testEXDOCMessageDecoderLine.ExtraCertificates.ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("Assigned Certificate Numbers", "9999999,8888888", testEXDOCMessageDecoderLine.AssignedCertificateNumbers.ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("Assigned Certificate Templates", "H4534,J4534", testEXDOCMessageDecoderLine.AssignedCertificateTemplates.ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("Export Permit Number", "34434534", testEXDOCMessageDecoderLine.ExportPermitNumber);
			AssertEquals("Export Permit Date", EXDOCPermitTypeCodes.Codes.AustralianFisheriesManagementAuthority, testEXDOCMessageDecoderLine.ExportPermitAuthority);
		}

		public void TestGroup12ProcessDTM()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Export Permit Date", testEXDOCMessageDecoderLine.ExportPermitDate.IsEmpty);
			SegmentGroup12 group12 = group11.Group12.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateDTM(group12.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.DocumentMessageDateTime, "20030708", DateTimePeriodFormatQualifierList.Ccyymmdd);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Export Permit Date", new ZDateTime(2003, 7, 8), testEXDOCMessageDecoderLine.ExportPermitDate);
		}

		public void TestProcessPNA()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("AMLC Performance Exporter Number is empty", testEXDOCMessageDecoderLine.AMLCPerformanceExporterNumber.IsEmpty);
			SegmentGroup13 group13 = group11.Group13.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group13.PNA.InstantiateAChildAndAddItToChildrenCollection(), PartyQualifierList.Exporter, "3453445");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("AMLC Performance Exporter Number", "3453445", testEXDOCMessageDecoderLine.AMLCPerformanceExporterNumber);
		}

		public void TestProcessPAC()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Outer Pack Count is empty", testEXDOCMessageDecoderLine.OuterPackCount.IsEmpty);
			Assert("Outer Pack Type is empty", testEXDOCMessageDecoderLine.OuterPackType.IsEmpty);
			Assert("Intermediate Pack Count is empty", testEXDOCMessageDecoderLine.IntermediatePackCount.IsEmpty);
			Assert("Intermediate Pack Type is empty", testEXDOCMessageDecoderLine.IntermediatePackType.IsEmpty);
			Assert("Inner Pack Count is empty", testEXDOCMessageDecoderLine.InnerPackCount.IsEmpty);
			Assert("Inner Pack Type is empty", testEXDOCMessageDecoderLine.InnerPackType.IsEmpty);
			SegmentGroup15 group15 = group11.Group15.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePAC(group15.PAC.InstantiateAChildAndAddItToChildrenCollection(), "34", PackagingLevelCodedList.Outer, EXDOCPackTypeCodes.Codes.Bins);
			EXDOCMessageUtilities.PopulatePAC(group15.PAC.InstantiateAChildAndAddItToChildrenCollection(), "98", PackagingLevelCodedList.Intermediate, EXDOCPackTypeCodes.Codes.Cuttings);
			EXDOCMessageUtilities.PopulatePAC(group15.PAC.InstantiateAChildAndAddItToChildrenCollection(), "9834", PackagingLevelCodedList.Inner, EXDOCPackTypeCodes.Codes.Envelopes);
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Outer Pack Count", 34, testEXDOCMessageDecoderLine.OuterPackCount);
			AssertEquals("Outer Pack Type", EXDOCPackTypeCodes.Codes.Bins, testEXDOCMessageDecoderLine.OuterPackType);
			AssertEquals("Intermediate Pack Count", 98, testEXDOCMessageDecoderLine.IntermediatePackCount);
			AssertEquals("Intermediate Pack Type", EXDOCPackTypeCodes.Codes.Cuttings, testEXDOCMessageDecoderLine.IntermediatePackType);
			AssertEquals("Inner Pack Count", 9834, testEXDOCMessageDecoderLine.InnerPackCount);
			AssertEquals("Inner Pack Type", EXDOCPackTypeCodes.Codes.Envelopes, testEXDOCMessageDecoderLine.InnerPackType);
		}

		public void TestProcessPCI()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Shipping Marks are empty", testEXDOCMessageDecoderLine.ShippingMarks.IsEmpty);
			SegmentGroup15 group15 = group11.Group15.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePCI(group15.PCI.InstantiateAChildAndAddItToChildrenCollection(), "Shipping Marks used in the sending of a declaration to Quarantine that was then reissued to another palyer");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Shipping Marks", "Shipping Marks used in the sending of a declaration to Quarantine that was then reissued to another palyer", testEXDOCMessageDecoderLine.ShippingMarks);
		}

		public void TestGroup15ProcessMEA()
		{
			testEXDOCMessageDecoderLine.Process();
			Assert("Outer Pack Accuracy is empty", testEXDOCMessageDecoderLine.OuterPackAccuracy.IsEmpty);
			Assert("Outer Pack Weight is empty", testEXDOCMessageDecoderLine.OuterPackWeight.IsEmpty);
			Assert("Outer Pack Weight Unit is empty", testEXDOCMessageDecoderLine.OuterPackWeightUnit.IsEmpty);
			Assert("Intermediate Pack Accuracy is empty", testEXDOCMessageDecoderLine.IntermediatePackAccuracy.IsEmpty);
			Assert("Intermediate Pack Weight is empty", testEXDOCMessageDecoderLine.IntermediatePackWeight.IsEmpty);
			Assert("Intermediate Pack Weight Unit is empty", testEXDOCMessageDecoderLine.IntermediatePackWeightUnit.IsEmpty);
			Assert("Inner Pack Accuracy is empty", testEXDOCMessageDecoderLine.InnerPackAccuracy.IsEmpty);
			Assert("Inner Pack Weight is empty", testEXDOCMessageDecoderLine.InnerPackWeight.IsEmpty);
			Assert("Inner Pack Weight Unit is empty", testEXDOCMessageDecoderLine.InnerPackWeightUnit.IsEmpty);
			SegmentGroup15 group15 = group11.Group15.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateMEA(group15.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.Package, PropertyMeasuredCodedList.NetWeight, MeasurementSignificanceCodedList.GetFromString(EXDOCPackAccuracyCodes.Codes.Approximate), EXDOCMetricWeightUnitCodes.Codes.Kilogram, "234.454");
			EXDOCMessageUtilities.PopulateMEA(group15.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.Package, PropertyMeasuredCodedList.NetWeight, MeasurementSignificanceCodedList.GetFromString(EXDOCPackAccuracyCodes.Codes.Approximate), EXDOCMetricWeightUnitCodes.Codes.MetricTon, "435.453");
			EXDOCMessageUtilities.PopulateMEA(group15.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.Package, PropertyMeasuredCodedList.NetWeight, MeasurementSignificanceCodedList.GetFromString(EXDOCPackAccuracyCodes.Codes.EqualTo), EXDOCMetricWeightUnitCodes.Codes.Number, "34.56");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Outer Pack Accuracy", EXDOCPackAccuracyCodes.Codes.Approximate, testEXDOCMessageDecoderLine.OuterPackAccuracy);
			AssertEquals("Outer Pack Weight", 234.454m, testEXDOCMessageDecoderLine.OuterPackWeight);
			AssertEquals("Outer Pack Weight Unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, testEXDOCMessageDecoderLine.OuterPackWeightUnit);
			AssertEquals("Intermediate Pack Accuracy", EXDOCPackAccuracyCodes.Codes.Approximate, testEXDOCMessageDecoderLine.IntermediatePackAccuracy);
			AssertEquals("Intermediate Pack Weight", 435.453m, testEXDOCMessageDecoderLine.IntermediatePackWeight);
			AssertEquals("Intermediate Pack Weight Unit", EXDOCMetricWeightUnitCodes.Codes.MetricTon, testEXDOCMessageDecoderLine.IntermediatePackWeightUnit);
			AssertEquals("Inner Pack Accuracy", EXDOCPackAccuracyCodes.Codes.EqualTo, testEXDOCMessageDecoderLine.InnerPackAccuracy);
			AssertEquals("Inner Pack Weight", 34.56m, testEXDOCMessageDecoderLine.InnerPackWeight);
			AssertEquals("Inner Pack Weight Unit", EXDOCMetricWeightUnitCodes.Codes.Number, testEXDOCMessageDecoderLine.InnerPackWeightUnit);
		}

		public void TestProcessContainers()
		{
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Container Count is 0", 0, testEXDOCMessageDecoderLine.Containers.Count);
			SegmentGroup16 group16_1 = group11.Group16.InstantiateAChildAndAddItToChildrenCollection();
			SegmentGroup17 group17_1 = group16_1.Group17.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateEQD(group16_1.EQD.InstantiateAChildAndAddItToChildrenCollection(), EquipmentQualifierList.Container, "ABOC12345");
			EXDOCMessageUtilities.PopulateSEL(group17_1.SEL.InstantiateAChildAndAddItToChildrenCollection(), "1234", "", "");
			SegmentGroup16 group16_2 = group11.Group16.InstantiateAChildAndAddItToChildrenCollection();
			SegmentGroup17 group17_2 = group16_2.Group17.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateEQD(group16_2.EQD.InstantiateAChildAndAddItToChildrenCollection(), EquipmentQualifierList.Container, "ABOC67890");
			EXDOCMessageUtilities.PopulateSEL(group17_2.SEL.InstantiateAChildAndAddItToChildrenCollection(), "5678", "", "");
			testEXDOCMessageDecoderLine.Process();
			AssertEquals("Container Count is 2", 2, testEXDOCMessageDecoderLine.Containers.Count);
			AssertEquals("Container Number 1", "ABOC12345", testEXDOCMessageDecoderLine.Containers[0].containerNumber);
			AssertEquals("Container Seal 1", "1234", testEXDOCMessageDecoderLine.Containers[0].containerSeal);
			AssertEquals("Container Number 2", "ABOC67890", testEXDOCMessageDecoderLine.Containers[1].containerNumber);
			AssertEquals("Container Seal 2", "5678", testEXDOCMessageDecoderLine.Containers[1].containerSeal);
		}

		protected override void SetUp()
		{
			base.SetUp();
			group11 = new SegmentGroup11();
			testEXDOCMessageDecoderLine = new EXDOCMessageDecoderLine(group11);
		}

		void PopulateDOC(DOCSegment dOC, DocumentMessageNameCodedList documentMessageNameCoded, string documentMessageName, DocumentMessageStatusCodedList documentMessageStatusCoded)
		{
			PopulateDOC(dOC, documentMessageNameCoded, documentMessageName, ZString.Empty, documentMessageStatusCoded);
		}

		void PopulateDOC(DOCSegment dOC, DocumentMessageNameCodedList documentMessageNameCoded, string documentMessageName, string documentMessageNumber, DocumentMessageStatusCodedList documentMessageStatusCoded)
		{
			dOC.DocumentMessageName.DocumentMessageNameCoded = documentMessageNameCoded;
			dOC.DocumentMessageName.DocumentMessageName = documentMessageName;
			dOC.DocumentMessageDetails.DocumentMessageNumber = documentMessageNumber;
			dOC.DocumentMessageDetails.DocumentMessageStatusCoded = documentMessageStatusCoded;
		}

		SegmentGroup11 group11;
		EXDOCMessageDecoderLine testEXDOCMessageDecoderLine;
	}
}
