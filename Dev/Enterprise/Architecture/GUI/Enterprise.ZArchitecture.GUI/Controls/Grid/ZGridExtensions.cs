using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZGridExtensions
	{
		#region GetCurrent

		public static BusinessObject GetCurrent(this ZGrid grid)
		{
			BusinessObject result = null;

			var listManager = grid.ListManager;
			if (listManager != null)
			{
				var positon = listManager.Position;
				if (positon >= 0 && positon < listManager.Count)
				{
					result = (BusinessObject)listManager.GetCurrent();
				}
			}

			return result;
		}

		#endregion

		#region GetCurrentPK

		public static ZGuid GetCurrentPK(this ZGrid grid)
		{
			var bizO = grid.GetCurrent();
			return (bizO != null) ? bizO.PK : ZGuid.Empty;
		}

		#endregion

		#region SelectSingleElementByPK

		public static void SelectSingleElementByPK(this ZGrid grid, ZGuid pk)
		{
			if (pk.IsValid)
			{
				var bizO = ((IBusinessObjectCollection)grid.List).FindByPK(pk);
				if (bizO != null)
				{
					grid.SelectSingleElement(bizO);
				}
			}
		}

		#endregion

		#region IsDoubleClickOnRow

		public static bool IsDoubleClickOnRow(this ZGrid grid, MouseEventArgs e)
		{
			return e.Clicks == 2 && e.Button == MouseButtons.Left && grid.HitTest(e.X, e.Y).Row > -1;
		}

		#endregion

		#region HookDoubleClickToOpenJob

		public static void HookDoubleClickToOpenJob<T>(this ZGrid grid, ControllerID controllerID, Func<T, BusinessObject> getBizOToOpen = null)
			where T : BusinessObject
		{
			grid.MouseDown += (sender, e) =>
			{
				if (grid.IsDoubleClickOnRow(e))
				{
					var bizO = grid.GetCurrent() as T;
					if (bizO != null)
					{
						var controller = ZControllerFactory.Create(controllerID);
						var bizOToOpen = (getBizOToOpen != null) ? getBizOToOpen(bizO) : bizO;
						if (bizOToOpen != null)
						{
							if (bizOToOpen.HasChanges)
							{
								Globals.Message.Show(Res.GetString("ZGrid|PleaseSaveError", "Please save your changes before trying to open a related form."));
							}
							else
							{
								controller.ShowEditForm(bizOToOpen);
							}
						}

						#region Test
#if DEBUG
						ControllerForTesting = controller;
#endif
						#endregion
					}
				}
			};
		}

		#endregion

		#region Test
#if DEBUG
		[ThreadStatic]
		internal static ZController ControllerForTesting;
#endif
		#endregion
	}

	//#warning Dave to write test on next checkin for GetCurrent(grid)
}
