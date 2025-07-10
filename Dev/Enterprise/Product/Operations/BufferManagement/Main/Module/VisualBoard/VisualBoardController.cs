using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Telemetry;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class VisualBoardController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.VisualBoard; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.VisualBoard; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMBoard); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException(GetType().Name + " handles showing the board form itself");
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return (IBusiness)factory.Load<BMBoard>(sourceEntityPK) ?? factory.Load<BMBoardSlideshow>(sourceEntityPK);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(VisualBoardController)}.{nameof(ShowEditForm)}");
			var boardProvider = (IVisualBoardProvider)sourceEntity;

			if (!VisualBoardSecurity.IsVisibleToCurrentCompany(boardProvider))
			{
				return null;
			}

			if (ArgsForNewForm != null && ArgsForNewForm.Contains(VisualBoardForm.OpenOnTheWebArgName))
			{
				VisualBoardForm.OpenBoardOnWeb(boardProvider.PK, boardProvider.HumanReadableShortcutName);
			}
			else
			{
				VisualBoardFormDisplayer.ShowBoard(boardProvider, args: ArgsForNewForm);
			}

			return null;
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.VisualBoards; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.VisualBoards; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.VisualBoards; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.VisualBoards; }
		}

		#endregion
	}
}
