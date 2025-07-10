using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public static class GridEntityFormOpener
	{
		public static FormOpenResult OpenFormForSavedParent(ZForm form, ZGrid grid, MouseEventArgs mouseEventArgs, ControllerID controllerID, Func<BusinessObject> bizoGetter = null)
		{
			var hit = grid.HitTest(mouseEventArgs.Location);

			if (hit.Row >= 0)
			{
				var tryShowForm = true;

				if (bizoGetter == null)
				{
					bizoGetter = () => grid.ListManager.GetCurrent() as BusinessObject;
				}

				if (form.BusinessEntityForHasChanges.HasChanges)
				{
					var result = Globals.Message.Show(Res.GetString("18d85059-cadf-4d2b-9467-f0856f42c52b", "You must save this form first. Would you like to save now?"), Res.GetString("c2abe12f-c7ae-45ce-af59-837acc70acdf", "Cannot Edit"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);

					tryShowForm = result == DialogResult.Yes && form.FireSaveButton() == ContinueWithSave.Yes;
				}

				if (tryShowForm)
				{
					return OpenForm(grid, mouseEventArgs, bizoGetter, controllerID);
				}
			}

			return FormOpenResult.Empty;
		}

		public static FormOpenResult OpenForm(ZGrid grid, MouseEventArgs mouseEventArgs, Func<BusinessObject> bizoGetter, ControllerID controllerID)
		{
			var row = grid.GetRow(mouseEventArgs);

			return OpenForm(row, bizoGetter, controllerID);
		}

		public static FormOpenResult OpenForm(int row, Func<BusinessObject> bizoGetter, ControllerID controllerID)
		{
			if (row >= 0)
			{
				var entity = bizoGetter();

				if (entity != null)
				{
					if (!entity.IsInDatabase)
					{
						Globals.Message.Show(Res.GetString("c910c82e-9329-467e-b9d4-3f6da54b00ab", "Please select a saved row to edit."), Res.GetString("b8d79981-0b63-4eb7-be7d-3b4a63d3d979", "Cannot Edit"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					else
					{
						var controller = ZControllerFactory.Create(controllerID);
						var form = controller.ShowEditForm(entity);

						return new FormOpenResult(controller, form);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("c8360be8-4240-421d-826c-eba26f31c4a9", "Please select a valid row."), Res.GetString("488ff795-43c9-47e6-b43f-ff5927e7def4", "Nothing selected"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}

			return FormOpenResult.Empty;
		}
	}
}
