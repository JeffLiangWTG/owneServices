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
	public class AirCTOHawbExportController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AirCTOHawbExportForm((ExportCustomsManifestLines)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AirCTOHawbExport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ExportCustomsManifestLines); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsAirCTOExportEdit; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsAirCTOExportEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsAirCTOExportEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsAirCTOExport; }
		}

		#endregion
	}
}
