using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class JobBillingExRateConfigController : ZSingletonController
	{
		public override ControllerID ID => ControllerIDs.JobBillingExRateSysConfig;

		public override Type TypeOfTopLevelBusinessObject => null;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.JobExRateSysConfigsModify;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var exRateSysConfigCol = new AccExchangeRateConfigurationCollection(Factory);
			exRateSysConfigCol.Load();
			return exRateSysConfigCol;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JobExRateSysConfigForm((AccExchangeRateConfigurationCollection)businessEntity);
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}

		public override ModuleIdentifier ModuleID => null;
	}
}
