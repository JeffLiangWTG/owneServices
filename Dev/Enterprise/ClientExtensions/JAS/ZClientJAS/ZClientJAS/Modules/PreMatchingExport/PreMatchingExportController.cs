using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Matching;
using Enterprise.Client.JAS.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class PreMatchingExportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ClientControllerRegistration.ExportAccountingDataForPreMatching; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(PreMatchedDataExporter); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ExportPreMatchingDataForm((PreMatchedDataExporter)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new PreMatchedDataExporter();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
