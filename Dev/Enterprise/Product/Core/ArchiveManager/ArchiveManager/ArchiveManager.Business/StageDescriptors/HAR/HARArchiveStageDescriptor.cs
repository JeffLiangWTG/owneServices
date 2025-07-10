using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Integration;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors
{
	public class HARArchiveStageDescriptor : CommonArchiveStageDescriptor
	{
		#region IArchiveStageDescriptor Members

		public override string Name
			=> Res.GetString("BCF4686C-C692-45C8-84B2-C8D92ADC5C41", "HVLV Archive");

		public override SchemaColumn MainArchivePKColumn
			=> JobShipmentSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobShipmentSchema.JS_UniqueConsignRef;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobShipmentSchema.JS_E_ARV;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var consignmentHeaderSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignmentHeader), HVLVConsignmentHeaderSchema.HCH_JS_Shipment);
			_ = consignmentHeaderSubQuery.AddToFilter(HVLVConsignmentHeaderSchema.HCH_IsArchived, false);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			_ = shipmentQuery.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.Equal, Core.Constants.ShipmentTypes.HighVolumeLowValue);
			_ = shipmentQuery.AddToFilter(JobShipmentSchema.JS_E_ARV, SQLComparisonOperator.LessThan, config.ArchiveJobsOnOrBeforeThisDate);
			_ = shipmentQuery.AddToFilter(JobShipmentSchema.JS_E_ARV, SQLComparisonOperator.NotEqual, null);
			shipmentQuery.AddSubQuery(consignmentHeaderSubQuery, JoinCondition.And);

			shipmentQuery.OrderBy = MainDateFilterColumn.Name;

			return shipmentQuery;
		}
		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> ArchiveRelationships.SetupHVLVArchiveRelationships(systemSetup);

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			yield return new HVLVArchiveAction(logger, set, config);
		}

		public override void OnArchiveSetProcessed(IArchiveSet set)
			=> OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(set, ProcessingInfoPerTable);

		#endregion
	}
}
