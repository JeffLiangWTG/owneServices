using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCountryData.Testing
{
	public class OrgCountryDataInterceptorTest : TestCaseWithFactory
	{
		public void TestWhenBlank()
		{
			TestRunner("", "",
						"", "");
		}

		public void TestWhenBothGood()
		{
			TestRunner("&lt;IamLegalXml1/&gt;", "&lt;IamLegalXml2/&gt;",
						"<IamLegalXml1/>", "<IamLegalXml2/>");
		}
		public void TestWhenOneBlankAndOneGood()
		{
			TestRunner("&lt;IamLegalXml/&gt;", "",
						"<IamLegalXml/>", "");
		}

		public void TestWhenOneBlankAndOneBad()
		{
			TestRunner("&lt;IamLegalXml/&gt;", "XXXXXX",
						"<IamLegalXml/>", "");
		}

		public void TestWhenBothBad()
		{
			TestRunner("XXXXX;", "YYYYY",
						"", "");
		}

		void TestRunner(string inputImport, string inputExport, string expectedResultImport, string expectedReultExport)
		{
			var xml = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.OrgCountryData.TestFile.xml")).ReadToEnd();
			xml = xml.Replace("{{IMPORT}}", inputImport);
			xml = xml.Replace("{{EXPORT}}", inputExport);
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DJC123LHR"));
			var ocd = factory.LoadTop1<MasterFiles.Business.OrgCountryData>(new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, org.PK));
			AssertEquals(expectedReultExport, ocd.OV_CustomsEconomicGroupAddInfo);
			AssertEquals(expectedResultImport, ocd.OV_ImportCustomsDefaultAddInfo);
		}

		#region Test ClientCountryRelation

		public void TestAcceptValidClientCountryRelation()
		{
			var jpRefCountry = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "JP"));
			AssertNotNull("RefCountry with code JP exists", jpRefCountry);

			var xml = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.OrgCountryData.TestFileForCountry.xml")).ReadToEnd();
			xml = xml.Replace("{{CountryCode}}", "JP");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DJC123LHR"));
			var ocd = factory.LoadTop1<MasterFiles.Business.OrgCountryData>(new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, org.PK));
			AssertEquals("JP country data is imported", "JP", ocd.OV_RN_NKClientCountryRelation);
		}

		public void TestAcceptEUClientCountryRelation()
		{
			var xml = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.OrgCountryData.TestFileForCountry.xml")).ReadToEnd();
			xml = xml.Replace("{{CountryCode}}", "EU");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DJC123LHR"));
			var ocd = factory.LoadTop1<MasterFiles.Business.OrgCountryData>(new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, org.PK));
			AssertEquals("EU country data is imported", "EU", ocd.OV_RN_NKClientCountryRelation);

			var euRefCountry = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "EU"));
			AssertNull("There is no RefCountry with code EU", euRefCountry);
		}

		public void TestRejectInvalidClientCountryRelation()
		{
			var zzRefCountry = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "ZZ"));
			AssertNull("Precondition: There is no RefCountry with code ZZ", zzRefCountry);

			var xml = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.OrgCountryData.TestFileForCountry.xml")).ReadToEnd();
			xml = xml.Replace("{{CountryCode}}", "ZZ");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DJC123LHR"));
			AssertNull("Org is not imported", org);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();

			sessionServices = new AncillaryImportServices();
			manager = new ImportHandler(sessionServices);
			setting = new OrgCountryDataSetting();
			setting.Enable = true;
			context = new UpdateContext(sessionServices, new FactoryProvider());
			setting.Context = context;
			context.InterceptorSettings.Add(setting);
			interceptor = new OrgCountryDataInterceptor(setting, sessionServices);
			setting.Interceptor = interceptor;
		}

		AncillaryImportServices sessionServices;
		ImportHandler manager;
		OrgCountryDataSetting setting;
		EntityContext context;
		BusinessObjectFactory factory;
		OrgCountryDataInterceptor interceptor;
	}
}
