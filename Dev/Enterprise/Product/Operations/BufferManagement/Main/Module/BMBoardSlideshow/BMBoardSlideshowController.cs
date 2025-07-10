using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMBoardSlideshowController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMBoardSlideshow; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMBoardSlideshow; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMBoardSlideshow); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BMBoardSlideshowForm((BMBoardSlideshow)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.BMSlideShowsView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.BMSlideShowsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BMSlideShowsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.BMSlideShowsDelete; }
		}

		#endregion
	}
}
