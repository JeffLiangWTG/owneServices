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
	public class RefDocTypeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefDocType; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefDocType; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefDocType); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefDocTypeForm(businessEntity as RefDocType);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DocumentTypesModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DocumentTypesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DocumentTypesModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DocumentTypes; }
		}
	}
}
