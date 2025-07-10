using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AirCTOExportController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AirCTOExportController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AirCTOExport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AirCTOExportCustomsManifestHeader); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<AirCTOExportCustomsManifestHeader>();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			AirCTOExportCustomsManifestHeader exportManifest = (AirCTOExportCustomsManifestHeader)businessEntity;

			ZForm result = new AirCTOExportForm(exportManifest);
			return result;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsAirCTOExport; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsAirCTOExportEdit; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsAirCTOExportNew; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsAirCTOExportDelete; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
