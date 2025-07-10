using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.Client.JAS.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class JXCImportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ClientControllerRegistration.ImportJXCFile; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JXCDataImporterBizO); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JXCImporterForm((JXCDataImporterBizO)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new JXCDataImporterBizO();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
