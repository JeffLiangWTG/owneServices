using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeOrgWebUrlObjectHelper : TestCaseWithFactory
	{
		#region Import

		public void TestImportFromValueObjectCollection()
		{
			Xsd.SysMergeOrgWebUrlCollection xsdWebUrlCollection = new Xsd.SysMergeOrgWebUrlCollection();
			Xsd.SysMergeOrgWebUrl webUrl1 = xsdWebUrlCollection.AddNew();
			Xsd.SysMergeOrgWebUrl webUrl2 = xsdWebUrlCollection.AddNew();
			Xsd.SysMergeOrgWebUrl webUrl3 = xsdWebUrlCollection.AddNew();

			webUrl1.PK = Guid.NewGuid().ToString();
			webUrl1.URL = "www.test.com";

			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());

			webUrl2.PK = Guid.NewGuid().ToString();
			webUrl2.URL = "www.test222.com";

			webUrl3.PK = Guid.NewGuid().ToString();
			webUrl3.URL = "www.test333.com";

			OrgHeaderForDataTransfer org = Factory.New<OrgHeaderForDataTransfer>();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), new NotificationBuffer());
			WebUrlHelper.ImportFromValueObjectCollection(xsdWebUrlCollection, org, context);

			ZQuery query = new ZQuery(OrgWebURLSchema.PU_OH, org.PK);
			OrgWebURL[] webUrls = org.Factory.Load<OrgWebURL>(query);

			AssertEquals("3 imported webUrls should exist", 3, webUrls.Length);

			AssertEquals("www.test.com", webUrls[0].PU_URL);
			AssertEquals("WebUrl01 PK", webUrl1.PK, webUrls[0].PK.ToString());

			AssertEquals("www.test222.com", webUrls[1].PU_URL);
			AssertEquals("WebUrl02 PK", webUrl2.PK, webUrls[1].PK.ToString());

			AssertEquals("www.test333.com", webUrls[2].PU_URL);
			AssertEquals("WebUrl03 PK", webUrl3.PK, webUrls[2].PK.ToString());
		}

		#endregion

		#region Export

		public void TestExportToValueObjectCollection()
		{
			SysMergeOrgWebUrlValueObjectHelper helper = new SysMergeOrgWebUrlValueObjectHelper("Error context");
			OrgHeaderForDataTransfer organisation = Factory.New<OrgHeaderForDataTransfer>();

			OrgWebURL url1 = Factory.New<OrgWebURL>();
			url1.PU_OH = organisation.PK;
			url1.PU_URL = "www.test";

			OrgWebURL url2 = Factory.New<OrgWebURL>();
			url2.PU_OH = organisation.PK;
			url2.PU_URL = "www.test2";

			OrgWebURL url3 = Factory.New<OrgWebURL>();
			url3.PU_OH = organisation.PK;
			url3.PU_URL = "";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.SysMergeOrgWebUrlCollection webUrlValueCollection = new Xsd.SysMergeOrgWebUrlCollection();
			helper.ExportToValueObjectCollection(organisation, webUrlValueCollection, notify);

			AssertEquals("There should be 3 webUrls exported", 3, webUrlValueCollection.Count);

			for (int i = 0; i < webUrlValueCollection.Count; i++)
			{
				switch (webUrlValueCollection[i].URL)
				{
					case "www.test":
						AssertEquals("Url", "www.test", webUrlValueCollection[i].URL);
						break;

					case "www.test2":
						AssertEquals("Url", "www.test2", webUrlValueCollection[i].URL);
						break;

					case "":
						AssertEquals("Url", "", webUrlValueCollection[i].URL);
						break;

					default:
						Fail("Unmatching Web URL: " + webUrlValueCollection[i].URL);
						break;
				}
			}
		}

		#endregion

		#region Implementation

		readonly SysMergeOrgWebUrlValueObjectHelper WebUrlHelper = new SysMergeOrgWebUrlValueObjectHelper("");

		#endregion
	}
}
