using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	/// <summary>
	/// Only allows one new/edit/delete form for this controller to be shown at a time, in the whole
	/// of Enterprise.
	/// </summary>
	public abstract class ZSingletonController : ZPopupController
	{
		protected override Guid GetIDForBusinessEntity(IBusiness entity)
		{
			// form cache will always register forms with same GUID, so only one can be opened at once
			// form cache should really be refactored to use ZGuids, ModuleIDs, Guidless rego for popup modules etc.
			return Guid.Empty;
		}

		public override IZForm ShowNewForm()
		{
			IZForm result = null;
			if (IsFormShown())
			{
				SwitchToForm();
			}
			else
			{
				result = base.ShowNewForm();
			}
			return result;
		}
	}
}
