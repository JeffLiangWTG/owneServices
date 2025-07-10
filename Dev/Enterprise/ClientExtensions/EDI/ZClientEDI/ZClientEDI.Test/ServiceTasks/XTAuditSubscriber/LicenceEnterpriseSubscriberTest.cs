using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ServiceTasks.XT.Subscribers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.XTAuditSubscriber
{
	[TestedType(typeof(LicenceEnterpriseSubscriberForTest))]
	class LicenceEnterpriseSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new LicenceEnterpriseSubscriberForTest();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestEnterpriseCodeChangeFromBlank()
		{
			ProcessChanges(EDIInterchangeTransportTypeList.Codes.xT, "InterchangeProdFromBlank.xml", "", "WTL");
			ProcessChanges(EDIInterchangeTransportTypeList.Codes.tXT, "InterchangeTestFromBlank.xml", "", "WTL");
		}

		public void TestEnterpriseCodeChangeToBlank()
		{
			ProcessChanges(EDIInterchangeTransportTypeList.Codes.xT, "", "WTL", "");
			ProcessChanges(EDIInterchangeTransportTypeList.Codes.tXT, "", "WTL", "");
		}

		public void TestProcessChangesException()
		{
			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			SetupDataRow(changeRow, ZGuid.NewZGuid());
			changeTable.Rows.Add(changeRow);

			var subscriber = new LicenceEnterpriseSubscriber();

			AssertExceptionThrown(typeof(AggregateException), "Add nor Delete operation is not expected.", () => { subscriber.ProcessChanges(new LoggerForTest(), changeTable); }, true);
		}

		#region Implementation

		void ProcessChanges(string transportType, string xmlFileName, string oldEnterpriseCode, string newEnterpriseCode)
		{
			var currentTime = ZDateTime.UtcNow;
			var enterpriseID = "E000001";
			var licenceType = transportType == EDIInterchangeTransportTypeList.Codes.xT ? "PRD" : "TST";

			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);

			var factory = new BusinessObjectFactory();

			var enterprisePK = CreateEnterprise(factory, licenceType);

			var changeTable = GetTestDataTable();
			var changeRow = changeTable.NewRow();

			SetupDataRow(changeRow, enterprisePK, oldEnterpriseCode, enterpriseID);
			changeTable.Rows.Add(changeRow);

			changeRow.AcceptChanges();
			changeRow[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode] = newEnterpriseCode;

			var subscriber = (LicenceEnterpriseSubscriberForTest)NewDataChangeSubscriber();
			ZDateTime.TryParseIgnoreTimezone("2021-09-01 10:00", CultureInfo.InvariantCulture, out ZDateTime changeDateTime);
			subscriber.ChangeDateTime = changeDateTime;
			subscriber.LicenceType = licenceType;
			subscriber.ProcessChanges(new LoggerForTest(), changeTable);

			var result = factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_IsActive, true));
			if (string.IsNullOrEmpty(newEnterpriseCode))
			{
				AssertEquals(0, result.Length);
				return;
			}
			AssertEquals(2, result.Length);
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
			changeTable.Columns.Add(LicenceEnterpriseSchema.Constants.PK, typeof(ZGuid));
			changeTable.Columns.Add(LicenceEnterpriseSchema.Constants.LE_IsInternal, typeof(bool));
			changeTable.Columns.Add(LicenceEnterpriseSchema.Constants.LE_EnterpriseCode, typeof(string));
			changeTable.Columns.Add(LicenceEnterpriseSchema.Constants.LE_OH, typeof(ZGuid));
			changeTable.Columns.Add(LicenceEnterpriseSchema.Constants.LE_EnterpriseID, typeof(string));
			return changeTable;
		}

		void SetupDataRow(DataRow changeRow, ZGuid enterprisePK, string enterpriseCode = "", string enterpriseID = "E000001")
		{
			var orgHeaderPk = ZGuid.NewZGuid();

			changeRow[LicenceEnterpriseSchema.Constants.PK] = enterprisePK;
			changeRow[LicenceEnterpriseSchema.Constants.LE_IsInternal] = false;
			changeRow[LicenceEnterpriseSchema.Constants.LE_OH] = orgHeaderPk;
			changeRow[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode] = enterpriseCode;
			changeRow[LicenceEnterpriseSchema.Constants.LE_EnterpriseID] = enterpriseID;
		}

		public static LicenceDatabase CreateLicenceDatabase(BusinessObjectFactory factory, ZGuid enterprisePK, string licenceType)
		{
			var licenceDatabase = factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = enterprisePK;
			licenceDatabase.LD_LicenceType = licenceType;
			return licenceDatabase;
		}

		public static string GetFileResourceString(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".ServiceTasks.XTAuditSubscriber.TestFiles." + resourceName);
			return new StreamReader(stream).ReadToEnd();
		}

		public static ZGuid CreateEnterprise(BusinessObjectFactory factory, string orgCode)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;

			var licenceEnterprise = factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeader.PK;
			factory.Save();
			return licenceEnterprise.PK;
		}

		#endregion
	}

	class LicenceEnterpriseSubscriberForTest : LicenceEnterpriseSubscriber
	{
		public ZDateTime ChangeDateTime { get; set; }
		public string LicenceType { get; set; }

		protected override string CreateXMLMessage(string serverCode, string licenceType, string password, string oldPassword, string changeType, string enterpriseCode, ZDateTime changeDateTime, string databaseNumber)
		{
			return base.CreateXMLMessage(serverCode, licenceType, password, oldPassword, changeType, enterpriseCode, ChangeDateTime, databaseNumber);
		}

		protected override LicenceDatabase[] GetLicenceDatabaseRecords(string enterprisePK)
		{
			ZGuid.TryParse(enterprisePK, out var pk);

			var firstRecord = DataFactory.New<LicenceDatabase>();
			firstRecord.LD_LE = pk;
			firstRecord.LD_LicenceType = LicenceType;
			firstRecord.LD_ServerCode = "R&D";

			var secondRecord = DataFactory.New<LicenceDatabase>();
			secondRecord.LD_LE = pk;
			secondRecord.LD_LicenceType = LicenceType;
			secondRecord.LD_ServerCode = "BBB";

			var licenceDatabaseRecords = new List<LicenceDatabase>()
			{
				firstRecord,
				secondRecord
			};

			return licenceDatabaseRecords.ToArray();
		}
	}
}

