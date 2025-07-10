using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.NUnit;
using D99BCUSRES = Enterprise.Edifact.D99B.Messages.CUSRES;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class D99BMessageUtilitiesGetDetailsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetDocumentName()
		{
			var testMessage = B3SyntaxErrorMessageText.Replace("\r\n", "'");
			var message = GetEDIReleaseMessage(testMessage);
			var cusresMessage = (D99BCUSRES.CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new Edifact.D99B.EdifactD99BMessageFactory(), characterSet);
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetDocumentName(cusresMessage), NUnit.Framework.Is.EqualTo("000000250"));
		}

		[ExpectNoExceptions]
		public void TestGetAccountSecurityNumber()
		{
			var testMessage = B3SyntaxErrorMessageText.Replace("\r\n", "'");
			var message = GetEDIReleaseMessage(testMessage);
			var cusresMessage = (D99BCUSRES.CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new Edifact.D99B.EdifactD99BMessageFactory(), characterSet);
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetAccountSecurityNumber(cusresMessage), NUnit.Framework.Is.EqualTo("02345"));
		}

		[ExpectNoExceptions]
		public void TestGetAddress()
		{
			var nadSection = new NADSegmentMessageSection(1);
			var nad = nadSection.InstantiateAChildAndAddItToChildrenCollection();
			nad.Parse(characterSet, "NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708");
			var address = D99BMessageUtilities.GetAddress(nadSection, PartyFunctionCodeQualifierList.Seller);
			AssertAddress(address, "MITSUBISHI MATERIALS USA CORP.", "US", "CA", "92708");

			nad.Parse(characterSet, "NAD+SE++MITSUBISHI MATERIALS USA CORP.++++CA");
			address = D99BMessageUtilities.GetAddress(nadSection, PartyFunctionCodeQualifierList.Seller);
			AssertAddress(address, "MITSUBISHI MATERIALS USA CORP.", "CA", string.Empty, string.Empty);
		}

		[ExpectNoExceptions]
		public void TestGetLocation()
		{
			var locSection = new LOCSegmentMessageSection(1);
			var loc = locSection.InstantiateAChildAndAddItToChildrenCollection();
			loc.Parse(characterSet, "LOC+27+JP+UCA+3801");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetLocation(locSection, LocationFunctionCodeQualifierList.CountryOfOrigin), NUnit.Framework.Is.EqualTo("JP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetRelatedLocationOne(locSection, LocationFunctionCodeQualifierList.CountryOfOrigin), NUnit.Framework.Is.EqualTo("UCA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetRelatedLocationTwo(locSection, LocationFunctionCodeQualifierList.CountryOfOrigin), NUnit.Framework.Is.EqualTo("3801").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetTransportModeAndCarrierCode()
		{
			var group4Section = new SegmentGroup4MessageSection(1);
			var group4 = group4Section.InstantiateAChildAndAddItToChildrenCollection();
			var tdt = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.Parse(characterSet, "TDT+11++2++3713");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetTransportMode(group4Section, TransportStageCodeQualifierList.AtBorder), NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetCarrierCode(group4Section, TransportStageCodeQualifierList.AtBorder), NUnit.Framework.Is.EqualTo("3713").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetDate()
		{
			var dtmSection = new DTMSegmentMessageSection(1);
			var dtm = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
			dtm.Parse(characterSet, "DTM+129:20090521:102");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetDate(dtmSection, DateTimePeriodFunctionCodeQualifierList.ExportationDate), NUnit.Framework.Is.EqualTo(new ZDate(2009, 5, 21)));
		}

		[ExpectNoExceptions]
		public void TestGetAmount()
		{
			var moaSection = new MOASegmentMessageSection(1);
			var moa = moaSection.InstantiateAChildAndAddItToChildrenCollection();
			moa.Parse(characterSet, "MOA+43:12340");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue), NUnit.Framework.Is.EqualTo(123.4m).Using(CustomComparers.TypeComparison));

			moa.Parse(characterSet, "MOA+43:0");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue), NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));

			moa.Parse(characterSet, "MOA+43:4");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue), NUnit.Framework.Is.EqualTo(0.04m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetCurrency()
		{
			var moaSection = new MOASegmentMessageSection(1);
			var moa = moaSection.InstantiateAChildAndAddItToChildrenCollection();
			moa.Parse(characterSet, "MOA+6::USD");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetCurrency(moaSection, MonetaryAmountTypeCodeQualifierList.AmountReferenceCurrency), NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetReference()
		{
			var rffSection = new RFFSegmentMessageSection(1);
			var rff = rffSection.InstantiateAChildAndAddItToChildrenCollection();
			rff.Parse(characterSet, "RFF+TN:123456789:Y");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetReference(rffSection, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber), NUnit.Framework.Is.EqualTo("123456789").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetReferenceIndicator()
		{
			var rffSection = new RFFSegmentMessageSection(1);
			var rff = rffSection.InstantiateAChildAndAddItToChildrenCollection();
			rff.Parse(characterSet, "RFF+TN:123456789:Y");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetReferenceIndicator(rffSection, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			rff.Parse(characterSet, "RFF+TN:123456789:N");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetReferenceIndicator(rffSection, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			rff.Parse(characterSet, "RFF+TN:123456789");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetReferenceIndicator(rffSection, ReferenceFunctionCodeQualifierList.TransactionReferenceNumber), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetGINAsString()
		{
			var group35Section = new SegmentGroup35MessageSection(1);
			var group35 = group35Section.InstantiateAChildAndAddItToChildrenCollection();
			var gin = group35.GIN.InstantiateAChildAndAddItToChildrenCollection();
			gin.Parse(characterSet, "GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARS");
			gin = group35.GIN.InstantiateAChildAndAddItToChildrenCollection();
			gin.Parse(characterSet, "GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS2");
			const string expected = "NUMBER DESCRIPTION UP TO 39 CHARACTERS1NUMBER DESCRIPTION UP TO 39 CHARSNUMBER DESCRIPTION UP TO 39 CHARACTERS2";
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetGINAsString(group35Section, ObjectIdentificationCodeQualifierList.PartNumber), NUnit.Framework.Is.EqualTo(expected));
		}

		[ExpectNoExceptions]
		public void TestGetQuantityAndUOM()
		{
			var meaSection = new MEASegmentMessageSection(1);
			var mea = meaSection.InstantiateAChildAndAddItToChildrenCollection();
			mea.Parse(characterSet, "MEA+AAR++KGM:10000");
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetQuantity(meaSection, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity), NUnit.Framework.Is.EqualTo(10m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(D99BMessageUtilities.GetUnitOfMeasure(meaSection, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity), NUnit.Framework.Is.EqualTo("KGM").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new CACharSet();
		}

		CACharSet characterSet;

		[ExpectNoExceptions]
		void AssertAddress(IDocAddress address, string company, string country, string state, string postcode)
		{
			NUnit.Framework.Assert.That(address.E2_CompanyName, NUnit.Framework.Is.EqualTo(company).Using(CustomComparers.TypeComparison), "E2_CompanyName");
			NUnit.Framework.Assert.That(address.CountryCode, NUnit.Framework.Is.EqualTo(country).Using(CustomComparers.TypeComparison), "CountryCode");
			NUnit.Framework.Assert.That(address.E2_State, NUnit.Framework.Is.EqualTo(state).Using(CustomComparers.TypeComparison), "E2_State");
			NUnit.Framework.Assert.That(address.E2_Postcode, NUnit.Framework.Is.EqualTo(postcode).Using(CustomComparers.TypeComparison), "E2_Postcode");
		}

		EDIMessage GetEDIReleaseMessage(string messageText)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = "SYN";
			message.EM_MessageText = messageText.Replace("\r\n", "");
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIInterchange.Status.Received;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			return message;
		}

		const string B3SyntaxErrorMessageText = @"UNH+1+CUSRES:D:99B:UN+2345
BGM+:::000250+080+11
DTM+137:201007071025:203
GIS+14
ERP+2:35:29
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:ELEM TOO LONG
FTX+AAO+++SEGMENTMOALINE7ELE POS1,2:INVALID CHARS
FTX+AAO+++SEGMENTGINLINE21ELE POS2,1:ELEM TOO LONG
FTX+AAO+++SEGMENTLOC- BYTE OFFSET256:MAND SEG MISSING
FTX+AAO+++BLA BLA
UNT+11+1";
	}
}
