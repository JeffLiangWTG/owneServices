using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManOBLHeaderSeaCargoReportHeaderTest : TestCaseWithFactory
	{
		public void TestICSRelease()
		{
			AssertEquals(ZString.Empty, ReportHeader.ConsigneeIdentifier);
			AssertEquals(ZString.Empty, ReportHeader.ConsigneeABN);
			AssertEquals(ZString.Empty, ReportHeader.ConsigneeCAC);
			AssertEquals(ZString.Empty, ReportHeader.ConsignorIdentifier);
			AssertEquals(ZString.Empty, ReportHeader.ConsignorVendor);
		}

		public void TestHouseBill()
		{
			AssertEquals("HouseBill", ZString.Empty, ReportHeader.HouseBill);
		}

		public void TestParentBill()
		{
			AssertEquals("ParentBill", ZString.Empty, ReportHeader.ParentBill);
		}

		public void TestOceanBill()
		{
			Header.BO_OceanBill = "123";
			AssertEquals("OceanBill", "123", ReportHeader.OceanBill);
		}

		public void TestRouting()
		{
			AssertNull(ReportHeader.Routings);
		}

		public void TestVoyage()
		{
			TransportHeader.BT_VoyageNum = "321";
			AssertEquals("Voyage", "321", ReportHeader.Voyage);
		}

		public void TestLloydsNumber()
		{
			TransportHeader.BT_VesselName = "ADMIRALENGRACHT";
			AssertEquals("LloydsNumber", "8811924", ReportHeader.LloydsNumber);
		}

		public void TestNotifyParty()
		{
			AssertEquals("NotifyParty", null, ReportHeader.NotifyParty);
		}

		public void TestOriginCountry()
		{
			Header.BO_RN_NKGoodsCountryOfOrigin = "US";
			AssertEquals("OriginCountry", "US", ReportHeader.OriginCountry);
		}

		public void TestIsConsolidation()
		{
			Header.BO_FreightForwarderIndicator = true;
			AssertEquals("IsConsolidation", true, ReportHeader.IsConsolidation);
		}

		const string ABN = "21092706806";
		public void TestPrincipalID()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ABN;
			AssertEquals("PrincipalID", ABN, ReportHeader.PrincipalID);
		}

		public void TestBusinessObject()
		{
			AssertEquals("BusinessObject", Header, ReportHeader.BusinessObject);
		}

		public void TestMethodOfPayment()
		{
			Header.BO_PaymentMethod = "ABC";
			AssertEquals("MethodOfPayment", "ABC", ReportHeader.MethodOfPayment);
		}

		public void TestOrigin()
		{
			Header.BO_RL_NKOriginPort = "NZAKL";
			AssertEquals("Origin", "NZAKL", ReportHeader.Origin);
		}

		public void TestDestination()
		{
			Header.BO_RL_NKDestinationPort = "AUSYD";
			AssertEquals("Destination", "AUSYD", ReportHeader.Destination);
		}

		public void TestLoading()
		{
			Header.BO_RL_NKLoadPort = "USLAX";
			AssertEquals("Loading", "USLAX", ReportHeader.Loading);
		}

		public void TestDischarge()
		{
			Header.BO_RL_NKDischargePort = "AUMEL";
			AssertEquals("Discharge", "AUMEL", ReportHeader.Discharge);
		}

		public void TestFirstArrivalPort()
		{
			var arrival = TransportHeader.Arrivals.AddNew();
			arrival.BA_RL_NKArrivalPort = "AUSYD";
			arrival.BA_IsFirstArrival = true;
			Header.BO_RL_NKDischargePort = "NZAKL";
			Header.BO_RL_NKDestinationPort = "NZAKL";
			AssertEquals("FirstArrivalPort", "AUSYD", ReportHeader.FirstArrivalPort);
			Header.BO_RL_NKDischargePort = "AUSYD";
			AssertEquals("FirstArrivalPort", ZString.Empty, ReportHeader.FirstArrivalPort);
		}

		public void TestConsignee()
		{
			Header.BO_OH_Consignee = Factory.New(typeof(OrgHeader)).PK;
			Header.Consignee.OH_FullName = "Senior";
			AssertEquals("Consignee", "Senior", ReportHeader.ConsigneeName);
		}

		public void TestConsignor()
		{
			Header.BO_OH_Consignor = Factory.New(typeof(OrgHeader)).PK;
			Header.Consignor.OH_FullName = "Senior";
			AssertEquals("Consignor", "Senior", ReportHeader.ConsignorName);
		}

		public void TestConsigneeNameAndAddressGeneral()
		{
			Header.BO_ConsigneeName = "Name";
			Header.BO_ConsigneeAddress1 = "Street";
			Header.BO_ConsigneeAddress2 = "Street2";
			Header.BO_ConsigneeCity = "City";
			Header.BO_ConsigneePostCode = "";
			Header.BO_RN_NKConsigneeCountryCode = "AU";
			AssertEquals("Street Street2 City  AU", ReportHeader.ConsigneeGeneralAddress);
		}

		public void TestConsignorNameAndAddressGeneral()
		{
			Header.BO_ConsignorName = "Name";
			Header.BO_ConsignorAddress1 = "Street";
			Header.BO_ConsignorAddress2 = "Street2";
			Header.BO_ConsignorCity = "City";
			Header.BO_ConsignorPostCode = "";
			Header.BO_RN_NKConsignorCountryCode = "AU";
			AssertEquals("Street Street2 City  AU", ReportHeader.ConsignorGeneralAddress);
		}

		public void TestConsigneeName()
		{
			Header.BO_ConsigneeName = "ZZZ";
			AssertEquals("ConsigneeName", "ZZZ", ReportHeader.ConsigneeName);
		}

		public void TestConsigneeStreet()
		{
			Header.BO_ConsigneeAddress1 = "XXX";
			AssertEquals("ConsigneeStreet", "XXX", ReportHeader.ConsigneeStreet);
		}

		public void TestConsigneeStreet2()
		{
			Header.BO_ConsigneeAddress2 = "XXX";
			AssertEquals("ConsigneeStreet2", "XXX", ReportHeader.ConsigneeStreet2);
		}

		public void TestConsigneeCity()
		{
			Header.BO_ConsigneeCity = "PPP";
			AssertEquals("ConsigneeCity", "PPP", ReportHeader.ConsigneeCity);
		}

		public void TestConsigneePostCode()
		{
			Header.BO_ConsigneePostCode = "123";
			AssertEquals("ConsigneePostcode", "123", ReportHeader.ConsigneePostCode);
		}

		public void TestConsigneeCountry()
		{
			Header.BO_RN_NKConsigneeCountryCode = "AU";
			AssertEquals("ConsigneeCountry", "AU", ReportHeader.ConsigneeCountry);
		}

		public void TestConsignorName()
		{
			Header.BO_ConsignorName = "QQQ";
			AssertEquals("ConsignorName", "QQQ", ReportHeader.ConsignorName);
		}

		public void TestConsignorStreet()
		{
			Header.BO_ConsignorAddress1 = "TTT";
			AssertEquals("ConsignorStreet", "TTT", ReportHeader.ConsignorStreet);
		}

		public void TestConsignorStreet2()
		{
			Header.BO_ConsignorAddress2 = "SSS";
			AssertEquals("ConsignorStreet2", "SSS", ReportHeader.ConsignorStreet2);
		}

		public void TestConsignorCity()
		{
			Header.BO_ConsignorCity = "RRR";
			AssertEquals("ConsignorCity", "RRR", ReportHeader.ConsignorCity);
		}

		public void TestConsignorPostCode()
		{
			Header.BO_ConsignorPostCode = "555";
			AssertEquals("ConsignorPostcode", "555", ReportHeader.ConsignorPostCode);
		}

		public void TestConsignorCountry()
		{
			Header.BO_RN_NKConsignorCountryCode = "US";
			AssertEquals("ConsignorCountry", "US", ReportHeader.ConsignorCountry);
		}

		public void TestLines()
		{
			AssertEquals("Lines.Length", 0, ReportHeader.Lines.Length);
			Header.Details.AddNew();
		}

		public void TestDatabaseLines()
		{
			Header.Details.AddNew();
			AssertEquals("DatabaseLines.Length", 0, ReportHeader.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("DatabaseLines.Length", 1, ReportHeader.DatabaseLines.Length);
		}

		CusSeaManOBLHeaderSeaCargoReportHeader ReportHeader => new CusSeaManOBLHeaderSeaCargoReportHeader(Header);

		CusSeaManOBLHeader header;
		CusSeaManOBLHeader Header => header ?? (header = TransportHeader.OceanBills.AddNew());

		CusSeaManTranHead transportHeader;
		CusSeaManTranHead TransportHeader => transportHeader ?? (transportHeader = Factory.New<CusSeaManTranHead>());
	}
}
