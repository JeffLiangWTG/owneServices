using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.HAR
{
	class HARArchiveStageDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor => new HARArchiveStageDescriptor();

		protected override IArchiveSystemDescriptor SystemDescriptor => new HARArchiveSystemDescriptor();

		protected override string ExpectedName
			=> "HVLV Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobShipmentSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobShipmentSchema.JS_UniqueConsignRef;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = JobShipmentSchema.JS_E_ARV;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
				var consignmentHeaderSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignmentHeader), HVLVConsignmentHeaderSchema.HCH_JS_Shipment);
				_ = consignmentHeaderSubQuery.AddToFilter(HVLVConsignmentHeaderSchema.HCH_IsArchived, false);

				var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
				_ = shipmentQuery.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.Equal, Core.Constants.ShipmentTypes.HighVolumeLowValue);
				_ = shipmentQuery.AddToFilter(ExpectedMainArchiveDateFilterColumn, SQLComparisonOperator.LessThan, config.ArchiveJobsOnOrBeforeThisDate);
				_ = shipmentQuery.AddToFilter(ExpectedMainArchiveDateFilterColumn, SQLComparisonOperator.NotEqual, null);
				shipmentQuery.AddSubQuery(consignmentHeaderSubQuery, JoinCondition.And);

				shipmentQuery.OrderBy = ExpectedMainArchiveDateFilterColumn.Name;

				return shipmentQuery;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
			=> ExpectedMainArchiveFilterWithoutDeclarations;

		protected override Type[] ExpectedPreparationActions
			=> new Type[] { typeof(HVLVArchiveAction) };

		protected override Type[] ExpectedArchiveActions
			=> new Type[] { typeof(PeriodArchiveCommencedAction), typeof(NullifyFKAction) };

		public override void TestGetPreparationAction()
		{
			var logger = new TestArchiveLogger();
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveSet = new TestArchiveSet(new HARArchiveSystemDescriptor(), "Dummy Stage Name", Guid.Empty, mainArchiveItem);
			var cache = new ArchiveSystemCache();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);

			var preparationAction = StageDescriptor.GetPreparationAction(logger, archiveSet, cache, config).ToList();

			CombineAssertions("Could not get preparation action properly", () =>
			{
				AssertEquals(1, preparationAction.Count);
				AssertType<HVLVArchiveAction>(preparationAction[0]);
			});
		}
	}
}
