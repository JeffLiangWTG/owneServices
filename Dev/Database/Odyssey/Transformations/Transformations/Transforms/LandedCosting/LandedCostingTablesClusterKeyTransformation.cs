using System.Collections.Generic;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.LandedCosting
{
	public sealed class LandedCostingTablesClusterKeyTransformation : DataTransformation
	{
		#region Overrides of DataTransformation

		public override string UserDescription => "Landed Costing Tables ClusterKey Transformation";

		protected override void OnlinePreUpgradeTransform()
		{
			ClusterKeyCreator.Do(manager, isOnline: true);
		}

		protected override void OfflinePreUpgradeTransform()
		{
			ClusterKeyCreator.Do(manager, isOnline: false);
		}

		#endregion

		IClusterKeyCreator ClusterKeyCreator => new LandedCostingTablesClusterKeyCreator();
	}

	sealed class LandedCostingTablesClusterKeyCreator : IClusterKeyCreator
	{
		#region Implementation of IClusterKeyCreator

		/// <summary>
		/// JobDeclaration(JE_ClusterKey)|JobOrderHeader(no cluster key yet)
		/// |- LandedCostHeader
		/// |  |- LandCostInput
		/// |  |- LandedCostHistory
		/// |  |  |- LandedLineCostItem
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="isOnline"></param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Do(IUpgradeTaskWorkflowLogger logger, bool isOnline)
		{
			const string declarationClusterKeyNumberFountain = "CustomsDeclarationClusterKey";
			var clusterKeyDoer = ClusterKeyDoer.New(logger, isOnline);

			UpdateAsWorkerInstance(clusterKeyDoer, declarationClusterKeyNumberFountain);
		}

		#endregion

		void UpdateAsWorkerInstance(ClusterKeyDoer clusterKeyDoer, string clusterKeyNumberFountain)
		{
			var jobDeclarationDefinitions = new List<KeyDefinition>
			{
				new KeyDefinition(JobDeclarationSchema.PK, LandedCostHeaderSchema.LT_ParentID, isMidLevelMaster: true),
				new KeyDefinition(LandedCostHeaderSchema.PK, LandCostInputSchema.LI_LT),
				new KeyDefinition(LandedCostHeaderSchema.PK, LandedCostHistorySchema.LH_LT),
				new KeyDefinition(LandedCostHistorySchema.PK, LandedLineCostItemSchema.LZ_LH),
			};

			clusterKeyDoer.Do(JobDeclarationSchema.PK, jobDeclarationDefinitions, clusterKeyNumberFountain);
		}
	}
}
