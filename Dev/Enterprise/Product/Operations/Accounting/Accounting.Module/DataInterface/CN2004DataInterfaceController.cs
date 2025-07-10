using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Aggregator;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004;
using Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT19581_2004;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CN2004DataInterfaceController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			new AggregateController().PerformAggregationIfRequired();
			return new CN2004DataInterfaceGUI(new ChinaStandard2004DataInterfaceWrapper());
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CN2004DataInterface; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ChinaStandard2004DataInterfaceWrapper); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ChinaStandard2004DataInterfaceWrapper();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}
	}
}
