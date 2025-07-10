using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class TransactionNumberSettingController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CATransactionNumberSetting; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SystemRegistry; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TransactionNumberSettingForm(new TransactionNumberSettingBO(Factory));
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override ZArchitecture.Core.ODisplayMode GetDisplayModeForNew()
		{
			return ZArchitecture.Core.ODisplayMode.Browse;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
