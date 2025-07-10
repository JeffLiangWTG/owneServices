using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.PlugIn
{
	public abstract class ZAlwaysLoadPlugIn : ZPlugIn
	{
		protected ZAlwaysLoadPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		/// <summary>
		/// This method is called every time any data is changed in the top level business entity
		/// or any of its registered editable children.  It will be called many times.
		/// Any code written in this method should be kept to a minmum.
		/// </summary>
		public virtual void OnDataChangedInHost()
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				UnhookHasChangedEntity();
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			// an always load plug in must always load / create it's business entity
			var createdBusinessEntity = BusinessEntity;
		}

		#region Implementation

		protected internal override ZBool IsActive
		{
			get { return Enabled; }
		}

		IBusiness HasChangesEntity;
		bool InHasChanges;

		internal void HookupHasChangesChangedEvent(IBusiness businessEntity)
		{
			HasChangesEntity = businessEntity;
			businessEntity.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(BusinessEntity_HasChangesChanged);
		}

		void BusinessEntity_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (!InHasChanges)
			{
				try
				{
					InHasChanges = true;
					OnDataChangedInHost();
				}
				finally
				{
					InHasChanges = false;
				}
			}
		}

		void UnhookHasChangedEntity()
		{
			if (HasChangesEntity != null)
			{
				HasChangesEntity.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(BusinessEntity_HasChangesChanged);
				HasChangesEntity = null;
			}
		}

		#endregion
	}
}
