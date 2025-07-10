using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public partial class MXWsVucemTest
	{
		public void TestUrlsValids()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "MXD";
			company1.GC_RN_NKCountryCode = "MX";
			company1.Branches.AddNew().GB_Code = "MXD";
			Factory.Save();

			var testItem = new MXWsVucem(new FallbackLevel(company1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSResponse = "https://127.0.0.1/wsdl?";
			testItem.SeaModeWSResponse = "http://127.0.0.1/wsdl?";

			AssertNoErrors(testItem.AirModeWSResponseInfo);
			AssertNoErrors(testItem.SeaModeWSResponseInfo);

			testItem.AirModeWSResponse = "invalid://127.0.0.1/wsdl?";
			testItem.SeaModeWSResponse = "ivnalidurl";
			AssertHasErrorContaining(testItem.AirModeWSResponseInfo, "Please enter a valid website address (URL).\r\n\r\nA valid address is commonly found in the format");
			AssertHasErrorContaining(testItem.SeaModeWSResponseInfo, "Please enter a valid website address (URL).\r\n\r\nA valid address is commonly found in the format");
		}

		public void TestAirModeWSResponse()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "MXD";
			company1.GC_RN_NKCountryCode = "MX";
			company1.Branches.AddNew().GB_Code = "MXD";
			Factory.Save();

			var testItem = new MXWsVucem(new FallbackLevel(company1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSResponse = "https://127.0.0.1/wsdl?";

			AssertNoNotifications(testItem.AirModeWSResponseInfo);

			testItem.AirModeWSResponse = "";
			AssertHasWarningContaining(testItem.AirModeWSResponseInfo, "You have not entered an URL for the Air Mode.");
		}

		public void TestSeaModeWSResponse()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "MXD";
			company1.GC_RN_NKCountryCode = "MX";
			company1.Branches.AddNew().GB_Code = "MXD";
			Factory.Save();

			var testItem = new MXWsVucem(new FallbackLevel(company1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.SeaModeWSResponse = "https://127.0.0.1/wsdl?";

			AssertNoNotifications(testItem.SeaModeWSResponseInfo);

			testItem.SeaModeWSResponse = "";
			AssertHasWarningContaining(testItem.SeaModeWSResponseInfo, "You have not entered an URL for the Sea Mode.");
		}

		public void TestUsernameWSAirMode()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "MXD";
			company1.GC_RN_NKCountryCode = "MX";
			company1.Branches.AddNew().GB_Code = "MXD";
			Factory.Save();

			var testItem = new MXWsVucem(new FallbackLevel(company1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSUsername = "ADMINVUCEM1";

			AssertNoErrors(testItem.AirModeWSUsernameInfo);

			testItem.AirModeWSResponse = "https://127.0.0.1/wsdl?";
			testItem.AirModeWSUsername = "";
			AssertHasErrorContaining(testItem.AirModeWSUsernameInfo, "Please enter an Username for the Air Mode.");
		}

		public void TestPasswordWSAirMode()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "MXD";
			company1.GC_RN_NKCountryCode = "MX";
			company1.Branches.AddNew().GB_Code = "MXD";
			Factory.Save();

			var testItem = new MXWsVucem(new FallbackLevel(company1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSPassword = "9974567890";

			AssertNoErrors(testItem.AirModeWSPasswordInfo);

			testItem.AirModeWSResponse = "https://127.0.0.1/wsdl?";
			testItem.AirModeWSPassword = "";
			AssertHasErrorContaining(testItem.AirModeWSPasswordInfo, "Please enter a Password for the Air Mode.");
		}

		public void TestUsernameWSSeaMode()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "MXD";
			company1.GC_RN_NKCountryCode = "MX";
			company1.Branches.AddNew().GB_Code = "MXD";
			Factory.Save();

			var testItem = new MXWsVucem(new FallbackLevel(company1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.SeaModeWSUsername = "ADMINVUCEM13";

			AssertNoErrors(testItem.SeaModeWSUsernameInfo);

			testItem.SeaModeWSResponse = "http://127.0.0.1/wsdl?";
			testItem.SeaModeWSUsername = "";
			AssertHasErrorContaining(testItem.SeaModeWSUsernameInfo, "Please enter an Username for the Sea Mode.");
		}

		public void TestPasswordWSSeaMode()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "MXD";
			company1.GC_RN_NKCountryCode = "MX";
			company1.Branches.AddNew().GB_Code = "MXD";
			Factory.Save();

			var testItem = new MXWsVucem(new FallbackLevel(company1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.SeaModeWSPassword = "9974567890";

			AssertNoErrors(testItem.SeaModeWSPasswordInfo);

			testItem.SeaModeWSResponse = "http://127.0.0.1/wsdl?";
			testItem.SeaModeWSPassword = "";
			AssertHasErrorContaining(testItem.SeaModeWSPasswordInfo, "Please enter a Password for the Sea Mode.");
		}
	}
}
