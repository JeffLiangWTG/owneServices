using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class SEACRMessageBuilderTest : TestCaseWithFactory
	{
		public void TestDocumentName()
		{
			AssertEquals("SEACR", Builder.DocumentName);
		}

		public void TestWithdrawEndToEnd()
		{
			#region ExpectedMessage

			ZString expectedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+933:::SEACR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:1+50'
RFF+BH:1'
RFF+MB:123456789'
TDT+20+123++11++++8811924::11'
LOC+12+AUSYD::6'
UNT+7+<<MSGNO PLACEHOLDER>>'
";
			#endregion
			messageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertMultilineEquals("ExpectedMessage", expectedMessage.Replace("\r", "").Replace("\n", ""), GeneratedMessage, '\'');
		}

		public void TestEDIMessageIsOnHold()
		{
			ocean.CB_RL_NKPortOfDischarge = "AUSYD";
			ocean.CB_DateOfArrival = ZDateTime.UtcNow.AddDays(5);
			var builder = new SEACRMessageBuilder(scaHouse, "123", true);
			builder.MessageSubType = messageSubType;
			builder.Messages = scaHouse.Messages;
			var message = builder.PopulateMessagesReturningResult();
			AssertNotEquals(ZDateTime.Empty, message.EM_HeldUntilDate);
			ocean.CB_DateOfArrival = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", ZDateTime.UtcNow.AddHours(-1).ToDateTime());
			message = builder.PopulateMessagesReturningResult();
			AssertEquals(ZDateTime.Empty, message.EM_HeldUntilDate);
		}

		public void TestParentBillOfLading()
		{
			Assert(GeneratedMessage.Contains("RFF+BM:123'"));
		}

		public void TestHouseBillOfLading()
		{
			scaHouse.CA_HouseBill = "24680";
			Assert(GeneratedMessage.Contains("RFF+BH:24680'"));
		}

		public void TestOceanBillOfLading()
		{
			scaHouse.OceanBill.CB_OceanBill = "1234567890";
			Assert(GeneratedMessage.Contains("RFF+MB:1234567890'"));
		}

		public void TestMethodOfPayment()
		{
			scaHouse.CA_PrepaidCollectOther = "MX";
			Assert(GeneratedMessage.Contains("RFF+PQ:MX'"));
		}

		public void TestConsignee()
		{
			AssertContains("NAD+CN++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'", GeneratedMessage);
		}

		public void TestConsignor()
		{
			AssertContains("NAD+CZ++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'", GeneratedMessage);
		}

		public void TestTrustedTraderIdentification()
		{
			scaHouse.Consignee.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			scaHouse.Consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			scaHouse.Consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			scaHouse.Consignee.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "IMPTIN1234567890", Core.Constants.CountryCodes.Australia);
			scaHouse.Consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "CONTIN1234567890", Core.Constants.CountryCodes.Australia);

			var generatedMessage = GeneratedMessage;

			AssertContains("Generated message has Importer TIN code. Was: " + generatedMessage, "NAD+AU+IMPTIN1234567890::95'", generatedMessage);
			AssertContains("Generated message has Consignor TIN code. Was: " + generatedMessage, "NAD+CZ+CONTIN1234567890+NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'", generatedMessage);

			AssertNotContains("Importer (IM), Authorised Importer (AT) and SubEntity (WP) are not sent with TIN", "NAD+IM", generatedMessage);
			AssertNotContains("Importer (IM), Authorised Importer (AT) and SubEntity (WP) are not sent with TIN", "NAD+AT", generatedMessage);
			AssertNotContains("Importer (IM), Authorised Importer (AT) and SubEntity (WP) are not sent with TIN", "NAD+WP", generatedMessage);
		}

		public void TestNotifyParty()
		{
			Assert("Notify Party from Organisation entered", GeneratedMessage.Contains("NAD+NI+++NAME+ADDRESS1:ADDRESS2+CITY NSW++12345+AU'"));
		}

		public void TestEnteredNotifyParty()
		{
			scaHouse.CA_NotifyName = "JOHN SMITH";
			scaHouse.CA_NotifyAddress1 = "UNIT 2219 TARMAC BUILDING";
			scaHouse.CA_NotifyAddress2 = "KINGSFORD SMITH AIRPORT";
			scaHouse.CA_NotifySuburb = "MASCOT";
			scaHouse.CA_NotifyPostcode = "2215";
			scaHouse.CA_RN_NKNotifyCountryCode = "XX";
			Assert("Notify Party from entered Name & Address details", GeneratedMessage.Contains("NAD+NI+++JOHN SMITH+UNIT 2219 TARMAC BUILDING:KINGSFORD SMITH AIRPORT+MASCOT++2215+XX'"));
		}

		public void TestResponsiblePartyClientID()
		{
			Assert(GeneratedMessage.Contains("NAD+VW+41065894724::95'"));
		}

		public void TestResponsiblePartyClientIDFromCusSCAOceanBillRecord()
		{
			scaHouse.OceanBill.CB_ResponsiblePartyID = "92010195669";
			Assert(GeneratedMessage.Contains("NAD+VW+92010195669::95'"));
		}

		public void TestPrincipalAgentID()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "1234567890");
			scaHouse.OceanBill.CB_OH_ShippingLine = shippingLine.PK;
			var generatedMessage = this.GeneratedMessage;
			Assert(generatedMessage.Contains("NAD+AH+1234567890::95'"));
		}

		public void TestTransportDetails()
		{
			Assert(GeneratedMessage.Contains("TDT+20+123++11++++8811924::11'"));
		}

		public void TestPortOfDestination()
		{
			scaHouse.CA_RL_NK_PortOfDestination = "AUNTL";
			Assert(GeneratedMessage.Contains("LOC+8+AUNTL::6'"));
		}

		public void TestPortOfDischarge()
		{
			scaHouse.OceanBill.CB_RL_NKPortOfDischarge = "AUBNE";
			Assert(GeneratedMessage.Contains("LOC+12+AUBNE::6'"));
		}

		public void TestOriginalPortOfLoading()
		{
			scaHouse.OceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			Assert(GeneratedMessage.Contains("LOC+76+NZAKL::6'"));
		}

		public void TestFirstAustralianPort()
		{
			scaHouse.OceanBill.CB_RL_NKPortOfDischarge = "SGSIN";
			scaHouse.OceanBill.CB_RL_NKPortOfFirstArrival = "AUSYD";
			scaHouse.OceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			scaHouse.CA_RL_NK_PortOfDestination = "SGSIN";
			Assert(GeneratedMessage.Contains("LOC+79+AUSYD::6'"));
		}

		public void TestWaybillOrigin()
		{
			scaHouse.CA_RL_NK_PortOfOrigin = "NZAKL";
			Assert(GeneratedMessage.Contains("LOC+73+NZAKL::6'"));
		}

		public void TestFreightForwarderIndicator()
		{
			scaHouse.CA_IsMasterHouse = true;
			Assert(GeneratedMessage.Contains("GIS+FFO:109:95'"));
			scaHouse.CA_IsMasterHouse = false;
			Assert(!GeneratedMessage.Contains("GIS+FFO:109:95'"));
		}

		public void TestGroup7ContainerNumber()
		{
			var container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "CHLU2787765";
			var packingLine = scaHouse.Pivot.AddNew();
			packingLine.CV_CN = container.PK;

			string expected = @"CNI++:::I'RFF+AAQ:CHLU2787765'GID+1'";
			Assert("Container number RFF", GeneratedMessage.Contains(expected));
		}

		public void TestGroup7SealNumber()
		{
			var container = ocean.Containers.AddNew();
			container.CN_SealNumber = "1000000001";

			var packingLine = scaHouse.Pivot.AddNew();
			packingLine.CV_CN = container.PK;

			string expected = @"RFF+SN:1000000001'GID+1'";
			Assert("Seal number in RFF", GeneratedMessage.Contains(expected));
		}

		public void TestGroup7PACSegments()
		{
			var container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "CHLU2787765";
			container.CN_ContainerMode = "LCL";
			var packingLine = scaHouse.Pivot.AddNew();
			packingLine.CV_CN = container.PK;
			packingLine.CV_PackageCount = 500;
			packingLine.CV_PackageType = "BX";

			var expectedContainerStatus = @"PAC+500'";
			var expectedContainerCount = "PAC+++BX:185:95'";
			Assert("PAC segments in group7", GeneratedMessage.Contains(expectedContainerStatus));
			Assert("PAC segments in group7", GeneratedMessage.Contains(expectedContainerCount));
		}

		public void TestGroup7FTXSegment()
		{
			var container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "CHLU2787765";
			var packingLine = scaHouse.Pivot.AddNew();
			packingLine.CV_CN = container.PK;
			packingLine.CV_GoodsDescription = "VARIOUS GOODS";
			var expected = @"FTX+AAA+++VARIOUS GOODS'";
			Assert("FTX segments in group7", GeneratedMessage.Contains(expected));
		}

		public void TestGroup7MEASegments()
		{
			var container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "CHLU2787765";
			var packingLine = scaHouse.Pivot.AddNew();
			packingLine.CV_CN = container.PK;
			packingLine.CV_Volume = 500;
			packingLine.CV_Weight = 500;
			packingLine.CV_NetWeight = 400;
			packingLine.CV_WeightUQ = "KG";

			var expected = @"MEA+AAE+AAL+KG:400.00'MEA+AAE+G+KG:500.00'MEA+AAE+ABJ+CU:500.00'";
			Assert("MEA segments in group7", GeneratedMessage.Contains(expected));
		}

		public void TestPCISegmentInGroup7()
		{
			var container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "CHLU2787765";
			var packingLine = scaHouse.Pivot.AddNew();
			packingLine.CV_CN = container.PK;
			packingLine.CV_MarksAndNumbers = "MARKRECR382BB";
			var expected = @"PCI+28+MARKRECR382BB'";
			Assert("PCI segment in group 7", GeneratedMessage.Contains(expected));
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "41065894724");
			ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_Voyage = "123";
			ocean.CB_VesselName = "ADMIRALENGRACHT";
			ocean.CB_OceanBill = "123456789";
			ocean.CB_RL_NKPortOfLoading = "NZAKL";
			ocean.CB_RL_NKPortOfDischarge = "AUSYD";
			scaHouse = ocean.HouseBills.AddNew();
			scaHouse.CA_HouseBill = "1";
			scaHouse.CA_MasterHouseBill = "123";
			scaHouse.CA_PrepaidCollectOther = "A";
			scaHouse.CA_RL_NK_PortOfOrigin = "NZAKL";
			scaHouse.CA_RL_NK_PortOfDestination = "AUSYD";
			scaHouse.CA_OH_Notify = SetupOrgHeader().PK;
			SetConsignee(scaHouse, SetupOrgHeader());
			SetConsignor(scaHouse, SetupOrgHeader());
		}

		protected ZString GeneratedMessage => Builder.GeneratedMessageStrings[0];

		protected OrgHeader SetupOrgHeader()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_Address1 = "Address1";
			header.MainAddress.OA_Address2 = "Address2";
			header.MainAddress.OA_City = "CITY";
			header.MainAddress.OA_PostCode = "12345";
			header.MainAddress.OA_Phone = "1234567890";
			return header;
		}

		protected virtual void SetConsignee(CusSCAHouse house, OrgHeader consignee)
		{
			house.CA_OA_ConsigneeAddress = consignee.MainAddress.PK;
		}

		protected virtual void SetConsignor(CusSCAHouse house, OrgHeader consignor)
		{
			house.CA_OA_ConsignorAddress = consignor.MainAddress.PK;
		}

		SEACRMessageBuilder Builder
		{
			get
			{
				var builder = new SEACRMessageBuilder(scaHouse);
				builder.MessageSubType = messageSubType;
				builder.Messages = scaHouse.Messages;
				return builder;
			}
		}

		Common.MessageBuilders.MessageSubTypes messageSubType = Common.MessageBuilders.MessageSubTypes.Create;
		protected CusSCAHouse scaHouse;
		protected CusSCAOceanBill ocean;
	}
}
