using System;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDMessageLineTest : BaseMessageLineAbstractTest
	{
		public override void TestPopulate()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABABEU");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "AAA3336347E");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			invoiceLine.JI_Description = "DESCRIPTION ON LINE";
			invoiceLine.AddInfo.ZA_ORG = "IT";
			invoiceLine.AddInfo.ZA_POC = "ZA";
			invoiceLine.AddInfo.ZA_DCX = "NZ";
			invoiceLine.AddInfo.ZA_FOD = "300305";
			invoiceLine.AddInfo.ZA_ISS = 54.35M;
			invoiceLine.AddInfo.ZA_LCP = 12.25M;

			AQISPackage package = invoiceLine.AQISPackages.AddNew();
			package.Number = 100;
			package.Type = " RL ";

			invoiceLine.JI_LinePrice = 1500.00M;
			PopulateFieldsForSegmentGroup35();
			invoiceLine.AddInfo.ZA_WRN = "9B50480001C";
			invoiceLine.AddInfo.ZA_WRL = 45;

			AQISPremisesIdAndProcessingType pAndPbizObj = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			pAndPbizObj.PremisesId = "Test ID";
			pAndPbizObj.ProcessingType = "RPT";

			invoiceLine.AddInfo.ZA_WETQ = "Y";
			invoiceLine.AddInfo.ZA_LCTQ = "Y";
			invoiceLine.AddInfo.ZA_MLPI = "Y";
			invoiceLine.AddInfo.ZA_LCTI = "Y";
			invoiceLine.AddInfo.ZA_PUP = "Y";
			invoiceLine.AddInfo.ZA_REL_Hidden = "Y";
			invoiceLine.AddInfo.ZA_ODF = 123.23M;
			invoiceLine.AddInfo.ZA_DRE = 0.5M;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 123M;
			invoiceLine.AddInfo.ZA_QT2 = 543M;
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			invoiceLine.AddInfo.ZA_WAR = "9515C";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERMIT3";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERMIT2";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERMIT1";
			testDec.DoMerge();

			var iMDMessageLineToTest = new IMDMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[0], new SegmentGroup30(), false, true);
			iMDMessageLineToTest.Populate(1, LineAction.Insert);
			ZString result = iMDMessageLineToTest.Group30.ToString(new UNOCCMRCharacterSet());

			ZString expectedResult =
@"CST+1+I::95+N10::95'
FTX+AAA+++ENTRY=9B50480001C, LINE=45 ?: DESCRIPTION ON LINE'
LOC+27+IT::5'
LOC+30+ZA::5'
LOC+35+NZ::5'
LOC+18+9515C::5'
DTM+4:20050330:102'
MEA+AAA++NO:123.00000'
MEA+AAA++LA:543.00000'
MEA+ABX++LCO:12.25'
NAD+SU+AAA3336347E::95'
PAC+100++RL:185:194'
MOA+38:1500.00'
RFF+ABD:20033040'
RFF+AED:05'
RFF+ASA:GSTE'
RFF+DA:WETE'
RFF+AHW:PERMIT1'
RFF+AHW:PERMIT2'
RFF+AHW:PERMIT3'
RFF+ABA:123'
RFF+ABL:SCN'
RFF+AGW:PST'
RFF+ANG:PRT'
RFF+ABC:DSN'
RFF+ABI:LCTE'
RFF+ACE:SSS'
RFF+AFG:00000000'
RFF+AFD:TRE'
RFF+AHX:TRN'
RFF+AJY:ISC'
RFF+AKZ:TAN'
RFF+AWA:RNO'
RFF+IP:ICN'
RFF+ADY:11111111'
RFF+ADY:22222222'
RFF+AEA:12345678::TTT'
RFF+AFM:22223333::III'
RFF+AIP:33334444::PPP'
RFF+AKG:55555555'
RFF+AKG:66666666'
RFF+AKO:COUNTRY'
RFF+AES:CODE::INT'
RFF+ACD:WMC'
RFF+WE:9B50480001C'
RFF+LI:45'
RFF+ANJ:PERMIT ONE'
RFF+ANJ:PERMIT TWO'
RFF+ABU:COM1'
RFF+ABU:COM2'
RFF+ACL:DOC NUMBER 1::DOCTYPE1'
RFF+ACL:DOC NUMBER 2::DOCTYPE2'
RFF+AAQ:OCLU10000042'
RFF+AFV:TV'
DOC+1'
LOC+90+TEST ID::194:RPT'
GIS+WET:109:95'
GIS+LCT:109:95'
GIS+LCQ:109:95'
GIS+MLP:109:95'
GIS+PUP:109:95'
GIS+REL:109:95'
TAX+1+ADD+++:::0.5000'
TAX+9+CUD+++:::123.2300'";

			AssertMultilineEquals("Should be no differences", expectedResult.Replace("\r\n", ""), result, '\'');
		}

		public void TestLineTransportAndInsuranceInLocalCurrencies()
		{
			var futureDate = ZDateTime.Today.AddDays(1);
			var testHelper = new ZTestHelper(Factory);
			testHelper.SetExchangeRate(futureDate, futureDate.AddDays(1), 0.7629m, testHelper.USDCurrency);

			invoiceLine.Declaration.JE_ExportDate = futureDate;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = testHelper.USDCurrency.RX_Code;

			invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2394.00m, "USD");
			testDec.DoMerge();

			IMDMessageLineToTest.Populate(1, "I");
			var result = Group30String;
			AssertEquals("MOA Segment", true, result.Contains("MOA+68:2394.00:USD"));
		}

		public void TestPopulateZeroManualDuty()
		{
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine.AddInfo.ZA_SendZeroDutyOverride_Hidden = false;
			invoiceLine.AddInfo.ZA_DTY = 100m;

			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			ZString result = Group30String;
			AssertEquals("MOA Segment with duty override", true, result.Contains("MOA+55:100.00:AUD"));

			invoiceLine.AddInfo.ZA_SendZeroDutyOverride_Hidden = true;
			invoiceLine.AddInfo.ZA_DTY = 100m;

			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			result = Group30String;
			AssertEquals("MOA Segment with duty override", true, result.Contains("MOA+55:100.00:AUD"));

			invoiceLine.AddInfo.ZA_SendZeroDutyOverride_Hidden = true;
			invoiceLine.AddInfo.ZA_DTY = 0m;

			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			result = Group30String;
			AssertEquals("MOA Segment with duty override", true, result.Contains("MOA+55:0.00:AUD"));

			invoiceLine.AddInfo.ZA_SendZeroDutyOverride_Hidden = false;
			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			result = Group30String;
			AssertEquals("MOA Segment with duty override", false, result.Contains("MOA+55:"));
		}

		public void TestPopulateWithTILVEmptyAmount()
		{
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.AddInfo.ZA_TILV = "0.00AUD";

			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			ZString result = Group30String;
			AssertEquals("MOA Segment with TILV Empty amount", true, result.Contains("MOA+68:0.00:AUD"));

			invoiceLine.AddInfo.ZA_TILV = "";
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			result = Group30String;
			AssertEquals("MOA Segment with TILV Empty amount", false, result.Contains("MOA+68:0.00:AUD"));
		}

		public void TestN10EntriesSendVIDValues()
		{
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.AddInfo.ZA_TILV = "0.00AUD";

			var declaration = invoiceLine.Declaration;
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.OH_IsConsignee = true;

			AUOrgSupplierPart part = Factory.New<AUOrgSupplierPart>();
			invoiceLine.SetPartForTesting(part);
			declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 1, true);
			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
			invoiceLine.JI_PartAttrib1 = "FromPartAttrib1";

			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			ZString result = Group30String;
			AssertEquals("Vehicle ID is sourced from part attribute if the VIN AddInfo field is empty", true, result.Contains("RFF+AKG:FROMPARTATTRIB1"));

			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			result = Group30String;
			AssertEquals("Vehicle ID from part AddInfo field", true, result.Contains("RFF+AKG:FROMADDINFO"));
		}

		public void TestN10EntriesSendAddInfoVIN()
		{
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.AddInfo.ZA_TILV = "0.00AUD";

			var declaration = invoiceLine.Declaration;
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.OH_IsConsignee = true;

			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.Populate(1, "I");
			ZString result = Group30String;
			AssertEquals("Vehicle ID from part AddInfo field", true, result.Contains("RFF+AKG:FROMADDINFO"));
		}

		public void TestCSTForEachNatureType()
		{
			IMDMessageLineToTest.PopulateCST("I");
			ZString result = Group30String;
			AssertEquals("CST Segment", true, result.Contains("::95+N10::95'"));

			invoiceLine.JI_IsPackToBondForLine = true;
			IMDMessageLineToTest.PopulateCST("I");
			result = Group30String;
			AssertEquals("CST Segment", true, result.Contains("::95+N20::95'"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			IMDMessageLineToTest.PopulateCST("I");
			result = Group30String;
			AssertEquals("CST Segment", true, result.Contains("::95+N30::95'"));
		}

		[TestDate(2017, 4, 11)]
		public void TestCST_Vendor()
		{
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 4, 1)))
			{
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "Test 1";
				supplier.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "12345678901");

				var expectedNadVendorText = "NAD+VN+12345678901::95'";

				invoiceHeader.JZ_OH_Supplier = supplier.PK;
				IMDMessageLineToTest.PopulateCST("I");
				AssertContains("CST Segment Nature 10", expectedNadVendorText, Group30String);

				invoiceLine.JI_IsPackToBondForLine = true;
				IMDMessageLineToTest.Group30 = new SegmentGroup30();
				IMDMessageLineToTest.PopulateCST("I");
				AssertNotContains("CST Segment Nature 20", expectedNadVendorText, Group30String);

				testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				IMDMessageLineToTest.Group30 = new SegmentGroup30();
				IMDMessageLineToTest.PopulateCST("I");
				AssertNotContains("CST Segment Nature 30", expectedNadVendorText, Group30String);
			}
		}

		[TestDate(2017, 4, 11)]
		public void TestCST_Vendor_SplitABNAndCAC()
		{
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 4, 1)))
			{
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "Test 1";
				supplier.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "540 919 970 27/ 001");

				var expectedNadVendorText = "NAD+VN+54091997027::95'";

				invoiceHeader.JZ_OH_Supplier = supplier.PK;
				IMDMessageLineToTest.PopulateCST("I");
				AssertContains("CST Segment Nature 10", expectedNadVendorText, Group30String);
			}
		}

		public void TestCompileN20Entries()
		{
			invoiceLine.AddInfo.ZA_WRN = "9B50480001C";
			invoiceLine.AddInfo.ZA_WRL = 45;
			invoiceLine.JI_Description = "DESCRIPTION ON LINE";
			IMDMessageLineToTest.PopulateFTX();
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;

			AssertEquals("Description with Multiple Clearance Code", true, result.Contains("FTX+AAA+++ENTRY=9B50480001C, LINE=45 ?: DESCRIPTION ON LINE'"));
			AssertEquals("Multiple Clearance Code", true, result.Contains("RFF+ACD:9B504800'"));
			AssertEquals("WRN", false, result.Contains("RFF+WE:9B50480001C'"));
			AssertEquals("WRL", false, result.Contains("RFF+LI:45'"));

			imdMessageLineToTest = null;
			invoiceLine.AddInfo.ZA_WRN = "9B504800";
			invoiceLine.AddInfo.ZA_WRL = 45;
			invoiceLine.JI_Description = "DESCRIPTION ON LINE";
			IMDMessageLineToTest.PopulateFTX();
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;

			AssertEquals("Description with Multiple Clearance Code", true, result.Contains("FTX+AAA+++DESCRIPTION ON LINE'"));
			AssertEquals("Multiple Clearance Code", false, result.Contains("RFF+ACD:9B504800'"));
			AssertEquals("WRN", true, result.Contains("RFF+WE:9B504800'"));
			AssertEquals("WRL", true, result.Contains("RFF+LI:45'"));

			imdMessageLineToTest = null;
			invoiceLine.AddInfo.ZA_WMC = "111";
			invoiceLine.AddInfo.ZA_WRN = "9B50480001C";
			invoiceLine.AddInfo.ZA_WRL = 45;
			invoiceLine.JI_Description = "DESCRIPTION ON LINE";
			IMDMessageLineToTest.PopulateFTX();
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;

			AssertEquals("Description with Multiple Clearance Code", true, result.Contains("FTX+AAA+++ENTRY=9B50480001C, LINE=45 ?: DESCRIPTION ON LINE'"));
			AssertEquals("Multiple Clearance Code", true, result.Contains("RFF+ACD:111'"));
			AssertEquals("WRN", true, result.Contains("RFF+WE:9B50480001C'"));
			AssertEquals("WRL", true, result.Contains("RFF+LI:45'"));
		}

		public void TestAmberLineProcessingInFTXSegment()
		{
			invoiceHeader.AddInfo.ZA_AMB = "C";
			IMDMessageLineToTest.PopulateFTX();
			ZString result = Group30String;
			AssertEquals("Amber Line Processing", true, result.Contains("FTX+ABA++CLASS::95'"));

			invoiceLine.AddInfo.ZA_AMB = "D";
			IMDMessageLineToTest.PopulateFTX();
			result = Group30String;
			AssertEquals("Amber Line Processing", true, result.Contains("FTX+ABA++DUMP::95'"));
		}

		public void TestMultipleAmberLineProcessingInFTXSegment()
		{
			invoiceHeader.AddInfo.ZA_AMB = "CDOP";
			IMDMessageLineToTest.PopulateFTX();
			ZString result = Group30String;
			AssertEquals("Multiple Amber Line Processing", true, result.Contains("FTX+ABA++CLASS::95'FTX+ABA++DUMP::95'FTX+ABA++ORIGIN::95'FTX+ABA++PREFER::95'"));

			invoiceLine.AddInfo.ZA_AMB = "QTV";
			IMDMessageLineToTest.PopulateFTX();
			result = Group30String;
			AssertEquals("Multiple Amber Line Processing", true, result.Contains("FTX+ABA++QUANTITY::95'FTX+ABA++TREATCODE::95'FTX+ABA++VALUE::95'"));
		}

		public void TestSortAmberLineProcessingBeforeGenerating()
		{
			invoiceLine.AddInfo.ZA_AMB = "CpDo";
			IMDMessageLineToTest.PopulateFTX();
			ZString result = Group30String;
			AssertEquals("FTX Segments for AmberLineProcessing should be in the right order", true, result.Contains("FTX+ABA++CLASS::95'FTX+ABA++DUMP::95'FTX+ABA++ORIGIN::95'FTX+ABA++PREFER::95'"));
		}

		public void TestOriginInLOCSegment()
		{
			invoiceHeader.AddInfo.ZA_ORG = "JP";
			IMDMessageLineToTest.PopulateLOC();
			ZString result = Group30String;
			AssertEquals("Origin Code", true, result.Contains("LOC+27+JP::5'"));

			invoiceLine.AddInfo.ZA_ORG = "IT";
			IMDMessageLineToTest.PopulateLOC();
			result = Group30String;
			AssertEquals("Origin Code", true, result.Contains("LOC+27+IT::5'"));
		}

		public void TestPreferenceOriginCodeInLOCSegment()
		{
			invoiceHeader.AddInfo.ZA_POC = "JP";
			IMDMessageLineToTest.PopulateLOC();
			ZString result = Group30String;
			AssertEquals("Preference Origin Code", true, result.Contains("LOC+30+JP::5'"));

			invoiceLine.AddInfo.ZA_POC = "IT";
			IMDMessageLineToTest.PopulateLOC();
			result = Group30String;
			AssertEquals("Preference Origin Code", true, result.Contains("LOC+30+IT::5'"));
		}

		public void TestDumpingExportCountryCodeInLOCSegment()
		{
			invoiceLine.AddInfo.ZA_DCX = "IT";
			IMDMessageLineToTest.PopulateLOC();
			ZString result = Group30String;
			AssertEquals("Dumping Export Country Code", true, result.Contains("LOC+35+IT::5'"));
		}

		public void TestPopulateDTM()
		{
			invoiceLine.AddInfo.ZA_FOD = "300305";
			IMDMessageLineToTest.PopulateDTM();
			ZString result = Group30String;
			AssertEquals("Firm Order Date", true, result.Contains("DTM+4:20050330:102'"));
		}

		public void TestPopulateMEA()
		{
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.ZA_ISS = 54.35M;
			invoiceLine.AddInfo.ZA_LCP = 12.25M;
			IMDMessageLineToTest.PopulateMEA();
			ZString result = Group30String;
			AssertEquals("Percentage of alchol", true, result.Contains("MEA+AAG++ISS:54.35'"));
			AssertEquals("Local Content Percentage", true, result.Contains("MEA+ABX++LCO:12.25'"));
		}

		public void TestMEASegmentDoesnNotContainISSForNatureOtherThan30()
		{
			AssertEquals("Preassertion: nvoice line shouldn't be Nature 30", false, invoiceLine.CusEntryLine.IsNature30);
			invoiceLine.AddInfo.ZA_ISS = 54.35M;
			IMDMessageLineToTest.PopulateMEA();
			ZBool result = false;
			foreach (MEASegment mEA in IMDMessageLineToTest.Group30.MEA)
			{
				if (mEA.MeasurementAttributeCode.ToString() == MeasurementAttributeCodeList.PercentageOfAlcoholByVolume
					&& mEA.ValueRange.MeasurementUnitCode == "ISS")
				{
					result = true;
				}
			}
			AssertEquals("Should not contain ISS because it is not Nature 30", false, result);
		}

		public void TestPopulateMEAForWarehouseQTYAndUQ()
		{
			invoiceLine.AddInfo.ZA_WRQ = 123.45M;
			invoiceLine.AddInfo.ZA_WRU = "KG";
			IMDMessageLineToTest.PopulateMEA();
			ZString result = Group30String;
			AssertEquals("Warehouse Unit of Quantity", false, result.Contains("MEA+AAA+::WAR+KG:123.45'"));

			invoiceLine.JI_IsPackToBondForLine = true;
			IMDMessageLineToTest.PopulateMEA();
			result = Group30String;
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA+::WAR+KG:123.45000'"));

			invoiceLine.JI_IsPackToBondForLine = false;
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			IMDMessageLineToTest.PopulateMEA();
			result = Group30String;
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA+::WAR+KG:123.45000'"));
		}

		public void TestPopulateMEAForWarehouse()
		{
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 123M;
			IMDMessageLineToTest.PopulateMEA();
			ZString result = Group30String;
			AssertEquals("Customs QTY & UQ", true, result.Contains("MEA+AAA++NO:123.00000'"));

			invoiceLine.JI_IsPackToBondForLine = true;
			IMDMessageLineToTest.PopulateMEA();
			result = Group30String;
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA+::WAR+NO:123.00000'"));
		}

		public void TestPopulateMEAForWarehouseWhenUQAreTheSame()
		{
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 150M;
			invoiceLine.AddInfo.ZA_WRQ = 150M;
			invoiceLine.AddInfo.ZA_WRU = "KG";
			IMDMessageLineToTest.PopulateMEA();
			ZString result = Group30String;
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA+::WAR+KG:150.00000'"));
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA++NO:150.00000'"));

			invoiceLine.AddInfo.ZA_WRU = "NO";
			IMDMessageLineToTest.PopulateMEA();
			result = Group30String;
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA+::WAR+NO:150.00000'"));
		}

		public void TestPopulateMEAForAmendmentForNature10()
		{
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 150M;
			invoiceLine.AddInfo.ZA_QT2 = 543M;
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			IMDMessageLine iMDMessageLineToTest = new IMDMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[0], new SegmentGroup30(), false, true);
			iMDMessageLineToTest.Populate(1, LineAction.Amend);
			ZString result = iMDMessageLineToTest.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Unit of Quantity", true, result.Contains("MEA+AAA++NO:150.00000'"));
			AssertEquals("Second Customs QTY & UQ", true, result.Contains("MEA+AAA++LA:543.00000'"));
		}

		public void TestPopulateMEAForAmendmentForNature20()
		{
			invoiceLine.AddInfo.ZA_WRQ = 150M;
			invoiceLine.AddInfo.ZA_WRU = "KG";
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 150M;
			invoiceLine.AddInfo.ZA_QT2 = 543M;
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			IMDMessageLine iMDMessageLineToTest = new IMDMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[0], new SegmentGroup30(), false, true);
			iMDMessageLineToTest.Populate(1, LineAction.Amend);
			ZString result = iMDMessageLineToTest.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA+::WAR+KG:150.00000'"));
			AssertEquals("Customs Unit of Quantity", true, result.Contains("MEA+AAA++NO:150.00000'"));
			AssertEquals("Second Customs QTY & UQ", true, result.Contains("MEA+AAA++LA:543.00000'"));
		}

		public void TestPopulateMEAForAmendmentForNature30()
		{
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.ZA_WRQ = 150M;
			invoiceLine.AddInfo.ZA_WRU = "KG";
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 150M;
			invoiceLine.AddInfo.ZA_QT2 = 543M;
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			IMDMessageLine iMDMessageLineToTest = new IMDMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[0], new SegmentGroup30(), false, true);
			iMDMessageLineToTest.Populate(1, LineAction.Amend);
			ZString result = iMDMessageLineToTest.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Warehouse Unit of Quantity", true, result.Contains("MEA+AAA+::WAR+KG:150.00000'"));
			AssertEquals("Customs Unit of Quantity", true, result.Contains("MEA+AAA++NO:150.00000'"));
			AssertEquals("Second Customs QTY & UQ", true, result.Contains("MEA+AAA++LA:543.00000'"));
		}

		public void TestPopulateNADSegmentWithSupplierCodeFromHeader()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABABEU");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "AAA3336347E");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			IMDMessageLineToTest.PopulateNAD();
			ZString result = Group30String;
			AssertContains("CustomsClientID from Supplier on header", "NAD+SU+AAA3336347E::95'", result);
		}

		public void TestPopulateNADSegmentWithSupplierCodeFromLine()
		{
			OrgHeader supplier1 = OrgHeader.LoadFromCode(Factory, "ABABEU");
			supplier1.CustomsClientID = "AAA3336347E";
			OrgHeader supplier2 = OrgHeader.LoadFromCode(Factory, "AUSENG");
			supplier2.CustomsClientID = "BBB1234567Y";

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceHeader.JZ_OH_Supplier = supplier1.PK;
			invoiceLine.JI_OH_Supplier = supplier2.PK;

			IMDMessageLineToTest.PopulateNAD();
			ZString result = Group30String;
			AssertContains("CustomsClientID from Supplier on line", "NAD+SU+BBB1234567Y::95'", result);
		}

		public void TestPopulateNADSegmentWithEmptySupplierCode()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABABEU");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			IMDMessageLineToTest.PopulateNAD();
			ZString result = Group30String;
			AssertEquals("Supplier Code", false, result.Contains("NAD+SU+::95'"));
		}

		public void TestPopulateNADSegmentWithSupplierTIN()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Test 1";
			supplier.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "SUPTIN1234567890");

			var expectedNadSupplierTINText = "NAD+AU+SUPTIN1234567890::95'";

			invoiceLine.JI_OH_Supplier = supplier.PK;
			IMDMessageLineToTest.PopulateNAD();
			AssertContains("Group30 NAD Segment Supplier TIN on Nature 10", expectedNadSupplierTINText, Group30String);

			invoiceLine.JI_IsPackToBondForLine = true;
			IMDMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.PopulateNAD();
			AssertContains("Group30 NAD Segment Supplier TIN on Nature 20", expectedNadSupplierTINText, Group30String);

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			IMDMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.PopulateNAD();
			AssertNotContains("Group30 NAD Segment Supplier TIN on Nature 30", expectedNadSupplierTINText, Group30String);
		}

		public void TestPopulateGroup31()
		{
			AQISPackage package1 = invoiceLine.AQISPackages.AddNew();
			package1.Number = 100;
			package1.Type = "One";
			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			ZString result = Group30String;
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+100++ONE:185:194'"));

			AQISPackage package2 = invoiceLine.AQISPackages.AddNew();
			package2.Number = 200;
			package2.Type = "Two";
			Factory.Save();
			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			result = Group30String;
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+100++ONE:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+200++TWO:185:194'"));

			AQISPackage package3 = invoiceLine.AQISPackages.AddNew();
			package3.Number = 300;
			package3.Type = "Thr";

			AQISPackage package4 = invoiceLine.AQISPackages.AddNew();
			package4.Number = 400;
			package4.Type = "Four";

			AQISPackage package5 = invoiceLine.AQISPackages.AddNew();
			package5.Number = 500;
			package5.Type = "Five";

			AQISPackage package6 = invoiceLine.AQISPackages.AddNew();
			package6.Number = 600;
			package6.Type = "Six";

			AQISPackage package7 = invoiceLine.AQISPackages.AddNew();
			package7.Number = 700;
			package7.Type = "Sev";

			AQISPackage package8 = invoiceLine.AQISPackages.AddNew();
			package8.Number = 800;
			package8.Type = "Eig";

			AQISPackage package9 = invoiceLine.AQISPackages.AddNew();
			package9.Number = 900;
			package9.Type = "Nine";

			AQISPackage package10 = invoiceLine.AQISPackages.AddNew();
			package10.Number = 1000;
			package10.Type = "Ten";

			Factory.Save();
			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			result = Group30String;
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+100++ONE:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+200++TWO:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+300++THR:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+400++FOUR:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+500++FIVE:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+600++SIX:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+700++SEV:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+800++EIG:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+900++NINE:185:194'"));
			AssertEquals("AQIS Package Type", true, result.Contains("PAC+1000++TEN:185:194'"));
		}

		public void TestBuildAQISPackagesInOrder()
		{
			AQISPackage package1 = invoiceLine.AQISPackages.AddNew();
			package1.Number = 100;
			package1.Type = "ZZZ";

			AQISPackage package2 = invoiceLine.AQISPackages.AddNew();
			package2.Number = 200;
			package2.Type = "AAA";
			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			ZString result = Group30String;
			AssertEquals("AQIS Package Types should be built after ordered", true, result.Contains("PAC+200++AAA:185:194'PAC+100++ZZZ:185:194'"));
		}

		public void TestPopulateGroup32()
		{
			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "00000000";
			profile.CP_CommunityProtectionRiskIdentifier = 29;

			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			invoiceLine.JI_Tariff = "00000000";

			CMRCusEntryCPDec communityProtectionQuestion = entryLine.Questions.AddNew();
			communityProtectionQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			communityProtectionQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			communityProtectionQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;
			communityProtectionQuestion.ON_Permit = "ABC";

			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			ZString result = Group30String;
			AssertEquals("Lodgement Questions", false, result.Contains("FTX+RAH+++00029:00400:Y:ABC'"));

			CMRCommunityProtectionRisk risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_LodgementQuestionIdentifier = 400;
			risk.CK_StartDate = ZDateTime.Today;
			risk.CK_PermitApplicationIndicator = true;
			risk.CK_PermitType = "ABC";
			risk.CK_Identifier = 29;

			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			result = Group30String;
			AssertEquals("Lodgement Questions", true, result.Contains("FTX+RAH+++00029:00400:Y:ABC'"));
		}

		public void TestPopulateGroup32WhenGeneratingForAmendmentDetection()
		{
			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "00000000";
			profile.CP_CommunityProtectionRiskIdentifier = 29;

			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			CMRCommunityProtectionRisk risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_LodgementQuestionIdentifier = 400;
			risk.CK_StartDate = ZDateTime.Today;
			risk.CK_PermitApplicationIndicator = true;
			risk.CK_PermitType = "ABC";
			risk.CK_Identifier = 29;

			CMRCusEntryCPDec communityProtectionQuestion = entryLine.Questions.AddNew();
			communityProtectionQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			communityProtectionQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			communityProtectionQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;
			communityProtectionQuestion.ON_Permit = risk.CK_PermitType;

			invoiceLine.JI_Tariff = "00000000";

			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			ZString result = Group30String;
			AssertEquals("Lodgement Questions", true, result.Contains("FTX+RAH+++00029:00400:Y:ABC'"));

			IMDMessageLine messageBuilder = new IMDMessageLine(entryLine, new SegmentGroup30(), false, true);
			messageBuilder.PopulateGroup31AndGroup32();
			result = messageBuilder.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Lodgement Questions", false, result.Contains("FTX+RAH+++00029:00400:Y:ABC'"));
		}

		public void TestOrderCPDecQuestionsBeforeGeneratingMessages()
		{
			invoiceLine.JI_Tariff = "00000000";

			CMRLodgementQuestion lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 100;
			lodgementQuestion.CQ_LodgementQuestionStartDate = ZDateTime.Today;

			CMRLodgementQuestion lodgementQuestion50 = CMRLodgementQuestion.New(Factory);
			lodgementQuestion50.CQ_LodgementQuestionIdentifier = 50;
			lodgementQuestion50.CQ_LodgementQuestionStartDate = ZDateTime.Today;

			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_CommunityProtectionRiskIdentifier = 1000;
			profile.CP_TariffClassificationNumberfield = "00000000";

			CMRCommunityProtectionRisk riskFor50 = CMRCommunityProtectionRisk.New(Factory);
			riskFor50.CK_Identifier = 1000;
			riskFor50.CK_StartDate = ZDateTime.Today;
			riskFor50.CK_LodgementQuestionIdentifier = 50;

			CMRCommunityProtectionRisk riskFor100 = CMRCommunityProtectionRisk.New(Factory);
			riskFor100.CK_Identifier = 1000;
			riskFor100.CK_StartDate = ZDateTime.Today;
			riskFor100.CK_LodgementQuestionIdentifier = 100;

			CMRCusEntryCPDec cPDecQ1 = invoiceLine.CusEntryLine.Questions.AddNew();
			cPDecQ1.ON_CPDecNum = 100;
			cPDecQ1.ON_AnswerCode = "Y";
			cPDecQ1.ON_CPDecStartDate = ZDateTime.Today;

			CMRCusEntryCPDec cPDecQ2 = invoiceLine.CusEntryLine.Questions.AddNew();
			cPDecQ2.ON_CPDecNum = 50;
			cPDecQ2.ON_AnswerCode = "Y";
			cPDecQ2.ON_CPDecStartDate = ZDateTime.Today;

			IMDMessageLineToTest.PopulateGroup31AndGroup32();
			ZString result = Group30String;
			AssertEquals("CP Dec Questions are ordered before used in messages", true, result.Contains("FTX+RAH+++01000:00050:Y'PCI+1'FTX+RAH+++01000:00100:Y'"));
		}

		public override void TestSegmentGroup33()
		{
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceLine.AddInfo.ZA_TILV = "140.50AUD";
			invoiceLine.AddInfo.ZA_DXP = "200.50";
			invoiceLine.JI_LinePrice = 1500.00M;
			invoiceLine.AddInfo.ZA_ADJ = "345.45";
			invoiceLine.AddInfo.AdjustmentDollarPercentage_Hidden = "$";
			invoiceLine.AddInfo.AdjustmentCurrency_Hidden = "USD";
			testDec.DoMerge();
			IMDMessageLineToTest.PopulateGroup33();
			ZString result = Group30String;
			AssertEquals("Customs Value - Not Nature 30", false, result.Contains("MOA+40"));
			AssertEquals("Transport and Insurance", true, result.Contains("MOA+68"));
			AssertEquals("Dumping Export Price", true, result.Contains("MOA+290"));
			AssertEquals("Price", true, result.Contains("MOA+38"));
			AssertEquals("Price Adjustment", true, result.Contains("MOA+5"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			entryLine.CL_CustomsValue = 123.45M;
			imdMessageLineToTest = null;
			IMDMessageLineToTest.PopulateGroup33();
			result = Group30String;
			AssertEquals("Customs Value", true, result.Contains("MOA+40"));
			AssertEquals("Transport and Insurance", true, result.Contains("MOA+68"));
			AssertEquals("Dumping Export Price", true, result.Contains("MOA+290"));
			AssertEquals("Price - Nature 30", false, result.Contains("MOA+38"));
			AssertEquals("Price Adjustment", true, result.Contains("MOA+5"));
		}

		public void TestSegmentGroup33WithEmptyInvoicePrice()
		{
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceLine.JI_LinePrice = 0.00M;
			IMDMessageLineToTest.PopulateGroup33();
			ZString result = Group30String;
			AssertEquals("Price", true, result.Contains("MOA+38:0.00:AUD"));
		}

		public void TestSegmentGroup33WithOverridenDutyAndStandardDuty()
		{
			invoiceLine.JI_LinePrice = 1500.00M;
			invoiceLine.AddInfo.ZA_DTY = 140.50m;
			invoiceLine.AddInfo.ZA_STD = 200.50m;
			IMDMessageLineToTest.PopulateGroup33();
			ZString result = Group30String;
			AssertEquals("Standard duty populated", true, result.Contains("MOA+155:200.50"));
			AssertEquals("Duty overriden populated", true, result.Contains("MOA+55:140.50"));
		}

		public void TestRFFSegmentWithAllPosibleValues()
		{
			PopulateFieldsForSegmentGroup35();
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;

			AssertEquals("AQIS Line Container", true, result.Contains("RFF+AAQ:OCLU10000042'"));
			AssertEquals("Line Valuation Advice Number", true, result.Contains("RFF+ABA:123'"));
			AssertEquals("Dumping Specification Number", true, result.Contains("RFF+ABC:DSN'"));
			AssertEquals("Tariff Classification Number", true, result.Contains("RFF+ABD:20033040'"));
			AssertEquals("Lixury Car Tax Expemption Code", true, result.Contains("RFF+ABI:LCTE'"));
			AssertEquals("Security Identifier", true, result.Contains("RFF+ABL:SCN'"));
			AssertEquals("AQIS Commodity Code", true, result.Contains("RFF+ABU:COM1'RFF+ABU:COM2'"));
			AssertEquals("Multiple Clearance Code", true, result.Contains("RFF+ACD:WMC'"));
			AssertEquals("Second Treatment Code", true, result.Contains("RFF+ACE:SSS'"));
			AssertEquals("Second Treatment code occurs only once", 1, result.Occurrences("RFF+ACE:SSS'"));
			AssertEquals("AQIS Document Number and Type", true, result.Contains("RFF+ACL:DOC NUMBER 1::DOCTYPE1'RFF+ACL:DOC NUMBER 2::DOCTYPE2'"));
			AssertEquals("ELAC Number with the first number", true, result.Contains("RFF+ADY:11111111'"));
			AssertEquals("ELAC number occurrence with the first number", 1, result.Occurrences("RFF+ADY:11111111'"));
			AssertEquals("ELAC Number with the second number", true, result.Contains("RFF+ADY:22222222'"));
			AssertEquals("ELAC number occurrence with the second number", 1, result.Occurrences("RFF+ADY:22222222'"));
			AssertEquals("Tariff Classification Instrument Number and Type", true, result.Contains("RFF+AEA:12345678::TTT'"));
			AssertEquals("Tariff Classification Instrument Occurrence", 1, result.Occurrences("RFF+AEA:12345678::TTT'"));
			AssertEquals("Statistical Code", true, result.Contains("RFF+AED:05'"));
			AssertEquals("Treatment Instrument Number and Type", true, result.Contains("RFF+AES:CODE::INT'"));
			AssertEquals("Treatment Code", true, result.Contains("RFF+AFD:TRE'"));
			AssertEquals("Second Tariff Classification Number", true, result.Contains("RFF+AFG:00000000'"));
			AssertEquals("Second treatment instrument number and code", true, result.Contains("RFF+AFM:22223333::III'"));
			AssertEquals("Second treatment instrument Occurrence", 1, result.Occurrences("RFF+AFM:22223333::III'"));
			AssertEquals("Valudation Basis Type", true, result.Contains("RFF+AFV:TV'"));
			AssertEquals("Preference Scheme Type", true, result.Contains("RFF+AGW:PST'"));
			AssertEquals("Treatment Code Rate Number", true, result.Contains("RFF+AHX:TRN'"));
			AssertEquals("Preference instrument number and type", true, result.Contains("RFF+AIP:33334444::PPP'"));
			AssertEquals("Preference instrument number and type", 1, result.Occurrences("RFF+AIP:33334444::PPP'"));
			AssertEquals("Instrument Security Code", true, result.Contains("RFF+AJY:ISC'"));
			AssertEquals("Vehicle ID First Number", true, result.Contains("RFF+AKG:55555555'"));
			AssertEquals("Vehicle ID First Number Occurrences", 1, result.Occurrences("RFF+AKG:55555555'"));
			AssertEquals("Vehicle ID Second Number", true, result.Contains("RFF+AKG:66666666'"));
			AssertEquals("Vehicle ID Second Number Occurrences", 1, result.Occurrences("RFF+AKG:66666666'"));
			AssertEquals("Dumping Exemption Type", true, result.Contains("RFF+AKO:COUNTRY'"));
			AssertEquals("Tariff Advice Number", true, result.Contains("RFF+AKZ:TAN'"));
			AssertEquals("Preference Rule Type", true, result.Contains("RFF+ANG:PRT'"));
			AssertEquals("AQIS Permit Number", true, result.Contains("RFF+ANJ:PERMIT ONE'RFF+ANJ:PERMIT TWO'"));
			AssertEquals("GST Exemption Code", true, result.Contains("RFF+ASA:GSTE"));
			AssertEquals("Tariff Classification Rate Number", true, result.Contains("RFF+AWA:RNO'"));
			AssertEquals("Wine Equalisation Tax Exemption Code", true, result.Contains("RFF+DA:WETE'"));
			AssertEquals("Import Credit Number", true, result.Contains("RFF+IP:ICN'"));
			AssertEquals("Warehouse Reference Line Number", true, result.Contains("RFF+WE:WRN'"));
			AssertEquals("Warehouse Reference Declaration ID", true, result.Contains("RFF+LI:123'"));
		}

		public void TestPSTAndPRTInRFFSegment()
		{
			invoiceHeader.AddInfo.ZA_PST = "PST";
			invoiceHeader.AddInfo.ZA_PRT = "PRT";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("Preference Rule Type", true, result.Contains("RFF+ANG:PRT'"));
			AssertEquals("Preference Scheme Type", true, result.Contains("RFF+AGW:PST'"));

			invoiceLine.AddInfo.ZA_PST = "PSI";
			invoiceLine.AddInfo.ZA_PRT = "PRI";
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("Preference Rule Type", true, result.Contains("RFF+ANG:PRI'"));
			AssertEquals("Preference Scheme Type", true, result.Contains("RFF+AGW:PSI'"));
		}

		public void TestRFFSegmentDoesNotContainPRTForNature20()
		{
			invoiceLine.JI_IsPackToBondForLine = ZBool.True;
			Assert("Preassertion: Invoice line is Nature 20", invoiceLine.IsNature20);
			invoiceLine.AddInfo.ZA_PRT = "PRT";
			IMDMessageLineToTest.PopulateGroup35();
			bool result = false;
			foreach (RFFSegment rFF in IMDMessageLineToTest.Group35.RFF)
			{
				if (rFF.Reference.ReferenceFunctionCodeQualifier.ToString() == ReferenceFunctionCodeQualifierList.TechnicalRegulation)
				{
					result = true;
				}
			}
			AssertEquals("RFF+ANG segment should not have been generated for a Nature 20 line", false, result);
		}

		public void TestVANInRFFSegment()
		{
			invoiceHeader.AddInfo.ZA_VAN = "VAN";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("Line Valuation Advice Number", true, result.Contains("RFF+ABA:VAN'"));

			invoiceLine.AddInfo.ZA_VAN = "LINE";
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("Line Valuation Advice Number", true, result.Contains("RFF+ABA:LINE'"));
		}

		public void TestSCNInRFFSegment()
		{
			invoiceHeader.AddInfo.ZA_SCN = "SCN";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("Security Number", true, result.Contains("RFF+ABL:SCN'"));

			invoiceLine.AddInfo.ZA_SCN = "LSN";
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("Security Number", true, result.Contains("RFF+ABL:LSN'"));
		}

		public void TestWRLInRFFSegment()
		{
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("WRL", false, result.Contains("RFF+WE'"));

			invoiceHeader.AddInfo.ZA_WRL = 123;
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("WRL", true, result.Contains("RFF+LI:123'"));

			invoiceLine.AddInfo.ZA_WRL = 586;
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("WRL", true, result.Contains("RFF+LI:586'"));
		}

		public void TestWRNInRFFSegment()
		{
			invoiceHeader.AddInfo.ZA_WRN = "WRN";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("WRN", true, result.Contains("RFF+WE:WRN'"));

			invoiceLine.AddInfo.ZA_WRN = "LINE";
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("WRN", true, result.Contains("RFF+WE:LINE'"));
		}

		public void TestValuationBasisInRFFSegment()
		{
			invoiceHeader.AddInfo.ZA_VALB_Hidden = "TV";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("Valuation Basis", true, result.Contains("RFF+AFV:TV'"));

			invoiceLine.AddInfo.ZA_VALB_Hidden = "BB";
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("Valuation Basis", true, result.Contains("RFF+AFV:BB'"));
		}

		public void TestPopulatePreferenceOriginRuleWithGenerateRate()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			AssertEquals("IsGeneralRate", true, invoiceLine.IsGeneralRate);

			invoiceLine.AddInfo.ZA_POC = "NP";
			invoiceLine.AddInfo.ZA_PRT = "P50";

			IMDMessageLine messageBuilder = new IMDMessageLine(entryLine, new SegmentGroup30(), false, true);
			messageBuilder.PopulateLOC();//POC
			messageBuilder.PopulateGroup35();//PRT

			ZString messageResult = Group30String;
			AssertEquals("Message generated should not have POC as it is GEN", false, messageResult.Contains("LOC+30+NP::5"));
			AssertEquals("Message generated should not have PRT as it is GEN", false, messageResult.Contains("RFF+ANG:P50"));

			CMRTariffRatePeriodSnapshot tariffRateWithoutPreferentialRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRateWithoutPreferentialRate.TT_TariffClassificationNumber = "00000000";
			tariffRateWithoutPreferentialRate.TT_StartDate = new ZDateTime(2005, 10, 25);
			tariffRateWithoutPreferentialRate.TT_PreferenceSchemeType = "GEN";

			invoiceLine.JI_Tariff = "00000000 00";
			invoiceLine.AddInfo.ZA_PST = "";
			invoice.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			AssertEquals("IsGeneralRate", true, invoiceLine.IsGeneralRate);
			messageBuilder = new IMDMessageLine(entryLine, new SegmentGroup30(), false, true);
			messageBuilder.PopulateLOC();//POC
			messageBuilder.PopulateGroup35();//PRT
			messageResult = messageBuilder.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Message generated should not have POC as it is GEN", false, messageResult.Contains("LOC+30+NP::5"));
			AssertEquals("Message generated should not have PRT as it is GEN", false, messageResult.Contains("RFF+ANG:P50"));

			CMRTariffRatePeriodSnapshot tariffRateWithPreferentialRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRateWithPreferentialRate.TT_TariffClassificationNumber = "00000001";
			tariffRateWithPreferentialRate.TT_StartDate = new ZDateTime(2005, 10, 25);
			tariffRateWithPreferentialRate.TT_PreferenceSchemeType = "DC";

			invoiceLine.JI_Tariff = "00000001";
			invoice.AddInfo.ZA_PST = "DC";
			invoiceLine.AddInfo.ZA_POC = "NP";
			invoiceLine.AddInfo.ZA_PRT = "P50";
			AssertEquals("IsGeneralRate", false, invoiceLine.IsGeneralRate);
			messageBuilder = new IMDMessageLine(entryLine, new SegmentGroup30(), false, true);
			messageBuilder.PopulateLOC();//POC
			messageBuilder.PopulateGroup35();//PRT
			messageResult = messageBuilder.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Message generated should have POC", true, messageResult.Contains("LOC+30+NP::5"));
			AssertEquals("Message generated should have PRT", true, messageResult.Contains("RFF+ANG:P50"));
			AssertEquals("Message generated should have PRT", true, messageResult.Contains("RFF+AGW:DC"));
		}

		public void TestVIDNumberForMoreThanOneInvoiceLines()
		{
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			invoiceLine2.JI_Description = "Description of the goods for line 1";
			invoiceLine2.JI_CountryOfOrigin = "JP";
			invoiceLine2.JI_LinePrice = 997.65M;
			invoiceLine2.JI_Tariff = "4410.90.00 25";
			invoiceLine2.JI_CustomsQuantity = 5.00M;
			invoiceLine2.AddInfo.ZA_VALB_Hidden = "UT";
			invoiceLine2.AddInfo.AdjustmentDollarPercentage_Hidden = "$";
			invoiceLine2.AddInfo.AdjustmentAmount_Hidden = 456.78M;
			invoiceLine2.AddInfo.AdjustmentCurrency_Hidden = "USD";

			invoiceLine.AddInfo.ZA_VID = "11111111, 22222222";
			invoiceLine2.AddInfo.ZA_VID = "33333333, 44444444";

			invoiceLine2.JI_CL = invoiceLine.JI_CL;//Merged into one CusEntryLine
			invoiceLine2.CusEntryLine.RefreshInvoiceLines();
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;

			AssertEquals("VID Number with the first number", true, result.Contains("RFF+AKG:11111111'"));
			AssertEquals("VID Number with the second number", true, result.Contains("RFF+AKG:22222222'"));
			AssertEquals("VID Number with the third number", true, result.Contains("RFF+AKG:33333333'"));
			AssertEquals("VID Number with the fourth number", true, result.Contains("RFF+AKG:44444444'"));
		}

		public void TestOrderVIDNumbers()
		{
			invoiceLine.AddInfo.ZA_VID = "22222222, 11111111";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;

			AssertEquals("VID Numbers should be ordered before used in the message", true, result.Contains("RFF+AKG:11111111'RFF+AKG:22222222'"));
		}

		public void TestDumpingExemptionTypeMappingInRFFSegment()
		{
			invoiceLine.AddInfo.ZA_DXT = "C";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("Dumping Exemption Type", true, result.Contains("RFF+AKO:COUNTRY'"));

			invoiceLine.AddInfo.ZA_DXT = "S";
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("Dumping Exemption Type", true, result.Contains("RFF+AKO:SUPPLIER'"));

			invoiceLine.AddInfo.ZA_DXT = "G";
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("Dumping Exemption Type", true, result.Contains("RFF+AKO:GOODS'"));
		}

		public void TestValuationBasisForNature30()
		{
			invoiceLine.AddInfo.ZA_VALB_Hidden = "TV";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("Valudation Basis Type", true, result.Contains("RFF+AFV:TV'"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			imdMessageLineToTest = null;
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("Valudation Basis Type", false, result.Contains("RFF+AFV:TV'"));
		}

		public void TestRefundReasonCodeForAmendment()
		{
			entryLine.RefundReasonCode = "12345";

			SegmentGroup30 group30 = new SegmentGroup30();

			imdMessageLineToTest = new IMDMessageLine(entryLine, group30, false, false);
			imdMessageLineToTest.Group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();

			IMDMessageLineToTest.PopulateRefundReasonCode(LineAction.Insert);
			ZString result = Group30String;
			AssertEquals("Refund Reason Code", false, result.Contains("RFF+ABE:12345'"));

			group30 = new SegmentGroup30();
			IMDMessageLine testLine = new IMDMessageLine(entryLine, group30, false, true);
			testLine.Group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();
			testLine.PopulateRefundReasonCode(LineAction.Insert);
			result = testLine.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Refund Reason Code", true, result.Contains("RFF+ABE:12345'"));
		}

		public void TestRefundReasonCodeForAmendmentHeaderAndLine()
		{
			entryLine.Header.RefundReasonCode = "XXXX";
			entryLine.RefundReasonCode = "1234";

			SegmentGroup30 group30 = new SegmentGroup30();

			imdMessageLineToTest = new IMDMessageLine(entryLine, group30, false, false);
			imdMessageLineToTest.Group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();

			IMDMessageLineToTest.PopulateRefundReasonCode(LineAction.Insert);
			ZString result = Group30String;
			AssertEquals("Refund Reason Code", false, result.Contains("RFF+ABE:1234'"));

			group30 = new SegmentGroup30();
			IMDMessageLine testLine = new IMDMessageLine(entryLine, group30, false, true);
			testLine.Group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();
			testLine.PopulateRefundReasonCode(LineAction.Insert);
			result = testLine.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Refund Reason Code", true, result.Contains("RFF+ABE:1234'"));
		}

		public void TestRefundReasonCodeForAmendmentHeaderOnly()
		{
			entryLine.Header.RefundReasonCode = "XXXX";

			SegmentGroup30 group30 = new SegmentGroup30();

			imdMessageLineToTest = new IMDMessageLine(entryLine, group30, false, false);
			imdMessageLineToTest.Group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();

			IMDMessageLineToTest.PopulateRefundReasonCode(LineAction.Insert);
			ZString result = Group30String;
			AssertEquals("Refund Reason Code", false, result.Contains("RFF+ABE:XXXX'"));

			group30 = new SegmentGroup30();
			IMDMessageLine testLine = new IMDMessageLine(entryLine, group30, false, true);
			testLine.Group35 = group30.Group35.InstantiateAChildAndAddItToChildrenCollection();
			testLine.PopulateRefundReasonCode(LineAction.Insert);
			result = testLine.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Refund Reason Code", true, result.Contains("RFF+ABE:XXXX'"));
		}

		public void TestAQISCommodityCode()
		{
			invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden = "COM1,COM2,COM3,COM4,COM5,COM6,COM7,COM8,COM9,COM0";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			ZString expectedResult = "RFF+ABU:COM0'RFF+ABU:COM1'RFF+ABU:COM2'RFF+ABU:COM3'RFF+ABU:COM4'RFF+ABU:COM5'RFF+ABU:COM6'RFF+ABU:COM7'RFF+ABU:COM8'RFF+ABU:COM9'";
			AssertEquals("AQIS Commodity Codes built in the message after sorted", true, result.Contains(expectedResult));
		}

		public void TestAQISPermitNumber()
		{
			invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "ONE,TWO,THREE";
			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			ZString expectedResult = "RFF+ANJ:ONE'RFF+ANJ:THREE'RFF+ANJ:TWO'";
			AssertEquals("AQIS Permit Number", true, result.Contains(expectedResult));
		}

		public void TestAQISDocumentNumber()
		{
			AQISDocument document1 = invoiceLine.AQISDocuments.AddNew();
			document1.Type = "DOCTYPE1";
			document1.Number = "DOC NUMBER 1";

			AQISDocument document2 = invoiceLine.AQISDocuments.AddNew();
			document2.Type = "DOCTYPE2";
			document2.Number = "DOC NUMBER 2";

			AQISDocument document3 = invoiceLine.AQISDocuments.AddNew();
			document3.Type = "DOCTYPE3";
			document3.Number = "DOC NUMBER 3";

			AQISDocument document4 = invoiceLine.AQISDocuments.AddNew();
			document4.Type = "DOCTYPE4";
			document4.Number = "DOC NUMBER 4";

			AQISDocument document5 = invoiceLine.AQISDocuments.AddNew();
			document5.Type = "DOCTYPE5";
			document5.Number = "DOC NUMBER 5";

			AQISDocument document6 = invoiceLine.AQISDocuments.AddNew();
			document6.Type = "DOCTYPE6";
			document6.Number = "DOC NUMBER 6";

			AQISDocument document7 = invoiceLine.AQISDocuments.AddNew();
			document7.Type = "DOCTYPE7";
			document7.Number = "DOC NUMBER 7";

			AQISDocument document8 = invoiceLine.AQISDocuments.AddNew();
			document8.Type = "DOCTYPE8";
			document8.Number = "DOC NUMBER 8";

			AQISDocument document9 = invoiceLine.AQISDocuments.AddNew();
			document9.Type = "DOCTYPE9";
			document9.Number = "DOC NUMBER 9";

			AQISDocument document10 = invoiceLine.AQISDocuments.AddNew();
			document10.Type = "DOCTYPE0";
			document10.Number = "DOC NUMBER 0";

			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;

			ZString expectedResult = "RFF+ACL:DOC NUMBER 0::DOCTYPE0'"
				+ "RFF+ACL:DOC NUMBER 1::DOCTYPE1'"
				+ "RFF+ACL:DOC NUMBER 2::DOCTYPE2'"
				+ "RFF+ACL:DOC NUMBER 3::DOCTYPE3'"
				+ "RFF+ACL:DOC NUMBER 4::DOCTYPE4'"
				+ "RFF+ACL:DOC NUMBER 5::DOCTYPE5'"
				+ "RFF+ACL:DOC NUMBER 6::DOCTYPE6'"
				+ "RFF+ACL:DOC NUMBER 7::DOCTYPE7'"
				+ "RFF+ACL:DOC NUMBER 8::DOCTYPE8'"
				+ "RFF+ACL:DOC NUMBER 9::DOCTYPE9'";

			AssertEquals("AQIS Document Numbers built in the message after sorted", true, result.Contains(expectedResult));
		}

		public void TestSCNForPreLodge()
		{
			invoiceLine.AddInfo.ZA_SCN = "SCN";
			IMDMessageLine messageLine = new IMDMessageLine(entryLine, new SegmentGroup30(), true, true);
			messageLine.PopulateGroup35();
			ZString result = messageLine.Group30.Group35.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Security Identifier", false, result.Contains("RFF+ABL:SCN'"));

			messageLine = new IMDMessageLine(entryLine, new SegmentGroup30(), false, true);
			messageLine.PopulateGroup35();
			result = messageLine.Group30.Group35.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Security Identifier", true, result.Contains("RFF+ABL:SCN'"));
		}

		public void TestSortElacNumbersBeforeUsingInMessages()
		{
			invoiceLine.AddInfo.ZA_ELA = "22222, 11111";
			IMDMessageLine messageLine = new IMDMessageLine(entryLine, new SegmentGroup30(), true, true);
			messageLine.PopulateGroup35();
			ZString result = messageLine.Group30.Group35.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Elac numbers are ordered before used in the messages", true, result.Contains("RFF+ADY:11111'RFF+ADY:22222'"));
		}

		public void TestAQISContainerNumbers()
		{
			AddContainersAndPacksToDeclaration();

			IMDMessageLineToTest.PopulateGroup35();
			ZString result = Group30String;
			AssertEquals("No AQIS containers", false, result.Contains("RFF+AAQ"));

			AddContainerToInvoiceLine(invoiceLine, "OCLU10000020");
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("AQIS containers", true, result.Contains("RFF+AAQ:OCLU10000020'"));

			AddContainerToInvoiceLine(invoiceLine, "OCLU10000031");
			IMDMessageLineToTest.PopulateGroup35();
			result = Group30String;
			AssertEquals("AQIS containers", true, result.Contains("RFF+AAQ:OCLU10000020'"));
			AssertEquals("AQIS containers", true, result.Contains("RFF+AAQ:OCLU10000031'"));

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_Description = "Description of the goods for line 1";
			invoiceLine2.JI_CountryOfOrigin = "JP";
			invoiceLine2.JI_LinePrice = 997.65M;
			invoiceLine2.JI_Tariff = "4410.90.00 25";
			invoiceLine2.JI_CustomsQuantity = 5.00M;
			invoiceLine2.AddInfo.ZA_VALB_Hidden = "UT";
			invoiceLine2.AddInfo.AdjustmentDollarPercentage_Hidden = "$";
			invoiceLine2.AddInfo.AdjustmentAmount_Hidden = 456.78M;
			invoiceLine2.AddInfo.AdjustmentCurrency_Hidden = "USD";
			AddContainerToInvoiceLine(invoiceLine2, "OCLU10000031");
			AddContainerToInvoiceLine(invoiceLine2, "OCLU10000042");
			testDec.DoMerge();

			IMDMessageLine newLineToTest = new IMDMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[1], new SegmentGroup30(), false, true);
			newLineToTest.PopulateGroup35();
			result = newLineToTest.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("AQIS containers", true, result.Contains("RFF+AAQ:OCLU10000042'"));
			AssertEquals("AQIS containers", true, result.Contains("RFF+AAQ:OCLU10000031'"));
			AssertEquals("AQIS containers", false, result.Contains("RFF+AAQ:OCLU10000020'"));
		}

		public void TestAQISContainerNumbersWhenIsAQISAEPLine()
		{
			AddContainersAndPacksToDeclaration();
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_IsNonAQISAEPLine_Hidden = true;
			AddContainerToInvoiceLine(invoiceLine, "OCLU10000020");
			AddContainerToInvoiceLine(invoiceLine, "OCLU10000031");
			AddContainerToInvoiceLine(invoiceLine2, "OCLU10000031");
			AddContainerToInvoiceLine(invoiceLine2, "OCLU10000042");
			testDec.DoMerge();

			IMDMessageLine newLineToTest = new IMDMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[1], new SegmentGroup30(), false, true);
			newLineToTest.PopulateGroup35();
			string result = newLineToTest.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("AQIS containers for invoice line 2 where IsNonAQISAEPLine = false", true, result.Contains("RFF+AAQ:OCLU10000042'"));
			AssertEquals("AQIS containers for invoice line 2 where IsNonAQISAEPLine = false", true, result.Contains("RFF+AAQ:OCLU10000031'"));

			newLineToTest = new IMDMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[0], new SegmentGroup30(), false, true);
			newLineToTest.PopulateGroup35();
			result = newLineToTest.Group30.ToString(new UNOCCMRCharacterSet());
			AssertEquals("No AQIS containers for invoice line 1 where IsNonAQISAEPLine = true", false, result.Contains("RFF+AAQ"));
		}

		public void TestSegmentGroup37()
		{
			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("Segment37", false, result.Contains("DOC+1'LOC+90+"));

			AQISPremisesIdAndProcessingType pAndPBizObj1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			pAndPBizObj1.PremisesId = "Test ID";
			pAndPBizObj1.ProcessingType = "RPT";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'"));

			AQISPremisesIdAndProcessingType pAndPBizObj2 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			pAndPBizObj2.ProcessingType = "RPT";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'DOC+1'LOC+90+::194:RPT'"));

			pAndPBizObj2.PremisesId = "Test ID 2";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'DOC+1'LOC+90+TEST ID 2::194:RPT'"));

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "PROD1";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'NAD+GD+PROD1::194'DOC+1'LOC+90+TEST ID 2::194:RPT'"));

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "PROD1,PROD2,PROD3";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'NAD+GD+PROD1::194'DOC+1'LOC+90+TEST ID 2::194:RPT'NAD+GD+PROD2::194'DOC+1'NAD+GD+PROD3::194'"));

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "";
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "ENTITYID1";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'NAD+MT+ENTITYID1::194'DOC+1'LOC+90+TEST ID 2::194:RPT'"));

			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "ENTITYID1,ENTITYID2,ENTITYID3";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'NAD+MT+ENTITYID1::194'DOC+1'LOC+90+TEST ID 2::194:RPT'NAD+MT+ENTITYID2::194'DOC+1'NAD+MT+ENTITYID3::194'"));

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "PROD1,PROD2,PROD3";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'NAD+GD+PROD1::194'NAD+MT+ENTITYID1::194'DOC+1'LOC+90+TEST ID 2::194:RPT'NAD+GD+PROD2::194'NAD+MT+ENTITYID2::194'DOC+1'NAD+GD+PROD3::194'NAD+MT+ENTITYID3::194'"));
		}

		public void TestSegmentGroup37ForN30()
		{
			// set up entry as N30 type
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Assert("IsNature30", invoiceLine.Declaration.IsNature30);
			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("Segment37", false, result.Contains("DOC+1'LOC+90+"));

			AQISPremisesIdAndProcessingType pAndPBizObj1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			pAndPBizObj1.PremisesId = "Test ID";
			pAndPBizObj1.ProcessingType = "RPT";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'"));

			AQISPremisesIdAndProcessingType pAndPBizObj2 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			pAndPBizObj2.ProcessingType = "RPT";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'DOC+1'LOC+90+::194:RPT'"));

			pAndPBizObj2.PremisesId = "Test ID 2";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'DOC+1'LOC+90+TEST ID 2::194:RPT'"));

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "PROD1";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'NAD+GD+PROD1::194'DOC+1'LOC+90+TEST ID 2::194:RPT'"));

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "PROD1,PROD2,PROD3";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("Segment37", true, result.Contains("DOC+1'LOC+90+TEST ID::194:RPT'NAD+GD+PROD1::194'DOC+1'LOC+90+TEST ID 2::194:RPT'NAD+GD+PROD2::194'DOC+1'NAD+GD+PROD3::194'"));
		}

		public void TestSegmentGroup37ForAQISProcessingType()
		{
			AQISPremisesIdAndProcessingType processingType1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType1.ProcessingType = "RPT";

			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("AQIS Processing Type", true, result.Contains("DOC+1'LOC+90+::194:RPT'"));

			AQISPremisesIdAndProcessingType processingType2 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType2.ProcessingType = "RPT";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("AQIS Processing Type", true, result.Contains("DOC+1'LOC+90+::194:RPT'DOC+1'LOC+90+::194:RPT'"));
		}

		public void TestSG37ForAQISProcessingTypeStillOutputsForN30()
		{
			// set up entry as N30 type
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Assert("IsNature30", invoiceLine.Declaration.IsNature30);
			AQISPremisesIdAndProcessingType processingType1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType1.ProcessingType = "RPT";

			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("AQIS Processing Type", true, result.Contains("DOC+1'LOC+90+::194:RPT'"));

			AQISPremisesIdAndProcessingType processingType2 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType2.ProcessingType = "RPT";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("AQIS Processing Type", true, result.Contains("DOC+1'LOC+90+::194:RPT'DOC+1'LOC+90+::194:RPT'"));
		}

		public void TestSegmentGroup37ForAQISPremisesID()
		{
			AQISPremisesIdAndProcessingType premisesId1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId1.PremisesId = "RPT";

			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("AQISPremisesID", true, result.Contains("DOC+1'LOC+90+RPT::194'"));

			AQISPremisesIdAndProcessingType premisesId2 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId2.PremisesId = "RPT";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("AQISPremisesID", true, result.Contains("DOC+1'LOC+90+RPT::194'DOC+1'LOC+90+RPT::194'"));
		}

		public void TestSegmentGroup37ForAQISProducerCode()
		{
			AQISProducerCode producerCode = invoiceLine.AQISProducerCodes.AddNew();
			producerCode.Code = "PROD";

			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("AQISProducerCode", true, result.Contains("DOC+1'NAD+GD+PROD::194'"));

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "PROD1,PROD2";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("AQISProducerCode", true, result.Contains("DOC+1'NAD+GD+PROD1::194'DOC+1'NAD+GD+PROD2::194'"));
		}

		public void TestSegmentGroup37ForAQISEntityID()
		{
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "EntityID1";

			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("AQISEntityCode", true, result.Contains("DOC+1'NAD+MT+ENTITYID1::194'"));

			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "EntityID1,EntityID2";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("AQISEntityCode", true, result.Contains("DOC+1'NAD+MT+ENTITYID1::194'DOC+1'NAD+MT+ENTITYID2::194'"));
		}

		public void TestSegmentGroup37ForAQISEntityIDForN30()
		{
			// set up entry as N30 type
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Assert("Pre-condition: IsNature30", invoiceLine.Declaration.IsNature30);
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "EntityID1";

			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("AQISEntityCode SHOULD NOT be transmitted in a N30 entry", false, result.Contains("DOC+1'NAD+MT+ENTITYID1::194'")); // 3. This element is not allowed when the Line Nature Type is N30.

			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "EntityID1,EntityID2";
			IMDMessageLineToTest.PopulateGroup37();
			result = Group30String;
			AssertEquals("AQISEntityCode SHOULD NOT be transmitted in a N30 entry", false, result.Contains("DOC+1'NAD+MT+ENTITYID1::194'DOC+1'NAD+MT+ENTITYID2::194'"));
		}

		public void TestSegmentGroup37ForControlErrorBug()
		{
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "202750";

			AQISPremisesIdAndProcessingType processingType1 = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType1.ProcessingType = "D";

			IMDMessageLineToTest.PopulateGroup37();
			ZString result = Group30String;
			AssertEquals("Group37", true, result.Contains("DOC+1'LOC+90+::194:D'NAD+MT+202750::194'"));
		}

		public void TestSegmentGroup40()
		{
			IMDMessageLineToTest.PopulateGroup40();
			ZString result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Group 40", false, result.Contains("GIS+"));

			invoiceLine.AddInfo.ZA_WETQ = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Wine Equalisation Tax Quote", true, result.Contains("GIS+WET:109:95'"));

			invoiceLine.AddInfo.ZA_LCTQ = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Wine Equalisation Tax Quote", true, result.Contains("GIS+WET:109:95'"));
			AssertEquals("Luxury Car Tax Quote", true, result.Contains("GIS+LCQ:109:95'"));

			invoiceLine.AddInfo.ZA_MLPI = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Wine Equalisation Tax Quote", true, result.Contains("GIS+WET:109:95'"));
			AssertEquals("Luxury Car Tax Quote", true, result.Contains("GIS+LCQ:109:95'"));
			AssertEquals("Manual Line Processing", true, result.Contains("GIS+MLP:109:95'"));

			invoiceLine.AddInfo.ZA_LCTI = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Wine Equalisation Tax Quote", true, result.Contains("GIS+WET:109:95'"));
			AssertEquals("Luxury Car Tax Quote", true, result.Contains("GIS+LCQ:109:95'"));
			AssertEquals("Manual Line Processing", true, result.Contains("GIS+MLP:109:95'"));
			AssertEquals("Luxury Car Tax Payable", true, result.Contains("GIS+LCT:109:95'"));

			invoiceLine.AddInfo.ZA_PUP = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Wine Equalisation Tax Quote", true, result.Contains("GIS+WET:109:95'"));
			AssertEquals("Luxury Car Tax Quote", true, result.Contains("GIS+LCQ:109:95'"));
			AssertEquals("Manual Line Processing", true, result.Contains("GIS+MLP:109:95'"));
			AssertEquals("Luxury Car Tax Payable", true, result.Contains("GIS+LCT:109:95'"));
			AssertEquals("Paid Under Protest", true, result.Contains("GIS+PUP:109:95'"));

			invoiceLine.AddInfo.ZA_REL_Hidden = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Wine Equalisation Tax Quote", true, result.Contains("GIS+WET:109:95'"));
			AssertEquals("Luxury Car Tax Quote", true, result.Contains("GIS+LCQ:109:95'"));
			AssertEquals("Manual Line Processing", true, result.Contains("GIS+MLP:109:95'"));
			AssertEquals("Luxury Car Tax Payable", true, result.Contains("GIS+LCT:109:95'"));
			AssertEquals("Paid Under Protest", true, result.Contains("GIS+PUP:109:95'"));
			AssertEquals("Related Transaction", true, result.Contains("GIS+REL:109:95'"));
		}

		public void TestRELInGroup40()
		{
			invoiceLine.AddInfo.ZA_REL_Hidden = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			ZString result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Related Transaction", true, result.Contains("GIS+REL:109:95'"));

			invoiceLine.AddInfo.ZA_REL_Hidden = "N";
			imdMessageLineToTest.Group30 = new SegmentGroup30();
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Related Transaction", false, result.Contains("GIS+REL:109:95'"));

			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.ZA_REL_Hidden = "Y";
			IMDMessageLineToTest.PopulateGroup40();
			result = IMDMessageLineToTest.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("No Related Transaction for N30", ZString.Empty, result);
		}

		public void TestSegmentGroup40ForPreLodge()
		{
			invoiceLine.AddInfo.ZA_SEC = "Y";
			IMDMessageLine messageLine = new IMDMessageLine(entryLine, new SegmentGroup30(), true, true);
			messageLine.PopulateGroup40();
			ZString result = messageLine.Group30.Group40.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Security Calculate", true, result.Contains("GIS+SEC:109:95'"));
		}

		public void TestSegmentGroup40ForPreLodgeInNature20()
		{
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.AddInfo.ZA_SEC = "Y";

			AssertEquals("Is Nature 20", CusEntryHeader.NatureTypesForImportCMR.Nature20, entryLine.Header.Nature);

			IMDMessageLine messageLine = new IMDMessageLine(entryLine, new SegmentGroup30(), true, true);
			messageLine.PopulateGroup40();
			ZString result = Group30String;
			AssertEquals("Security Calculate", false, result.Contains("GIS+SEC:109:95'"));
		}

		public void TestSegmentGroup41()
		{
			IMDMessageLineToTest.PopulateGroup41();
			ZString result = Group30String;
			AssertEquals("Group 41 is empty", false, result.Contains("TAX+"));

			invoiceLine.AddInfo.ZA_ODF = 123.23M;
			IMDMessageLineToTest.PopulateGroup41();
			result = Group30String;
			AssertEquals("Other Duty Factor", true, result.Contains("TAX+9+CUD+++:::123.2300'"));

			invoiceLine.AddInfo.ZA_DRE = 0.5M;
			IMDMessageLineToTest.PopulateGroup41();
			result = Group30String;
			AssertEquals("Other Duty Factor", true, result.Contains("TAX+9+CUD+++:::123.2300'"));
			AssertEquals("Dumping Exchange Rate", true, result.Contains("TAX+1+ADD+++:::0.5000'"));
		}

		protected override ZString Group30String => IMDMessageLineToTest.Group30.ToString(new UNOCCMRCharacterSet());

		protected override BaseMessageLine GetMessageLineToTest => new IMDMessageLine(entryLine, new SegmentGroup30(), false, false);

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
		}

		IMDMessageLine imdMessageLineToTest;
		IMDMessageLine IMDMessageLineToTest
		{
			get
			{
				if (imdMessageLineToTest == null)
				{
					imdMessageLineToTest = ((IMDMessageLine)messageLineToTest);
					imdMessageLineToTest.Group30 = new SegmentGroup30();
				}

				return imdMessageLineToTest;
			}
		}

		void PopulateFieldsForSegmentGroup35()
		{
			AddContainersAndPacksToDeclaration();

			invoiceLine.AddInfo.ZA_VAN = "123";
			invoiceLine.AddInfo.ZA_DSN = "DSN";
			invoiceLine.JI_Tariff = "2003.30.40 05";
			invoiceLine.AddInfo.ZA_LCTE = "LCTE";
			invoiceLine.AddInfo.ZA_SCN = "SCN";
			invoiceLine.AddInfo.ZA_WMC = "WMC";
			invoiceLine.AddInfo.ZA_InstrumentType_Hidden = "INT";
			invoiceLine.AddInfo.ZA_InstrumentCode_Hidden = "Code";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "TRE";
			invoiceLine.AddInfo.ZA_VALB_Hidden = "TV";
			invoiceLine.AddInfo.ZA_PST = "PST";
			invoiceLine.AddInfo.ZA_TRN = "TRN";
			invoiceLine.AddInfo.ZA_ISC = "ISC";
			invoiceLine.AddInfo.ZA_DXT = "C";
			invoiceLine.AddInfo.ZA_TAN = "TAN";
			invoiceLine.AddInfo.ZA_PRT = "PRT";
			invoiceLine.AddInfo.ZA_GSTE = "GSTE";
			invoiceLine.AddInfo.ZA_RNO = "RNO";
			invoiceLine.AddInfo.ZA_WETE = "WETE";
			invoiceLine.AddInfo.ZA_ICN = "ICN";
			invoiceLine.AddInfo.ZA_WRL = 123;
			invoiceLine.AddInfo.ZA_WRN = "WRN";
			invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "PERMIT ONE,PERMIT TWO";
			invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden = "COM1,COM2";

			AQISDocument document1 = invoiceLine.AQISDocuments.AddNew();
			document1.Type = "DOCTYPE1";
			document1.Number = "DOC NUMBER 1";

			AQISDocument document2 = invoiceLine.AQISDocuments.AddNew();
			document2.Type = "DOCTYPE2";
			document2.Number = "DOC NUMBER 2";

			AddContainerToInvoiceLine(invoiceLine, "OCLU10000042");
			invoiceLine.AddInfo.ZA_TR2 = "SSS";//Second Treatment
			invoiceLine.AddInfo.ZA_ELA = "11111111, 22222222";
			invoiceLine.AddInfo.ZA_TCI = "TTT:12345678";
			invoiceLine.AddInfo.ZA_CL2 = "0000.00.00";
			invoiceLine.AddInfo.ZA_TI2 = "III:22223333";
			invoiceLine.AddInfo.ZA_PRI = "PPP:33334444";
			invoiceLine.AddInfo.ZA_VID = "55555555, 66666666";
		}

		void AddContainerToInvoiceLine(JobComInvoiceLine invoiceLine, ZString containerNumber)
		{
			foreach (Customs.Business.NonPersistentCusContainer currentContainer in invoiceLine.ContainersForInvoiceLinesForBindingOnly)
			{
				if (currentContainer.ContainerNumber == containerNumber)
				{
					currentContainer.IsForInvoiceLine = ZBool.True;
				}
			}
		}

		void AddContainersAndPacksToDeclaration()
		{
			container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000020";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU10000031";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU10000042";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
		}

		CusContainer container1;
		CusContainer container2;
		CusContainer container3;
	}
}
