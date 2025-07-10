using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMBoardModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BMBoard; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.BMBoard);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BMBoardFilterControl(GridCollection, (BMBoardFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BMBoardCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BMBoardFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.BMBoard; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			if (ViewMenuItem != null)
			{
				ViewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("7B44DF30-E865-4FD5-96A1-D07F6480CD7E", "Visual Board"), HandleViewClick));

				if (BMSRegistry.Instance.PAVEOnTheWeb.Value)
				{
					ViewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("89F4312B-DD28-4CD1-88F5-CCC63A182BD5", "Visual Board on the Web"), HandleViewWebClick));
				}
				ViewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("392B6A83-751A-4020-97E8-EEFE0592C054", "Visual Board Configuration"), HandleViewConfigurationClick));
			}

			return menuItems.ToArray();
		}

		#region IModuleDecisionProvider Overrides

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new BMBoardModuleDecisionProvider(this);

		class BMBoardModuleDecisionProvider : DefaultModuleDecisionProvider, IHaveSpecialDefaultAction
		{
			public BMBoardModuleDecisionProvider(ZFilterModule module)
				: base(module)
			{
			}

			public override void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				switch (defaultActionViewMode)
				{
					case BoardViewMode.Config:
						base.HandleDefaultAction(selectedBusinessObjects);
						return;

					case BoardViewMode.Web:
						throw new InvalidOperationException("Opening web board cannot be default action for BMBoard - neither when double clicking a record nor when clicking a shortcut in recent items.");

					default:
						if (selectedBusinessObjects != null && selectedBusinessObjects.Length != 0)
						{
							var board = selectedBusinessObjects[0] as BMBoard;
							ShowBoard(board);
						}
						return;
				}
			}

			BoardViewMode defaultActionViewMode;

			IDisposable IHaveSpecialDefaultAction.DefineDefaultActionTriggeredByRecentItems()
			{
				defaultActionViewMode = BoardViewMode.Config;
				return new DisposableAction(() => defaultActionViewMode = BoardViewMode.Default);
			}
		}

		#endregion

		#region View

		public enum BoardViewMode
		{
			Default,
			Web,
			Config
		}

		BoardViewMode boardViewMode;

		protected override void HandleViewClickCore(object sender, EventArgs e)
		{
			boardViewMode = BoardViewMode.Default;
			base.HandleViewClickCore(sender, e);
		}

		void HandleViewWebClick(object sender, EventArgs e)
		{
			boardViewMode = BoardViewMode.Web;
			base.HandleViewClickCore(sender, e);
		}

		void HandleViewConfigurationClick(object sender, EventArgs e)
		{
			boardViewMode = BoardViewMode.Config;
			base.HandleViewClickCore(sender, e);
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			var board = selectedBusinessObject as BMBoard;

			switch (boardViewMode)
			{
				case BoardViewMode.Web:
					OpenBoardOnWeb(board);
					return null;

				case BoardViewMode.Config:
					return base.ShowViewForm(selectedBusinessObject);

				default:
					ShowBoard(board);
					return null;
			}
		}

		#endregion

		#region For Test
#if DEBUG

		public void SetBoardViewMode_ForTest(BoardViewMode viewMode)
		{
			boardViewMode = viewMode;
		}

#endif
		#endregion

		#region Implementation

		static void ShowBoard(BMBoard board)
		{
			if (board != null)
			{
				VisualBoardFormDisplayer.ShowBoard(board);
			}
		}

		static void OpenBoardOnWeb(BMBoard board)
		{
			if (board != null)
			{
				VisualBoardForm.OpenBoardOnWeb(board.PK, board.HumanReadableShortcutName);
			}
		}

		#endregion
	}
}
