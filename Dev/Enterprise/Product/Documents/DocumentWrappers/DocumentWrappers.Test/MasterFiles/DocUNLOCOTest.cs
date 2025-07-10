using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocUNLOCO))]
	public class DocUNLOCOTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocUNLOCO.New(BizUNLOCO, Factory) };
		}

		public void TestPortAndCountryName()
		{
			AssertPortAndCountryName("SGSIN", "Singapore");
			AssertPortAndCountryName("HKHKG", "Hong Kong");
			AssertPortAndCountryName("SMSAI", "San Marino");
			AssertPortAndCountryName("MCMON", "Monaco");
			AssertPortAndCountryName("AUSYD", "Sydney, Australia");
			AssertPortAndCountryName("FRCRQ", "Craponne, France");
			AssertPortAndCountryName("GBXNU", "Nutfield, United Kingdom");
			AssertPortAndCountryName("BEAES", "Asse, Belgium");
		}

		public void TestInvalidCountry()
		{
			var invalidCountryUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			invalidCountryUNLOCO.RL_RN_NKCountryCode = null;
			var invalidDocUNLOCO = DocUNLOCO.New(invalidCountryUNLOCO, Factory);
			AssertEquals(ZString.Empty, invalidDocUNLOCO.CountryName);
			AssertEquals(ZString.Empty, invalidDocUNLOCO.CountryCode);
			AssertEquals(string.Empty, invalidDocUNLOCO.CountryCodeAndName);

			invalidCountryUNLOCO.RL_RN_NKCountryCode = "AA";
			invalidDocUNLOCO = DocUNLOCO.New(invalidCountryUNLOCO, Factory);
			AssertEquals(ZString.Empty, invalidDocUNLOCO.CountryName);
			AssertEquals(ZString.Empty, invalidDocUNLOCO.CountryCode);
			AssertEquals(string.Empty, invalidDocUNLOCO.CountryCodeAndName);
		}

		void AssertPortAndCountryName(string portCode, string expected)
		{
			var unloco = DocUNLOCO.New(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portCode), Factory);
			AssertEquals(expected, unloco.PortNameAndCountryName);
		}

		public void TestPortFromAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.MainAddress.OA_RL_NKRelatedPortCode = "";
			header.OH_RL_NKClosestPort = "AUBNE";

			JobDocAddress address = Factory.New<JobDocAddress>();
			address.E2_OA_Address = header.MainAddress.PK;

			AssertEquals("AUBNE", DocUNLOCO.New(address, Factory).Code);
			AssertEquals("AUBNE", DocUNLOCO.New(header.MainAddress, Factory).Code);

			header.MainAddress.OA_RL_NKRelatedPortCode = "AUCNS";
			AssertEquals("AUCNS", DocUNLOCO.New(address, Factory).Code);
			AssertEquals("AUCNS", DocUNLOCO.New(header.MainAddress, Factory).Code);

			address.E2_AddressOverride = true;
			address.E2_City = "London";
			address.E2_RN_NKCountryCode = "GB";
			AssertEquals("GBLON", DocUNLOCO.New(address, Factory).Code);

			address.E2_City = "Garbage";
			AssertNull(DocUNLOCO.New(address, Factory));

			AssertNull(DocUNLOCO.New((JobDocAddress)null, Factory));
			AssertNull(DocUNLOCO.New((OrgAddress)null, Factory));
		}

		public void TestCode()
		{
			ZString code = new ZString("TEEST");

			BizUNLOCO.RL_Code = code;
			AssertEquals("Wrapped Value", code, DocUNLOCO.Code);
		}

		public void TestPortName()
		{
			ZString portName = new ZString("Test Description");

			BizUNLOCO.RL_PortName = portName;
			AssertEquals("Wrapped Value", portName, DocUNLOCO.PortName);
		}

		public void TestPortNameAndCountryName()
		{
			BizUNLOCO.RL_PortName = "Dong's Port";
			BizUNLOCO.Country.RN_Desc = "Ding's Country";
			AssertEquals("Wrapped Value", "Dong's Port, Ding's Country", DocUNLOCO.PortNameAndCountryName);
		}

		public void TestCountryNameIsNullError()
		{
			var uNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			var bizo = DocUNLOCO.New(uNLOCO, Factory);
			BizUNLOCO.Country.RN_Desc = "Invalid code";
			AssertEquals(string.Empty, bizo.CountryName);
		}

		[TestDate(2006, 12, 25)]
		public void TestBeginDaySaving()
		{
			RefUNLOCO uNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();

			uNLOCO.RL_R3 = timeZoneSet.PK;
			timeZoneSet.HasDaylightSavings = true;
			timeZoneSet.DaylightSavingZone.StartDateRules.AddNew();
			timeZoneSet.DaylightSavingZone.EndDateRules.AddNew();

			DocUNLOCO = DocUNLOCO.New(uNLOCO, Factory);

			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_FromYear = 2000;
			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_ToYear = 2007;
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_FromYear = 2000;
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_ToYear = 2007;

			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_DaylightSavingDate = new ZDateTime(2006, 05, 01, 02, 05, 09);
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_DaylightSavingDate = new ZDateTime(2006, 12, 01, 02, 05, 09);

			Factory.Save();

			AssertEquals("BeginDaySaving is not set correctly", timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_DaylightSavingDate, DocUNLOCO.BeginDaySaving);

			timeZoneSet.DaylightSavingZone.Delete();
			AssertEquals("BeginDaySaving should be empty", ZDateTime.Empty, DocUNLOCO.BeginDaySaving);

			timeZoneSet.Delete();
			AssertEquals("BeginDaySaving should be empty", ZDateTime.Empty, DocUNLOCO.BeginDaySaving);
		}

		[TestDate(2006, 12, 25)]
		public void TestEndDaySaving()
		{
			RefUNLOCO uNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();

			uNLOCO.RL_R3 = timeZoneSet.PK;
			timeZoneSet.HasDaylightSavings = true;
			timeZoneSet.DaylightSavingZone.StartDateRules.AddNew();
			timeZoneSet.DaylightSavingZone.EndDateRules.AddNew();

			DocUNLOCO = DocUNLOCO.New(uNLOCO, Factory);

			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_FromYear = 2000;
			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_ToYear = 2007;
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_FromYear = 2000;
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_ToYear = 2007;

			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			timeZoneSet.DaylightSavingZone.StartDateRules[0].R4_DaylightSavingDate = new ZDateTime(2006, 05, 01, 02, 05, 09);
			timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_DaylightSavingDate = new ZDateTime(2006, 12, 01, 02, 05, 09);

			Factory.Save();

			AssertEquals("EndDaySaving is not set correctly", timeZoneSet.DaylightSavingZone.EndDateRules[0].R4_DaylightSavingDate, DocUNLOCO.EndDaySaving);

			timeZoneSet.DaylightSavingZone.Delete();
			AssertEquals("EndDaySaving should be empty", ZDateTime.Empty, DocUNLOCO.EndDaySaving);

			timeZoneSet.Delete();
			AssertEquals("EndDaySaving should be empty", ZDateTime.Empty, DocUNLOCO.EndDaySaving);
		}

		public void TestGMT()
		{
			RefUNLOCO uNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();

			uNLOCO.RL_R3 = timeZoneSet.PK;
			timeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = (ZShort)240;

			DocUNLOCO = DocUNLOCO.New(uNLOCO, Factory);

			AssertEquals("GMT is set incorrectly", timeZoneSet.StandardZone.R2_OffsetMinutesFromUTC, DocUNLOCO.GMT);

			timeZoneSet.StandardZone.Delete();
			AssertEquals("GMT should be zero", ZShort.Zero, DocUNLOCO.GMT);

			timeZoneSet.Delete();
			AssertEquals("GMT should be zero", ZShort.Zero, DocUNLOCO.GMT);
		}

		public void TestHasDaylightSaving()
		{
			RefUNLOCO uNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			uNLOCO.RL_R3 = timeZoneSet.PK;

			timeZoneSet.HasDaylightSavings = true;
			DocUNLOCO = DocUNLOCO.New(uNLOCO, Factory);

			AssertEquals("HasDaylightSaving is set incorrectly", timeZoneSet.HasDaylightSavings, DocUNLOCO.HasDaylightSaving);

			timeZoneSet.Delete();
			AssertEquals("HasDaylightSaving should be false", false, DocUNLOCO.HasDaylightSaving);
		}

		public void TestIsActive()
		{
			ZBool isActive = ZBool.True;

			BizUNLOCO.RL_IsActive = isActive;
			AssertEquals("Wrapped Value", isActive, DocUNLOCO.IsActive);
		}

		public void TestToString()
		{
			ZString code = new ZString("TEEST");

			BizUNLOCO.RL_Code = code;
			AssertEquals("Wrapped Value", code, DocUNLOCO.ToString());
		}

		#region Implementation

		protected override void SetUp()
		{
			BizUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			DocUNLOCO = (DocUNLOCO)GetDocumentWrappers()[0];
			AssertNotNull("PreCondition: Valid DocUNLOCO", DocUNLOCO);

			base.SetUp();
		}

		RefUNLOCO BizUNLOCO;
		DocUNLOCO DocUNLOCO;

		#endregion
	}
}
