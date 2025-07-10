using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Client.EDI.ServiceTasks.XT.Subscribers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.XTAuditSubscriber
{
	[TestedType(typeof(LicenceDatabaseSubscriberForTest))]
	class LicenceDatabaseSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new LicenceDatabaseSubscriberForTest();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestProcessChanges()
		{
			ProcessChanges("Add", EDIInterchangeTransportTypeList.Codes.xT, "InterchangeProdAdd.xml", "PRD");
			ProcessChanges("Update", EDIInterchangeTransportTypeList.Codes.xT, "InterchangeProdUpdate.xml", "PRD");
			ProcessChanges("Delete", EDIInterchangeTransportTypeList.Codes.xT, "InterchangeProdDelete.xml", "PRD");
			ProcessChanges("ToActive", EDIInterchangeTransportTypeList.Codes.xT, "InterchangeProdToActive.xml", "PRD");
			ProcessChanges("ToInactive", EDIInterchangeTransportTypeList.Codes.xT, "InterchangeProdToInactive.xml", "PRD");
			ProcessChanges("AddedInactive", EDIInterchangeTransportTypeList.Codes.xT, "", "PRD");
		}

		public void TestProcessChangesForTest()
		{
			ProcessChanges("Add", EDIInterchangeTransportTypeList.Codes.tXT, "InterchangeTestAdd.xml", "TST");
			ProcessChanges("Update", EDIInterchangeTransportTypeList.Codes.tXT, "InterchangeTestUpdate.xml", "TST");
			ProcessChanges("Delete", EDIInterchangeTransportTypeList.Codes.tXT, "InterchangeTestDelete.xml", "TST");
			ProcessChanges("ToActive", EDIInterchangeTransportTypeList.Codes.tXT, "InterchangeTestToActive.xml", "TST");
			ProcessChanges("ToInactive", EDIInterchangeTransportTypeList.Codes.tXT, "InterchangeTestToInactive.xml", "TST");
			ProcessChanges("AddedInactive", EDIInterchangeTransportTypeList.Codes.tXT, "", "TST");
		}

		public void TestProcessChangesException()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			(Guid pK, Guid enterprisePk) = SetupDataRow(changeRow);
			changeTable.Rows.Add(changeRow);

			var subscriber = new LicenceDatabaseSubscriber();

			AssertExceptionThrown(typeof(AggregateException), "Error processing LicenceDatabase changes", () => { subscriber.ProcessChanges(new LoggerForTest(), changeTable); }, true);
		}

		public void TestSkipChangesNotRelated()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(GlbBranchSchema.Constants.PK, typeof(Guid));
			var changeRow = changeTable.NewRow();
			changeRow[GlbBranchSchema.Constants.PK] = Guid.NewGuid();
			changeTable.Rows.Add(changeRow);

			var subscriber = NewDataChangeSubscriber();
			var exception = AssertExceptionThrown<ArgumentException>("Error processing LicenceDatabase changes", () => { subscriber.ProcessChanges(new LoggerForTest(), changeTable); });
			AssertContains("Column 'LD_IsActive' does not belong to table", exception.Message);
		}

		public void TestSkipBlankEnterpriseCode()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			(Guid pK, Guid enterprisePk) = SetupDataRow(changeRow, "TST");
			changeTable.Rows.Add(changeRow);

			var subscriber = (LicenceDatabaseSubscriberForTest)NewDataChangeSubscriber();
			ZDateTime.TryParseIgnoreTimezone("2021-09-01 10:00", CultureInfo.InvariantCulture, out ZDateTime changeDateTime);
			subscriber.ChangeDateTime = changeDateTime;
			subscriber.EnterpriseCode = "";
			subscriber.ProcessChanges(new LoggerForTest(), changeTable);

			var factory = new BusinessObjectFactory();
			var result = factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_IsActive, true));
			AssertEquals(0, result.Length);
		}

		public void TestEnterpriseCodeChanged()
		{
			var currentTime = ZDateTime.UtcNow;

			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);

			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			(Guid pK, Guid enterprisePk) = SetupDataRow(changeRow, "TST");
			changeTable.Rows.Add(changeRow);

			changeRow.AcceptChanges();
			changeRow[LicenceDatabaseSchema.Constants.LD_LE] = "3FB41C57-84B2-40CA-A1F4-8F48EDF7A862";
			changeRow[LicenceDatabaseSchema.Constants.LD_LE] = "3FB41C57-84B2-40CA-A1F4-8F48EDF7A862";

			var factory = new BusinessObjectFactory();

			var subscriber = (LicenceDatabaseSubscriberForTest)NewDataChangeSubscriber();
			ZDateTime.TryParseIgnoreTimezone("2021-09-01 10:00", CultureInfo.InvariantCulture, out ZDateTime changeDateTime);
			subscriber.ChangeDateTime = changeDateTime;
			subscriber.EnterpriseCode = "WTL";
			subscriber.ProcessChanges(new LoggerForTest(), changeTable);

			var result = factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_IsActive, true));
			AssertEquals(2, result.Length);

			var ediInterchangeDelete = result.FirstOrDefault();
			var ediInterchangeAdd = result.LastOrDefault();

			var expectedMessageDelete = GetFileResourceString("InterchangeTestDelete.xml");
			var expectedMessageAdd = GetFileResourceString("InterchangeTestAdd.xml");

			AssertXMLEquals(expectedMessageDelete, ediInterchangeDelete.EI_BodyText.ToString());
			AssertEquals("TXT", ediInterchangeDelete.EI_TransportType);
			AssertXMLEquals(expectedMessageAdd, ediInterchangeAdd.EI_BodyText.ToString());
			AssertEquals("TXT", ediInterchangeDelete.EI_TransportType);
		}

		public void TestGetEnterpriseCodeFromAuditServer()
		{
			var subscriber = (LicenceDatabaseSubscriberForTest)NewDataChangeSubscriber();
			Guid guid = Guid.NewGuid();

			AssertExceptionThrown<ApplicationException>(() => subscriber.GetEnterpriseCodeTest(guid));
		}

		#region Implementation

		void ProcessChanges(string operation, string transportType, string xmlFileName, string licenceType)
		{
			var currentTime = ZDateTime.UtcNow;

			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);

			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			(Guid pK, Guid enterprisePk) = SetupDataRow(changeRow, licenceType);
			changeTable.Rows.Add(changeRow);

			switch (operation)
			{
				case "Update":
					changeRow.AcceptChanges();
					changeRow[LicenceDatabaseSchema.Constants.LD_Password] = "NewPassword";
					break;
				case "Delete":
					changeRow.AcceptChanges();
					changeRow.Delete();
					break;
				case "ToActive":
					changeRow[LicenceDatabaseSchema.Constants.LD_IsActive] = false;
					changeRow.AcceptChanges();
					changeRow[LicenceDatabaseSchema.Constants.LD_IsActive] = true;
					break;
				case "ToInactive":
					changeRow.AcceptChanges();
					changeRow[LicenceDatabaseSchema.Constants.LD_IsActive] = false;
					break;
				case "AddedInactive":
					changeRow[LicenceDatabaseSchema.Constants.LD_IsActive] = false;
					break;
				default:
					break;
			}

			var factory = new BusinessObjectFactory();

			var subscriber = (LicenceDatabaseSubscriberForTest)NewDataChangeSubscriber();
			ZDateTime.TryParseIgnoreTimezone("2021-09-01 10:00", CultureInfo.InvariantCulture, out ZDateTime changeDateTime);
			subscriber.ChangeDateTime = changeDateTime;
			subscriber.EnterpriseCode = "WTL";
			subscriber.ProcessChanges(new LoggerForTest(), changeTable);

			var result = factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_IsActive, true));
			if (operation == "AddedInactive")
			{
				AssertEquals(0, result.Length);
				return;
			}
			AssertEquals(1, result.Length);

			var ediInterchange = result.FirstOrDefault();

			var expectedMessage = GetFileResourceString(xmlFileName);

			var currentCompany = GlbCompany.GetCurrentCompany(factory);
			AssertXMLEquals(expectedMessage, ediInterchange.EI_BodyText.ToString());
			AssertEquals(transportType, ediInterchange.EI_TransportType);
			AssertEquals(currentCompany.LicenceKeyIdentifier, ediInterchange.EI_From);
			AssertEquals("XH", ediInterchange.EI_To);
			AssertEquals(EDIInterchange.Direction.Transmit, ediInterchange.EI_ReceiveTransmit);
			AssertEquals(EDIInterchange.Status.Queued, ediInterchange.EI_Status);
			AssertEquals(EDIInterchangeTypeList.Codes.XMS, ediInterchange.EI_InterchangeType);
			AssertEquals(ApplicationCodeList.Codes.XMS, ediInterchange.EI_ApplicationCode);
			AssertEquals(false, string.IsNullOrEmpty(ediInterchange.EI_InterchangeNum));
			AssertGreaterThanOrEqualTo(ediInterchange.EI_SystemCreateTimeUtc, currentTime);
			AssertEquals((currentCompany.FirstActiveBranch ?? currentCompany.Branches[0]).PK, ediInterchange.EI_GB);
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(LicenceDatabaseSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(LicenceDatabaseSchema.Constants.LD_IsActive, typeof(bool));
			changeTable.Columns.Add(LicenceDatabaseSchema.Constants.LD_LE, typeof(Guid));
			changeTable.Columns.Add(LicenceDatabaseSchema.Constants.LD_LicenceType, typeof(string));
			changeTable.Columns.Add(LicenceDatabaseSchema.Constants.LD_Password, typeof(string));
			changeTable.Columns.Add(LicenceDatabaseSchema.Constants.LD_ServerCode, typeof(string));
			changeTable.Columns.Add(LicenceDatabaseSchema.Constants.LD_DatabaseNumber, typeof(string));
			return changeTable;
		}

		(Guid pk, Guid enterprisePk) SetupDataRow(DataRow changeRow, string licenceType = "TST")
		{
			var pK = Guid.NewGuid();
			var enterprisePk = Guid.NewGuid();

			changeRow[LicenceDatabaseSchema.Constants.PK] = pK;
			changeRow[LicenceDatabaseSchema.Constants.LD_IsActive] = true;
			changeRow[LicenceDatabaseSchema.Constants.LD_LE] = enterprisePk;
			changeRow[LicenceDatabaseSchema.Constants.LD_LicenceType] = licenceType;
			changeRow[LicenceDatabaseSchema.Constants.LD_Password] = "password";
			changeRow[LicenceDatabaseSchema.Constants.LD_ServerCode] = licenceType;
			changeRow[LicenceDatabaseSchema.Constants.LD_DatabaseNumber] = 0;
			return (pK, enterprisePk);
		}

		public static string GetFileResourceString(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".ServiceTasks.XTAuditSubscriber.TestFiles." + resourceName);
			return new StreamReader(stream).ReadToEnd();
		}

		#endregion
	}

	class LicenceDatabaseSubscriberForTest : LicenceDatabaseSubscriber
	{
		public ZDateTime ChangeDateTime { get; set; }
		public string EnterpriseCode { get; set; }

		protected override string GetEnterpriseCode(Guid enterprisePK, bool isDeleted, ILogger logger)
		{
			return EnterpriseCode;
		}

		protected override string GetXMLMessage(DataRow changeRow, DataRowVersion rowVersion, string enterpriseCode, ZDateTime changeDateTime, DataRowState dataRowState)
		{
			return base.GetXMLMessage(changeRow, rowVersion, enterpriseCode, ChangeDateTime, dataRowState);
		}

		public string GetEnterpriseCodeTest(Guid enterprisePK)
		{
			return base.GetEnterpriseCode(enterprisePK, true, new LoggerForTest());
		}
	}
}

