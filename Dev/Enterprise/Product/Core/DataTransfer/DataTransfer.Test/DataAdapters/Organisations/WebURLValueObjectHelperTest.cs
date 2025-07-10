using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class WebURLValueObjectHelperTest : TestCaseWithFactory
	{
		#region Import

		public void TestImportFromValueObjectCollection()
		{
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.OrgWebURLCollection uRLsValueCollection = new Xsd.OrgWebURLCollection();

			Xsd.OrgWebURL mainURLValue = uRLsValueCollection.AddNew();
			mainURLValue.URL = "www.main";
			mainURLValue.Type = Enterprise.DataTransfer.Xml.XsdVersion1.OrgWebURLType.MAI;
			mainURLValue.IsPrimary = true;
			Xsd.OrgWebURL otherExistingURLValue = uRLsValueCollection.AddNew();
			otherExistingURLValue.URL = "www.existing";
			otherExistingURLValue.Description = "new desc";
			otherExistingURLValue.Type = Enterprise.DataTransfer.Xml.XsdVersion1.OrgWebURLType.CRT;
			Xsd.OrgWebURL otherNewURLValue = uRLsValueCollection.AddNew();
			otherNewURLValue.URL = "www.other";
			otherNewURLValue.Type = Enterprise.DataTransfer.Xml.XsdVersion1.OrgWebURLType.CRT;

			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgWebURL existingURL = organisation.OrgWebURLs.AddNew();
			existingURL.PU_Type = OrgWebUrlList.Codes.CartageTracking;
			existingURL.PU_URL = "www.existing";
			existingURL.PU_Description = "old desc";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			Helper.ImportFromValueObjectCollection(uRLsValueCollection, organisation, context);
			AssertEquals("There should be 3 addresses imported", 3, organisation.OrgWebURLs.Count);
			AssertEquals("Already existing url should be updated", "www.existing", organisation.OrgWebURLs[0].PU_URL);
			AssertEquals("Already existing url should be updated", "new desc", organisation.OrgWebURLs[0].PU_Description);
			AssertEquals("Existing url should not be primary", false, organisation.OrgWebURLs[0].PU_IsPrimary);
			AssertEquals("Main url", "www.main", organisation.OrgWebURLs[1].PU_URL);
			AssertEquals("Main url should be primary", true, organisation.OrgWebURLs[1].PU_IsPrimary);
			AssertEquals("New url should be added", "www.other", organisation.OrgWebURLs[2].PU_URL);
			AssertEquals("New url should not be primary", false, organisation.OrgWebURLs[2].PU_IsPrimary);
		}

		#endregion

		#region Export

		public void TestExportToValueObjectCollection()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			var url = organisation.OrgWebURLs.AddNew();
			var mainUrl = organisation.OrgWebURLs.AddNew();

			url.PU_Type = OrgWebUrlList.Codes.CartageTracking;
			url.PU_URL = "www.wht";
			url.PU_IsPrimary = false;
			url.PU_Description = "not Main";

			mainUrl.PU_URL = "www.main";
			mainUrl.PU_IsPrimary = true;
			mainUrl.PU_Description = "Main";
			mainUrl.PU_Type = OrgWebUrlList.Codes.MainWebsite;

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.OrgWebURLCollection urlValueCollection = new Xsd.OrgWebURLCollection();
			Helper.ExportToValueObjectCollection(organisation.OrgWebURLs, urlValueCollection, notify);

			AssertEquals("There should be 2 urls exported", 2, urlValueCollection.Count);
			AssertEquals("Sequence", 1, urlValueCollection[0].Sequence);
			AssertEquals("MainWebURL must be exported first", "www.main", urlValueCollection[0].URL);
			AssertEquals("Sequence", 2, urlValueCollection[1].Sequence);
			AssertEquals("Other urls are exported subsequently", "www.wht", urlValueCollection[1].URL);
		}

		#endregion

		#region Implementation

		readonly WebURLValueObjectHelper Helper = new WebURLValueObjectHelper("");

		#endregion
	}
}
