using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Business.Testing
{
	public abstract class StmDocumentDeliveryManagerTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			InitialEnv();

			var manager = new StmDocumentDeliveryManagerForTest();
			var jobs = new List<StmDocumentDelivery>();
			for (int i = 0; i < 10; i++)
			{
				jobs.Add(CreateNewDocumentDelivery());
			}
			Factory.Save();

			CombineAssertions("TestExecute - ToProcess", () =>
			{
				AssertJobsCountToProcess(10);
				AssertJobsCountProcessed(0);
			});

			manager.Execute(jobs.ToArray());
			CombineAssertions("TestExecute - Processed", () =>
			{
				AssertJobsCountToProcess(0);
				AssertJobsCountProcessed(10);
				AssertJobsCountRetried(0);
			});
		}

		public void TestExecute_WithRetry()
		{
			InitialEnv();

			var manager = new StmDocumentDeliveryManagerForTest();
			var jobs = new List<StmDocumentDelivery>();
			for (int i = 0; i < 5; i++)
			{
				jobs.Add(CreateNewDocumentDelivery());
			}

			for (int i = 0; i < 5; i++)
			{
				var documentDelivery = CreateNewDocumentDelivery();
				documentDelivery.SDL_RetryAttempts = 1;
				jobs.Add(documentDelivery);
			}
			Factory.Save();

			CombineAssertions("TestExecute_WithRetry - ToProcess", () =>
			{
				AssertJobsCountToProcess(10);
				AssertJobsCountProcessed(0);
			});

			manager.Execute(jobs.ToArray(), new LoggerForTest());
			AssertContains("Has ErrorReporter Key", "BDD(Background Document Delivery) Run Failure when Execute.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			CombineAssertions("TestExecute_WithRetry - Processed", () =>
			{
				AssertJobsCountToProcess(0);
				AssertJobsCountProcessed(5);
				AssertJobsCountRetried(5);
			});
		}

		public void TestPurgeOld()
		{
			InitialEnv();

			var manager = new StmDocumentDeliveryManagerForTest();
			var jobs = new List<StmDocumentDelivery>();
			for (int i = 0; i < 5; i++)
			{
				jobs.Add(CreateNewDocumentDelivery());
			}

			for (int i = 0; i < 5; i++)
			{
				var documentDelivery = CreateNewDocumentDelivery();
				documentDelivery.SDL_IsProcessed = true;
				jobs.Add(documentDelivery);
			}
			Factory.Save();

			CombineAssertions("PurgeOld - ToProcess", () =>
			{
				AssertJobsCountToProcess(5);
				AssertJobsCountProcessed(5);
			});

			manager.PurgeOld(new LoggerForTest());
			CombineAssertions("PurgeOld - Processed", () =>
			{
				AssertJobsCountToProcess(5);
				AssertJobsCountProcessed(0);
			});
		}

		public StmDocumentDelivery CreateNewDocumentDelivery()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "DEFRA";

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.AddShipment(shipment);

			var forwardingDocumentSupporter = consol as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, Factory);
			var documentCommand = customization.Menus.AddNew();
			documentCommand.SU_MenuName = "Test Primary Doc" + ZGuid.NewZGuid();
			documentCommand.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_BusinessContext = "Shipment";
			documentCommand.SU_MenuIndex = 0;
			documentCommand.SU_FilterList = "\"<JS_TransportMode>\" == \"AIR\"";

			var documentDelivery = Factory.New<StmDocumentDelivery>();
			documentDelivery.SDL_IsProcessed = false;
			documentDelivery.SDL_SU = DocumentCommand.PK;
			documentDelivery.SDL_ParentId = Shipment.PK;
			documentDelivery.SDL_ParentControllerIdOrTableCode = "JobShipment";
			documentDelivery.SDL_RetryAttempts = 0;
			documentDelivery.SDL_Instructions = "{\"Language\":\"EN-GB\",\"NumberOfCopies\":\"1\",\"Recipients\":[{\"DeliveryMethod\":\"EML\",\"AttachmentType\":\"PDF\",\"DeliveryAddress\":\"b@b.com\"}],\"DocumentsToBeDelivered\":[{\"Copies\":\"1\",\"Identifier\":\"4O6phfxbZ3rBBhJAqkq2ag==\"},{\"Copies\":\"1\",\"Identifier\":\"g67AmY6a16GJnXIcH14rDw==\"},{\"Copies\":\"1\",\"Identifier\":\"XP0oHKUI2q70D+pH8l1oTA==\"}],\"EDocsToBeDelivered\":[]}";
			documentDelivery.SDL_GS = Staff.PK;
			documentDelivery.SDL_GB = Branch.PK;
			documentDelivery.SDL_GE = Department.PK;

			Factory.Save();
			return documentDelivery;
		}

		void InitialEnv()
		{
			Staff = Factory.NewWithValidTestData<GlbStaff>();
			Staff.GS_EmailAddress = "a@b.com";
			Staff.GS_Code = "ABC";
			Staff.GS_LoginName = "ABC";
			Staff.GS_FullName = "Damien Li";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Branch = company.Branches.AddNew();
			Branch.GB_Code = "GB1";
			Department = Factory.NewWithValidTestData<GlbDepartment>();
			Department.GE_Code = "GE1";

			Shipment = Factory.New<Forwarding.IForwardingShipment>();
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.AddShipment(Shipment);
			var forwardingDocumentSupporter = consol as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, Factory);
			DocumentCommand = customization.Menus.AddNew();
			Factory.Save();
		}

		void AssertJobsCountToProcess(int count)
		{
			var query = new ZQuery(StmDocumentDeliverySchema.SDL_IsProcessed, false);
			query.AddToFilter(StmDocumentDeliverySchema.SDL_RetryAttempts, SQLComparisonOperator.LessThan, (byte)3);
			AssertEquals(count, Factory.GetDatabaseCount(typeof(StmDocumentDelivery), query));
		}

		void AssertJobsCountRetried(int count)
		{
			var query = new ZQuery(StmDocumentDeliverySchema.SDL_IsProcessed, false);
			query.AddToFilter(StmDocumentDeliverySchema.SDL_RetryAttempts, SQLComparisonOperator.GreaterThanOrEqualTo, (byte)3);
			AssertEquals(count, Factory.GetDatabaseCount(typeof(StmDocumentDelivery), query));
		}

		void AssertJobsCountProcessed(int count)
		{
			AssertEquals(count, Factory.GetDatabaseCount(typeof(StmDocumentDelivery), new ZQuery(StmDocumentDeliverySchema.SDL_IsProcessed, true)));
		}

		GlbStaff Staff;
		GlbBranch Branch;
		GlbDepartment Department;
		DocumentCommand DocumentCommand;
		Forwarding.IForwardingShipment Shipment;

		public virtual void TestProcessDocumentDelivery()
		{
			Assert(true);
		}
	}
}
