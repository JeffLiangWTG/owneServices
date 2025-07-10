using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	public class SqlOrganizationScavengingRepositoryTest : TestCaseWithFactory
	{
		public void TestHandleHugeIM_Content()
		{
			var notifier = new TestNotifications();
			var repository = new SqlOrganizationScavengingRepository(notifier);
			var item = repository.Select(10).ToArray().First();
			var testContent = item.Content.ToString();
			var itemID = item.ID;
			repository.contentLengthLimit = testContent.Length - 1;
			AssertEquals(0, repository.Select(10).ToArray().Length);
			AssertEquals(string.Format("Messages exceeded the length limit({0}): {1}. This message will be moved to archive", repository.contentLengthLimit, itemID), notifier.LastNotificationMessage);
		}

		[ExpectNoExceptions]
		public void TestSelectAndDelete()
		{
			var repository = new SqlOrganizationScavengingRepository();
			var items = repository.Select(10).ToArray();
			AssertEquals("One item should be selected", 1, items.Length);
			ScavengingItem item = items.First();
			CombineAssertions(delegate
			{
				AssertEquals("Item ID should be the same as IM_PK", "307c6f7c-fcff-4102-ad93-03f9b616efcc", item.ID.ToString());
				AssertEquals("Item client should be the same as IM_ClientID", "EJZORDORD", item.Client);
				AssertEquals("Item type should be the same as IM_MessageType", "http://www.cargowise.com/Schemas/Native", item.Type);
				AssertEquals("Item date should be the same as IM_InsertUTC", "19/02/2012 12:29:50 PM", item.Date.ToString());
				AssertEquals("Item content as string", "<Native xmlns=\"http://www.cargowise.com/Schemas/Native/2011/11\" version=\"2.0\"></Native>", item.Content.ToString());
			});
			repository.Delete(item.ID);
			AssertEquals(0, GetRowsCount());
		}

		public void TestSelect()
		{
			string script = @"
INSERT INTO dbo.ClientStatisticsXML([IM_PK], [IM_ClientID], [IM_MessageTrackingID], [IM_MessageType], [IM_ApplicationCode], [IM_InsertUTC], [IM_Content]) VALUES
('DC414E4C-DEBF-47FF-8924-31AD4CA4DBAE', 'EJZORDORD', 'E13CD8D9-B0D1-496C-883F-A13FD41E7E40', 'http://www.cargowise.com/Schemas/Native', 'SCV', '2013-10-19 12:29:50.927', 'H4sIAAAAAAAEALOxr8jNUShLLSrOzM+zVTLUM1BSSM1Lzk/JzEu3VSotSdO1ULK34+Wy8UssySxLVQCqziu2VcooKSmw0tcvLy/XS04sSs8vzyxO1UvOz9UPTs5IzU0s1oco1zcyMDTUNzRUQthgBLQBZB5UhR0Aylna9oEAAAA='),
('01B66E6C-4602-443D-AA42-D98A30025717', 'EJZORDORD', 'F7C02529-B9B5-4A36-9334-FEFB02FB99F0', 'http://www.cargowise.com/Schemas/Native', 'SCV', '2013-10-20 12:29:50.927', 'H4sIAAAAAAAEALOxr8jNUShLLSrOzM+zVTLUM1BSSM1Lzk/JzEu3VSotSdO1ULK34+Wy8UssySxLVQCqziu2VcooKSmw0tcvLy/XS04sSs8vzyxO1UvOz9UPTs5IzU0s1oco1zcyMDTUNzRUQthgBLQBZB5UhR0Aylna9oEAAAA='),
('3CCCB8E3-A156-4D38-AC9E-B6137CC599E8', 'EJZORDORD', '18079EA0-E1C1-4763-A1FB-6A7454061CA7', 'http://www.cargowise.com/Schemas/Native', 'SCV', '2013-10-21 12:29:50.927', 'H4sIAAAAAAAEALOxr8jNUShLLSrOzM+zVTLUM1BSSM1Lzk/JzEu3VSotSdO1ULK34+Wy8UssySxLVQCqziu2VcooKSmw0tcvLy/XS04sSs8vzyxO1UvOz9UPTs5IzU0s1oco1zcyMDTUNzRUQthgBLQBZB5UhR0Aylna9oEAAAA=')";
			Db.Connection.ExecuteNonQuery(script);
			AssertEquals("PRE: There are 4 rows in ClientStatisticsXML table", 4, GetRowsCount());
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sqlRepository = new SqlOrganizationScavengingRepository();
				var items = sqlRepository.Select(10).ToArray();
				AssertEquals("Four items should be selected", 4, items.Length);
				var item1 = items[1];
				AssertEquals("First Item Client", "EJZORDORD", item1.Client);
				AssertEquals("First Item Date", new DateTime(2013, 10, 19, 12, 29, 50, 927), item1.Date);
				var item2 = items[2];
				AssertEquals("Second Item Client", "EJZORDORD", item2.Client);
				AssertEquals("Second Item Date", new DateTime(2013, 10, 20, 12, 29, 50, 927), item2.Date);
				var item3 = items[3];
				AssertEquals("Third Item Client", "EJZORDORD", item3.Client);
				AssertEquals("Third Item Date", new DateTime(2013, 10, 21, 12, 29, 50, 927), item3.Date);
			}
		}

		public void TestShouldMoveMessageToArchiveWhenDataIsCorrupted()
		{
			string script = @"
INSERT INTO dbo.ClientStatisticsXML([IM_PK], [IM_ClientID], [IM_MessageTrackingID], [IM_MessageType], [IM_ApplicationCode], [IM_InsertUTC], [IM_Content]) VALUES
('43560CAD-4AA8-4E2F-B7F6-E70D2B905EC5', 'EJZORDORD', '893B0009-BA60-414C-B8DF-F0A7C7B7CB12', 'http://www.cargowise.com/Schemas/Native', 'SCV', '2013-10-19 12:29:50.927', 'H4sIAAAAAAAEALOxr8jNUShLLSrOzM+zVTLUM1BSSM1Lzk/JzEu3VSotSdO1ULK34+Wy8UssySxLVQCqziu2VcooKSmw0tcvLy/XS04sSs8vzyxO1UvOz9UPTs5IzU0s1oco1zcyMDTUNzRUQthgBLQBZB5UhR0Aylna9oEAAAA='),
('DC414E4C-DEBF-47FF-8924-31AD4CA4DBAE', 'HYESYDSYD', 'E13CD8D9-B0D1-496C-883F-A13FD41E7E40', 'http://www.cargowise.com/Schemas/Native', 'SCV', '2013-10-19 12:29:50.927', 'AH4sIAAAAAAAEAHPycfQAAOiMvPgEAAAA')";
			Db.Connection.ExecuteNonQuery(script);
			AssertEquals("PRE: There is 3 row in ClientStatisticsXML table", 3, GetRowsCount());
			using (var command = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM dbo.ClientStatisticsXMLArchive")))
			{
				AssertEquals("PRE: There is 0 row in ClientStatisticsXMLArchive table", 0, (int)command.ExecuteScalar());
			}

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sqlRepository = new SqlOrganizationScavengingRepository();
				AssertExceptionThrown("Exception message should contain item ID", typeof(System.IO.InvalidDataException), "Data found corrupted when processing item: dc414e4c-debf-47ff-8924-31ad4ca4dbae", () => sqlRepository.Select(10).ToArray(), true);
			}

			using (var command = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM dbo.ClientStatisticsXMLArchive")))
			{
				AssertEquals("PRE: There is 1 row in ClientStatisticsXMLArchive table", 1, (int)command.ExecuteScalar());
			}
		}

		int GetRowsCount()
		{
			return int.Parse(Db.Connection.ExecuteScalar("select count(*) from dbo.ClientStatisticsXML").ToString());
		}

		#region Implementaion
		protected override void SetUp()
		{
			base.SetUp();
			InsertTestData();
		}

		protected override void TearDown()
		{
			ClearTestData();
			base.TearDown();
		}

		void InsertTestData()
		{
			Db.Connection.ExecuteNonQuery(insertOrgXMLScript);
			AssertEquals("PRE: There is one row in ClientStatisticsXML table", 1, GetRowsCount());
		}

		void ClearTestData()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.ClientStatisticsXML");
		}

		#region InsertOrgXMLScript
		const string insertOrgXMLScript = @"if not exists (select null from dbo.ClientStatisticsXML where IM_PK = '307C6F7C-FCFF-4102-AD93-03F9B616EFCC')
  begin
	  Insert into dbo.ClientStatisticsXML
	  (
		[IM_PK]
		,[IM_ClientID]
		,[IM_MessageTrackingID]
		,[IM_MessageType]
		,[IM_ApplicationCode]
		,[IM_InsertUTC]
		,[IM_Content]
	  )
	  values
	  (
		'307C6F7C-FCFF-4102-AD93-03F9B616EFCC',
		'EJZORDORD',
		'9dc25b40-99dc-427b-a84c-86cc42904c73',
		'http://www.cargowise.com/Schemas/Native',
		'SCV',
		'2012-02-19 12:29:50.927',
		'H4sIAAAAAAAEALOxr8jNUShLLSrOzM+zVTLUM1BSSM1Lzk/JzEu3VSotSdO1ULK34+Wy8UssySxLVQCqziu2VcooKSmw0tcvLy/XS04sSs8vzyxO1UvOz9UPTs5IzU0s1oco1zcyMDTUNzRUQthgBLQBZB5UhR0Aylna9oEAAAA=')
  end"; //orgCode is "CHACAM"
		#endregion
		#endregion
	}

	public class TestNotifications : INotifications
	{
		public string LastNotificationMessage
		{
			get; set;
		}

		public void Add(INotification notification)
		{
			LastNotificationMessage = notification.Message;
		}
	}
}
