using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class CommunicationTest : TestCaseWithFactory
	{
		public void TestImportCommunication()
		{
			using (var dataStream = NativeDataTransferTestHelper.ExportToStream(Communication))
			{
				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(dataStream);

				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: Communication
--- Import Process Finished -----------------------------------------------------------
OrgSalesCall - 0 inserts, 0 updates, 0 deletes
OrgSalesCallAdditionalAttendee - 0 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);

				var loadedCommunication = Factory.LoadFromUniqueKey<OrgSalesCall>(OrgSalesCallSchema.OQ_CommunicationID, new ZString("CM00001234"));
				AssertEquals(Communication.OQ_OH, loadedCommunication.OQ_OH);
				AssertEquals(Communication.OQ_OC, loadedCommunication.OQ_OC);
				AssertEquals(Communication.OQ_GS_NKSalesRep, loadedCommunication.OQ_GS_NKSalesRep);
				AssertEquals(1,loadedCommunication.AdditionalAttendeesContact.Count);
				AssertEquals(Attendee.O6_AttendeeID, loadedCommunication.AdditionalAttendeesContact[0].O6_AttendeeID);
			}
		}

		public void TestExportCommunication()
		{
			using (var stream = NativeDataTransferTestHelper.ExportToStream(Communication))
			{
				var reader = new StreamReader(stream, Encoding.UTF8);
				var text = reader.ReadToEnd();
				CombineAssertions("Export Communication XML is incorrect", () =>
				{
					AssertContains(@"<OrgSalesCall Action=""MERGE"">", text);
					AssertContains($"<PK>{Communication.PK}</PK>", text);
					AssertNotContains($"<PK>{Communication2.PK}</PK>", text);
					AssertContains("<CommunicationID>CM00001234</CommunicationID>", text);
					AssertNotContains("<CommunicationID>CM00001235</CommunicationID>", text);
					AssertContains($"<PK>{TestOrg.PK}</PK>", text);
					AssertContains($"<PK>{TestContact.PK}</PK>", text);
					AssertContains("<ContactName>Peter</ContactName>", text);
					AssertContains(@"<SalesRep TableName=""GlbStaff"">", text);
					AssertContains($"<PK>{TestStaff.PK}</PK>", text);
					AssertContains(@"<OrgSalesCallAdditionalAttendee Action=""MERGE"">", text);
					AssertContains($"<PK>{Attendee.PK}</PK>", text);
					AssertNotContains($"<PK>{Attendee2.PK}</PK>", text);
					AssertContains($"<AttendeeID>{Attendee.O6_AttendeeID}</AttendeeID>", text);
					AssertNotContains("<GrowthOutlook>", text);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestContact = TestOrg.Contacts.AddNew();
			TestContact.OC_ContactName = "Peter";

			TestStaff = Factory.New<GlbStaff>();
			TestStaff.GS_Code = "XYZ";

			Communication = Factory.New<OrgSalesCall>();
			Communication.OQ_CommunicationID = "CM00001234";
			Communication.OQ_OH = TestOrg.PK;
			Communication.OQ_OC = TestContact.PK;
			Communication.OQ_GS_NKSalesRep = TestStaff.GS_Code;

			Communication2 = Factory.New<OrgSalesCall>();
			Communication2.OQ_CommunicationID = "CM00001235";
			Communication2.OQ_OH = TestOrg.PK;
			Communication2.OQ_OC = TestContact.PK;
			Communication2.OQ_GS_NKSalesRep = TestStaff.GS_Code;

			Attendee = Factory.New<OrgSalesCallAdditionalAttendee>();
			Attendee.O6_OQ = Communication.PK;
			Attendee.O6_AttendeeID = TestContact.PK;

			Attendee2 = Factory.New<OrgSalesCallAdditionalAttendee>();
			Attendee2.O6_OQ = Communication2.PK;
			Attendee2.O6_AttendeeID = TestContact.PK;

			Factory.Save();
		}

		OrgHeader TestOrg { get; set; }
		OrgContact TestContact { get; set; }
		GlbStaff TestStaff { get; set; }
		OrgSalesCall Communication { get; set; }
		OrgSalesCall Communication2 { get; set; }
		OrgSalesCallAdditionalAttendee Attendee { get; set; }
		OrgSalesCallAdditionalAttendee Attendee2 { get; set; }
	}
}
