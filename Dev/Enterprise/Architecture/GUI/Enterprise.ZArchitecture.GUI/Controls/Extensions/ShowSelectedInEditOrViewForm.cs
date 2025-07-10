using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	public class ShowSelectedInEditOrViewForm
	{
		public ShowSelectedInEditOrViewForm(IShowEditOrViewForm parent)
		{
			this.parent = parent;
		}

		protected IShowEditOrViewForm parent;

		public bool ShowEditOrViewForm()
		{
			using (var module = parent.NewModuleFromModuleID())
			{
				if (module != null && module.HasActions)
				{
					var showingEditOrViewFormEventArgs = parent.InitialiseForm(module);

					if (showingEditOrViewFormEventArgs.AllowEdit)
					{
						ShowEditForm(module);
						return true;
					}
					else if (showingEditOrViewFormEventArgs.AllowView)
					{
						ShowViewForm(module);
						return true;
					}
				}
			}
			return false;
		}

		public virtual void ShowEditForm(ZFilterModule module)
		{
			if (string.IsNullOrEmpty(parent.SearchCode))
			{
				if (parent.ShowNewFormWhenEmpty && parent.AllowNewForm && !parent.ReadOnly)
				{
					ShowNewFormForEmptyCode(module);
				}
			}
			else if (module.AllowEdit)
			{
				var bizObjs = parent.GetBizObjsToEditOrView()?.ToArray();
				if (bizObjs != null && bizObjs.Any())
				{
					if (bizObjs.IsCountMoreThan(1))
					{
						parent.ShowMoreThanOneSelectedMessageAndPopup(true);
					}
					else if (bizObjs.FirstOrDefault() != null)
					{
						module.ShowEditForm(bizObjs.First());
					}
				}
				else if (parent.AllowNewForm)
				{
					ShowNewFormForNonExistentCode(module);
				}
			}
		}

		protected virtual void ShowNewFormForEmptyCode(ZFilterModule module)
		{
			if (module.AllowNew)
			{
				module.ShowNewForm();
			}
			else
			{
				module.SecurityCheckpoint.ShowError();
			}
		}

		protected virtual void ShowNewFormForNonExistentCode(ZFilterModule module)
		{
			if (module.AllowNew)
			{
				if (parent.ShowDefaultMessageWhenCreatingANewBizObjFromFindBox(module) == DialogResult.Yes)
				{
					var form = module.ShowNewForm();

					if (form != null)
					{
						parent.SetCodePropertyFromText(form);
					}
				}
			}
			else
			{
				if (!module.DefaultMessageOverridingSecurityRightMessage.IsEmpty)
				{
					Globals.Message.Show(module.DefaultMessageOverridingSecurityRightMessage, Res.GetString("2E360E52-FBD1-4F9C-868B-25B133FFB653", "Form can not be opened"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					module.SecurityCheckpoint.ShowError();
				}
			}
		}

		public virtual void ShowViewForm(ZFilterModule module)
		{
			var bizObjs = module.GetBizObjsToEditOrView(module, parent.SearchCode, () => parent.GetBizObjsToEditOrView()).ToArray();
			if (bizObjs.IsCountMoreThan(1))
			{
				parent.ShowMoreThanOneSelectedMessageAndPopup(false);
			}
			else
			{
				var bizObj = bizObjs.FirstOrDefault();
				if (bizObj != null)
				{
					module.ShowViewForm(bizObj);
				}
			}
		}
	}

	public class ShowingEditOrViewFormEventArgs : EventArgs
	{
		public ShowingEditOrViewFormEventArgs(ZFilterModule module, bool allowEdit, bool allowView)
		{
			Module = module;
			AllowEdit = allowEdit;
			AllowView = allowView;
		}

		public readonly ZFilterModule Module;
		public bool AllowEdit;
		public bool AllowView;
	}
}
