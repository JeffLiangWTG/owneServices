using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Web;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Web
{
	[TestedType(typeof(Report_WebSecurityProfile))]
	class Report_WebSecurityProfileTest : DbCreateScriptTest
	{
		public void TestETailWebPortalSecurityProfileV2()
		{
			var result = DataUtils.GetDataTableFromQuery(
				TestConnection,
				string.Format("select * from Report_WebSecurityProfile('{0}', NULL, NULL, '')",
				orgCode));
			CombineAssertions(() =>
			{
				Assert("Report should contain ECommerceShipperPortal", result.Columns.Contains("ECommerceShipperPortal"));
				Assert("Report should contain ECommerceOriginDepotPortal", result.Columns.Contains("ECommerceOriginDepotPortal"));
				Assert("Report should contain ECommerceDestinationDepotPortal", result.Columns.Contains("ECommerceDestinationDepotPortal"));
				Assert("Report should contain ECommerceViewCarriersAndDepots", result.Columns.Contains("ECommerceViewCarriersAndDepots"));
				Assert("Report should contain ECommerceViewRelatedParties", result.Columns.Contains("ECommerceViewRelatedParties"));
				Assert("Report should contain ECommerceConfirmBookingHeader", result.Columns.Contains("ECommerceConfirmBookingHeader"));
				Assert("Report should contain ECommerceReceiveBookingHeader", result.Columns.Contains("ECommerceReceiveBookingHeader"));
				Assert("Report should contain ECommerceLodgeOriginLoadList", result.Columns.Contains("ECommerceLodgeOriginLoadList"));
				Assert("Report should contain ECommerceCalculateDepotAndLMC", result.Columns.Contains("ECommerceCalculateDepotAndLMC"));
				Assert("Report should contain ECommerceLastMileCarrierBooking", result.Columns.Contains("ECommerceLastMileCarrierBooking"));
				Assert("Report should contain ECommerceAutocreateBookingHeader", result.Columns.Contains("ECommerceAutocreateBookingHeader"));
			});
		}

		public void TestSampleCall()
		{
			orgPK = Guid.NewGuid();
			orgCode = "TESTORG";

			string[] webSecurityRightsY =
				new[]
				{
					"Web Quoting",
					"Web Invoicing and Statements",
					"Web Booking (View)",
					"Web CFS Shipment (View)",
					"Web Booking (Add/Edit)",
					"Web Declaration (View)",
					"Web Orders (View)",
					"Web Orders (Add/Edit)",
					"Web Orders (Allow Split)",
					"Web Inventory (View)",
					"Web Warehouse Orders (View)",
					"Web Warehouse Orders (Add/Edit)",
					"Web Warehouse Products (View)",
					"Web Warehouse Receipts (View)",
					"Web Warehouse Receipts (Add/Edit)",
					"Web Transport Job (View)",
					"Web Containers (View)",
					"Web Containers (Edit)",
					"Web Containers (Add/Edit) Number",
					"Container (Edit) Client Reference",
					"Container Required Delivery (Edit)",
					"Container Confirmed Delivery (Edit)",
					"Container Actual Delivery (Edit)",
					"Container Estimated De-hire (Edit)",
					"Container Pickup (Edit)",
					"Container Actual De-hire (Edit)",
					"Container Sequence (Edit)",
					"Web Reports",
					"Web Organizations (Add)",
					"Web Shipping Bookings (View)",
					"Web Shipping Bookings (Add/Edit)",
					"Web Shipping Bills of Lading (View)",
					"Web Shipping Fwd Instruction (Edit)",
					"Web Documents (View)",
					"Web Shipments (View)",
				};

			string[] webSecurityRightsN =
				new[]
				{
					"Web Booking - Select Schedules",
					"Web Publish Layouts",
					"Web Shipment Delivery (Add)",
					"Web Shipment Delivery (Edit)",
					"Web Documents (Add)",
					"Web MAWB (View)",
					"Web MAWB (Edit)",
					"Web MAWB (Send)",
					"Web MAWB (Admin)",
					"Web HAWB (View)",
					"Web HAWB (Edit)",
					"Web HAWB (Send)",
					"Web HAWB (Admin)",
					"Estimated Milestones (Update)",
					"Actual Milestones (Update)",
					"Web Events (View)",
					"Transport Customer Portal",
					"Netting Participant Portal",
					"US AMS",
					"eCommerce Destination Depot Portal",
					"eCommerce Origin Depot Portal",
					"eCommerce Shipper Portal",
					"eCommerce View Carriers and Depots",
					"eCommerce View Related Parties",
					"eCommerce Confirm Booking Header",
					"eCommerce Receive Booking Header",
					"eCommerce Lodge Origin Load List",
					"eCommerce Calculate Depot and LMC",
					"eCommerce Last Mile Carrier Booking",
					"eCommerce Autocreate Booking Header",
					"Web ISF (View)",
					"Web ISF (Add/Edit)",
					"Web ISF (Send)",
					"Web ISF (Delete)"
				};

			string createOrgSQL = string.Format(@"
insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) values ('{0}', '{1}', 'Test Organisation')
insert into dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_WebAccessEnabled, OC_OH) values (NEWID(), 'Test User', 'test.user@cargowise.com', 1, 1, '{0}')",
				orgPK, orgCode);
			TestConnection.ExecuteNonQuery(createOrgSQL);

			AssertSampleCall("Y", "{0}: allowed", webSecurityRightsY);
			AssertSampleCall("N", "{0}: prohibited", webSecurityRightsN);

			StringBuilder restrictRightsSQL = new StringBuilder();
			foreach (string right in webSecurityRightsY)
			{
				restrictRightsSQL.AppendLine(string.Format("insert into dbo.OrgSecurity (OX_PK, OX_Granted, OX_SecurityItemName, OX_OH) values (NEWID(), 0, '{0}', '{1}')", right, orgPK));
			}
			foreach (string right in webSecurityRightsN)
			{
				restrictRightsSQL.AppendLine(string.Format("insert into dbo.OrgSecurity (OX_PK, OX_Granted, OX_SecurityItemName, OX_OH) values (NEWID(), 1, '{0}', '{1}')", right, orgPK));
			}
			TestConnection.ExecuteNonQuery(restrictRightsSQL.ToString());

			AssertSampleCall("N", "{0}: prohibited", webSecurityRightsY);
			AssertSampleCall("Y", "{0}: allowed", webSecurityRightsN);
		}

		public void TestParameterStaffWorks()
		{
			orgPK = Guid.NewGuid();
			var orgPk2 = Guid.NewGuid();
			orgCode = "OrgCode";
			var staffCode = "JNC";
			var personPk = Guid.NewGuid();

			var sql = $@"
				INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{personPk}', 'asd')
				INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_FullName) VALUES ('{orgPK}','{orgCode}','Test Organisation')
				INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_FullName) VALUES ('{orgPk2}','OrgCode2','Test Organisation2')
				INSERT INTO dbo.OrgContact(OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_WebAccessEnabled, OC_OH) 
				VALUES (NEWID(), 'Test User1', 'test.user1@cargowise.com', 1, 1, '{orgPK}')
				INSERT INTO dbo.OrgContact(OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_WebAccessEnabled, OC_OH) 
				VALUES (NEWID(), 'Test User2', 'test.user2@cargowise.com', 1, 1, '{orgPk2}')
				INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_EmailAddress, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES (NEWID(), '{staffCode}', 'Justin Chen', 'i@just1n.net', '{personPk}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				INSERT INTO dbo.OrgStaffAssignments (O8_PK, O8_Role, O8_Department, O8_OH, O8_GS_NKPersonResponsible) VALUES (NEWID(), 'CUS', 'ALL', '{orgPK}', '{staffCode}')
			";

			TestConnection.ExecuteNonQuery(sql);

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"select * from Report_WebSecurityProfile('{orgCode}', NULL, NULL, '')");
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection,
				"select * from Report_WebSecurityProfile('', NULL, NULL, '')");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection,
				"select * from Report_WebSecurityProfile('', NULL, NULL, 'NON')");
			AssertEquals("Result should have no rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"select * from Report_WebSecurityProfile('', NULL, NULL, '{staffCode}')");
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
		}

		#region Implementation

		string orgCode;
		Guid orgPK;

		static string GetOutputColumnName(string webSecurityRight)
		{
			string[] parts = webSecurityRight.Split(new[] { ' ', '(', ')', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < parts.Length; i++)
			{
				parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
			}

			return string.Join("", parts);
		}

		void AssertSampleCall(string expectedValue, string messageTemplate, string[] webSecurityRights)
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select * from Report_WebSecurityProfile('{0}', NULL, NULL, '')", orgCode));
			AssertNotEquals("Result should have rows", 0, result.Rows.Count);

			foreach (string webSecurityRight in webSecurityRights)
			{
				string message = string.Format(messageTemplate, webSecurityRight);
				AssertEquals(message, expectedValue, result.Rows[0][GetOutputColumnName(webSecurityRight)]);
			}
		}
		#endregion
	}
}

