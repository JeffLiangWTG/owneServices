using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using D99BMessageTypeList = Enterprise.Edifact.D99B.Elements.MessageTypeList;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class D99BMessageUtilitiesPopulateDetailsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPopulateUNH()
		{
			var uNH = new UNHSegment();
			D99BMessageUtilities.PopulateUNH(uNH, "123456", D99BMessageTypeList.CustomsDeclarationMessage, "S", "99B", ControllingAgencyList.UnCefact);
			NUnit.Framework.Assert.That(uNH.ToString(characterSet), NUnit.Framework.Is.EqualTo("UNH+123456+CUSDEC:S:99B:UN'"), "UNH");
		}

		[ExpectNoExceptions]
		public void TestPopulateBGM()
		{
			var bGM = new BGMSegment();
			D99BMessageUtilities.PopulateBGM(bGM, "AA", "A12", MessageFunctionCodeList.Original);
			NUnit.Framework.Assert.That(bGM.ToString(characterSet), NUnit.Framework.Is.EqualTo("BGM+:::AA+A12+9'"), "BGM");

			bGM = new BGMSegment();
			D99BMessageUtilities.PopulateBGM(bGM, "AA", "A12", "1234", MessageFunctionCodeList.Original);
			NUnit.Framework.Assert.That(bGM.ToString(characterSet), NUnit.Framework.Is.EqualTo("BGM+:::AA+A12:1234+9'"), "BGM");

			bGM = new BGMSegment();
			D99BMessageUtilities.PopulateBGM(bGM, "AA", ZString.Empty, MessageFunctionCodeList.Original);
			NUnit.Framework.Assert.That(bGM.ToString(characterSet), NUnit.Framework.Is.EqualTo("BGM+:::AA++9'"), "BGM");
		}

		[ExpectNoExceptions]
		public void TestPopulateCST()
		{
			var cST = new CSTSegment();
			D99BMessageUtilities.PopulateCST(cST, "A");
			NUnit.Framework.Assert.That(cST.ToString(characterSet), NUnit.Framework.Is.EqualTo("CST++A'"), "CST");

			cST = new CSTSegment();
			D99BMessageUtilities.PopulateCST(cST, "257", "POS", "1", "2", "123", "10.10.10.10");
			NUnit.Framework.Assert.That(cST.ToString(characterSet), NUnit.Framework.Is.EqualTo("CST+257+POS+1+2+123+10.10.10.10'"), "CST");

			cST = new CSTSegment();
			D99BMessageUtilities.PopulateCST(cST, "257", "POS", "1", "2", "123", string.Empty);
			NUnit.Framework.Assert.That(cST.ToString(characterSet), NUnit.Framework.Is.EqualTo("CST+257+POS+1+2+123'"), "CST");

			cST = new CSTSegment();
			D99BMessageUtilities.PopulateCST(cST, ZString.Empty, "POS", "1", "2", "123", string.Empty);
			NUnit.Framework.Assert.That(cST.ToString(characterSet), NUnit.Framework.Is.EqualTo("CST++POS+1+2+123'"), "CST");
		}

		[ExpectNoExceptions]
		public void TestPopulateLOC()
		{
			var lOC = new LOCSegment();
			D99BMessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.CustomsOfficeOfEntry, "0497");
			NUnit.Framework.Assert.That(lOC.ToString(characterSet), NUnit.Framework.Is.EqualTo("LOC+41+0497'"), "LOC");

			lOC = new LOCSegment();
			D99BMessageUtilities.PopulateLOC(lOC, "VAR", "UNY", "MX");
			NUnit.Framework.Assert.That(lOC.ToString(characterSet), NUnit.Framework.Is.EqualTo("LOC+27+VAR+UNY+MX'"), "LOC");
		}

		[ExpectNoExceptions]
		public void TestPopulateRFF()
		{
			var rFF = new RFFSegment();
			D99BMessageUtilities.PopulateRFF(rFF, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber, "123456789");
			NUnit.Framework.Assert.That(rFF.ToString(characterSet), NUnit.Framework.Is.EqualTo("RFF+TN:123456789'"), "RFF");

			rFF = new RFFSegment();
			D99BMessageUtilities.PopulateRFF(rFF, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber, string.Empty, "123456789");
			NUnit.Framework.Assert.That(rFF.ToString(characterSet), NUnit.Framework.Is.EqualTo("RFF+TN::123456789'"), "RFF");

			rFF = new RFFSegment();
			D99BMessageUtilities.PopulateRFF(rFF, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber, "123456789", "987");
			NUnit.Framework.Assert.That(rFF.ToString(characterSet), NUnit.Framework.Is.EqualTo("RFF+TN:123456789:987'"), "RFF");
		}

		[ExpectNoExceptions]
		public void TestPopulateERC()
		{
			var erc = new ERCSegment();
			D99BMessageUtilities.PopulateERC(erc, "12345");
			NUnit.Framework.Assert.That(erc.ToString(characterSet), NUnit.Framework.Is.EqualTo("ERC+12345'"), "ERC");
		}

		[ExpectNoExceptions]
		public void TestPopulateTDT()
		{
			var tDT = new TDTSegment();
			D99BMessageUtilities.PopulateTDT(tDT, TransportStageCodeQualifierList.AtBorder, "AIR", "1234");
			NUnit.Framework.Assert.That(tDT.ToString(characterSet), NUnit.Framework.Is.EqualTo("TDT+11++AIR++1234'"), "RFF");
		}

		[ExpectNoExceptions]
		public void TestPopulateDOC()
		{
			var dOC = new DOCSegment();
			D99BMessageUtilities.PopulateDOC(dOC, DocumentNameCodeList.CargoManifest, "CCN123456789");
			NUnit.Framework.Assert.That(dOC.ToString(characterSet), NUnit.Framework.Is.EqualTo("DOC+785+CCN123456789'"), "DOC");

			dOC = new DOCSegment();
			D99BMessageUtilities.PopulateDOC(dOC, DocumentNameCodeList.CustomsInvoice, string.Empty);
			NUnit.Framework.Assert.That(dOC.ToString(characterSet), NUnit.Framework.Is.EqualTo("DOC+935'"), "DOC");

			dOC = new DOCSegment();
			D99BMessageUtilities.PopulateDOC(dOC, DocumentNameCodeList.PreviousCustomsDocumentMessage, "TRN12345", "12");
			NUnit.Framework.Assert.That(dOC.ToString(characterSet), NUnit.Framework.Is.EqualTo("DOC+998+TRN12345::12'"), "DOC");
		}

		[ExpectNoExceptions]
		public void TestPopulateDTM()
		{
			var dTM = new DTMSegment();
			D99BMessageUtilities.PopulateDTM(dTM, DateTimePeriodFunctionCodeQualifierList.ExportationDate, new ZDate(2009, 5, 21));
			NUnit.Framework.Assert.That(dTM.ToString(characterSet), NUnit.Framework.Is.EqualTo("DTM+129:20090521:102'"), "DTM");
		}

		[ExpectNoExceptions]
		public void TestPopulateGIS()
		{
			var gis = new GISSegment();
			D99BMessageUtilities.PopulateGIS(gis, ProcessingIndicatorDescriptionCodeList.ErrorMessage);
			NUnit.Framework.Assert.That(gis.ToString(characterSet), NUnit.Framework.Is.EqualTo("GIS+14'"), "GIS");
		}

		[ExpectNoExceptions]
		public void TestPopulateERP()
		{
			var erp = new ERPSegment();
			D99BMessageUtilities.PopulateERP(erp, MessageSectionCodedList.DetailSectionOfAMessage, "A", "B");
			NUnit.Framework.Assert.That(erp.ToString(characterSet), NUnit.Framework.Is.EqualTo("ERP+2:A:B'"), "ERP");

			erp = new ERPSegment();
			D99BMessageUtilities.PopulateERP(erp, null, "A", "B");
			NUnit.Framework.Assert.That(erp.ToString(characterSet), NUnit.Framework.Is.EqualTo("ERP+:A:B'"), "ERP");

			erp = new ERPSegment();
			D99BMessageUtilities.PopulateERP(erp, MessageSectionCodedList.CommercialHeadingSectionOfCusdec, "A", ZString.Empty);
			NUnit.Framework.Assert.That(erp.ToString(characterSet), NUnit.Framework.Is.EqualTo("ERP+8:A'"), "ERP");
		}

		[ExpectNoExceptions]
		public void TestPopulateFTX()
		{
			var ftx = new FTXSegment();
			D99BMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.AbsenceDeclaration, "FREE TEXT1", "FREE TEXT2");
			NUnit.Framework.Assert.That(ftx.ToString(characterSet), NUnit.Framework.Is.EqualTo("FTX+ACG+++FREE TEXT1:FREE TEXT2'"), "FTX");
		}

		[ExpectNoExceptions]
		public void TestPopulateMOA()
		{
			var mOA = new MOASegment();
			D99BMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeCodeQualifierList.AmountReferenceCurrency, "CAD");
			NUnit.Framework.Assert.That(mOA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MOA+6::CAD'"), "MOA");

			mOA = new MOASegment();
			D99BMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, 123.4m);
			NUnit.Framework.Assert.That(mOA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MOA+43:12340'"), "MOA");

			mOA = new MOASegment();
			D99BMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, 0.23556m);
			NUnit.Framework.Assert.That(mOA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MOA+43:024'"), "MOA");
		}

		[ExpectNoExceptions]
		public void TestPopulateMOARounded()
		{
			var mOA = new MOASegment();
			D99BMessageUtilities.PopulateMOARounded(mOA, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, 123.5m);
			NUnit.Framework.Assert.That(mOA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MOA+43:124'"), "MOA");

			mOA = new MOASegment();
			D99BMessageUtilities.PopulateMOARounded(mOA, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, 0.23556m);
			NUnit.Framework.Assert.That(mOA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MOA+43:0'"), "MOA");
		}

		[ExpectNoExceptions]
		public void TestPopulateUNS()
		{
			var uNS = new UNSSegment();
			D99BMessageUtilities.PopulateUNS(uNS, SectionIdentificationList.HeaderDetailSectionSeparation);
			NUnit.Framework.Assert.That(uNS.ToString(characterSet), NUnit.Framework.Is.EqualTo("UNS+D'"), "UNS");
		}

		[ExpectNoExceptions]
		public void TestPopulateDMS()
		{
			var dMS = new DMSSegment();
			D99BMessageUtilities.PopulateDMS(dMS, "INV987654321");
			NUnit.Framework.Assert.That(dMS.ToString(characterSet), NUnit.Framework.Is.EqualTo("DMS+INV987654321'"), "DMS");
		}

		[ExpectNoExceptions]
		public void TestPopulateNAD()
		{
			var vendor = Factory.New<JobDocAddress>();
			vendor.E2_CompanyName = "VENDOR NAME";
			var nAD = new NADSegment();
			D99BMessageUtilities.PopulateNAD(nAD, PartyFunctionCodeQualifierList.Seller, vendor, ZString.Empty, ZString.Empty);
			NUnit.Framework.Assert.That(nAD.ToString(characterSet), NUnit.Framework.Is.EqualTo("NAD+SE++VENDOR NAME'"), "NAD");

			nAD = new NADSegment();
			D99BMessageUtilities.PopulateNAD(nAD, PartyFunctionCodeQualifierList.Seller, vendor, "UCA", ZString.Empty);
			NUnit.Framework.Assert.That(nAD.ToString(characterSet), NUnit.Framework.Is.EqualTo("NAD+SE++VENDOR NAME++++UCA'"), "NAD");

			nAD = new NADSegment();
			D99BMessageUtilities.PopulateNAD(nAD, PartyFunctionCodeQualifierList.Seller, vendor, " UCAX", " 123456");
			NUnit.Framework.Assert.That(nAD.ToString(characterSet), NUnit.Framework.Is.EqualTo("NAD+SE++VENDOR NAME++++UCA+12345'"), "NAD");

			nAD = new NADSegment();
			D99BMessageUtilities.PopulateNAD(nAD, PartyFunctionCodeQualifierList.Seller, vendor, "PR", ZString.Empty);
			NUnit.Framework.Assert.That(nAD.ToString(characterSet), NUnit.Framework.Is.EqualTo("NAD+SE++VENDOR NAME++++PR '"), "NAD");

			vendor.E2_CompanyName = "1234567890123456789012345678901";
			nAD = new NADSegment();
			D99BMessageUtilities.PopulateNAD(nAD, PartyFunctionCodeQualifierList.Seller, vendor, ZString.Empty, ZString.Empty);
			NUnit.Framework.Assert.That(nAD.ToString(characterSet), NUnit.Framework.Is.EqualTo("NAD+SE++123456789012345678901234567890'"), "NAD");
		}

		[ExpectNoExceptions]
		public void TestPopulatePAT()
		{
			var pAT = new PATSegment();
			D99BMessageUtilities.PopulatePAT(pAT, PaymentTermsTypeCodeQualifierList.Basic, "CONSIGN", "12", TimeReferenceCodeList.SpecifiedDate, PeriodTypeCodeList.Day, "23");
			NUnit.Framework.Assert.That(pAT.ToString(characterSet), NUnit.Framework.Is.EqualTo("PAT+1+CONSIGN:::12+66::D:23'"), "PAT");

			pAT = new PATSegment();
			D99BMessageUtilities.PopulatePAT(pAT, PaymentTermsTypeCodeQualifierList.Basic, "AAAAA", "12", TimeReferenceCodeList.SpecifiedDate, PeriodTypeCodeList.GetFromString(string.Empty), "23");
			NUnit.Framework.Assert.That(pAT.ToString(characterSet), NUnit.Framework.Is.EqualTo("PAT+1+AAAAA:::12'"), "PAT");

			pAT = new PATSegment();
			D99BMessageUtilities.PopulatePAT(pAT, PaymentTermsTypeCodeQualifierList.Basic, "CONSIGN", "12", TimeReferenceCodeList.SpecifiedDate, PeriodTypeCodeList.Day, string.Empty);
			NUnit.Framework.Assert.That(pAT.ToString(characterSet), NUnit.Framework.Is.EqualTo("PAT+1+CONSIGN:::12'"), "PAT");
		}

		[ExpectNoExceptions]
		public void TestPopulateGIN()
		{
			var builder = new ZStringBuilder();
			builder.Append("NUMBER DESCRIPTION UP TO 39 CHARACTERS1");

			var group35 = new SegmentGroup35();
			var interpretation = new MessageInterpretation(group35, new CACharSet());
			D99BMessageUtilities.PopulateGIN(group35, ObjectIdentificationCodeQualifierList.PartNumber, new ZString[] { builder.ToString() }, interpretation);
			NUnit.Framework.Assert.That(group35.ToString(characterSet), NUnit.Framework.Is.EqualTo("GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1'"), "GIN");

			builder.Append("NUMBER DESCRIPTION UP TO 39 CHARACTERS2");
			builder.Append("NUMBER DESCRIPTION UP TO 39 CHARACTERS3");
			builder.Append("NUMBER DESCRIPTION UP TO 39 CHARACTERS4");
			builder.Append("NUMBER DESCRIPTION UP TO 39 CHARACTERS5 AAA");
			group35 = new SegmentGroup35();
			D99BMessageUtilities.PopulateGIN(group35, ObjectIdentificationCodeQualifierList.PartNumber, new ZString[] { builder.ToString() }, interpretation);
			NUnit.Framework.Assert.That(group35.ToString(characterSet), NUnit.Framework.Is.EqualTo(@"GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS2+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS3+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS4+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS5'GIN+PN+ AAA'"), "GIN");

			group35 = new SegmentGroup35();
			D99BMessageUtilities.PopulateGIN(group35, ObjectIdentificationCodeQualifierList.PartNumber, new ZString[] { builder.ToString(), "NUMBER DESCRIPTION UP TO 39 CHARACTERS6" }, interpretation);
			NUnit.Framework.Assert.That(group35.ToString(characterSet), NUnit.Framework.Is.EqualTo(@"GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS2+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS3+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS4+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS5'GIN+PN+ AAA'GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS6'"), "GIN");
		}

		[ExpectNoExceptions]
		public void TestPopulateTAX()
		{
			var tAX = new TAXSegment();
			D99BMessageUtilities.PopulateTAX(tAX, DutyTaxFeeFunctionQualifierList.Tax, DutyTaxFeeTypeNameCodeList.ValueAddedTax, "RATEOFGST1");
			NUnit.Framework.Assert.That(tAX.ToString(characterSet), NUnit.Framework.Is.EqualTo("TAX+7+VAT++RATEOFGST1'"), "TAX");

			tAX = new TAXSegment();
			D99BMessageUtilities.PopulateTAX(tAX, DutyTaxFeeFunctionQualifierList.CustomsDuty, "RATE");
			NUnit.Framework.Assert.That(tAX.ToString(characterSet), NUnit.Framework.Is.EqualTo("TAX+5+++RATE'"), "TAX");

			tAX = new TAXSegment();
			D99BMessageUtilities.PopulateTAXTypeName(tAX, DutyTaxFeeFunctionQualifierList.CustomsDuty, "K90");
			NUnit.Framework.Assert.That(tAX.ToString(characterSet), NUnit.Framework.Is.EqualTo("TAX+5+:::K90'"), "TAX");
		}

		[ExpectNoExceptions]
		public void TestPopulateGIR()
		{
			var gIR = new GIRSegment();
			D99BMessageUtilities.PopulateGIR(gIR, SetIdentificationQualifierList.Product, "12");
			NUnit.Framework.Assert.That(gIR.ToString(characterSet), NUnit.Framework.Is.EqualTo("GIR+1+12'"), "GIR");
		}

		[ExpectNoExceptions]
		public void TestPopulateMEA()
		{
			var mEA = new MEASegment();
			D99BMessageUtilities.PopulateMEA(mEA, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity, MessageConstants.CBSAWeightUnits.Kilogram, 10);
			NUnit.Framework.Assert.That(mEA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MEA+AAR++KGM:10000'"), "MEA");

			mEA = new MEASegment();
			D99BMessageUtilities.PopulateMEA(mEA, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity, MessageConstants.CBSAWeightUnits.Kilogram, 10.4567);
			NUnit.Framework.Assert.That(mEA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MEA+AAR++KGM:10457'"), "MEA");

			mEA = new MEASegment();
			D99BMessageUtilities.PopulateMEARounded(mEA, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity, MessageConstants.CBSAWeightUnits.Kilogram, 10.4);
			NUnit.Framework.Assert.That(mEA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MEA+AAR++KGM:10'"), "MEA");

			mEA = new MEASegment();
			D99BMessageUtilities.PopulateMEARounded(mEA, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity, MessageConstants.CBSAWeightUnits.Kilogram, 10.5);
			NUnit.Framework.Assert.That(mEA.ToString(characterSet), NUnit.Framework.Is.EqualTo("MEA+AAR++KGM:11'"), "MEA");
		}

		[ExpectNoExceptions]
		public void TestPopulateUNT()
		{
			var uNT = new UNTSegment();
			D99BMessageUtilities.PopulateUNT(uNT, "10", "20");
			NUnit.Framework.Assert.That(uNT.ToString(characterSet), NUnit.Framework.Is.EqualTo("UNT+10+20'"), "UNT");
		}

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new CACharSet();
		}

		CACharSet characterSet;
	}
}
