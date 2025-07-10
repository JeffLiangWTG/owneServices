using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.ArchiveEligibility;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.ManifestBase;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ContainerPenalty = Enterprise.Freight.Business.ContainerPenalty;

namespace Enterprise.ArchiveManager.Test.ArchiveEligibility
{
	public class RelatedRecordHelperTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(AutoDummyBizo.Schema.TableName);
			TestCaseHelper.ClearTable(AutoDummyDependentBizo.Schema.TableName);
			TestCaseHelper.ClearTable(AutoStmNote.Schema.TableName);
		}

		public void TestNoRelationships()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships();

			SetupSimpleDummyObjectsAndSave(out var parent, out _);
			AssertRelatedRecords(parent, [], archiveStage);
		}

		public void TestDummyForwardRelationship()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships((DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, isReversed: false));

			SetupSimpleDummyObjectsAndSave(out var parent, out var children);
			AssertRelatedRecords(parent, children, archiveStage);
		}

		public void TestDummyReverseRelationship()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships(
				(JobHeaderSchema.JH_ParentID, JobHeaderSchema.PK, isReversed: true),
				(JobHeaderSchema.JH_ParentID, DummyBizoSchema.PK, isReversed: true));

			var parent = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var child1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var child2 = Factory.New<DummyBusinessObject>();

			parent.JH_ParentID = child1.PK;
			child1.JH_ParentID = child2.PK;
			Factory.Save();

			AssertRelatedRecords(parent, [child1, child2], archiveStage);
		}

		public void TestDummyPivotRelationship()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships(
				(DummyBizoSchema.PK, DummyPivotSchema.ZDP_Z0, isReversed: false),
				(DummyPivotSchema.ZDP_ZD1, DummyDependentBizoSchema.PK, isReversed: false));

			var parent = Factory.New<DummyBusinessObject>();
			var pivot1 = Factory.New<DummyPivot>();
			var child1 = Factory.New<DummyDependantBusinessObject>();
			var pivot2 = Factory.New<DummyPivot>();
			var child2 = Factory.New<DummyDependantBusinessObject>();

			pivot1.ZDP_Z0 = parent.PK;
			pivot1.ZDP_ZD1 = child1.PK;

			pivot2.ZDP_Z0 = parent.PK;
			pivot2.ZDP_ZD1 = child2.PK;
			Factory.Save();

			AssertRelatedRecords(parent, [pivot1, pivot2, child1, child2], archiveStage);
		}

		public void TestDummyWithStmNote()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships((DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, isReversed: false));
			archiveStage.AddRelationshipToAllPKs(StmNoteSchema.ST_ParentID);

			SetupSimpleDummyObjectsAndSave(out var parent, out var children);
			SetupNotesOnBizosAndSave(children, out var notes);

			AssertRelatedRecords(parent, [..children, ..notes], archiveStage);
		}

		public void TestCircularRelationship()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships((JobHeaderSchema.PK, JobHeaderSchema.JH_JH_ParentJob, isReversed: false));

			var parent = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var child1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var child2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			parent.JH_JH_ParentJob = child1.PK;
			child1.JH_JH_ParentJob = child2.PK;
			child2.JH_JH_ParentJob = parent.PK;
			Factory.Save();

			AssertRelatedRecords(parent, [child1, child2], archiveStage);
		}

		public void TestSelfRelationship()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships((JobHeaderSchema.PK, JobHeaderSchema.JH_JH_ParentJob, isReversed: false));

			var parent = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			parent.JH_JH_ParentJob = parent.PK;
			Factory.Save();

			AssertRelatedRecords(parent, [], archiveStage);
		}

		public void TestRecordRelatedFromMultipleRelationshipsInOneBizoIsRetrievedExactlyOnce()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships(
				(DummyBizoSchema.Z0_Number, DummyDependentBizoSchema.ZD1_Number, isReversed: false),
				(DummyBizoSchema.Z0_AnotherNumber, DummyDependentBizoSchema.ZD1_Number, isReversed: false));

			var parent = Factory.New<DummyBusinessObject>();
			var child = Factory.New<DummyDependantBusinessObject>();

			// parent to child via Z0_Number and Z0_AnotherNumber
			parent.Z0_Number = 2001_06_01;
			parent.Z0_AnotherNumber = 2001_06_01;
			child.ZD1_Number = 2001_06_01;
			Factory.Save();

			AssertRelatedRecords(parent, [child], archiveStage);
		}

		public void TestRecordRelatedFromMultipleRelationshipsInMultipleBizosIsRetrievedExactlyOnce()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships(
				(DummyBizoSchema.Z0_Number, DummyBizoSchema.Z0_AnotherNumber, isReversed: false),
				(DummyBizoSchema.Z0_Decimal, DummyBizoSchema.Z0_AnotherDecimal, isReversed: false),
				(DummyBizoSchema.Z0_Date, DummyBizoSchema.Z0_AnotherDate, isReversed: false));

			var parent = Factory.New<DummyBusinessObject>();
			var child1 = Factory.New<DummyBusinessObject>();
			var child2 = Factory.New<DummyBusinessObject>();

			// parent to child1
			parent.Z0_Number = 2004_06_17;
			child1.Z0_AnotherNumber = 2004_06_17;

			// parent to child2
			parent.Z0_Decimal = 2007_05_22;
			child2.Z0_AnotherDecimal = 2007_05_22;

			// child1 to child2
			child1.Z0_Date = new ZDateTime(2010, 6, 17);
			child2.Z0_AnotherDate = new ZDateTime(2010, 6, 17);
			Factory.Save();

			AssertRelatedRecords(parent, [child1, child2], archiveStage);
		}

		public void TestDummyComplexSet()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships(
				(DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, isReversed: false),
				(DummyBizoSchema.PK, DummyPivotSchema.ZDP_Z0, isReversed: false),
				(DummyPivotSchema.ZDP_ZD1, DummyDependentBizoSchema.PK, isReversed: false));
			archiveStage.AddRelationshipToAllPKs(StmNoteSchema.ST_ParentID);

			var parent = Factory.New<DummyWithDependentsBusinessObject>();
			var bizosRelatedViaPivots = new List<BusinessObject>();

			foreach (var i in Enumerable.Range(1, 25))
			{
				var pivot = Factory.New<DummyPivot>();
				var dummyFromPivot = Factory.New<DummyDependantBusinessObject>();

				pivot.ZDP_Z0 = parent.PK;
				pivot.ZDP_ZD1 = dummyFromPivot.PK;
				parent.Dependents.AddNew().ZD1_Number = i;

				bizosRelatedViaPivots.AddRange([pivot, dummyFromPivot]);
			}

			SetupNotesOnBizosAndSave([..parent.Dependents, ..bizosRelatedViaPivots], out var notes);
			AssertRelatedRecords(parent, [..parent.Dependents, ..bizosRelatedViaPivots, ..notes], archiveStage);
		}

		public void TestDummyLargeSet()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships(
				(DummyBizoSchema.PK, DummyPivotSchema.ZDP_Z0, isReversed: false),
				(DummyPivotSchema.ZDP_ZD1, DummyDependentBizoSchema.PK, isReversed: false));
			archiveStage.AddRelationshipToAllPKs(StmNoteSchema.ST_ParentID);

			var parent = Factory.New<DummyBusinessObject>();
			var relatedItems = new List<BusinessObject>();

			foreach (var i in Enumerable.Range(1, 250))
			{
				var pivot = Factory.New<DummyPivot>();
				var attachedViaPivot = Factory.New<DummyDependantBusinessObject>();

				pivot.ZDP_Z0 = parent.PK;
				pivot.ZDP_ZD1 = attachedViaPivot.PK;
				pivot.ZDP_AddInfo = i.ToString("D5");

				relatedItems.AddRange([pivot, attachedViaPivot]);
			}

			SetupNotesOnBizosAndSave(relatedItems.ToArray(), out var notes);
			AssertRelatedRecords(parent, [..relatedItems, ..notes], archiveStage);
		}

		public void TestRelationshipsForOPSWithDeclarations()
		{
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, true);
			var archiveStage = ArchiveSystemSetupWithStageRelationships(new OPSArchiveStageDescriptor(), config);

			SetupTestObjectsForOPSRelationshipTest(out var jobHeader, out var relatedItems, out var relatedDeclarationItems);
			AssertRelatedRecords(jobHeader, [..relatedItems, ..relatedDeclarationItems], archiveStage);
		}

		public void TestRelationshipsForOPSWithoutDeclarations()
		{
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, false);
			var archiveStage = ArchiveSystemSetupWithStageRelationships(new OPSArchiveStageDescriptor(), config);

			SetupTestObjectsForOPSRelationshipTest(out var jobHeader, out var relatedItems, out _);
			AssertRelatedRecords(jobHeader, relatedItems, archiveStage);
		}

		public void TestRelationshipsForHAR()
		{
			var archiveStage = ArchiveSystemSetupWithStageRelationships(new HARArchiveStageDescriptor(), default);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = consignmentHeader.Consignments.AddNew();
			var consignment2 = consignmentHeader.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();

			item1.Lines.AddNew().HVS_Quantity = 1;
			item1.Lines.AddNew().HVS_Quantity = 2;
			item2.Lines.AddNew().HVS_Quantity = 3;
			item2.Lines.AddNew().HVS_Quantity = 4;

			var returnPivot = Factory.New<HVLVReturnPivot>();
			var consignmentAttachedViaPivotIsNotRelated = Factory.New<HVLVConsignment>();
			returnPivot.HVP_HVC_Former = consignment2.PK;
			returnPivot.HVP_HVC_Return = consignmentAttachedViaPivotIsNotRelated.PK;
			Factory.Save();

			AssertRelatedRecords(shipment, [consignmentHeader, consignment1, consignment2, item1, ..item1.Lines, item2, ..item2.Lines, returnPivot], archiveStage);
		}

		public void TestRelationshipsForSTAAsycudaManifestHeader()
		{
			var archiveStage = ArchiveSystemSetupWithStageRelationships(new STAAsycudaManifestHeaderArchiveStageDescriptor(), default);
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var asycudaBill = asycudaManifestHeader.Bills.AddNew();
			var asycudaPackedItem = asycudaBill.PackedItems.AddNew();
			var asycudaPack = asycudaBill.Packs.AddNew();
			var asycudaPackedItemTax = asycudaPackedItem.AsycudaTaxes.AddNew();

			asycudaPackedItemTax.AET_MethodOfCalculation = "A50";
			Factory.Save();

			AssertRelatedRecords(asycudaManifestHeader, [asycudaBill, asycudaPack, asycudaPackedItem, asycudaPackedItemTax], archiveStage);
		}

		public void TestNoDatabaseCommandsExecutedUntilFirstRecordIsRead()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships((DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, isReversed: false));
			var relationships = ((ArchiveStage)archiveStage).ArchiveableRelationships;

			SetupSimpleDummyObjectsAndSave(out var parent, out _);

			using (Db.Connection.TrackExecutedCommands())
			{
				var relatedRecords = RelatedRecordHelper.GetRelatedRecords(parent, relationships);
				AssertEquals("No commands should be executed until the first record is read", 0, Db.Connection.ExecutedCommands.Count());

				_ = relatedRecords.ToArray();
				AssertEquals("In this case, all child records are directly related to the parent, and there are no other relationships, so only one command should be executed",
					1, Db.Connection.ExecutedCommands.Count());
			}
		}

		public void TestNoLockOnReaderIfSeparateDatabaseCommandIsExecutedBeforeEnumerationCompletes()
		{
			var archiveStage = ArchiveSystemSetupWithRelationships((DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, isReversed: false));
			var relationships = ((ArchiveStage)archiveStage).ArchiveableRelationships;

			SetupSimpleDummyObjectsAndSave(out var parent, out _);

			var relatedRecords = RelatedRecordHelper.GetRelatedRecords(parent, relationships);
			var currentIndex = -1;

			foreach (var record in relatedRecords)
			{
				// InvalidOperationException: There is already an open DataReader associated with this Command which must be closed first.
				AssertNoExceptionThrown($"Executing command after accessing index {++currentIndex} should not throw error",
					() => Db.Connection.ExecuteScalar("select * from dbo.DummyBizo"));
			}

			var elementsSeen = currentIndex + 1;
			AssertGreaterThanOrEqualTo("There should have been at least two elements, or this test might have failed silently", elementsSeen, 2);
		}

		#region Internals

		void SetupSimpleDummyObjectsAndSave(out DummyBusinessObject parent, out DummyDependantBusinessObject[] children)
		{
			parent = Factory.New<DummyBusinessObject>();
			var child1 = Factory.New<DummyDependantBusinessObject>();
			var child2 = Factory.New<DummyDependantBusinessObject>();
			var child3 = Factory.New<DummyDependantBusinessObject>();

			child1.ZD1_Z0 = parent.PK;
			child2.ZD1_Z0 = parent.PK;
			child3.ZD1_Z0 = parent.PK;
			children = [child1, child2, child3];

			Factory.Save();
		}

		void SetupNotesOnBizosAndSave(BusinessObject[] bizos, out StmNote[] notes)
		{
			var notesList = new List<StmNote>();

			foreach (var bizo in bizos)
			{
				var note = Factory.New<StmNote>();
				note.ST_ParentID = bizo.PK;
				note.ST_Table = bizo.TableName;
				note.ST_NoteText = $"Test note on {bizo.TableName}({bizo.PK}) - 534852454B324F4E445644";

				notesList.Add(note);
			}

			notes = notesList.ToArray();
			Factory.Save();
		}

		void SetupTestObjectsForOPSRelationshipTest(out JobHeader parent, out BusinessObject[] relatedItems, out BusinessObject[] relatedDeclarationItems)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				parent = Factory.NewJobForTesting<JobHeader>();
				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				var caCustomsTestDataCreator = ObjectFactory.Get<ICACustomsTestDataCreator>();

				forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: false, out _, out _);
				caCustomsTestDataCreator.CreateAttachedCusCAeMHMaster(consol1PK);

				parent.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				parent.JH_ParentID = consol1PK;
				Factory.Save();

				var jobConsol = Factory.Load<ForwardingConsol>(consol1PK);
				var jobCartage = (BusinessObject)Factory.LoadTop1<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, consol1PK));
				var eMHMaster = Factory.LoadTop1<Enterprise.Integration.Customs.CA.ICusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol1PK));
				var jobContainer = Factory.LoadTop1<CommonContainer>(new ZQuery(JobContainerSchema.JC_JK, consol1PK));
				var jobContainerPenalty = Factory.LoadTop1<ContainerPenalty>(new ZQuery(JobContainerPenaltySchema.CPY_JC_Container, jobContainer.PK));
				var jobConsolTransport = Factory.LoadTop1<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, consol1PK));

				relatedItems = [jobConsol, jobCartage, jobContainer, jobContainerPenalty, jobConsolTransport];
				relatedDeclarationItems = [(BusinessObject)eMHMaster, ..eMHMaster.Containers.Cast<BusinessObject>()];
			}
		}

		void AssertRelatedRecords(BusinessObject parent, BusinessObject[] children, IArchiveSystemSetup archiveStageWithRelationships)
		{
			var relationships = ((ArchiveStage)archiveStageWithRelationships).ArchiveableRelationships;

			var expected = children
				.Select(ArchiveItemHelper.FromBizo)
				.Select(a => $"{a.PKColumn.TableName}({a.PK})");

			var actual = RelatedRecordHelper.GetRelatedRecords(parent, relationships)
				.Select(a => $"{a.PKColumn.TableName}({a.PK})");

			CombineAssertions("Precondition", () =>
			{
				AssertContainsExactElementsInAnyOrder("Expected items must be unique", expected.Distinct(), expected);
				AssertEquals("Expected items cannot contain null", 0, expected.Count(i => i is null));
			});

			AssertContainsExactElementsInAnyOrder("Expected the following related records", expected, actual);
		}

		IArchiveSystemSetup ArchiveSystemSetupWithRelationships(params (SchemaColumn parentPKColumn, SchemaColumn childFKColumn, bool isReversed)[] relationships)
		{
			var archiveStage = new ArchiveStage(new DummyArchiveStage(), new DummyArchiveSystem());

			foreach (var (parentPKColumn, childFKColumn, isReversed) in relationships)
			{
				((IArchiveSystemSetup)archiveStage).AddRelationship(parentPKColumn, childFKColumn, isReversed);
			}

			return archiveStage;
		}

		IArchiveSystemSetup ArchiveSystemSetupWithStageRelationships(IArchiveStageDescriptor stageDescriptor, IArchiveConfiguration config)
		{
			var archiveStage = ArchiveSystemSetupWithRelationships();
			stageDescriptor.SetupArchiveRelationships(archiveStage, config);

			return archiveStage;
		}

		#endregion
	}
}
