using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class ExportCustomsManifestModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.ExportCustomsManifest; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.ExportCustomsManifest);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ExportCustomsManifestFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ExportCustomsManifestHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ExportCustomsManifestFilterBusinessObject(true, true);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerCMRExportDeclaration; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ExportManifestOnShipping; }
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			MenuItem importMenuOption = new ZMenuItem("Import Data", new EventHandler(HandleImportDataClick));
			result.Add(importMenuOption);
			return result.ToArray();
		}

		protected virtual void HandleImportDataClick(object sender, EventArgs e)
		{
			ExportCustomsManifestHeader header = null;

			if (new ManifestDataImporter().DoImport(Factory, ref header))
			{
				ExportCustomsManifestController controller = (ExportCustomsManifestController)this.GetNewController(header);
				controller.SetNewBusinessObjectToReturn(header);
				controller.ShowNewForm();
			}
		}
	}
}
