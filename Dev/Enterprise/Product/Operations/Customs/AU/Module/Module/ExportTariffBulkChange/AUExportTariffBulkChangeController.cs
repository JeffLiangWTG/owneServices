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
	public class AUExportTariffBulkChangeController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AUExportTariffBulkChange topTariffBulkChange = new AUExportTariffBulkChange(factory);
			return new AUExportTariffBulkChangeStartForm(topTariffBulkChange);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ExportTariffBulkChange; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.ExportTariffBulkChange; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AUExportTariffBulkChange); }
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
