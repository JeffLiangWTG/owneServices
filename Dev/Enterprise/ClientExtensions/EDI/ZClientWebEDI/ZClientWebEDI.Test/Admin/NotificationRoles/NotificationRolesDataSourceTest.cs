using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class NotificationRolesDataSourceTest : TestCaseWithFactory
	{
		public void TestHasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var ds = new NotificationRolesDataSource(Factory, org.PK);
			AssertEquals(true, org.HasChanges);
			AssertEquals(true, ds.HasChanges);
			Factory.Save();
			AssertEquals(false, org.HasChanges);
			AssertEquals(false, ds.HasChanges);
			ds.HasChanges = true;
			AssertEquals(true, org.HasChanges);
			AssertEquals(true, ds.HasChanges);
		}

		public void TestBulkUpdate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.FillWithValidTestData();
			var contact2 = org.Contacts.AddNew();
			contact2.FillWithValidTestData();
			var contact3 = org.Contacts.AddNew();
			contact3.FillWithValidTestData();
			contact3.OC_Email = Guid.NewGuid().ToString();
			Factory.Save();
			AssertEquals(false, contact1.Documents.Any());
			AssertEquals(false, contact2.Documents.Any());
			AssertEquals(false, contact3.Documents.Any());
			var ds = new NotificationRolesDataSource(Factory, org.PK);
			ds.InitBulkUpdate(3, 1);
			AssertEquals(true, ds.AdminGroupItems.OfType<AdminGroupItem>().Select(x => x.GroupCode.ToString()).OrderBy(x => x).SequenceEqual(new[] { "CSV", "A/R", "BOR", "ERA", "IST", "CCP" }.OrderBy(x => x)));
			AssertEquals(2, ds.BulkUpdateModeCollection.Count);
			AssertEquals(3, ds.BulkUpdateModeCollection["ALL"].RecordsAffected);
			AssertEquals(1, ds.BulkUpdateModeCollection["SEL"].RecordsAffected);
			var csv = ds.AdminGroupItems.OfType<AdminGroupItem>().First(x => x.GroupCode == "CSV");
			AssertEquals(false, csv.Granted);
			AssertEquals(true, csv.Skip);
			csv.Granted = true;
			csv.Skip = false;
			AssertEquals(true, csv.Granted);
			AssertEquals(false, csv.Skip);
			ds.CancelBulkUpdate();
			AssertEquals(false, csv.Granted);
			AssertEquals(true, csv.Skip);
			csv.Granted = true;
			csv.Skip = false;
			var allContacts = new OrgContactCollection(Factory);
			allContacts.AddRange(org.Contacts);
			AssertEquals(3, allContacts.Count);
			ds.BulkUpdateSelected(allContacts, new[] { contact2.PK });
			AssertEquals(false, contact1.Documents.Any());
			AssertEquals(true, contact2.Documents.ContainsDocGroup("CSV"));
			AssertEquals(false, contact3.Documents.Any());
			ds.BulkUpdateAll(new ZQuery(OrgContactSchema.OC_OH, org.PK).AddToFilter(OrgContactSchema.OC_Email, contact3.OC_Email));
			AssertEquals(false, contact1.Documents.Any());
			AssertEquals(true, contact2.Documents.ContainsDocGroup("CSV"));
			AssertEquals(true, contact3.Documents.ContainsDocGroup("CSV"));
			csv.Granted = false;
			csv.Skip = false;
			ds.BulkUpdateAll(new ZQuery(OrgContactSchema.OC_OH, org.PK));
			AssertEquals(false, contact1.Documents.Any());
			AssertEquals(false, contact2.Documents.Any());
			AssertEquals(false, contact3.Documents.Any());
			csv.Granted = true;
			csv.Skip = false;
			ds.BulkUpdateAll(new ZQuery(OrgContactSchema.OC_OH, org.PK));
			AssertEquals(true, contact1.Documents.ContainsDocGroup("CSV"));
			AssertEquals(true, contact2.Documents.ContainsDocGroup("CSV"));
			AssertEquals(true, contact3.Documents.ContainsDocGroup("CSV"));
			csv.Granted = false;
			csv.Skip = true;
			ds.BulkUpdateAll(new ZQuery(OrgContactSchema.OC_OH, org.PK));
			AssertEquals(true, contact1.Documents.ContainsDocGroup("CSV"));
			AssertEquals(true, contact2.Documents.ContainsDocGroup("CSV"));
			AssertEquals(true, contact3.Documents.ContainsDocGroup("CSV"));
		}
	}
}