using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class UpdateHandlerTest : TestCaseWithFactory
	{
		// CusEntryNum have a Column CE_ParentTable to identify which table it refer to.
		// Native should be able to handle it.
		public void TestShouldBeAbleToHandlePolymorphicKeyForTableNameColumn()
		{
			var rowFactory = new RowRepository(connection);

			var jobDeclarationId = new Guid("0d8625a7-1b62-4afd-98e8-a8786c718a6a");
			var jobDeclarationRow = rowFactory.Create(Table.Get("JobDeclaration"), jobDeclarationId);
			jobDeclarationRow["JE_GB"] = GlbBranch.CurrentBranch.PK.ToGuid();
			jobDeclarationRow["JE_GC"] = GlbBranch.CurrentBranch.GB_GC.ToGuid();
			jobDeclarationRow["JE_ClusterKey"] = 1;
			jobDeclarationRow["JE_DataModel"] = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var cusEntryHeaderId = new Guid("603962ad-291a-41de-a2ef-e02e4fc2577a");
			var cusEntryHeaderRow = rowFactory.Create(Table.Get("CusEntryHeader"), cusEntryHeaderId);
			cusEntryHeaderRow["CH_JE"] = jobDeclarationId;
			cusEntryHeaderRow["CH_ClusterKey"] = jobDeclarationRow["JE_ClusterKey"];
			cusEntryHeaderRow["CH_SystemCreateTimeUtc"] = DateTime.UtcNow;
			cusEntryHeaderRow["CH_SystemCreateUser"] = "~BP";
			cusEntryHeaderRow["CH_SystemLastEditTimeUtc"] = DateTime.UtcNow;
			cusEntryHeaderRow["CH_SystemLastEditUser"] = "~BP";
			cusEntryHeaderRow["CH_DataModel"] = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var cusEntryNumId = new Guid("bc9fd4c8-0fb2-4dc7-bcc3-4e0a3ad746f9");
			var cusTntryNum = rowFactory.Create(Table.Get("CusEntryNum"), cusEntryNumId);
			cusTntryNum["CE_SystemCreateTimeUtc"] = DateTime.UtcNow;
			cusTntryNum["CE_SystemCreateUser"] = "~BP";
			cusTntryNum["CE_SystemLastEditTimeUtc"] = DateTime.UtcNow;
			cusTntryNum["CE_SystemLastEditUser"] = "~BP";
			cusTntryNum["CE_ParentTable"] = "JobDeclaration"; // need to give this column a valid value so that it can be saved

			rowFactory.Save();

			string sourceXML = @"
<Declaration xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <JobDeclaration Action=""MERGE"">
    <PK>0d8625a7-1b62-4afd-98e8-a8786c718a6a</PK>
    <CusEntryHeaderCollection>
      <CusEntryHeader Action=""MERGE"">
        <PK>603962ad-291a-41de-a2ef-e02e4fc2577a</PK>
        <CusEntryNumCollection>
          <CusEntryNum Action=""MERGE"">
            <PK>bc9fd4c8-0fb2-4dc7-bcc3-4e0a3ad746f9</PK>
            <EntryNum>XXX</EntryNum>
            <Category>CUS</Category>
            <CountryCode>
              <Code>ZA</Code>
            </CountryCode>
          </CusEntryNum>
        </CusEntryNumCollection>
      </CusEntryHeader>
    </CusEntryHeaderCollection>
  </JobDeclaration>
</Declaration>
".Trim();

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			var message = XElement.Parse(sourceXML);

			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var newFactory = new BusinessObjectFactory();
			var newCusEntryNum = newFactory.Load<CusEntryNumber>(cusEntryNumId);
			AssertEquals("cusEntryNum.CE_ParentTable should have correct table name it refer to", "CusEntryHeader", newCusEntryNum.CE_ParentTable);
			AssertEquals("cusEntryNum.CE_ParentId should have correct PK it refer to", cusEntryHeaderId, newCusEntryNum.CE_ParentID);
		}

		public void TestUpdateDuplicateColumns_ReportErrorMessageContainsSourceXmlInfo()
		{
			var sourceXML = @"
<Declaration xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <JobDeclaration Action=""MERGE"">
    <PK>0d8625a7-1b62-4afd-98e8-a8786c718a6a</PK>
    <ApplicationCode>TST</ApplicationCode>
    <ApplicationCode>TTT</ApplicationCode>
  </JobDeclaration>
</Declaration>";

			var message = XElement.Parse(sourceXML);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());
			var responseMessageAsText = string.Join("\r\n", response.Informations);

			const string expectedErrorStartsWith = "Error - Validation errors found in Native XML:\r\nDuplicate Property [ApplicationCode, TTT] added to Entity";

			AssertStartsWith("Duplicate Property ApplicationCode", expectedErrorStartsWith, responseMessageAsText);
		}

		public void TestConstraintErrorOnSaveHandledNicely()
		{
			var sourceXML = @"
			<Organization version=""2.0"" xmlns =""http://www.cargowise.com/Schemas/Native/2011/11"">
  <OrgHeader Action=""MERGE"">
    <PK>f13c9c6a-0da9-4f4e-ae62-55d7844227f6</PK>
    <Code>SOMEORG1</Code>
    <Language>ENG</Language>
    <IsActive>true</IsActive>
    <FullName>SOME ORG NAME</FullName>
    <OrgAddressCollection>
      <OrgAddress Action=""MERGE"">
        <PK>bdfc0aed-b0b8-4e89-89b0-9558e7e8a69e</PK>
        <IsActive>true</IsActive>
        <Code>Address1</Code>
        <Language>ENG</Language>
        <Address1>1 Street</Address1>
        <City>City</City>
        <PostCode>1000</PostCode>
        <OrgAddressCapabilityCollection>
          <OrgAddressCapability Action=""MERGE"">
            <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87</PK>
            <AddressType>PAD</AddressType>
            <IsMainAddress>true</IsMainAddress>
          </OrgAddressCapability>
        </OrgAddressCapabilityCollection>
      </OrgAddress>
    </OrgAddressCollection>
    <ClosestPort TableName=""RefUNLOCO"">
      <Code>BGSOF</Code>
    </ClosestPort>
  </OrgHeader>
</Organization>";

			var message = XElement.Parse(sourceXML);
			var request = new Request { EntitySets = new[] { message } };

			var factoryProvider = new BadFactoryProvider();
			var updateHandler = new UpdateHandler(factoryProvider);
			updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();
			var response = updateHandler.Execute(request, new DummyResponseFactory());
			var responseMessageAsText = string.Join("\r\n", response.Informations);

			const string expectedErrorStartsWith = "Error - Japan!";

			AssertStartsWith("Duplicate Property ApplicationCode", expectedErrorStartsWith, responseMessageAsText);
		}

		class BadFactoryProvider : INativeFactoryProvider
		{
			protected DataRow MakeDummyRow()
			{
				DataTable table = new DataTable("BlahBlah");
				DataColumn col = new DataColumn("PK", typeof(Guid));
				table.Columns.Add(col);
				table.PrimaryKey = new DataColumn[] { col };
				DataRow result = table.NewRow();
				return result;
			}
			public BusinessObjectFactory GetNewFactory(DbConnection connection)
			{
				var f = new BusinessObjectFactory(connection);
				f.Saving += _ => throw new ZSaveException(new FriendlyDataException(new Exception(), MakeDummyRow(), connection), f);
				return f;
			}
		}

		[Serializable]
		class FriendlyDataException : ZDataException
		{
			public FriendlyDataException(Exception innerEx, DataRow row, DbConnection connection)
				: this(innerEx, "Japan!", "Whop whop whop", row, connection)
			{
			}

#if NETFRAMEWORK
			protected FriendlyDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif

			protected FriendlyDataException(Exception innerEx, string friendlyMessage, string debugMessage, DataRow row, DbConnection connection) : base(innerEx, friendlyMessage, debugMessage, row, connection)
			{
			}
		}

		public void TestNullFactoryProvider_ReturnsUpdateResponseWithErrorLog()
		{
			using (new DisposableAction(() => ErrorReporter.Clear()))
			{
				var handler = new UpdateHandler(null);
				var request = GetRequestForOrgWithContact();
#if NET
				string expectedLogMessage =
					@"Error - Value cannot be null. (Parameter 'factoryProvider')";
#else
				string expectedLogMessage =
					@"Value cannot be null.
Parameter name: factoryProvider";
				
#endif
				Response response = null;

				AssertNoExceptionThrown(() => response = handler.Execute(request, new DummyResponseFactory()));

				AssertNotNull(response);
				var entityInfo = response.EntityInfo;
				CombineAssertions(() =>
				{
					AssertContains(expectedLogMessage, (string)response.Informations.GetValue(0));
					AssertNull(entityInfo.Name);
					AssertNull(entityInfo.TableName);
					AssertNull(entityInfo.PrimaryKey);
				});
			}
		}

		public void TestExceptionUseToString()
		{
			var handler = new UpdateHandler(new FactoryProvider());
			handler.Parser.DefinitionFinder = new ExplosiveIDefinitionFinder(new NullReferenceException("I got sunshine on a cloudy day."));

			var request = GetRequestForOrgWithContact();

			var response = handler.Execute(request, new DummyResponseFactory());

			var responseMessageAsText = string.Join("\r\n", response.Informations);
			AssertContains("responseMessageAsText should have Type and Exception Message", @"Error - I got sunshine on a cloudy day.
This error has been submitted to WTG for further investigation.

   at Enterprise.DataTransfer.Native.Business.Update.UpdateHandlerTest.ExplosiveIDefinitionFinder.FindByEntitySetName(String entitySetName) in ", responseMessageAsText);
			AssertContains("responseMessageAsText should have Call Stack, or at least the top of it.", "at Enterprise.DataTransfer.Native.Business.Update.UpdateHandler.Deserialize", responseMessageAsText);
			AssertContains("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestZDataExceptionWithoutFriendlyMessageIsSentToWTG()
		{
			var handler = new UpdateHandler(new FactoryProvider());
			SqlErrorCollection sqlErrors = SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(1, 2, 3, "MyServer", "Get me a higher life.", "MyProcedure", 4));
			SqlException sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrors);
			handler.Parser.DefinitionFinder = new ExplosiveIDefinitionFinder(new ZDataException(sqlException, null, null));

			var request = GetRequestForOrgWithContact();
			var response = handler.Execute(request, new DummyResponseFactory());

			var responseMessageAsText = string.Join("\r\n", response.Informations);
			AssertContains("responseMessageAsText should have Stack Trace", @"Error - <ROW IS NULL>
InnerException Message = Get me a higher life.
This error has been submitted to WTG for further investigation.

   at Enterprise.DataTransfer.Native.Business.Update.UpdateHandlerTest.ExplosiveIDefinitionFinder.FindByEntitySetName", responseMessageAsText);
			AssertContains("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestZDataExceptionWithFriendlyMessageNotSendToWTG()
		{
			var handler = new UpdateHandler(new FactoryProvider());
			SqlErrorCollection sqlErrors = SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(4060, 1, 1, "MyServer", "Cannot open database", "MyProcedure", 1));
			SqlException sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrors);
			handler.Parser.DefinitionFinder = new ExplosiveIDefinitionFinder(new ZDataException(sqlException, null, null));

			var request = GetRequestForOrgWithContact();
			var response = handler.Execute(request, new DummyResponseFactory());

			var responseMessageAsText = string.Join("\r\n", response.Informations);

			AssertContains("responseMessageAsText should have Friendly Message", @"Database login failed - please check the server error log.
If the problem persists then please contact your system administrator.

Message: Cannot open database", responseMessageAsText);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestCreateColumnWithoutUpgrade()
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.CusClassPartPivot ADD CI_FooNonExistent INT");
				var sourceXML = @"
<Product xmlns:knc=""CargoWise"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
	<PartNum>PART-ABC</PartNum>
	<Desc>Part description</Desc>
	<StockKeepingUnit>UNT</StockKeepingUnit>
	<Weight>1.000</Weight>
	<WeightUQ>KG</WeightUQ>
	<NetWeight>1.000</NetWeight>
	<CusClassPartPivotCollection>
	  <CusClassPartPivot Action=""MERGE"">
		<TariffNum>9403509041</TariffNum>
		<ChildType>HTI</ChildType>
		<Country>
			<Code>US</Code>
		</Country>
		<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
		  <ComponentCusClassPartPivot Action=""MERGE"">
			<TariffNum>9403509041</TariffNum>
			<ChildListOrder>1</ChildListOrder>
			<DateStart>2019-08-05T00:00:00</DateStart>
			<DateEnd>2019-08-05T00:00:00</DateEnd>
			<Country TableName=""RefCountry"">
			  <Code>US</Code>
			</Country>
		  </ComponentCusClassPartPivot>
		</ComponentCusClassPartPivotCollection>
	  </CusClassPartPivot>
	</CusClassPartPivotCollection>
	<OrgPartRelationCollection>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>OWN</Relationship>
		<OrgHeader>
		  <Code>ARTNRO</Code>
		</OrgHeader>
	  </OrgPartRelation>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>SUP</Relationship>
		<OrgHeader>
		  <Code>EDICUS</Code>
		</OrgHeader>
	  </OrgPartRelation>
	</OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>";
				var message = XElement.Parse(sourceXML);
				var request = new Request { EntitySets = new[] { message } };
				var updateHandler = new UpdateHandler(new FactoryProvider());
				updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();
				AssertNoExceptionThrown(() =>
				{
					updateHandler.Execute(request, new DummyResponseFactory());
				});
			}
		}

		public void TestCreateColumnWithoutUpgrade_ForeignKey()
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.CusClassPartPivot ADD CI_Foo_NonExistent INT");
				var sourceXML = @"
<Product xmlns:knc=""CargoWise"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
	<PartNum>PART-ABC</PartNum>
	<Desc>Part description</Desc>
	<StockKeepingUnit>UNT</StockKeepingUnit>
	<Weight>1.000</Weight>
	<WeightUQ>KG</WeightUQ>
	<NetWeight>1.000</NetWeight>
	<CusClassPartPivotCollection>
	  <CusClassPartPivot Action=""MERGE"">
		<TariffNum>9403509041</TariffNum>
		<ChildType>HTI</ChildType>
		<Country>
			<Code>US</Code>
		</Country>
		<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
		  <ComponentCusClassPartPivot Action=""MERGE"">
			<TariffNum>9403509041</TariffNum>
			<ChildListOrder>1</ChildListOrder>
			<DateStart>2019-08-05T00:00:00</DateStart>
			<DateEnd>2019-08-05T00:00:00</DateEnd>
			<Country TableName=""RefCountry"">
			  <Code>US</Code>
			</Country>
		  </ComponentCusClassPartPivot>
		</ComponentCusClassPartPivotCollection>
	  </CusClassPartPivot>
	</CusClassPartPivotCollection>
	<OrgPartRelationCollection>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>OWN</Relationship>
		<OrgHeader>
		  <Code>ARTNRO</Code>
		</OrgHeader>
	  </OrgPartRelation>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>SUP</Relationship>
		<OrgHeader>
		  <Code>EDICUS</Code>
		</OrgHeader>
	  </OrgPartRelation>
	</OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>";
				var message = XElement.Parse(sourceXML);
				var request = new Request { EntitySets = new[] { message } };
				var updateHandler = new UpdateHandler(new FactoryProvider());
				updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();
				AssertNoExceptionThrown(() =>
				{
					updateHandler.Execute(request, new DummyResponseFactory());
				});
			}
		}

		class ExplosiveIDefinitionFinder : IDefinitionFinder
		{
			public ExplosiveIDefinitionFinder(Exception exceptionToThrow)
			{
				this.exceptionToThrow = exceptionToThrow;
			}
			readonly Exception exceptionToThrow;

			#region Implementation for every interface method throwing exceptionToThrow.

			public bool HasDefinitionWithEntitySetName(string entitySetName)
			{
				throw exceptionToThrow;
			}

			public bool HasDefinitionWithTopTableName(string topLevelTableName)
			{
				throw exceptionToThrow;
			}

			public Common.Definitions.EntitySetDefinitions.EntitySetDefinition FindByEntitySetName(string entitySetName)
			{
				throw exceptionToThrow;
			}

			public Common.Definitions.EntitySetDefinitions.EntitySetDefinition FindByTopTableName(string topLevelTableName)
			{
				throw exceptionToThrow;
			}

			public Common.Definitions.EntityDefinitions.EntityDefinition FindDefinition(string entitySetName, string entityName)
			{
				throw exceptionToThrow;
			}

			#endregion
		}

		public void TestUpdateData_Insert()
		{
			var response = InsertOrgWithContact();

			AssertEquals(NativeResponseStatus.Accepted, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(i => i.Contains("Information - OrgHeader - 1 inserts, 0 updates, 0 deletes")));
			Assert(informations.Any(i => i.Contains("Information - OrgContact - 1 inserts, 0 updates, 0 deletes")));
		}

		public void TestUpdateData_Insert_DbLoggingAfterInsert()
		{
			var countOrgBeforeTest = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.StmALog where SL_Table = 'OrgHeader'");
			var countContactBeforeTest = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.StmALog where SL_Table = 'OrgContact'");

			InsertOrgWithContact();

			var countOrgAfterTest = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.StmALog where SL_Table = 'OrgHeader'");
			var countContactAfterTest = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.StmALog where SL_Table = 'OrgContact'");

			AssertEquals("Should add one record (DIM) to StmALog table after insert dbo.OrgHeader successfully", countOrgBeforeTest + 1, countOrgAfterTest);
			AssertEquals("Should NOT add a record to StmALog table after insert dbo.OrgContact successfully", countContactBeforeTest, countContactAfterTest);
		}

		public void TestDimAndAddOccurs()
		{
			InsertOrgWithContact();

			var orgPk = connection.ExecuteScalar("SELECT OH_PK FROM dbo.OrgHeader where OH_FullName = 'Zayden Zubin Rakhsh'");

			var logs = connection.ExecuteScalar(string.Format("SELECT SL_SE_NKEvent FROM dbo.StmALog where SL_Parent = '{0}' AND SL_SE_NKEvent = 'ADD'", orgPk));
			AssertNull("No ADD event", logs);

			logs = connection.ExecuteScalar(string.Format("SELECT SL_SE_NKEvent FROM dbo.StmALog where SL_Parent = '{0}' AND SL_SE_NKEvent = 'DIM'", orgPk));
			AssertEquals("DIM event should of occured", "DIM", logs);
		}

		Response InsertOrgWithContact()
		{
			var request = GetRequestForOrgWithContact();
			return updateHandler.Execute(request, new DummyResponseFactory());
		}

		Request GetRequestForOrgWithContact()
		{
			var input = new XElement(ns + "Organization",
				new XElement(ns + "OrgHeader",
					new XAttribute("Action", "MERGE"),
					new XElement(ns + "Code", "ZUBORG"),
					new XElement(ns + "FullName", "Zayden Zubin Rakhsh"),
					new XElement(ns + "ClosestPort",
						new XElement(ns + "Code", "AUBNE")
					),
					new XElement(ns + "OrgContactCollection",
						new XElement(ns + "OrgContact",
							new XAttribute("Action", "Insert"),
							new XElement(ns + "ContactName", "Rakhsh")
						)
					)
				)
			);
			return new Request { EntitySets = new[] { input } };
		}

		[UseSnapshotProtection]
		public void TestUpdateData_Delete_ConcurrencyError_ParallelLoad()
		{
			TestUpdateData_Delete_ConcurrencyError_Core(
				expectedMessage: "Warning - A data concurrency issue has occurred. Attempting to retry.",
				expectedToNotAppear: "Error - A data concurrency issue has occurred multiple times.",
				retries: 3);
		}

		[UseSnapshotProtection]
		public void TestUpdateData_Delete_ConcurrencyError_NoRetryToForceConcurrencyFailure()
		{
			TestUpdateData_Delete_ConcurrencyError_Core(
				expectedMessage: "Error - A data concurrency issue has occurred multiple times.",
				expectedToNotAppear: null,
				retries: 0);
		}

		void TestUpdateData_Delete_ConcurrencyError_Core(string expectedMessage, string expectedToNotAppear, int retries)
		{
			using (RunNonTransactioned())
			{
				var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.ProductMatchings.TestFiles.15. OWN and SUP are same.xml"));
				var manager = new ImportHandler(new AncillaryImportServices());
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "CRAIMPCHI";
				var buyer = Factory.New<OrgHeader>();
				buyer.OH_Code = "ABCEXPBNE";
				using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					Factory.Save();
					manager.Import(data.BaseStream);
					var responseList = new List<Response>();
					var loader = new OrgSupplierPart.Loader(Factory);
					var product = loader.Load("PRODUCTA", supplier.PK, buyer.PK);
					BusinessObjectFactory.ThrowExceptionWhenSaveCountReachesLimit(1, new ZDataConcurrencyException(new Exception(), ((INeedRow)product).Row, Db.Connection));
					var response = DeleteOrgPart(product.PK, retries);

					CombineAssertions(() =>
					{
						AssertContains("Information - Importing Product: PRODUCTA", string.Join("\r\n", response.Informations));
						AssertNotEquals(NativeResponseStatus.Accepted, response.Status);
						var actualMessage = string.Join("\r\n", response.Informations);
						AssertContains(expectedMessage, actualMessage, true);
						if (expectedToNotAppear != null)
						{
							AssertNotContains(expectedToNotAppear, actualMessage, true);
						}
					});
				}
			}
		}

		static Response DeleteOrgPart(ZGuid pk, int retries)
		{
			var request = GetRequestForOrgPart(pk);
			var updateHandler = new UpdateHandler(new FactoryProvider());
			updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();
			updateHandler.OverrideRetriesForTest(retries);
			return updateHandler.Execute(request, new DummyResponseFactory());
		}

		static Request GetRequestForOrgPart(ZGuid pk)
		{
			var xml = $@"
<Product version=""2.0"">
    <OrgSupplierPart Action=""DELETE"">
		<PartNum>PRODUCTA</PartNum>
		<PK>{pk}</PK>
		<OrgPartRelationCollection>
			<OrgPartRelation Action=""DELETE"">
				<Relationship>OWN</Relationship>
				<OrgHeader>
					<Code>CRAIMPCHI</Code>
				</OrgHeader>
			</OrgPartRelation>
			<OrgPartRelation Action=""DELETE"">
				<Relationship>SUP</Relationship>
				<OrgHeader>
					<Code>CRAIMPCHI</Code>
				</OrgHeader>
			</OrgPartRelation>
		</OrgPartRelationCollection>
    </OrgSupplierPart>
</Product>";
			return new Request { EntitySets = new[] { XElement.Parse(xml) } };
		}

		public void TestMergeData_Org()
		{
			var countOrgBefore = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgHeader");
			var countContactBefore = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgContact");

			var input = new XElement(ns + "Organization",
				new XElement(ns + "OrgHeader",
					new XAttribute("Action", "MERGE"),
					new XElement(ns + "Code", "ZUBORG"),
					new XElement(ns + "FullName", "Zayden Zubin Rakhsh"),
					new XElement(ns + "ClosestPort",
						new XElement(ns + "Code", "AUBNE")
					),
					new XElement(ns + "OrgContactCollection",
						new XElement(ns + "OrgContact",
							new XAttribute("Action", "Merge"),
							new XElement(ns + "ContactName", "Rakhsh")
						)
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var countOrgMid = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgHeader");
			var countContactMid = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgContact");

			AssertEquals(NativeResponseStatus.Accepted, response.Status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Information - OrgHeader - 1 inserts, 0 updates, 0 deletes")));
			Assert(informations.Any(e => e.Contains("Information - OrgContact - 1 inserts, 0 updates, 0 deletes")));

			AssertEquals("Should add a record to OrgHeader table after insert dbo.OrgHeader successfully", countOrgBefore + 1, countOrgMid);
			AssertEquals("Should add a record to OrgContact table after insert dbo.OrgContact successfully", countContactBefore + 1, countContactMid);
		}

		public void TestNoChangesNoUpdates()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);

			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 0 updates, 0 deletes";

			AssertEquals("No changes so should be no updates", expectedLogs, logs);
		}

		public void TestEdtEventOnUpdateOnly()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			header.GetLogs().CancelAll();
			AssertEquals("Precondition: No EDT event", 0, header.GetLogs().Find(l => l.SL_SE_NKEvent == Events.EditedARecordCode && !l.IsCancelled).Count());

			RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);

			AssertEquals("No EDT event", 0, header.GetLogs().Find(l => l.SL_SE_NKEvent == Events.EditedARecordCode && !l.IsCancelled).Count());

			header.OH_FullName = "UPDATED NAME";
			factory.Save();
			RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);

			AssertEquals("1 EDT event", 1, header.GetLogs().Find(l => l.SL_SE_NKEvent == Events.EditedARecordCode && !l.IsCancelled).Count());
		}

		public void TestUpdateBooleanValue()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddressCapability.PZ_IsMainAddress = !orgAddressCapability.PZ_IsMainAddress;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);
			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 1 updates, 0 deletes
Information - StmNote - 0 inserts, 0 updates, 0 deletes";

			AssertEquals("Boolean update", expectedLogs, logs);
		}

		public void TestUpdateBooleanValue_Empty()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddressCapability.PZ_IsMainAddress = !orgAddressCapability.PZ_IsMainAddress;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, isMainAddress: "<IsMainAddress></IsMainAddress>");

#if NET
			var expectedLogs = "String '' was not recognized as a valid Boolean";
#else
			var expectedLogs = "String was not recognized as a valid Boolean.";
#endif
			AssertContains(expectedLogs, logs);
		}

		public void TestUpdateBooleanValue_Null()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddressCapability.PZ_IsMainAddress = !orgAddressCapability.PZ_IsMainAddress;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, isMainAddress: "<IsMainAddress />");
#if NET
			var expectedLogs = "String '' was not recognized as a valid Boolean";
#else
			var expectedLogs = "String was not recognized as a valid Boolean.";
#endif

			AssertContains(expectedLogs, logs);
		}

		public void TestUpdateStringValue()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			orgAddress.Address1 = "NewAddress";
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);
			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 1 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 1 updates, 0 deletes";

			AssertEquals("String and char update", expectedLogs, logs);
		}

		public void TestUpdateStringValue_Empty()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			orgAddress.Address1 = "NewAddress";
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, address1: "<Address1></Address1>");
			var expectedLogs = @"Cannot import to table 'OrgAddress' because constraint";
			AssertContains(expectedLogs, logs);
		}

		public void TestUpdateStringValue_Null()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			orgAddress.Address1 = "NewAddress";
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, address1: "<Address1 />");
			var expectedLogs = @"Cannot import to table 'OrgAddress' because constraint";
			AssertContains(expectedLogs, logs);
		}

		public void TestUpdateVarBinaryValue()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			note = factory.Load<StmNote>(note.PK);
			note.ST_NoteData = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Tahoma;}}testing123testing\r\n\\viewkind4\\uc1\\pard\\f0\\fs17\\par\r\n}\r\n\0");
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);
			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 1 updates, 0 deletes";

			AssertEquals("VarBinary update", expectedLogs, logs);
		}

		public void TestUpdateVarBinaryValue_Empty()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			note = factory.Load<StmNote>(note.PK);
			note.ST_NoteData = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Tahoma;}}testing123testing\r\n\\viewkind4\\uc1\\pard\\f0\\fs17\\par\r\n}\r\n\0");
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, noteData: "<NoteData></NoteData>");

			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 1 updates, 0 deletes";

			AssertEquals("VarBinary update", expectedLogs, logs);

			var reloadFactory = new BusinessObjectFactory();
			note = reloadFactory.Load<StmNote>(note.PK);
			AssertEquals("Should clear var binary data", ZBlob.FromUTF8(""), note.ST_NoteData);
		}

		public void TestUpdateVarBinaryValue_Null()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			note = factory.Load<StmNote>(note.PK);
			note.ST_NoteData = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Tahoma;}}testing123testing\r\n\\viewkind4\\uc1\\pard\\f0\\fs17\\par\r\n}\r\n\0");
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, noteData: "<NoteData />");

			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 1 updates, 0 deletes";

			AssertEquals("VarBinary update", expectedLogs, logs);

			var reloadFactory = new BusinessObjectFactory();
			note = reloadFactory.Load<StmNote>(note.PK);
			AssertEquals("Should clear var binary data", ZBlob.FromUTF8(""), note.ST_NoteData);
		}

		public void TestUpdateDateValue()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddress.OA_PickupToTimeOnly = ZDateTime.Now;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);
			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 1 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 0 updates, 0 deletes";

			AssertEquals("Date update", expectedLogs, logs);
		}

		public void TestUpdateDateValue_Empty()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddress.OA_PickupToTimeOnly = ZDateTime.Now;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, pickupToTimeOnly: "<PickupToTimeOnly></PickupToTimeOnly>");
			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 1 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 0 updates, 0 deletes";

			AssertEquals("Date update", expectedLogs, logs);
		}

		public void TestUpdateDateValue_Null()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddress.OA_PickupToTimeOnly = ZDateTime.Now;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, pickupToTimeOnly: "<PickupToTimeOnly />");
			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 1 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 0 updates, 0 deletes";

			AssertEquals("Date update", expectedLogs, logs);
		}

		public void TestUpdateDecimalValue()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddress.OA_GroupNumber = 3;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);
			var expectedLogs =
@"Information - OrgHeader - 0 inserts, 0 updates, 0 deletes
Information - OrgAddress - 0 inserts, 1 updates, 0 deletes
Information - OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
Information - StmNote - 0 inserts, 0 updates, 0 deletes";

			AssertEquals("Date update", expectedLogs, logs);
		}

		public void TestUpdateDecimalValue_Empty()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddress.OA_GroupNumber = 3;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, groupNumber: "<GroupNumber></GroupNumber>");
#if NET
			var expectedLogs = "The input string '' was not in a correct format";
#else
			var expectedLogs = "Input string was not in a correct format.";
#endif
			AssertContains(expectedLogs, logs);
		}

		public void TestUpdateDecimalValue_Null()
		{
			var (factory, header, note, orgAddress, orgAddressCapability) = CreateObjectsForTest();

			orgAddress.OA_GroupNumber = 3;
			factory.Save();

			var logs = RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK, groupNumber: "<GroupNumber />");
#if NET
			var expectedLogs = "The input string '' was not in a correct format.";
#else
			var expectedLogs = "Input string was not in a correct format.";
#endif
			AssertContains(expectedLogs, logs);
		}

		public void TestUpdateData_InsertChildThroughJunctionTable()
		{
			var countryPK = TestUtil.PrepareCountryData();
			var code = "ZZZZ";
			var sql = string.Format("SELECT count(*) FROM dbo.RefZonePivot JOIN dbo.RefZoneHeader ON F2_FZ = FZ_PK where FZ_Code = '{0}'", code);

			var beforeCount = (int)connection.ExecuteScalar(sql);

			var input = new XElement(ns + "Country",
				new XElement(ns + "RefCountry",
					new XElement(ns + "PK", countryPK),
					new XElement(ns + "RefZoneHeaderCollection",
						new XElement(ns + "RefZoneHeader",
							new XAttribute("Action", "Insert"),
							new XElement(ns + "Code", code),
							new XElement(ns + "Description", code + " desc"),
							new XElement(ns + "ZoneType", "ALL"),
							new XElement(ns + "ZoneMode", "ALL"),
							new XElement(ns + "SystemCreateTimeUtc", DateTime.UtcNow),
							new XElement(ns + "SystemCreateUser", "~BP"),
							new XElement(ns + "SystemLastEditTimeUtc", DateTime.UtcNow),
							new XElement(ns + "SystemLastEditUser", "~BP")
						)
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			updateHandler.Execute(request, new DummyResponseFactory());

			var afterCount = (int)connection.ExecuteScalar(sql);
			AssertEquals(beforeCount + 1, afterCount);
		}

		public void TestInsertData_DontLoadBusinessObjectsForTemplateApplicationWhenTheyDontSupportTemplateApplication()
		{
			var code = "ZZZZ";
			var input = new XElement(ns + "Country",
				new XElement(ns + "RefCountry",
					new XElement(ns + "PK", TestUtil.PrepareCountryData()),
					new XElement(ns + "RefZoneHeaderCollection",
						new XElement(ns + "RefZoneHeader",
							new XAttribute("Action", "Insert"),
							new XElement(ns + "Code", code),
							new XElement(ns + "Description", code + " desc"),
							new XElement(ns + "ZoneType", "ALL"),
							new XElement(ns + "ZoneMode", "ALL"),
							new XElement(ns + "SystemCreateTimeUtc", DateTime.UtcNow),
							new XElement(ns + "SystemCreateUser", "~BP"),
							new XElement(ns + "SystemLastEditTimeUtc", DateTime.UtcNow),
							new XElement(ns + "SystemLastEditUser", "~BP")
						)
					)
				)
			);

			var refZoneHeaderObjectsInFactory = -1;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (factory.NameForDebugging.Equals("Native Xml"))
				{
					refZoneHeaderObjectsInFactory = factory.Load<RefZoneHeader>(new ZQuery { FetchOnlyFromLocalCache = true }).Length;
				}
			});

			var request = new Request { EntitySets = new[] { input } };
			updateHandler.Execute(request, new DummyResponseFactory());

			AssertNotNull("Precondition: Expecting RefZoneHeader is created from import", Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, code)));
			AssertNotEquals("Precondition: This test assumes 'Native XML' factory will be saved", -1, refZoneHeaderObjectsInFactory);

			AssertEquals("Tables that do not support workflow should not be loaded as business objects", 0, refZoneHeaderObjectsInFactory);
		}

		public void TestUpdateData_DeleteChildThroughJunctionTable()
		{
			var countryPK = TestUtil.PrepareCountryData();
			var zoneHeaderPK = TestUtil.PrepareZoneHeaderData();
			var zonePivotPK = TestUtil.PrepareZonePivotData(countryPK, zoneHeaderPK);

			var deleteRecordCount = (int)connection.ExecuteScalar("select count(*) from dbo.RefZoneHeader where FZ_PK = '" + zoneHeaderPK + "'");
			var junctionRecordCount = (int)connection.ExecuteScalar("select count(*) from dbo.RefZonePivot where F2_PK = '" + zonePivotPK + "'");

			var input = new XElement(ns + "Country",
				new XElement(ns + "RefCountry",
					new XElement(ns + "PK", countryPK),
					new XElement(ns + "RefZoneHeaderCollection",
						new XElement(ns + "RefZoneHeader",
							new XAttribute("Action", "Delete"),
							new XElement(ns + "PK", zoneHeaderPK)
						)
					)
				)
			);

			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Information - RefZoneHeader - 0 inserts, 0 updates, 1 deletes")));

			AssertEquals(deleteRecordCount - 1, (int)connection.ExecuteScalar("select count(*) from dbo.RefZoneHeader where FZ_PK = '" + zoneHeaderPK + "'"));
			AssertEquals(junctionRecordCount - 1, (int)connection.ExecuteScalar("select count(*) from dbo.RefZonePivot where F2_PK = '" + zonePivotPK + "'"));
		}

		public void TestUpdateData_InsertWithParentTableCode()
		{
			var count = (int)connection.ExecuteScalar("select count(*) FROM dbo.RefZonePivot Where F2_ParentID = '44EF0941-AC8E-412B-BC7D-BF248CE7F777' and F2_ParentTableCode = 'RL'");

			var input = new XElement(ns + "UNLOCO",
				new XElement(ns + "RefUNLOCO",
					new XAttribute("Action", "Update"),
					new XElement(ns + "PK", new Guid("44EF0941-AC8E-412B-BC7D-BF248CE7F777")),
					new XElement(ns + "RefZonePivotCollection",
						new XElement(ns + "RefZonePivot",
							new XAttribute("Action", "Insert"),
							new XElement(ns + "RefZoneHeader",
								new XElement(ns + "Code", "USCA")
							)
						)
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			CombineAssertions(() =>
			{
				var status = response.Status;
				AssertEquals(NativeResponseStatus.Accepted, status);

				var informations = response.Informations;
				Assert(informations.Any(e => e.Contains("Information - RefZonePivot - 1 inserts, 0 updates, 0 deletes")));

				AssertEquals(count + 1, (int)connection.ExecuteScalar("select count(*) FROM dbo.RefZonePivot Where F2_ParentID = '44EF0941-AC8E-412B-BC7D-BF248CE7F777' and F2_ParentTableCode = 'RL'"));
			});
		}

		public void TestUpdateData_InsertWithBelongsToEntity_Polymorphic()
		{
			TestUtil.AlterDummyTable();

			updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();

			var fk = TestUtil.PrepareRefUNLOCOTableData();
			var sql = "select count(*) FROM dbo.RefZonePivot Where F2_ParentID = '" + fk + "' and F2_ParentTableCode = 'RL'";
			var countBeforeTest = (int)connection.ExecuteScalar(sql);

			var input = new XElement(ns + "ZonePivot",
				new XElement(ns + "RefZonePivot",
					new XAttribute("Action", "Insert"),
					new XElement(ns + "RefZoneHeader",
						new XElement(ns + "Code", "USCA")
					),
					new XElement(ns + "OrgHeader",
						new XElement(ns + "PK", fk)
					),
					new XElement(ns + "RefUNLOCO",
						new XElement(ns + "PK", fk)
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			//"<?xml version='1.0' encoding='utf-16'?>";
			//"<UpdateDataResponse EntitySet='ZonePivot'>";
			//"  <Status>Accepted</Status>";
			//"  <Notifications>";
			//"    <Information>";
			//"      <Item>Information - RefZonePivot - 1 inserts, 0 updates, 0 deletes.</Item>";
			//"    </Information>";
			//"  </Notifications>";
			//"</UpdateDataResponse>";

			CombineAssertions(() =>
			{
				var status = response.Status;
				AssertEquals(NativeResponseStatus.Accepted, status);

				var informations = response.Informations;
				Assert(informations.Any(e => e.Contains("Information - RefZonePivot - 1 inserts, 0 updates, 0 deletes")));

				var countAfterTest = (int)connection.ExecuteScalar(sql);
				AssertEquals(countBeforeTest + 1, countAfterTest);
			});
		}

		public void TestUpdateData_InsertBelongsToWithInvalidKey()
		{
			TestUtil.AlterDummyTable();
			updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();

			var fk = Guid.NewGuid();
			var sql = "select count(*) FROM dbo.RefZonePivot Where F2_ParentID = '" + fk + "' and F2_ParentTableCode = 'FZ'";
			var countBeforeTest = (int)connection.ExecuteScalar(sql);

			var input = new XElement(ns + "ZonePivot",
				new XElement(ns + "RefZonePivot",
					new XAttribute("Action", "Insert"),
					new XElement(ns + "RefZoneHeader",
						new XElement(ns + "PK", fk)
					)
				)
			);

			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var countAfterTest = (int)connection.ExecuteScalar(sql);
			AssertEquals(countBeforeTest, countAfterTest);

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, status);

			var informations = response.Informations;
			var expectedMessage = string.Format("Could not insert/update the Included Locations (RefZonePivot) as it had an invalid reference to a RefZoneHeader (RefZoneHeader). There is no RefZoneHeader with the following values: [PK:{0}].", fk);
			AssertContains(expectedMessage, (string)informations.GetValue(0));
		}

		public void TestUpdateData_InsertBelongsToWithEmptyElement()
		{
			TestUtil.AlterDummyTable();
			updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();

			var fk = Guid.NewGuid();
			var sql = "select count(*) FROM dbo.RefZonePivot Where F2_ParentID = '" + fk + "' and F2_ParentTableCode = 'FZ'";
			var countBeforeTest = (int)connection.ExecuteScalar(sql);

			var input = new XElement(ns + "ZonePivot",
				new XElement(ns + "RefZonePivot",
					new XAttribute("Action", "Insert"),
					new XElement(ns + "RefZoneHeader"),
					new XElement(ns + "ParentTableCode", "RL")
				)
			);

			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var countAfterTest = (int)connection.ExecuteScalar(sql);
			AssertEquals(countBeforeTest, countAfterTest);

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, status);

			var informations = response.Informations;
			var expectedMessage = "Error - The Included Locations (RefZonePivot) cannot be inserted/updated, requires a reference to a valid RefZoneHeader.";
			AssertContains(expectedMessage, (string)informations.GetValue(0));
		}

		public void TestUpdateData_InsertOrUpdate()
		{
			var input = new XElement(ns + "CurrencyExchangeRate",
				new XElement(ns + "RefExchangeRate",
					new XAttribute("Action", "Update"),
					new XElement(ns + "PK", new Guid("fe99e9df-56d5-47ab-a23e-bdf02d118721")),
					new XElement(ns + "ExRateType", "CUS"),
					new XElement(ns + "ExpiryDate", "31/12/2030 12:00:00 AM"),
					new XElement(ns + "SellRate", 1.390000000),
					new XElement(ns + "StartDate", "1/01/2003 12:00:00 AM"),
					new XElement(ns + "RefCurrency",
						new XElement(ns + "PK", new Guid("60aae969-b80b-4a40-9b2d-810d3385c76e")),
						new XElement(ns + "Code", "USD")
					),
					new XElement(ns + "GlbCompany",
						new XElement(ns + "PK", new Guid("878d7aca-ffc3-49fc-9710-969ca0c0f2ac")),
						new XElement(ns + "Code", "EDI")
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			CombineAssertions(delegate
			{
				var status = response.Status;
				var informationElements = response.Informations;

				AssertEquals(informationElements.FirstOrDefault(i => i.Contains("Error")), NativeResponseStatus.Accepted, status);
				Assert(informationElements.Any(e => e.Contains("Information - RefExchangeRate - 0 inserts, 1 updates, 0 deletes")));
			});
		}

		public void TestUpdateData_InsertOrUpdate_ShouldInsertWhenUpdateFail()
		{
			var count = (int)connection.ExecuteScalar("select count(*) FROM dbo.RefExchangeRate Where RE_RX_NKExCurrency = 'USD'");
			var input = new XElement(ns + "CurrencyExchangeRate",
				new XElement(ns + "RefExchangeRate",
					new XAttribute("Action", "Update"),
					new XElement(ns + "ExRateType", "CUS"),
					new XElement(ns + "ExpiryDate", "31/12/2030 12:00:00 AM"),
					new XElement(ns + "SellRate", 1.390000000),
					new XElement(ns + "StartDate", "1/01/2003 12:00:00 AM"),
					new XElement(ns + "RefCurrency",
						new XElement(ns + "Code", "USD")
					),
					new XElement(ns + "GlbCompany",
						new XElement(ns + "PK", new Guid("878d7aca-ffc3-49fc-9710-969ca0c0f2ac")),
						new XElement(ns + "Code", "EDI")
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			//"<?xml version=\"1.0\" encoding=\"utf-16\"?>";
			//"<UpdateDataResponse EntitySet=\"CurrencyExchangeRate\">";
			//"  <Status>Accepted</Status>";
			//"  <Notifications>";
			//"    <Information>";
			//"      <Item>Information - RefExchangeRate - 1 inserts, 0 updates, 0 deletes.</Item>";
			//"    </Information>";
			//"  </Notifications>";
			//"</UpdateDataResponse>";

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, status);

			var informationElements = response.Informations;
			Assert(informationElements.Any(e => e.Contains("Information - RefExchangeRate - 1 inserts, 0 updates, 0 deletes")));

			AssertEquals(count + 1, (int)connection.ExecuteScalar("select count(*) FROM dbo.RefExchangeRate Where RE_RX_NKExCurrency = 'USD'"));
		}

		public void TestUpdateData_Update()
		{
			AssertNotEquals(Core.SharedConstants.Languages.German, connection.ExecuteScalar("select OC_Language from dbo.OrgContact where OC_PK = '72613114-24ea-41e3-bc95-d70e614e676f'"));

			const string code = "4BELEVORD";
			const string fullName = "Zayden Zubin Org";

			var input = new XElement(ns + "Organization",
				new XElement(ns + "OrgHeader",
					new XElement(ns + "PK", new Guid("24181eea-d3e5-4afe-8892-8684ab555879")),
					new XElement(ns + "Code", code),
					new XElement(ns + "FullName", fullName),
					new XElement(ns + "OrgContactCollection",
						new XElement(ns + "OrgContact",
							new XAttribute("Action", "Update"),
							new XElement(ns + "PK", new Guid("72613114-24ea-41e3-bc95-d70e614e676f")),
							new XElement(ns + "ContactName", "JOHN CHATFIELD"),
							new XElement(ns + "Language", Core.SharedConstants.Languages.German),
							new XElement(ns + "NotifyMode", "FAX")
						)
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Information - OrgContact - 0 inserts, 1 updates, 0 deletes")));

			AssertEquals(Core.SharedConstants.Languages.German, connection.ExecuteScalar("select OC_Language from dbo.OrgContact where OC_PK = '72613114-24ea-41e3-bc95-d70e614e676f'"));
		}

		public void TestUpdateData_Update_WithCandidateKey_DontAllowMixedCase()
		{
			bool oldValue = false;
			try
			{
				oldValue = Environment.Env.Registry.OrgAllowMixedCase;
				Environment.Env.Registry.SetOrgAllowMixedCase(false);

				const string code = "ZubOrg";
				Guid pk = TestUtil.PrepareOrgHeaderTableData(code);
				const string fullName = "Zayden Zubin Org";

				var input = new XElement(ns + "Organization",
					new XElement(ns + "OrgHeader",
						new XAttribute("Action", "Update"),
						new XElement(ns + "Code", code),
						new XElement(ns + "FullName", fullName)
					)
				);

				var request = new Request { EntitySets = new[] { input } };
				var response = updateHandler.Execute(request, new DummyResponseFactory());

				var status = response.Status;
				AssertEquals(NativeResponseStatus.Accepted, status);

				var informations = response.Informations;
				Assert(informations.Any(e => e.Contains("Information - OrgHeader - 0 inserts, 1 updates, 0 deletes")));

				var result = (string)connection.ExecuteScalar("select OH_FullName from dbo.OrgHeader where OH_PK = '" + pk + "'");
				AssertEquals(fullName.ToUpper(CultureInfo.CurrentCulture), result);
			}
			finally
			{
				Environment.Env.Registry.SetOrgAllowMixedCase(oldValue);
			}
		}

		public void TestUpdateData_Update_WithCandidateKey_AllowMixedCase()
		{
			bool oldValue = false;
			try
			{
				oldValue = Environment.Env.Registry.OrgAllowMixedCase;
				Environment.Env.Registry.SetOrgAllowMixedCase(true);

				const string code = "ZubOrg";
				Guid pk = TestUtil.PrepareOrgHeaderTableData(code);
				const string fullName = "Zayden Zubin Org";

				var input = new XElement(ns + "Organization",
					new XElement(ns + "OrgHeader",
						new XAttribute("Action", "Update"),
						new XElement(ns + "Code", code),
						new XElement(ns + "FullName", fullName)
					)
				);

				var request = new Request { EntitySets = new[] { input } };
				var response = updateHandler.Execute(request, new DummyResponseFactory());

				var status = response.Status;
				AssertEquals(NativeResponseStatus.Accepted, status);

				var informations = response.Informations;
				Assert(informations.Any(e => e.Contains("Information - OrgHeader - 0 inserts, 1 updates, 0 deletes")));

				var result = (string)connection.ExecuteScalar("select OH_FullName from dbo.OrgHeader where OH_PK = '" + pk + "'");
				AssertEquals(fullName, result);
			}
			finally
			{
				Environment.Env.Registry.SetOrgAllowMixedCase(oldValue);
			}
		}

		public void TestUpdateData_WithBelongsToEntity_Code()
		{
			AssertNotEquals("GRM", connection.ExecuteScalar("select OC_Language from dbo.OrgContact where OC_PK = '72613114-24ea-41e3-bc95-d70e614e676f'"));

			var input = new XElement("Organization",
				new XElement(ns + "OrgHeader",
					new XAttribute("Action", "Update"),
					new XElement(ns + "PK", new Guid("24181eea-d3e5-4afe-8892-8684ab555879")),
					new XElement(ns + "Code", "4BELEVORD"),
					new XElement(ns + "FullName", "Zayden Zubin Org"),
					new XElement(ns + "ClosestPort",
						new XElement(ns + "Code", "AUMEL")
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			//@"<?xml version='1.0' encoding='utf-16'?>";
			//@"<UpdateDataResponse EntitySet='Organization'>";
			//@"  <Status>Accepted</Status>";
			//@"  <Notifications>";
			//@"    <Information>";
			//@"      <Item>Information - OrgHeader - 0 inserts, 1 updates, 0 deletes.</Item>";
			//@"    </Information>";
			//@"  </Notifications>";
			//@"</UpdateDataResponse>";
			//@"";

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, status);

			var informationElements = response.Informations;
			Assert(informationElements.Any(e => e.Contains("Information - OrgHeader - 0 inserts, 1 updates, 0 deletes")));

			AssertEquals("AUMEL", connection.ExecuteScalar("select OH_RL_NKClosestPort from dbo.OrgHeader where OH_PK = '24181eea-d3e5-4afe-8892-8684ab555879'"));
		}

		public void TestUpdateData_WithBelongsToEntity_PK()
		{
			AssertNotEquals("GRM", connection.ExecuteScalar("select OC_Language from dbo.OrgContact where OC_PK = '72613114-24ea-41e3-bc95-d70e614e676f'"));

			var input = new XElement("Organization",
				new XElement(ns + "OrgHeader",
					new XAttribute("Action", "Update"),
					new XElement(ns + "PK", new Guid("24181eea-d3e5-4afe-8892-8684ab555879")),
					new XElement(ns + "Code", "4BELEVORD"),
					new XElement(ns + "FullName", "Zayden Zubin Org"),
					new XElement(ns + "ClosestPort",
						new XElement(ns + "PK", new Guid("2E35E0D5-5C31-4AD8-BB07-235579F2FBCC"))
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Information - OrgHeader - 0 inserts, 1 updates, 0 deletes")));

			AssertEquals("USNLD", connection.ExecuteScalar("select OH_RL_NKClosestPort from dbo.OrgHeader where OH_PK = '24181eea-d3e5-4afe-8892-8684ab555879'"));
		}

		public void TestUpdate_NonExistentRecordShouldNotDisplayInvestigationMessage()
		{
			var input = new XElement("UNLOCO",
				new XElement(ns + "RefUNLOCO",
					new XAttribute("Action", "Update"),
					new XElement(ns + "Code", "XXZZ_"),
					new XElement(ns + "PortName", "Sydney (My Town)")
					)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, status);

			var informations = response.Informations;
			Assert(informations.Any(e => !e.Contains("This error has been submitted to WTG for further investigation.")));
			Assert(informations.Any(e => e.Contains("Error - There is no UNLOCO with the following values: [Code:XXZZ_][PortName:Sydney (My Town)].")));
		}

		public void TestUpdateData_Delete()
		{
			var newOrgPk = TestUtil.PrepareOrgHeaderTableData();
			var newContactPk = TestUtil.PrepareOrgContactTableData(newOrgPk);

			var input = new XElement(ns + "Organization",
				new XElement(ns + "OrgHeader",
					new XElement(ns + "PK", newOrgPk),
					new XElement(ns + "OrgContactCollection",
						new XElement(ns + "OrgContact",
							new XAttribute("Action", "Delete"),
							new XElement(ns + "PK", newContactPk)
						)
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Information - OrgContact - 0 inserts, 0 updates, 1 deletes")));

			AssertEquals(0, (int)connection.ExecuteScalar("select count(*) from dbo.OrgContact where OC_PK = '" + newContactPk + "'"));
		}

		public void TestUpdateData_DeleteParentAndChild()
		{
			var newOrgPk = TestUtil.PrepareOrgHeaderTableData();
			var newContactPk = TestUtil.PrepareOrgContactTableData(newOrgPk);

			var input = new XElement(ns + "Organization",
				new XElement(ns + "OrgHeader",
					new XAttribute("Action", "Delete"),
					new XElement(ns + "PK", newOrgPk),
					new XElement(ns + "OrgContactCollection",
						new XElement(ns + "OrgContact",
							new XAttribute("Action", "Delete"),
							new XElement(ns + "PK", newContactPk),
							new XElement(ns + "ContactName", "Rakhsh")
							)
						)
					)
				);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, status);

			var informations = response.Informations;
			Assert(informations.Any(e => e.Contains("Information - OrgHeader - 0 inserts, 0 updates, 1 deletes")));
			Assert(informations.Any(e => e.Contains("Information - OrgContact - 0 inserts, 0 updates, 1 deletes")));

			AssertEquals(0, (int)connection.ExecuteScalar("select count(*) from dbo.OrgContact where OC_PK = '" + newContactPk + "'"));
			AssertEquals(0, (int)connection.ExecuteScalar("select count(*) from dbo.OrgHeader where OH_PK = '" + newOrgPk + "'"));
		}

		public void TestUpdateData_DeleteParentNotChild()
		{
			var newOrgPk = TestUtil.PrepareOrgHeaderTableData();
			var newContactPk = TestUtil.PrepareOrgContactTableData(newOrgPk);
			var input = new XElement(ns + "Organization",
				new XElement(ns + "OrgHeader",
					new XAttribute("Action", "Delete"),
					new XElement(ns + "PK", newOrgPk),
					new XElement(ns + "OrgContactCollection",
						new XElement(ns + "OrgContact",
							new XElement(ns + "PK", newContactPk),
							new XElement(ns + "ContactName", "Rakhsh")
						)
					)
				)
			);
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var status = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, status);

			var informations = response.Informations;
			Assert(informations.Any(i => i.Contains("Error - The Organization cannot be deleted, because there is at least one Contact referencing it")));

			AssertNotNull(new BusinessObjectFactory().Load<OrgContact>(newContactPk));
		}

		public void TestUpdateData_UserVisibleException_NoStackTrace()
		{
			var definitionFinder = new Mock<IDefinitionFinder>();
			var exception = new NativeXMLUserVisibleException("Message");
			definitionFinder.Setup(m => m.FindByEntitySetName(It.IsAny<string>())).Throws(exception);

			updateHandler.Parser.DefinitionFinder = definitionFinder.Object;

			var input = XElement.Parse("<Organization></Organization>");
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var nativeResponseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, nativeResponseStatus);

			var informations = response.Informations;
			Assert(informations.Any(i => i.Contains("Message")));
			Assert(informations.All(i => !i.Contains(exception.StackTrace)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			definitionFinder.VerifyAll();
		}

		public void TestUpdateData_OtherException_ShowStackTrack()
		{
			var definitionFinder = new Mock<IDefinitionFinder>();
			var exception = new Exception("Message");
			definitionFinder.Setup(m => m.FindByEntitySetName(It.IsAny<string>())).Throws(exception);

			updateHandler.Parser.DefinitionFinder = definitionFinder.Object;

			var input = XElement.Parse("<Organization></Organization>");
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var nativeResponseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, nativeResponseStatus);

			var informations = response.Informations;
			Assert(informations.Any(i => i.Contains("Message")));
			Assert(informations.Any(i => i.Contains(exception.StackTrace)));
			AssertContains("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
			AssertContains("Message", ErrorReporter.LastExceptionReported.Message);
			definitionFinder.VerifyAll();
			ErrorReporter.Clear();
		}

		public void TestUpdateData_UnhandledException()
		{
			var input = new XElement(ns + "UnhandledException", "Failed because of UnhandledException.");
			var request = new Request { EntitySets = new[] { input } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var nativeResponseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, nativeResponseStatus);

			var informations = response.Informations;
			Assert(informations.Any(i => i.Contains("Failed because of UnhandledException.")));
			Assert(informations.Any(i => i.Contains("at Enterprise.DataTransfer.Native.Business.Update.UpdateHandler.Execute(Request request) in ")));
			AssertContains("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		const string InvalidDateStartMessage = @"
<Product version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
    <PartNum>PRODUCTA</PartNum>
    <StockKeepingUnit>UNT</StockKeepingUnit>
    <Desc>New Description</Desc>
    <CusClassPartPivotCollection>
      <CusClassPartPivot Action=""MERGE"">
        <TariffNum>2501000000</TariffNum>
        <ChildType>HTI</ChildType>
        <AddInfoCollection>
          <AddInfo>
            <Key>ETRateCode</Key>
            <Value>NO</Value>
          </AddInfo>
          <AddInfo>
            <Key>ProvinceOfOrigin</Key>
            <Value />
          </AddInfo>
          <AddInfo>
            <Key>RN_NKOrigin</Key>
            <Value>US</Value>
          </AddInfo>
        </AddInfoCollection>
        <Country>
          <Code>CA</Code>
        </Country>
        <OrgPartRelation Action=""MERGE"">
          <Relationship>OWN</Relationship>
          <OrgHeader>
            <Code>MYSPIZMYC</Code>
          </OrgHeader>
        </OrgPartRelation>
        <CountryOfOrigin TableName=""RefCountry"">
          <Code>US</Code>
        </CountryOfOrigin>
        <DateStart><sx:value-of xmlns:sx=""http://www.servingxml.com/core"" select=""concat(substring(expirationDate, 1, 4), '-', substring(expirationDate, 5, 2), '-', substring(expirationDate, 7, 2))"" /></DateStart>
        <DateEnd>2015-03-13T00:00:00</DateEnd>
      </CusClassPartPivot>
    </CusClassPartPivotCollection>
    <OrgPartRelationCollection>
      <OrgPartRelation Action=""MERGE"">
        <Relationship>OWN</Relationship>
        <OrgHeader>
          <Code>MYSPIZMYC</Code>
        </OrgHeader>
      </OrgPartRelation>
      <OrgPartRelation Action=""MERGE"">
        <Relationship>SUP</Relationship>
        <OrgHeader>
          <Code>ORTDEMMEX</Code>
        </OrgHeader>
      </OrgPartRelation>
    </OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>
";

		public void TestUpdateData_HandleException_ZTypeValueException_DateTime()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "MYSPIZMYC";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ORTDEMMEX";

			CreateProduct(owner, supplier, "PRODUCTA");
			Factory.Save();

			var message = XElement.Parse(InvalidDateStartMessage);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var responseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, responseStatus);

			var informations = response.Informations;
			AssertEquals(1, informations.Length);
			AssertEquals(@"Error - Validation errors found in Native XML:
OrgSupplierPart.CusClassPartPivot.DateStart validation failed: '<value-of xmlns:sx=""http://www.servingxml.com/core"" select=""concat(substring(expirationDate, 1, 4), '-', substring(expirationDate, 5, 2), '-', substring(expirationDate, 7, 2))"" />' could not be converted to type smalldatetime
", informations.First());
			AssertEquals("", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestUpdateData_HandleException_ZTypeValueException_Decimal()
		{
			string msg = @"
<Product xmlns:knc=""CargoWise"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
	<PartNum>PART-ABC</PartNum>
	<Desc>Part description</Desc>
	<StockKeepingUnit>UNT</StockKeepingUnit>
	<Weight>Invalid weight</Weight> <!-- ***** Weight is not a decimal and should throw an Exception ***** -->
	<WeightUQ>KG</WeightUQ>
	<NetWeight>.0000</NetWeight>
	<CusClassPartPivotCollection>
	  <CusClassPartPivot Action=""MERGE"">
		<TariffNum>9403509041</TariffNum>
		<ChildType>HTI</ChildType>
		<Country>
			<Code>US</Code>
		</Country>
		<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
		  <ComponentCusClassPartPivot Action=""MERGE"">
			<TariffNum>9403509041</TariffNum>
			<ChildListOrder>1</ChildListOrder>
			<DateStart>2019-08-05T00:00:00</DateStart>
			<DateEnd>2019-08-05T00:00:00</DateEnd>
			<Country TableName=""RefCountry"">
			  <Code>US</Code>
			</Country>
		  </ComponentCusClassPartPivot>
		</ComponentCusClassPartPivotCollection>
	  </CusClassPartPivot>
	</CusClassPartPivotCollection>
	<OrgPartRelationCollection>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>OWN</Relationship>
		<OrgHeader>
		  <Code>OWN001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>SUP</Relationship>
		<OrgHeader>
		  <Code>SUP001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	</OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN001";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP001";

			CreateProduct(owner, supplier, "PRODUCTA");
			Factory.Save();

			var message = XElement.Parse(msg);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var responseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, responseStatus);

			var informations = response.Informations;
#if NET
			var matchError = "Error - [OrgSupplierPart.Weight] : The input string 'Invalid weight' was not in a correct format.Couldn't store <Invalid weight> in Weight Column.  Expected type is Decimal.";
#else
			var matchError = "Error - [OrgSupplierPart.Weight] : Input string was not in a correct format.Couldn't store <Invalid weight> in Weight Column.  Expected type is Decimal.";
#endif
			AssertEquals(1, informations.Count(i => i == matchError));
			AssertEquals("", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestUpdateData_DuplicatePK()
		{
			var sourceXML = @"
<Organization version=""2.0"" xmlns =""http://www.cargowise.com/Schemas/Native/2011/11"">
  <OrgHeader Action=""MERGE"">
    <PK>f13c9c6a-0da9-4f4e-ae62-55d7844227f6</PK>
    <Code>SOMEORG1</Code>
    <Language>ENG</Language>
    <IsActive>true</IsActive>
    <FullName>SOME ORG NAME</FullName>
    <OrgAddressCollection>
      <OrgAddress Action=""MERGE"">
        <PK>bdfc0aed-b0b8-4e89-89b0-9558e7e8a69e</PK>
        <IsActive>true</IsActive>
        <Code>Address1</Code>
        <Language>ENG</Language>
        <Address1>1 Street</Address1>
        <City>City</City>
        <PostCode>1000</PostCode>
        <OrgAddressCapabilityCollection>
          <OrgAddressCapability Action=""MERGE"">
            <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87</PK>
            <AddressType>PAD</AddressType>
            <IsMainAddress>true</IsMainAddress>
          </OrgAddressCapability>
        </OrgAddressCapabilityCollection>
      </OrgAddress>
      <OrgAddress Action=""MERGE"">
        <PK>57c6760b-1339-4e4a-955d-99703d6e61e5</PK>
        <IsActive>true</IsActive>
        <Code>Address2</Code>
        <Language>ENG</Language>
        <Address1>2 Street</Address1>
        <City>City</City>
        <PostCode>1000</PostCode>
        <OrgAddressCapabilityCollection>
          <OrgAddressCapability Action=""MERGE"">
            <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87</PK>
            <AddressType>PAD</AddressType>
            <IsMainAddress>true</IsMainAddress>
          </OrgAddressCapability>
        </OrgAddressCapabilityCollection>
      </OrgAddress>
    </OrgAddressCollection>
    <ClosestPort TableName=""RefUNLOCO"">
      <Code>BGSOF</Code>
    </ClosestPort>
  </OrgHeader>
</Organization>";

			var message = XElement.Parse(sourceXML);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());
			var responseMessageAsText = string.Join("\r\n", response.Informations);

			const string expectedErrorStartsWith = "Error - Database parent does not match entity parent.\r\nDuplicate PK 'dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87' found on multiple entities in the supplied XML.";

			AssertStartsWith("Duplicate PK", expectedErrorStartsWith, responseMessageAsText);
		}

		public void TestUpdateData_ExistingPK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SOMEORG";
			org.OH_FullName = "SOME ORG NAME";
			var address1 = org.Addresses.AddNew();
			address1.OA_Code = "Some address";
			address1.OA_Address1 = "1 Street";
			address1.OA_City = "City";
			address1.OA_PostCode = "1000";
			Factory.Save();

			var sourceXML = $@"
<Organization version=""2.0"" xmlns =""http://www.cargowise.com/Schemas/Native/2011/11"">
  <OrgHeader Action=""MERGE"">
    <PK>f13c9c6a-0da9-4f4e-ae62-55d7844227f6</PK>
    <Code>OTHERORG</Code>
    <Language>ENG</Language>
    <IsActive>true</IsActive>
    <FullName>OTHER ORG NAME</FullName>
    <OrgAddressCollection>
      <OrgAddress Action=""MERGE"">
        <PK>{address1.PK}</PK>
        <IsActive>true</IsActive>
        <Code>Other address</Code>
        <Language>ENG</Language>
        <Address1>2 Road</Address1>
        <City>Town</City>
        <PostCode>2000</PostCode>
        <OrgAddressCapabilityCollection>
          <OrgAddressCapability Action=""MERGE"">
            <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87</PK>
            <AddressType>PAD</AddressType>
            <IsMainAddress>true</IsMainAddress>
          </OrgAddressCapability>
        </OrgAddressCapabilityCollection>
      </OrgAddress>
    </OrgAddressCollection>
    <ClosestPort TableName=""RefUNLOCO"">
      <Code>BGSOF</Code>
    </ClosestPort>
  </OrgHeader>
</Organization>";

			var message = XElement.Parse(sourceXML);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());
			var responseMessageAsText = string.Join("\r\n", response.Informations);

			var expectedErrorStartsWith = "Error - Database parent does not match entity parent.\r\nPK '" + address1.PK + "' already exists on a row in the database, and is linked to a different parent row.";

			AssertStartsWith("Duplicate PK", expectedErrorStartsWith, responseMessageAsText);
		}

		public void TestOutOfRangeDateEnd()
		{
			const string msg = @"
<Product xmlns:knc=""CargoWise"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
	<PartNum>PART-ABC</PartNum>
	<Desc>Part description</Desc>
	<StockKeepingUnit>UNT</StockKeepingUnit>
	<CusClassPartPivotCollection>
	  <CusClassPartPivot Action=""MERGE"">
		<TariffNum>9403509041</TariffNum>
		<ChildType>HTI</ChildType>
		<Country>
			<Code>US</Code>
		</Country>
		<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
		  <ComponentCusClassPartPivot Action=""MERGE"">
			<TariffNum>9403509041</TariffNum>
			<ChildListOrder>1</ChildListOrder>
			<DateStart>2019-08-05T00:00:00</DateStart>
			<DateEnd>0002-11-30T00:00:00</DateEnd> <!-- ***** DateEnd is outside of the valid range ***** -->
			<Country TableName=""RefCountry"">
			  <Code>US</Code>
			</Country>
		  </ComponentCusClassPartPivot>
		</ComponentCusClassPartPivotCollection>
	  </CusClassPartPivot>
	</CusClassPartPivotCollection>
	<OrgPartRelationCollection>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>OWN</Relationship>
		<OrgHeader>
		  <Code>OWN001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>SUP</Relationship>
		<OrgHeader>
		  <Code>SUP001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	</OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>
";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN001";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP001";

			CreateProduct(owner, supplier, "PART-ABC");
			Factory.Save();

			var message = XElement.Parse(msg);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var responseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, responseStatus);

			var informations = response.Informations;
			AssertEquals(1, informations.Length);
			AssertEquals(@"Error - Validation errors found in Native XML:
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.DateEnd validation failed: '0002-11-30T00:00:00' is not within the range for smalldatetime (1900-01-01T00:00:00 - 2079-06-06T23:59:29)
", informations.First());
			// No error reports should have been raised
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestOutOfRangeDateStartAndEnd()
		{
			const string msg = @"
<Product xmlns:knc=""CargoWise"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
	<PartNum>PART-ABC</PartNum>
	<Desc>Part description</Desc>
	<StockKeepingUnit>UNT</StockKeepingUnit>
	<CusClassPartPivotCollection>
	  <CusClassPartPivot Action=""MERGE"">
		<TariffNum>9403509041</TariffNum>
		<ChildType>HTI</ChildType>
		<Country>
			<Code>US</Code>
		</Country>
		<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
		  <ComponentCusClassPartPivot Action=""MERGE"">
			<TariffNum>9403509041</TariffNum>
			<ChildListOrder>1</ChildListOrder>
			<DateStart>0003-12-10T10:00:00</DateStart> <!-- ***** DateStart is outside of the valid range ***** -->
			<DateEnd>0002-11-30T00:00:00</DateEnd> <!-- ***** DateEnd is outside of the valid range ***** -->
			<Country TableName=""RefCountry"">
			  <Code>US</Code>
			</Country>
		  </ComponentCusClassPartPivot>
		</ComponentCusClassPartPivotCollection>
	  </CusClassPartPivot>
	</CusClassPartPivotCollection>
	<OrgPartRelationCollection>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>OWN</Relationship>
		<OrgHeader>
		  <Code>OWN001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>SUP</Relationship>
		<OrgHeader>
		  <Code>SUP001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	</OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>
";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN001";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP001";

			CreateProduct(owner, supplier, "PART-ABC");
			Factory.Save();

			var message = XElement.Parse(msg);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var responseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, responseStatus);

			var informations = response.Informations;
			AssertEquals(1, informations.Length);
			AssertEquals(@"Error - Validation errors found in Native XML:
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.DateStart validation failed: '0003-12-10T10:00:00' is not within the range for smalldatetime (1900-01-01T00:00:00 - 2079-06-06T23:59:29)
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.DateEnd validation failed: '0002-11-30T00:00:00' is not within the range for smalldatetime (1900-01-01T00:00:00 - 2079-06-06T23:59:29)
", informations.First());
			// No error reports should have been raised
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestInvalidDateStart()
		{
			const string msg = @"
<Product xmlns:knc=""CargoWise"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
	<PartNum>PART-ABC</PartNum>
	<Desc>Part description</Desc>
	<StockKeepingUnit>UNT</StockKeepingUnit>
	<CusClassPartPivotCollection>
	  <CusClassPartPivot Action=""MERGE"">
		<TariffNum>9403509041</TariffNum>
		<ChildType>HTI</ChildType>
		<Country>
			<Code>US</Code>
		</Country>
		<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
		  <ComponentCusClassPartPivot Action=""MERGE"">
			<TariffNum>9403509041</TariffNum>
			<ChildListOrder>1</ChildListOrder>
			<DateStart>Non-date value</DateStart> <!-- ***** DateStart is not a valid smalldatetime ***** -->
			<DateEnd>2020-08-15T09:00:00</DateEnd>
			<Country TableName=""RefCountry"">
			  <Code>US</Code>
			</Country>
		  </ComponentCusClassPartPivot>
		</ComponentCusClassPartPivotCollection>
	  </CusClassPartPivot>
	</CusClassPartPivotCollection>
	<OrgPartRelationCollection>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>OWN</Relationship>
		<OrgHeader>
		  <Code>OWN001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>SUP</Relationship>
		<OrgHeader>
		  <Code>SUP001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	</OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>
";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN001";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP001";

			CreateProduct(owner, supplier, "PART-ABC");
			Factory.Save();

			var message = XElement.Parse(msg);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var responseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Rejected, responseStatus);

			var informations = response.Informations;
			AssertEquals(1, informations.Length);
			AssertEquals(@"Error - Validation errors found in Native XML:
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.DateStart validation failed: 'Non-date value' could not be converted to type smalldatetime
", informations.First());
			// No error reports should have been raised
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestDateEndIsNullable()
		{
			const string msg = @"
<Product xmlns:knc=""CargoWise"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <OrgSupplierPart Action=""MERGE"">
	<PartNum>PART-ABC</PartNum>
	<Desc>Part description</Desc>
	<StockKeepingUnit>UNT</StockKeepingUnit>
	<CusClassPartPivotCollection>
	  <CusClassPartPivot Action=""MERGE"">
		<TariffNum>9403509041</TariffNum>
		<ChildType>HTI</ChildType>
		<Country>
			<Code>US</Code>
		</Country>
		<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
		  <ComponentCusClassPartPivot Action=""MERGE"">
			<TariffNum>9403509041</TariffNum>
			<ChildListOrder>1</ChildListOrder>
			<DateStart>2019-08-05T00:00:00</DateStart>
			<DateEnd></DateEnd> <!-- ***** DateEnd empty - should be no errors ***** -->
			<Country TableName=""RefCountry"">
			  <Code>US</Code>
			</Country>
		  </ComponentCusClassPartPivot>
		</ComponentCusClassPartPivotCollection>
	  </CusClassPartPivot>
	</CusClassPartPivotCollection>
	<OrgPartRelationCollection>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>OWN</Relationship>
		<OrgHeader>
		  <Code>OWN001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	  <OrgPartRelation Action=""MERGE"">
		<Relationship>SUP</Relationship>
		<OrgHeader>
		  <Code>SUP001</Code>
		</OrgHeader>
	  </OrgPartRelation>
	</OrgPartRelationCollection>
  </OrgSupplierPart>
</Product>
";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN001";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP001";

			CreateProduct(owner, supplier, "PART-ABC");
			Factory.Save();

			var message = XElement.Parse(msg);
			var request = new Request { EntitySets = new[] { message } };
			var response = updateHandler.Execute(request, new DummyResponseFactory());

			var responseStatus = response.Status;
			AssertEquals(NativeResponseStatus.Accepted, responseStatus);

			var informations = response.Informations;
			AssertEquals(false, informations.Any(i => i.Contains("Error - Validation errors found in Native XML:")));
			// No error reports should have been raised
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			updateHandler = new UpdateHandler(new FactoryProvider());
			updateHandler.Parser.DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder();

			connection = TestUtil.Connection;
			ns = ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native;
		}

		UpdateHandler updateHandler;
		DbConnection connection;
		XNamespace ns;

		OrgSupplierPart CreateProduct(OrgHeader owner, OrgHeader supplier, string productCode)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = productCode;
			part.OP_Desc = "Dummy Product";

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = owner.PK == supplier.PK ? OrgPartRelation.RelationshipTypes.Both : OrgPartRelation.RelationshipTypes.Owner;
			ownerRelation.OU_OH = owner.PK;

			var supplierRelation = part.RelatedOrganisations.AddNew();
			supplierRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			supplierRelation.OU_OH = supplier.PK;

			var barcodes = new string[1] { "1313" };
			foreach (var barcode in barcodes)
			{
				var bc = part.PartBarcodes.AddNew();
				bc.PH_Barcode = barcode;
				bc.PH_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
			}

			return part;
		}
		#endregion

		(BusinessObjectFactory, OrgHeader, StmNote, OrgAddress, OrgAddressCapability) CreateObjectsForTest()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var note = Factory.NewWithValidTestData<StmNote>();
			note.ST_Table = "OrgHeader";
			var orgAddress = header.MainAddress;
			var orgAddressCapability = orgAddress.CapabilitiesCollection.AddNew();

			Factory.Save();
			RunXmlForTestObjects(header.PK, note.PK, orgAddress.PK, orgAddressCapability.PK);

			var reloadFactory = new BusinessObjectFactory();
			header = reloadFactory.Load<OrgHeader>(header.PK);
			note = reloadFactory.Load<StmNote>(note.PK);
			orgAddress = reloadFactory.Load<OrgAddress>(orgAddress.PK);
			orgAddressCapability = reloadFactory.Load<OrgAddressCapability>(orgAddressCapability.PK);
			return (reloadFactory, header, note, orgAddress, orgAddressCapability);
		}

		const string someFunNoteData = "<NoteData>e1xydGYxXGFuc2lcZGVmZjB7XGZvbnR0Ymx7XGYwXGZuaWxcZmNoYXJzZXQwIE1pY3Jvc29mdCBTYW5zIFNlcmlmO319DQpcdmlld2tpbmQ0XHVjMVxwYXJkXGxhbmczMDgxXGYwXGZzMjAgU3RyaW5nIE9mIERhdGEgc29tZWhvdyByZWxhdGluZyB0byB0aGlzIHRlc3RccGFyDQp9DQo=</NoteData>";

		string RunXmlForTestObjects(ZGuid headerPK, ZGuid notePK, ZGuid orgAddressPK, ZGuid orgAddressCapabilityPK,
			string noteData = someFunNoteData,
			string address1 = "<Address1>2 Road</Address1>",
			string isMainAddress = "<IsMainAddress>true</IsMainAddress>",
			string pickupToTimeOnly = "<PickupToTimeOnly>31/12/2010 12:00:00 AM</PickupToTimeOnly>",
			string groupNumber = "<GroupNumber>0</GroupNumber>")
		{
			var sourceXML = $@"
<Organization version=""2.0"" xmlns =""http://www.cargowise.com/Schemas/Native/2011/11"">
<OrgHeader Action=""MERGE"">
<PK>{headerPK}</PK>
<Code>OTHERORG</Code>
<Language>ENG</Language>
<IsActive>true</IsActive>
<FullName>OTHER ORG NAME</FullName>
<OrgAddressCollection>
    <OrgAddress Action=""MERGE"">
    <PK>{orgAddressPK}</PK>
    <IsActive>true</IsActive>
    <Code>Other address</Code>
    <Language>ENG</Language>
    {address1}
    <City>Town</City>
    <PostCode>2000</PostCode>
	{pickupToTimeOnly}
	{groupNumber}
    <OrgAddressCapabilityCollection>
        <OrgAddressCapability Action=""MERGE"">
        <PK>{orgAddressCapabilityPK}</PK>
        <AddressType>PAD</AddressType>
		{isMainAddress}
        </OrgAddressCapability>
    </OrgAddressCapabilityCollection>
    </OrgAddress>
</OrgAddressCollection>
    <StmNoteCollection>
        <StmNote Action=""MERGE"">
        <PK>{notePK}</PK>
        <Description>Customs Note Description</Description>
		{noteData}
        <NoteText></NoteText>
        <NoteType>INT</NoteType>
        <NoteContext>CEL</NoteContext>
        <IsCustomDescription>true</IsCustomDescription>
        <ForceRead>true</ForceRead>
        </StmNote>
    </StmNoteCollection>
<ClosestPort TableName=""RefUNLOCO"">
    <Code>BGSOF</Code>
</ClosestPort>
</OrgHeader>
</Organization>";

			var message = XElement.Parse(sourceXML);
			var request = new Request { EntitySets = new[] { message } };
			var response = new UpdateHandler(new FactoryProvider()).Execute(request, new DummyResponseFactory());

			return string.Join(System.Environment.NewLine, response.Informations);
		}
	}

	[UseSnapshotProtection]
	class UpdateHandlerTest_WithSnapshotProtection : TestCase
	{
		public void TestRetryOnConcurrency()
		{
			var connection = Db.Connection;
			var countOrgBefore = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgHeader");
			var sourceXML = $@"
<Organization version=""2.0"" xmlns =""http://www.cargowise.com/Schemas/Native/2011/11"">
  <OrgHeader Action=""MERGE"">
    <PK>f13c9c6a-0da9-4f4e-ae62-55d7844227f6</PK>
    <Code>OTHERORG</Code>
    <Language>ENG</Language>
    <IsActive>true</IsActive>
    <FullName>OTHER ORG NAME</FullName>
    <OrgAddressCollection>
      <OrgAddress Action=""MERGE"">
        <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e66</PK>
        <IsActive>true</IsActive>
        <Code>Other address</Code>
        <Language>ENG</Language>
        <Address1>2 Road</Address1>
        <City>Town</City>
        <PostCode>2000</PostCode>
        <OrgAddressCapabilityCollection>
          <OrgAddressCapability Action=""MERGE"">
            <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87</PK>
            <AddressType>PAD</AddressType>
            <IsMainAddress>true</IsMainAddress>
          </OrgAddressCapability>
        </OrgAddressCapabilityCollection>
      </OrgAddress>
    </OrgAddressCollection>
    <ClosestPort TableName=""RefUNLOCO"">
      <Code>BGSOF</Code>
    </ClosestPort>
  </OrgHeader>
</Organization>";

			var message = XElement.Parse(sourceXML);
			var request = new Request { EntitySets = new[] { message } };
			var response = new UpdateHandler(new FailingFactoryProvider(false)).Execute(request, new DummyResponseFactory());

			var countOrgMid = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgHeader");
			AssertEquals("Should add a record to OrgHeader table after insert dbo.OrgHeader successfully", countOrgBefore + 1, countOrgMid);

			var informations = string.Join(System.Environment.NewLine, response.Informations);
			AssertContains("Information - OrgHeader - 1 inserts, 0 updates, 0 deletes", informations);
			AssertEquals("Warning, because there was a concurrency error", NativeResponseStatus.Warning, response.Status);
		}

		public void TestReusingHandlerDoesntReuseInternals()
		{
			var connection = Db.Connection;
			var countOrgBefore = (int)connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgHeader");
			var sourceXML = $@"
<Organization version=""2.0"" xmlns =""http://www.cargowise.com/Schemas/Native/2011/11"">
  <OrgHeader Action=""MERGE"">
    <PK>f13c9c6a-0da9-4f4e-ae62-55d7844227f6</PK>
    <Code>OTHERORG</Code>
    <Language>ENG</Language>
    <IsActive>true</IsActive>
    <FullName>OTHER ORG NAME</FullName>
    <OrgAddressCollection>
      <OrgAddress Action=""MERGE"">
        <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e66</PK>
        <IsActive>true</IsActive>
        <Code>Other address</Code>
        <Language>ENG</Language>
        <Address1>2 Road</Address1>
        <City>Town</City>
        <PostCode>2000</PostCode>
        <OrgAddressCapabilityCollection>
          <OrgAddressCapability Action=""MERGE"">
            <PK>dbe8d8d6-2653-44e2-9ad4-6e794b3e5e87</PK>
            <AddressType>PAD</AddressType>
            <IsMainAddress>true</IsMainAddress>
          </OrgAddressCapability>
        </OrgAddressCapabilityCollection>
      </OrgAddress>
    </OrgAddressCollection>
    <ClosestPort TableName=""RefUNLOCO"">
      <Code>BGSOF</Code>
    </ClosestPort>
  </OrgHeader>
</Organization>";

			var message = XElement.Parse(sourceXML);
			var request = new Request { EntitySets = new[] { message } };
			var handler = new UpdateHandler(new FailingFactoryProvider(true));
			AssertEquals("Should be the same number of logs, because both should fail",
				handler.Execute(request, new DummyResponseFactory()).Informations.Length,
				handler.Execute(request, new DummyResponseFactory()).Informations.Length);
			ErrorReporter.Clear();
		}

		class FailingFactoryProvider : INativeFactoryProvider
		{
			public FailingFactoryProvider(bool alwaysFail)
			{
				this.alwaysFail = alwaysFail;
			}
			int count;
			readonly bool alwaysFail;
			public BusinessObjectFactory GetNewFactory(DbConnection connection)
			{
				var factory = new BusinessObjectFactory(connection);
				if (count == 0 || alwaysFail)
				{
					count++;
					var row = ((INeedRow)factory.LoadTop1<GlbStaff>(new ZQuery())).Row;
					factory.Saving += (s) => throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), row, connection), factory);
				}
				return factory;
			}
		}
	}

	class DummyResponseFactory : IResponseFactory
	{
		public Response GetNewResponse()
		{
			return new Response_Universal();
		}

		public IXmlSerializer GetResponseSerializer()
		{
			return new ObjectXmlSerializer<Response_Universal>();
		}

		public XNamespace NameSpace
		{
			get { return ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native; }
		}
	}
}
