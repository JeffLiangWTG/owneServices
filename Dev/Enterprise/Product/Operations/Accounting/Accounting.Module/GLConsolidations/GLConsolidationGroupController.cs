using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GLConsolidationGroupController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GLConsolidationGroups; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GLConsolidationGroups; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccConsolidationGroup); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GLConsolidationGroupForm((AccConsolidationGroup)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GLConsolidationGroupsView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GLConsolidationGroupsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GLConsolidationGroupsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GLConsolidationGroupsDelete; }
		}
	}
}