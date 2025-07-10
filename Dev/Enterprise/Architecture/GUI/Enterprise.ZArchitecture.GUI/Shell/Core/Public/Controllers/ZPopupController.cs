using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public abstract class ZPopupController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override Guid GetIDForBusinessEntity(IBusiness entity)
		{
			return IsOpeningSingletonForm ? Guid.Empty : Guid.NewGuid();
		}

		protected bool IsOpeningSingletonForm { get; private set; }

		protected bool IsFormShown()
		{
			return OpenedFormCache.GetInstance().Contains(Guid.Empty, ID.ToString());
		}

		protected void SwitchToForm()
		{
			OpenedFormCache.GetInstance().SwitchToCachedForm(Guid.Empty, ID.ToString());
		}

		public IZForm ShowSingletonForm()
		{
			IZForm result = null;
			if (IsFormShown())
			{
				SwitchToForm();
			}
			else
			{
				using (new DisposableAction(() => IsOpeningSingletonForm = false))
				{
					IsOpeningSingletonForm = true;
					result = ShowNewForm();
				}
			}
			return result;
		}

		#region Unsupported Form Showing Methods

		public sealed override IZForm ShowEditForm(BusinessObject entity)
		{
			throw new ModuleGuiNotSupportedException("Edit is not supported for popup controllers. Use ShowNewForm instead.");
		}

		public sealed override IZForm ShowDeleteForm(BusinessObject entity)
		{
			throw new ModuleGuiNotSupportedException("Delete is not supported for popup controllers. Use ShowNewForm instead.");
		}

		public sealed override IZForm ShowViewForm(BusinessObject entity)
		{
			throw new ModuleGuiNotSupportedException("View is not supported for popup controllers. Use ShowNewForm instead.");
		}

		#endregion

		#region Security Check Points

		protected sealed override SecurityCheckpoint CheckPointForEdit
		{
			get { return NoneCheckpoint; }
		}

		protected sealed override SecurityCheckpoint CheckPointForView
		{
			get { return NoneCheckpoint; }
		}

		protected sealed override SecurityCheckpoint CheckPointForDelete
		{
			get { return NoneCheckpoint; }
		}

		static SecurityCheckpoint NoneCheckpoint
		{
			get { return EnvProxy.Instance.Security.None as SecurityCheckpoint; }
		}

		#endregion
	}
}
