using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	public class ResourceStringsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ZController Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ResourceStrings; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ResourceStrings; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(HelpDataString); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HelpDataStringForm((HelpDataString)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region ShowNew\EditForm

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var item = (HelpDataString)sourceEntity.Clone();
			item.HasChanges = false;
			var form = new HelpDataStringForm(item);
			form.Show();
			return form;
		}

		public override IZForm ShowNewForm()
		{
			var newItem = new HelpDataString();
			newItem.HD_IsCheckedOut = true;
			newItem.HasChanges = false;
			var form = new HelpDataStringForm(newItem);
			form.Show();
			return form;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			var newItem = ((HelpDataString)inMemorySourceEntity).Clone();
			newItem.HD_IsCheckedOut = true;
			newItem.HasChanges = false;
			var form = new HelpDataStringForm(newItem);
			form.Show();
			return form;
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			return ShowEditForm(sourceEntity);
		}

		#endregion

		#region Delete

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (sourceEntity.CanDelete)
			{
				DeleteMultiple(new BusinessObject[] { sourceEntity });
			}
			else
			{
				Globals.Message.ShowInformation(sourceEntity.ReasonForNotAbleToDelete);
			}
			return null;
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			foreach (HelpDataString each in selectedBusinessObjects)
			{
				if (!each.HD_IsCheckedOut)
				{
					Globals.Message.ShowInformation(Res.GetString("b9f09cc5-3851-4ed3-aa43-338c65f9ca00", "One of the resource strings is not checked out to you. Cannot proceed with undo."));
					return;
				}
			}

			string message = Res.GetString("0ec77fe2-4a0e-4ac3-9bda-c3ea8146b9fe", "Are you sure you want to undo {0} resource strings?", selectedBusinessObjects.Length);
			if (Globals.Message.Show(message, Res.GetString("2d020736-f658-4c6b-880f-c66c98b838c3", "Please confirm..."), MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.No)
			{
				ResourceStringsFactory.UndoCheckout(Array.ConvertAll(selectedBusinessObjects, bizo => (HelpDataString)bizo));
			}
		}

		#endregion
	}
}
