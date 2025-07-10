using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Aggregator;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.Accounting.GUI.DataInterface;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CNReconciliationExportController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			new AggregateController().PerformAggregationIfRequired();
			return new CNReconciliationExportGUI(new ChinaReconciliationExportWrapper());
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CNReconciliationExport; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ChinaReconciliationExportWrapper); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ChinaReconciliationExportWrapper();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ChinaReconciliationExport; }
		}
	}
}
