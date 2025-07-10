using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.PAVE.MENT.Module
{
	public class MENTAgedScoreQueryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.MENTAgedScoreQuery; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.MENTAgedScoreQuery; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MENTAgedScoreQuery); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MENTAgedScoreQueryForm((MENTAgedScoreQuery)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MENTAgedScoreQuery; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.MENTAgedScoreQueryEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MENTAgedScoreQueryNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MENTAgedScoreQueryDelete; }
		}

		#endregion
	}
}
