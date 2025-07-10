using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class AIRCRMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2017, 4, 11)]
		public void TestPopulateGroup2_Vendor()
		{
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 4, 1)))
			{
				var consignor = Factory.New<OrgHeader>();
				consignor.OH_Code = "Test 2";
				consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");
				SetConsignor(hawb, consignor);

				AssertContains("NAD+VN+123::95'", GeneratedMessage);
			}
		}

		public void TestDocumentMessageNameCode()
		{
			AssertEquals(DocumentNameCodeList.CargoDeclarationArrival, Builder.DocumentNameCode);
		}

		public void TestDocumentName()
		{
			AssertEquals("AIRCR", Builder.DocumentName);
		}

		public void TestEDIMessageIsOnHold()
		{
			hawb.MAWB.CM_RL_NKDischargePort = "AUSYD";
			hawb.MAWB.CM_ArrivalDate = ZDateTime.UtcNow.AddDays(3);
			AIRCRMessageBuilder builder = new AIRCRMessageBuilder(hawb, true);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = hawb.Messages;
			var message = builder.PopulateMessagesReturningResult();
			AssertNotEquals(ZDateTime.Empty, message.EM_HeldUntilDate);
			hawb.MAWB.CM_ArrivalDate = Enterprise.ZArchitecture.Environment.EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", ZDateTime.UtcNow.AddHours(-1).ToDateTime());
			message = builder.PopulateMessagesReturningResult();
			AssertEquals(ZDateTime.Empty, message.EM_HeldUntilDate);
		}

		public void TestCreateEndToEnd()
		{
			#region ExpectedMessage

			ZString expectedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+933:::AIRCR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'
RFF+PQ:A'
RFF+HWB:1'
RFF+MWB:08144014703'
NAD+CN++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'
NAD+CZ++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'
NAD+VW+521309239::95'
TDT+20+439++6+QF::3'
LOC+8+AUSYD::6'
LOC+76+NZAKL::6'
LOC+12+AUSYD::6'
LOC+91+NZAKL::6'
DTM+178:20041126:102'
CNI+1'
RFF+UCN:<<SENDERS REFERENCE PLACE HOLDER>>'
MOA+44:100.00:AUD'
GID+1'
PAC+10'
FTX+AAA+++STUFF'
MEA+AAE+G+KG:100.00'
UNT+22+<<MSGNO PLACEHOLDER>>'
";
			#endregion
			AssertMultilineEquals("ExpectedMessage", expectedMessage.Replace("\r\n", ""), GeneratedMessage, '\'');
		}

		public void TestWithdrawEndToEnd()
		{
			#region ExpectedMessage

			ZString expectedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+933:::AIRCR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:1+50'
RFF+HWB:1'
RFF+MWB:08144014703'
TDT+20+439++6+QF::3'
DTM+178:20041126:102'
UNT+7+<<MSGNO PLACEHOLDER>>'
";
			#endregion
			messageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertMultilineEquals("ExpectedMessage", expectedMessage.Replace("\r\n", ""), GeneratedMessage, '\'');
		}

		public void TestParentSubMasterAirWaybillNumber()
		{
			hawb.CS_MasterHouseBill = "123";
			Assert(GeneratedMessage.Contains("RFF+AWB:123'"));
		}

		public void TestParentSubMasterAirWaybillNumberNoIncludedInWithdrawals()
		{
			messageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			hawb.CS_MasterHouseBill = "123";
			Assert(!GeneratedMessage.Contains("RFF+AWB:123'"));
		}

		public void TestHouseAirWaybillNumber()
		{
			hawb.CS_HAWB = "24680";
			Assert(GeneratedMessage.Contains("RFF+HWB:24680'"));
		}

		public void TestMasterAirWaybillNumber()
		{
			hawb.MAWB.CM_MAWB = "123 33333333";
			Assert(GeneratedMessage.Contains("RFF+MWB:12333333333'"));
		}

		public void TestMethodOfPayment()
		{
			hawb.CS_GoodsValue = 100m;
			hawb.CS_FreightPrepaidCollect = "MX";
			Assert(GeneratedMessage.Contains("RFF+PQ:MX'"));
		}

		public void TestLegacyMethodOfPayment()
		{
			hawb.CS_GoodsValue = 100m;
			hawb.CS_FreightPrepaidCollect = "CCX";
			Assert(GeneratedMessage.Contains("RFF+PQ:CC'"));
		}

		public void TestHVLVSpecialReporterNumber()
		{
			Env.Registry.AUCustoms.HVLVSpecialReporterNumber = "123456";
			hawb.CS_IsSpecialReporter = true;
			Assert(GeneratedMessage.IndexOf("NAD+AQ+123456::95'") != -1);
		}

		public void TestResponsiblePartyID()
		{
			Assert("Generated message was: " + GeneratedMessage, GeneratedMessage.Contains("NAD+VW+521309239::95'"));
			hawb.MAWB.CM_ResponsiblePartyID = "";
			Assert("Generated message was: " + GeneratedMessage, GeneratedMessage.Contains("NAD+VW+41065894724::95'"));
		}

		public void TestHVLVSpecialReporterNumberNotSentByDefault()
		{
			Env.Registry.AUCustoms.HVLVSpecialReporterNumber = "123456";
			hawb.CS_IsSpecialReporter = false;
			Assert(GeneratedMessage.IndexOf("NAD+AQ+123456::95'") == -1);
		}

		public void TestRemailSpecialReporterNumber()
		{
			Env.Registry.AUCustoms.RemailSpecialReporterNumber = "654321";
			hawb.CS_IsRemailReporter = true;
			Assert(GeneratedMessage.IndexOf("NAD+AQ+654321::95'") != -1);
		}

		public void TestConsignee()
		{
			Assert("Generated message was: " + GeneratedMessage, GeneratedMessage.Contains("NAD+CN++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'"));
		}

		public void TestConsignor()
		{
			Assert("Generated message was: " + GeneratedMessage, GeneratedMessage.Contains("NAD+CZ++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'"));
		}

		public void TestConsigneeManualEntry()
		{
			hawb.CS_OH_Consignee = ZGuid.Empty;
			hawb.CS_OA_ConsigneeAddress = ZGuid.Empty;
			hawb.CS_ConsigneeName = "MNAME";
			hawb.CS_ConsigneeStreet = "MADDRESS1";
			hawb.CS_ConsigneeStreet2 = "MADDRESS2";
			hawb.CS_ConsigneeCity = "MCITY";
			hawb.CS_ConsigneeState = "MSTATE";
			hawb.CS_ConsigneePostcode = "M12345";
			hawb.CS_RN_NKConsigneeCountry = "AU";
			hawb.CS_ConsigneePhone = "000";
			AssertContains("NAD+CN++MNAME::MADDRESS1 MADDRESS2 MCITY MSTATE M1:2345 AU'", GeneratedMessage);
		}

		public void TestConsignorManualEntry()
		{
			hawb.CS_OH_Consignor = ZGuid.Empty;
			hawb.CS_OA_ConsignorAddress = ZGuid.Empty;
			hawb.CS_ConsignorName = "MNAME";
			hawb.CS_ConsignorStreet = "MADDRESS1";
			hawb.CS_ConsignorStreet2 = "MADDRESS2";
			hawb.CS_ConsignorCity = "MCITY";
			hawb.CS_ConsignorState = "MSTATE";
			hawb.CS_ConsignorPostcode = "M12345";
			hawb.CS_RN_NKConsignorCountry = "AU";
			hawb.CS_ConsignorPhone = "000";
			AssertContains("NAD+CZ++MNAME::MADDRESS1 MADDRESS2 MCITY MSTATE M1:2345 AU'", GeneratedMessage);
		}

		public void TestResponsiblePartyClientID()
		{
			Assert(GeneratedMessage.Contains("NAD+VW+521309239::95'"));
		}

		public void TestAddress1AndAddress2AreLong()
		{
			hawb.Consignor.MainAddress.OA_Address1 = "THIS IS A REALLY LONG ADRESS THAT";
			hawb.Consignor.MainAddress.OA_Address2 = "WILL NEED TO BE SPLITUP";

			#region ExpectedMessage

			ZString expectedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+933:::AIRCR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'
RFF+PQ:A'
RFF+HWB:1'
RFF+MWB:08144014703'
NAD+CN++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'
NAD+CZ++NAME::THIS IS A REALLY LONG ADRESS THAT W:ILL NEED TO BE SPLITUP CITY NSW 123:45 AU'
NAD+VW+521309239::95'
TDT+20+439++6+QF::3'
LOC+8+AUSYD::6'
LOC+76+NZAKL::6'
LOC+12+AUSYD::6'
LOC+91+NZAKL::6'
DTM+178:20041126:102'
CNI+1'
RFF+UCN:<<SENDERS REFERENCE PLACE HOLDER>>'
MOA+44:100.00:AUD'
GID+1'
PAC+10'
FTX+AAA+++STUFF'
MEA+AAE+G+KG:100.00'
UNT+22+<<MSGNO PLACEHOLDER>>'
";
			#endregion
			AssertMultilineEquals("ExpectedMessage", expectedMessage.Replace("\r\n", ""), GeneratedMessage, '\'');
		}

		public void TestLongGoodsDescription()
		{
			hawb.CS_GoodsDescription = ZString.Empty;
			for (int i = 0; i < 40; i++)
			{
				hawb.CS_GoodsDescription += "FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.";
			}

			ZString expectedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+933:::AIRCR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'
RFF+PQ:A'
RFF+HWB:1'
RFF+MWB:08144014703'
NAD+CN++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'
NAD+CZ++NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'
NAD+VW+521309239::95'
TDT+20+439++6+QF::3'
LOC+8+AUSYD::6'
LOC+76+NZAKL::6'
LOC+12+AUSYD::6'
LOC+91+NZAKL::6'
DTM+178:20041126:102'
CNI+1'
RFF+UCN:<<SENDERS REFERENCE PLACE HOLDER>>'
MOA+44:100.00:AUD'
GID+1'
PAC+10'
FTX+AAA+++FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.:FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.:FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.:FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.:FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.FIVE HUNDRED AND TWELVE CHARACTERS OVER AND OVER AND OVER AGAIN.'
MEA+AAE+G+KG:100.00'
UNT+22+<<MSGNO PLACEHOLDER>>'
";

			AssertMultilineEquals("ExpectedMessage", expectedMessage.Replace("\r\n", ""), GeneratedMessage, '\'');
		}

		public void TestIdentification()
		{
			var consignee = SetupOrgHeader();
			consignee.OH_Code = "TSTCN";
			consignee.OH_FullName = "CONSIGNEE";
			consignee.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "112345678901");
			consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1987654321");
			SetConsignee(hawb, consignee);

			var consignor = SetupOrgHeader();
			consignor.OH_Code = "TSTCZ";
			consignor.OH_FullName = "CONSIGNOR";
			consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "212345678901");
			consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "2987654321");
			SetConsignor(hawb, consignor);

			var generatedMessage = GeneratedMessage;

			AssertContains("NAD+CN++CONSIGNEE::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'", generatedMessage);
			AssertContains("NAD+CZ++CONSIGNOR::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'", generatedMessage);
			AssertContains("Has Consignee CID", "NAD+IM+1987654321::95'", generatedMessage);
			AssertContains("Has Consignor CID", "NAD+SU+2987654321::95'", generatedMessage);
			AssertContains("Has ABN", "NAD+VN+212345678901::95'", generatedMessage);
		}

		public void TestTrustedTraderIdentification()
		{
			var consignee = SetupOrgHeader();
			consignee.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			consignee.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "IMPTIN1234567890", Core.Constants.CountryCodes.Australia);
			SetConsignee(hawb, consignee);

			var consignor = SetupOrgHeader();
			consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "CONTIN1234567890", Core.Constants.CountryCodes.Australia);
			SetConsignor(hawb, consignor);

			var generatedMessage = GeneratedMessage;

			AssertContains("Generated message has Importer TIN code.", "NAD+AU+IMPTIN1234567890::95'", generatedMessage);
			AssertContains("Generated message has Consignor TIN code.", "NAD+CZ+CONTIN1234567890+NAME::ADDRESS1 ADDRESS2 CITY NSW 12345 AU'", generatedMessage);

			AssertNotContains("Importer (IM), Authorised Importer (AT) and SubEntity (WP) are not sent with TIN", "NAD+IM", generatedMessage);
			AssertNotContains("Importer (IM), Authorised Importer (AT) and SubEntity (WP) are not sent with TIN", "NAD+AT", generatedMessage);
			AssertNotContains("Importer (IM), Authorised Importer (AT) and SubEntity (WP) are not sent with TIN", "NAD+WP", generatedMessage);
		}

		public void TestFlightNumber()
		{
			Assert(GeneratedMessage.Contains("TDT+20+439++6+QF::3'"));
		}

		public void TestPortOfDestination()
		{
			hawb.CS_RL_NKDestination = "AUNTL";
			Assert(GeneratedMessage.Contains("LOC+8+AUNTL::6'"));
		}

		public void TestPortOfDischarge()
		{
			hawb.MAWB.CM_RL_NKDischargePort = "AUBNE";
			Assert(GeneratedMessage.Contains("LOC+12+AUBNE::6'"));
		}

		public void TestOriginalPortOfLoading()
		{
			hawb.MAWB.CM_RL_NKLoadPort = "NZAKL";
			Assert(GeneratedMessage.Contains("LOC+76+NZAKL::6'"));
		}

		public void TestFirstAustralianPort()
		{
			hawb.CS_RL_NKOrigin = "NZAKL";
			hawb.MAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			hawb.CS_RL_NKDestination = "SGSIN";
			Assert(GeneratedMessage.Contains("LOC+79+AUSYD::6'"));
		}

		public void TestWaybillOrigin()
		{
			hawb.CS_RL_NKOrigin = "NZAKL";
			Assert(GeneratedMessage.Contains("LOC+91+NZAKL::6'"));
		}

		public void TestDateOfArrival()
		{
			hawb.MAWB.CM_ArrivalDate = new ZDateTime(2004, 11, 16);
			Assert(GeneratedMessage.Contains("DTM+178:20041116:102'"));
		}

		public void TestFreightForwarderIndicator()
		{
			hawb.CS_IsMasterHouse = true;
			Assert(GeneratedMessage.Contains("GIS+FFO:109:95'"));
			hawb.CS_IsMasterHouse = false;
			Assert(!GeneratedMessage.Contains("GIS+FFO:109:95'"));
		}

		public void TestReportableDocumentsIndicator()
		{
			hawb.IsDocuments = true;
			Assert("Sement Present", GeneratedMessage.Contains("GIS+DOC:109:95'"));
			hawb.IsDocuments = false;
			Assert("Sement Not Present", !GeneratedMessage.Contains("GIS+DOC:109:95'"));
		}

		public void TestUCN()
		{
			Assert(GeneratedMessage.Contains("CNI+1'RFF+UCN:"));
		}

		public void TestDeclaredValue()
		{
			hawb.CS_GoodsValue = 100m;
			hawb.CS_RX_NKGoodsCurrency = "USD";
			Assert(GeneratedMessage.Contains("MOA+44:100.00:USD'"));
			hawb.CS_GoodsValue = 0m;
			Assert(GeneratedMessage.Contains("MOA+96:NDV'"));
		}

		public void TestDeclaredValueSegmentIfThereIsNoCurrency()
		{
			hawb.CS_GoodsValue = 100m;
			hawb.CS_RX_NKGoodsCurrency = ZString.Empty;
			Assert(!GeneratedMessage.Contains("MOA+44:"));
		}

		public void TestPersonalEffectsIndicator()
		{
			hawb.CS_IsPersonalEffects = true;
			Assert("Sement Present", GeneratedMessage.Contains("GIS+PER:109:95'"));
			hawb.CS_IsPersonalEffects = false;
			Assert("Sement Not Present", !GeneratedMessage.Contains("GIS+PER:109:95'"));
		}

		public void TestSelfAssessedClearanceIndicator()
		{
			hawb.CS_IsSelfAssessedClearance = true;
			Assert("Sement Present", GeneratedMessage.Contains("GIS+SAC:109:95'"));
			hawb.CS_IsSelfAssessedClearance = false;
			Assert("Sement Not Present", !GeneratedMessage.Contains("GIS+SAC:109:95'"));
		}

		public void TestNumberOfPackages()
		{
			hawb.CS_PiecesManifested = 10;
			Assert(GeneratedMessage.Contains("GID+1'PAC+10'"));
		}

		public void TestGoodsDescription()
		{
			hawb.CS_GoodsDescription = "Description";
			Assert(GeneratedMessage.Contains("FTX+AAA+++DESCRIPTION'"));
			Assert(!GeneratedMessage.Contains("FTX+AAA+++DESCRIPTION:"));
		}

		public void TestDontPopulateGoodsDescriptionIfThereIsNone()
		{
			hawb.CS_GoodsDescription = ZString.Empty;
			Assert(!GeneratedMessage.Contains("FTX+AAA"));
		}

		public void TestGrossWeight()
		{
			Assert(GeneratedMessage.Contains("MEA+AAE+G+KG:100.00'"));
		}

		public void TestGrossWeightRounding()
		{
			hawb.CS_Weight = 1.005m;
			hawb.CS_WeightUQ = "KG";
			Assert(GeneratedMessage.Contains("MEA+AAE+G+KG:1.01'"));
		}

		public void TestGrossWeightToGrams()
		{
			hawb.CS_Weight = 0.005m;
			hawb.CS_WeightUQ = "KG";
			Assert(GeneratedMessage.Contains("MEA+AAE+G+G:5.00'"));
		}

		public void TestGrossWeightLBsRounding()
		{
			hawb.CS_Weight = 0.005m;
			hawb.CS_WeightUQ = "LB";
			Assert(GeneratedMessage.Contains("MEA+AAE+G+LB:0.01'"));
		}

		public void TestGrossWeightToGramsOnlyIfNecessary()
		{
			hawb.CS_Weight = 0.25m;
			hawb.CS_WeightUQ = "KG";
			Assert(GeneratedMessage.Contains("MEA+AAE+G+KG:0.25'"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "41065894724");
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_ResponsiblePartyID = "521309239";
			mawb.CM_MAWB = "08144014703";
			mawb.CM_FlightNo = "QF439";
			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "AUSYD";
			mawb.CM_ArrivalDate = new ZDateTime(2004, 11, 26);
			hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "1";
			hawb.CS_FreightPrepaidCollect = "A";
			hawb.CS_RL_NKDestination = "AUSYD";
			hawb.CS_RL_NKOrigin = "NZAKL";
			hawb.CS_GoodsValue = 100m;
			hawb.CS_RX_NKGoodsCurrency = "AUD";
			hawb.CS_PiecesManifested = 10;
			hawb.CS_GoodsDescription = "STUFF";
			hawb.CS_Weight = 100m;
			hawb.CS_WeightUQ = "KG";
			SetConsignee(hawb, SetupOrgHeader());
			SetConsignor(hawb, SetupOrgHeader());
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

		protected virtual void SetConsignee(CusHAWB house, OrgHeader consignee)
		{
			house.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
		}

		protected virtual void SetConsignor(CusHAWB house, OrgHeader consignor)
		{
			house.CS_OA_ConsignorAddress = consignor.MainAddress.PK;
		}

		AIRCRMessageBuilder Builder
		{
			get
			{
				var builder = new AIRCRMessageBuilder(hawb);
				builder.MessageSubType = messageSubType;
				builder.Messages = hawb.Messages;
				return builder;
			}
		}

		Common.MessageBuilders.MessageSubTypes messageSubType = Common.MessageBuilders.MessageSubTypes.Create;
		protected CusHAWB hawb;
	}
}
