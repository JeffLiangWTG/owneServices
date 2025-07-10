using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class CargoReportMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2017, 4, 11)]
		public void TestPopulateGroup2_ICS()
		{
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 4, 1)))
			{
				var consignee1 = Factory.New<OrgHeader>();
				consignee1.OH_Code = "Test";
				consignee1.OH_FullName = "consignee1";
				consignee1.OH_RL_NKClosestPort = "AUSYD";
				consignee1.MainAddress.OA_Address1 = "1 Main Street";
				consignee1.MainAddress.OA_City = "CITY";
				consignee1.MainAddress.OA_PostCode = "2011";
				consignee1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN12345678901");
				consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");

				var ocean1 = Factory.New<CusSCAOceanBill>();
				var house1 = ocean1.HouseBills.AddNew();
				house1.CA_ResponsiblePartyID = ABN;
				SetConsignee(house1, consignee1);
				var header1 = new CusSCAHouseSeaCargoReportHeader(house1, house1.Messages);
				var messageBuilder1 = new TestSeaCargoReportMessageBuilderHelper(header1);
				messageBuilder1.PopulateGroup2();

				var message1Text = messageBuilder1.MessageText;
				AssertContains("NAD+AT+ABN12345678::95'", message1Text);
				AssertContains("NAD+WP+123::95'", message1Text);
				AssertContains("NAD+CN++CONSIGNEE1::1 MAIN STREET  CITY NSW 2011 AU'", message1Text);

				var consignee2 = Factory.New<OrgHeader>();
				consignee2.OH_Code = "Test";
				consignee2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN12345678901");
				consignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
				consignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "CID12345678901");

				var ocean2 = Factory.New<CusSCAOceanBill>();
				var house2 = ocean2.HouseBills.AddNew();
				house2.CA_ResponsiblePartyID = ABN;
				SetConsignee(house2, consignee2);
				var header2 = new CusSCAHouseSeaCargoReportHeader(house2, house2.Messages);
				var messageBuilder2 = new TestSeaCargoReportMessageBuilderHelper(header2);
				messageBuilder2.PopulateGroup2();

				var message2Text = messageBuilder2.MessageText;
				AssertContains("NAD+IM+CID12345678::95'", message2Text);
				AssertNotContains("NAD+AT+ABN12345678::95'", message2Text);
				AssertNotContains("NAD+WP+123::95'", message2Text);

				var consignee3 = Factory.New<OrgHeader>();
				consignee3.OH_Code = "Test";
				consignee3.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN12345678901");
				consignee3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
				consignee3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "CID12345678901");
				consignee3.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "IMPTIN1234567890", Core.Constants.CountryCodes.Australia);

				var ocean3 = Factory.New<CusSCAOceanBill>();
				var house3 = ocean3.HouseBills.AddNew();
				house3.CA_ResponsiblePartyID = ABN;
				SetConsignee(house3, consignee3);
				var header3 = new CusSCAHouseSeaCargoReportHeader(house3, house3.Messages);
				var messageBuilder3 = new TestSeaCargoReportMessageBuilderHelper(header3);
				messageBuilder3.PopulateGroup2();

				var message3Text = messageBuilder3.MessageText;
				AssertContains("NAD+AU+IMPTIN1234567890::95'", message3Text);
				AssertNotContains("NAD+IM+CID12345678::95'", message3Text);
				AssertNotContains("NAD+AT+ABN12345678::95'", message3Text);
				AssertNotContains("NAD+WP+123::95'", message3Text);

				var consignor = consignee1;
				consignor.OH_FullName = "consignor1";
				consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "CIDAAA111");
				consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "CONTIN1234567890", Core.Constants.CountryCodes.Australia);

				var ocean4 = Factory.New<CusSCAOceanBill>();
				var house4 = ocean4.HouseBills.AddNew();
				house4.CA_ResponsiblePartyID = ABN;
				SetConsignor(house4, consignor);
				var header4 = new CusSCAHouseSeaCargoReportHeader(house4, house4.Messages);
				var messageBuilder4 = new TestSeaCargoReportMessageBuilderHelper(header4);
				messageBuilder4.PopulateGroup2();

				var message4Text = messageBuilder4.MessageText;
				AssertContains("NAD+CZ+CONTIN1234567890+CONSIGNOR1::1 MAIN STREET  CITY NSW 2011 AU'", message4Text);
				AssertContains("NAD+SU+CIDAAA111::95'", message4Text);
			}
		}

		public void TestPopulateWithGeneralDetailsNAD()
		{
			const string testName = "Senior Cardgraves";
			const string testAddress = "12 VeryManyLetterCreepy Lane Sleezeville NSW 2222 AU";
			messageBuilder.PopulateWithGeneralDetailsNADForHelper(testGroup, PartyFunctionCodeQualifierList.Consignee, testName, testAddress);
			AssertEquals("Senior Cardgraves", testGroup.NAD[0].NameAndAddress.NameAndAddressLine1);
			AssertEquals("12 VeryManyLetterCreepy Lane Sleeze", testGroup.NAD[0].NameAndAddress.NameAndAddressLine3);
			AssertEquals("ville NSW 2222 AU", testGroup.NAD[0].NameAndAddress.NameAndAddressLine4);
			AssertEquals("", testGroup.NAD[0].NameAndAddress.NameAndAddressLine5);
		}

		public void TestNewLineCharactersInNameAreTrimmedBeforeNADIsSplittedWhenPopulating()
		{
			var testName = "Senior Cardgraves" + "\r\n";
			const string testAddress = "12 VeryManyLetterCreepy Lane Sleezeville NSW 2222 AU";
			messageBuilder.PopulateWithGeneralDetailsNADForHelper(testGroup, PartyFunctionCodeQualifierList.Consignee, testName, testAddress);
			AssertEquals("Senior Cardgraves ", testGroup.NAD[0].NameAndAddress.NameAndAddressLine1);
		}

		public void TestPopulateNADNameAndAddressSegments()
		{
			const string testName = "HELLMANN WORLDWIDE LOGISTICS SOMEWHERE IN THE WORLD JUST TO TEST LONG NAMES LTD";
			const string testAddress = "1/F BLK A NO. 5,7,9 TONIC INDUSTRIA 26 KAI CHEUNG RD, KOWLOON BAY,KOWLO AS AGENTS FOR PELORUS OCEAN LINE  CN";

			messageBuilder.PopulateWithGeneralDetailsNAD(testGroup, PartyFunctionCodeQualifierList.Consignee, testName, testAddress);

			AssertEquals("HELLMANN WORLDWIDE LOGISTICS SOMEWH", testGroup.NAD[0].NameAndAddress.NameAndAddressLine1);
			AssertEquals("ERE IN THE WORLD JUST TO TEST LONG ", testGroup.NAD[0].NameAndAddress.NameAndAddressLine2);

			AssertEquals("1/F BLK A NO. 5,7,9 TONIC INDUSTRIA", testGroup.NAD[0].NameAndAddress.NameAndAddressLine3);
			AssertEquals(" 26 KAI CHEUNG RD, KOWLOON BAY,KOWL", testGroup.NAD[0].NameAndAddress.NameAndAddressLine4);
			AssertEquals("O AS AGENTS FOR PELORUS OCEAN LINE ", testGroup.NAD[0].NameAndAddress.NameAndAddressLine5);
		}

		public void TestPopulateWithGeneralDetailsNADShort()
		{
			const string testName = "Mr Cuckoo Squeaker";
			messageBuilder.PopulateWithGeneralDetailsNADForHelper(testGroup, PartyFunctionCodeQualifierList.Consignee, testName, "");
			AssertEquals("Mr Cuckoo Squeaker", testGroup.NAD[0].NameAndAddress.NameAndAddressLine1);
		}

		public void TestPopulateWithGeneralDetailsNADLong()
		{
			const string testName = "NameNameNameNameNameNameNameNameNam";
			const string testAddress = "Address1Address1Address1Address1AddAddress2Address2Address2Address2AddAddress3Address3Address3Address3Add";
			messageBuilder.PopulateWithGeneralDetailsNADForHelper(testGroup, PartyFunctionCodeQualifierList.Consignee, testName, testAddress);
			AssertEquals("NameNameNameNameNameNameNameNameNam", testGroup.NAD[0].NameAndAddress.NameAndAddressLine1);
			AssertEquals(35, testGroup.NAD[0].NameAndAddress.NameAndAddressLine3.Length);
			AssertEquals("Address1Address1Address1Address1Add", testGroup.NAD[0].NameAndAddress.NameAndAddressLine3);
			AssertEquals("Address2Address2Address2Address2Add", testGroup.NAD[0].NameAndAddress.NameAndAddressLine4);
			AssertEquals("Address3Address3Address3Address3Add", testGroup.NAD[0].NameAndAddress.NameAndAddressLine5);
		}

		public void TestPopulateWithGeneralDetailsNADLongName()
		{
			const string testName = "NameNameNameNameNameNameNameNameNameNameNameName";
			messageBuilder.PopulateWithGeneralDetailsNADForHelper(testGroup, PartyFunctionCodeQualifierList.Consignee, testName, "");
			AssertEquals("NameNameNameNameNameNameNameNameNam", testGroup.NAD[0].NameAndAddress.NameAndAddressLine1);
			AssertEquals("eNameNameName", testGroup.NAD[0].NameAndAddress.NameAndAddressLine2);
			AssertEquals("", testGroup.NAD[0].NameAndAddress.NameAndAddressLine3);
			AssertEquals("", testGroup.NAD[0].NameAndAddress.NameAndAddressLine4);
			AssertEquals("", testGroup.NAD[0].NameAndAddress.NameAndAddressLine5);
		}

		[ExpectNoExceptions]
		public void TestPopulateWithGeneralDetailsNADLongNameAndAddress()
		{
			const string testName = "Hunter's Cuckoo Squeaker shop";
			const string testAddress = "1 Somewherewithareallyreallylongstreetname StreetSomeother kind of street address that is long this is some more name and address stuff and it is really really exciting yeah";
			messageBuilder.PopulateWithGeneralDetailsNADForHelper(testGroup, PartyFunctionCodeQualifierList.Consignee, testName, testAddress);
			AssertEquals("Hunter's Cuckoo Squeaker shop", testGroup.NAD[0].NameAndAddress.NameAndAddressLine1);
			AssertEquals("1 Somewherewithareallyreallylongstr", testGroup.NAD[0].NameAndAddress.NameAndAddressLine3);
			AssertEquals("eetname StreetSomeother kind of str", testGroup.NAD[0].NameAndAddress.NameAndAddressLine4);
			AssertEquals("eet address that is long this is so", testGroup.NAD[0].NameAndAddress.NameAndAddressLine5);
		}

		public void TestGluckDataDoesNotCauseIndexOutOfBounds()
		{
			const string testName1 = "BALTRANS LOGISTICS (HONG KONG) LTD";
			const string testAddress1 = "ROOM 809, 8TH FLOOR, TOWER A NEW MA 14 SCIENCE MUSEUM ROAD, TSIMSHATSUI KOWLOON HK HONG KONG";
			messageBuilder.PopulateWithGeneralDetailsNADForHelper(testGroup, PartyFunctionCodeQualifierList.Consignee, testName1, testAddress1);
			AssertEquals("ROOM 809, 8TH FLOOR, TOWER A NEW MA", testGroup.NAD[0].NameAndAddress.NameAndAddressLine3);
			AssertEquals(" 14 SCIENCE MUSEUM ROAD, TSIMSHATSU", testGroup.NAD[0].NameAndAddress.NameAndAddressLine4);
			AssertEquals("I KOWLOON HK HONG KONG", testGroup.NAD[0].NameAndAddress.NameAndAddressLine5);
		}

		public void TestResponsiblePartyDefaultsFromCurrentCompanyABN()
		{
			const string OrgProxyABN = "41065894724";
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, OrgProxyABN);

			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			house.CA_ResponsiblePartyID = ABN;
			var header = new CusSCAHouseSeaCargoReportHeader(house, house.Messages);
			var messageBuilder = new TestSeaCargoReportMessageBuilderHelper(header);

			messageBuilder.PopulateGroup2();
			Assert("Responsible Party ID should come from CA_ResponsiblePartyID", messageBuilder.MessageText.Contains("NAD+VW+" + ABN + "::95'"));
			house.CA_ResponsiblePartyID = "";
			messageBuilder.PopulateGroup2();
			Assert("Responsible Party ID should come from OrgProxy", messageBuilder.MessageText.Contains("NAD+VW+" + OrgProxyABN + "::95'"));
		}

		public void TestResponsiblePartyOverrideFromHouse()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ResponsiblePartyForwarderABN;
			_ = CreateForwarder();
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			house.CA_ResponsiblePartyID = ABN;
			var header = new CusSCAHouseSeaCargoReportHeader(house, house.Messages);
			var messageBuilder = new TestSeaCargoReportMessageBuilderHelper(header);

			messageBuilder.PopulateGroup2();
			Assert("Responsible Party ID should be referenced in message", messageBuilder.MessageText.Contains(ABN));
		}

		public void TestLocations()
		{
			var header = new DummyAirCargoReportHeader();
			header.SetRoutings(null);
			var builder = new CargoReportMessageBuilderTestHelper(header);
			builder.CUSCAR = new CUSCARMessage();
			builder.PopulateLocations();

			AssertEquals(ZString.Empty, builder.MessageText);

			header.SetRoutings(new ZString[] { "AUSYD" });
			builder.CUSCAR = new CUSCARMessage();
			builder.PopulateLocations();
			Assert(builder.MessageText.Contains("AUSYD"));

			header.SetRoutings(new ZString[] { "AUSYD", "USLAX", "SGSIN" });
			builder.CUSCAR = new CUSCARMessage();
			builder.PopulateLocations();
			Assert(builder.MessageText.Contains("AUSYD"));
			Assert(builder.MessageText.Contains("SGSIN"));
			var resultWith3Ports = builder.MessageText;
			Assert(resultWith3Ports.Contains("USLAX"));

			header.SetRoutings(new ZString[] { "AUSYD", "USLAX", "SGSIN", "" });
			builder.CUSCAR = new CUSCARMessage();
			builder.PopulateLocations();
			AssertEquals(resultWith3Ports, builder.MessageText);
		}

		public void TestLOC79IsPopulatedWhenTransit()
		{
			var mock = new Mock<ICargoReportHeader>();
			var builder = GetNewBuilder(mock.Object);

			mock.Setup(m => m.Loading).Returns("HKHKG");
			mock.Setup(m => m.Destination).Returns("AQMCM");
			mock.Setup(m => m.FirstArrivalPort).Returns("AUSYD");

			builder.PopulateLocations();
			AssertContains("LOC+79+AUSYD", builder.MessageText);
			mock.Verify();
		}

		public void TestLOC79IsntPopulatedWhenImport()
		{
			var mock = new Mock<ICargoReportHeader>();
			var builder = GetNewBuilder(mock.Object);

			mock.Setup(m => m.Loading).Returns("HKHKG");
			mock.Setup(m => m.Destination).Returns("AUSYD");
			mock.Setup(m => m.FirstArrivalPort).Returns("AUBNE");

			builder.PopulateLocations();
			AssertNotContains("LOC 79 segment not in message", "LOC+79", builder.MessageText);
			AssertNotContains("AUBNE not in message", "AUBNE", builder.MessageText);
			mock.Verify();
		}

		protected override void SetUp()
		{
			base.SetUp();
			cUSCAR = new CUSCARMessage();
			testGroup = cUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection();
			messageBuilder = new CargoReportMessageBuilderTestHelper(null);
		}

		static CargoReportMessageBuilderTestHelper GetNewBuilder(ICargoReportHeader header)
		{
			var builder = new CargoReportMessageBuilderTestHelper(header);
			builder.CUSCAR = new CUSCARMessage();
			return builder;
		}

		void SetConsignee(CusSCAHouse house, OrgHeader consignee)
		{
			house.CA_OA_ConsigneeAddress = consignee.MainAddress.PK;
		}

		void SetConsignor(CusSCAHouse house, OrgHeader consignor)
		{
			house.CA_OA_ConsignorAddress = consignor.MainAddress.PK;
		}

		const string ResponsiblePartyForwarderABN = "93009568772";
		const string ABN = "753321942";

		OrgHeader CreateForwarder()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Test Forwarder";
			result.MainAddress.OA_Address1 = "Test Forwarder Address";
			result.OH_RL_NKClosestPort = "AUSYD";
			result.OH_IsForwarder = true;
			result.LocalBusinessRegNo = ResponsiblePartyForwarderABN;
			return result;
		}

		CUSCARMessage cUSCAR;
		SegmentGroup2 testGroup;
		CargoReportMessageBuilderTestHelper messageBuilder;

		sealed class TestSeaCargoReportMessageBuilderHelper : CargoReportMessageBuilder
		{
			public TestSeaCargoReportMessageBuilderHelper(ISeaCargoReportHeader reportHeader)
				: base(reportHeader, ZString.Empty)
			{
				CUSCAR = new CUSCARMessage();
			}

			protected internal override Type TypeOfMessage => typeof(CMRSEACRMessage);

			protected internal override ZString DocumentName => "SEACR";

			protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.SEACR;
		}

		sealed class DummyAirCargoReportHeader : IAirCargoReportHeader
		{
			ZString IAirCargoReportHeader.MatchConsignmentReference => ZString.Empty;

			public ZString MasterHouseBill => new ZString();

			public ZString ResponsiblePartyID => new ZString();

			public ZString HAWBNum => new ZString();

			public ZString MAWB => new ZString();

			public ZString FlightNo => new ZString();

			public ZDateTime ArivalDate => new ZDateTime();

			public bool IsMasterHouse => false;

			public bool IsDocuments => false;

			public bool IsPersonalEffects => false;

			public bool IsSelfAssessedClearance => false;

			public int PackageCount => 0;

			public ZString GoodsDescription => new ZString();

			public ZDecimal Weight => new ZDecimal();

			public ZString WeightUQ => new ZString();

			public ZDecimal GoodsValue => new ZDecimal();

			public ZString GoodsValueCurrency => new ZString();

			bool IAirCargoReportHeader.IsHVLVSpecialReporter => true;

			bool IAirCargoReportHeader.IsRemailSpecialReporter => false;

			public bool IsBureau => false;

			public bool CanDelaySending => false;

			public ZString MethodOfPayment => new ZString();

			public ZString Origin => new ZString();

			public ZString Destination => new ZString();

			public ZString Loading => new ZString();

			public ZString Discharge => new ZString();

			public ZString[] Routings { get; private set; }

			public void SetRoutings(ZString[] values)
			{
				Routings = values;
			}

			public ZString FirstArrivalPort => new ZString();

			public ZString ConsigneeName => new ZString();

			public ZString ConsigneeStreet => new ZString();

			public ZString ConsigneeStreet2 => new ZString();

			public ZString ConsigneeCity => new ZString();

			public ZString ConsigneePostCode => new ZString();

			public ZString ConsigneeCountry => new ZString();

			public ZString ConsigneeGeneralAddress => new ZString();

			public ZString ConsignorName => new ZString();

			public ZString ConsignorStreet => new ZString();

			public ZString ConsignorStreet2 => new ZString();

			public ZString ConsignorCity => new ZString();

			public ZString ConsignorPostCode => new ZString();

			public ZString ConsignorCountry => new ZString();

			public ZString ConsignorGeneralAddress => new ZString();

			public ZString ConsigneeIdentifier => new ZString();

			public ZString ConsigneeABN => new ZString();

			public ZString ConsigneeCAC => new ZString();

			public ZString ConsigneeTIN => new ZString();

			public ZString ConsignorIdentifier => new ZString();

			public ZString ConsignorVendor => new ZString();

			public ZString ConsignorTIN => new ZString();

			ZString ICargoReportHeader.NotifyPartyName => new ZString();

			ZString ICargoReportHeader.NotifyPartyStreet => new ZString();

			ZString ICargoReportHeader.NotifyPartyStreet2 => new ZString();

			ZString ICargoReportHeader.NotifyPartyCity => new ZString();

			ZString ICargoReportHeader.NotifyPartyPostCode => new ZString();

			ZString ICargoReportHeader.NotifyPartyCountry => new ZString();

			ZString ICargoReportHeader.NotifyPartyGeneralAddress => new ZString();
		}

		sealed class CargoReportMessageBuilderTestHelper : CargoReportMessageBuilder
		{
			public CargoReportMessageBuilderTestHelper(ICargoReportHeader reportHeader)
				: base(reportHeader, ZString.Empty)
			{
			}

			public void PopulateWithGeneralDetailsNADForHelper(SegmentGroup2 group2, PartyFunctionCodeQualifierList partyType, ZString name, ZString details)
			{
				PopulateWithGeneralDetailsNAD(group2, partyType, name, details);
			}

			protected internal override Type TypeOfMessage => typeof(CMRAIRCRMessage);

			protected internal override ZString DocumentName => "AIRCR";

			protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.AIRCR;
		}
	}
}
