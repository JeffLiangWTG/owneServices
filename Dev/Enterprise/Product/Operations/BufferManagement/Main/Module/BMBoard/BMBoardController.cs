using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMBoardController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMBoard; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMBoard; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMBoard); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BMBoardForm((BMBoard)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.BMBoardView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.BMBoardEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BMBoardNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.BMBoardDelete; }
		}

		#endregion

		#region ZController Overrides

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IVisualBoardProvider boardProvider = (IVisualBoardProvider)sourceEntity;

			if (!VisualBoardSecurity.IsVisibleToCurrentCompany(boardProvider))
			{
				return null;
			}

			if (VisualBoardSecurity.CanCurrentUserEditBoard(boardProvider))
			{
				ShowLoadedForm(sourceEntity, FormAction.Edit);
				return LastShownForm;
			}
			else
			{
				return base.ShowEditForm(sourceEntity); // will open view form based on CheckPointForEdit
			}
		}

		#endregion
	}
}
