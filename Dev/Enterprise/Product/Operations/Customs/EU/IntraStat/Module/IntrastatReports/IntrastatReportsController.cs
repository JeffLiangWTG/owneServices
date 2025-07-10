using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class IntrastatReportsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public IntrastatReportsController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.IntrastatReports; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.EU.IntrastatReports; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusIntrastatGroup); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.EuIntrastatReportsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EuIntrastatReportsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.EuIntrastatReportsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.EuIntrastatReportsView; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ReportsForm((CusIntrastatGroup)businessEntity);
		}
	}
}
