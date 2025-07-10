using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmALogController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.StmALog; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.StmALog; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmALog); }
		}
	}
}
