using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AUImportTariffBulkChangeController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AUImportTariffBulkChange topTariffBulkChange = new AUImportTariffBulkChange(factory);
			return new AUImportTariffBulkChangeStartForm(topTariffBulkChange);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImportTariffBulkChange; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.ImportTariffBulkChange; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AUImportTariffBulkChange); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
