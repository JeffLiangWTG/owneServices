using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	public class RefDocSourceController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefDocSource; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefDocSource; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefDocSource); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefDocSourceForm(businessEntity as RefDocSource);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DocumentSourcesModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DocumentSourcesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DocumentSourcesModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DocumentSources; }
		}
	}
}
