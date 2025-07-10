using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.Client.JAS.GUI.Cognos;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class CognosCsvExportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ClientControllerRegistration.ExportCognosCsv; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CognosDataExporterBizO); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CognosCsvExportForm((CognosDataExporterBizO)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new CognosDataExporterBizO();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
