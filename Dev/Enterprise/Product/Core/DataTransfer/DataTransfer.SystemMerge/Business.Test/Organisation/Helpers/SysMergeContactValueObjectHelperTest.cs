using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeContactValueObjectHelperTest : TestCaseWithFactory
	{
		#region Import

		public void TestImportFromValueObjectCollection()
		{
			Xsd.SysMergeOrgContactCollection xsdContactCollection = new Xsd.SysMergeOrgContactCollection();
			Xsd.SysMergeOrgContact contact1 = xsdContactCollection.AddNew();
			Xsd.SysMergeOrgContact contact2 = xsdContactCollection.AddNew();
			Xsd.SysMergeOrgContact contact3 = xsdContactCollection.AddNew();

			contact1.PK = Guid.NewGuid().ToString();
			contact1.ContactName = "Contact01";
			Xsd.SysMergeOrgDocument doc1 = contact1.OrgDocuments.AddNew();
			Xsd.SysMergeOrgDocument doc2 = contact1.OrgDocuments.AddNew();
			doc1.SU_MenuItemPK = Guid.NewGuid().ToString();
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			doc2.SU_MenuItemPK = menuItem.PK.ToString();

			contact2.PK = Guid.NewGuid().ToString();
			contact2.ContactName = "Contact02";
			contact2.Birthday = new DateTime(1981, 2, 24);
			contact2.WebAccessEnabled = true;

			contact3.PK = Guid.NewGuid().ToString();
			contact3.ContactName = "Contact03";
			contact3.WebAccessEnabled = false;
			contact3.Email = "noone@lovesme.com";

			OrgHeaderForDataTransfer org = Factory.New<OrgHeaderForDataTransfer>();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), new NotificationBuffer());
			ContactHelper.ImportFromValueObjectCollection(xsdContactCollection, org, context);

			ZQuery query = new ZQuery(OrgContactSchema.OC_OH, org.PK);
			OrgContact[] contacts = org.Factory.Load<OrgContact>(query);

			AssertEquals("3 imported contacts should exist", 3, contacts.Length);

			AssertEquals("Contact01", contacts[0].OC_ContactName);
			AssertEquals("Contact01 PK", contact1.PK, contacts[0].PK.ToString());
			AssertEquals("1 doc imported", 1, contacts[0].Documents.Count);
			AssertEquals("Correct doc imported", menuItem.PK, contacts[0].Documents[0].OD_SU_MenuItem);

			AssertEquals("Contact02", contacts[1].OC_ContactName);
			AssertEquals("Contact02 PK", contact2.PK, contacts[1].PK.ToString());
			AssertEquals("Contact02 DOB", new ZDateTime(1981, 2, 24), contacts[1].OC_Birthday);
			AssertEquals("Contact02 Web Access", true, contacts[1].OC_WebAccessEnabled);

			AssertEquals("Contact03", contacts[2].OC_ContactName);
			AssertEquals("Contact03 PK", contact3.PK, contacts[2].PK.ToString());
			AssertEquals("Contact03 Web Access", false, contacts[2].OC_WebAccessEnabled);
			AssertEquals("Contact03 Email", "noone@lovesme.com", contacts[2].OC_Email);
		}

		#endregion

		#region Export

		public void TestExportToValueObjectCollection()
		{
			SysMergeContactValueObjectHelper helper = new SysMergeContactValueObjectHelper("Error context");
			OrgHeaderForDataTransfer organisation = Factory.New<OrgHeaderForDataTransfer>();

			OrgContact contact1 = Factory.New<OrgContact>();
			contact1.OC_OH = organisation.PK;
			contact1.OC_ContactName = "Contact1";
			contact1.OC_Phone = "Phone1";
			OrgContact contact2 = Factory.New<OrgContact>();
			contact2.OC_OH = organisation.PK;
			contact2.OC_ContactName = "Contact2";
			contact2.OC_Language = Core.Constants.Languages.Malay;
			OrgContact contact3 = Factory.New<OrgContact>();
			contact3.OC_OH = organisation.PK;
			contact3.OC_ContactName = "";
			contact3.OC_Birthday = new ZDateTime(2009, 01, 04);

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = false;

			OrgDocument doc1 = contact1.Documents.AddNew();
			doc1.OD_SU_MenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_IsSystemDefined, true)).PK;
			doc1.OD_AttachmentType = "1";

			OrgDocument doc2 = contact1.Documents.AddNew();
			doc2.OD_SU_MenuItem = ZGuid.NewZGuid();
			doc2.OD_AttachmentType = "2";

			OrgDocument doc3 = contact1.Documents.AddNew();
			doc3.OD_DocumentGroup = "XXX";
			doc3.OD_AttachmentType = "3";

			OrgDocument doc4 = contact1.Documents.AddNew();
			doc4.OD_SU_MenuItem = menuItem.PK;
			doc4.OD_AttachmentType = "4";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.SysMergeOrgContactCollection contactValueCollection = new Xsd.SysMergeOrgContactCollection();
			helper.ExportToValueObjectCollection(organisation, contactValueCollection, notify);

			AssertEquals("There should be 3 contacts exported", 3, contactValueCollection.Count);

			for (int i = 0; i < contactValueCollection.Count; i++)
			{
				switch (contactValueCollection[i].ContactName)
				{
					case "Contact1":
						AssertEquals("Phone", "Phone1", contactValueCollection[i].Phone);
						AssertEquals("Birthday has value?", false, contactValueCollection[i].Birthday.HasValue);
						AssertEquals(2, contactValueCollection[i].OrgDocuments.Count);
						int index1 = 0, index2 = 1;
						if (contactValueCollection[i].OrgDocuments[0].AttachmentType != "1")
						{
							index2 = 0;
							index1 = 1;
						}
						AssertEquals("1", contactValueCollection[i].OrgDocuments[index1].AttachmentType);
						AssertEquals("3", contactValueCollection[i].OrgDocuments[index2].AttachmentType);
						break;

					case "Contact2":
						AssertEquals("Phone", Core.Constants.Languages.Malay, contactValueCollection[i].Language);
						AssertEquals("Language", Core.Constants.Languages.Malay, contactValueCollection[i].Language);
						AssertEquals("Birthday has value?", false, contactValueCollection[i].Birthday.HasValue);
						break;

					case "":
						AssertEquals("Phone", "", contactValueCollection[i].Phone);
						AssertEquals("Birthday", new DateTime(2009, 01, 04), contactValueCollection[i].Birthday.Value);
						break;

					default:
						Fail("Unmatching Contact Name: " + contactValueCollection[i].ContactName);
						break;
				}
			}
		}

		#endregion

		#region Implementation

		readonly SysMergeContactValueObjectHelper ContactHelper = new SysMergeContactValueObjectHelper("");

		#endregion
	}
}
