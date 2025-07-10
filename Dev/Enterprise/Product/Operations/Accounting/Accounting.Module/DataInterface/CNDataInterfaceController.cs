using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Aggregator;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CNDataInterfaceController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			new AggregateController().PerformAggregationIfRequired();
			return new CNDataInterfaceGUI(new ChinaStandard2010DataInterfaceWrapper());
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CNDataInterface; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ChinaStandard2010DataInterfaceWrapper); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ChinaStandard2010DataInterfaceWrapper();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ChinaDataInterface; }
		}
	}
}
