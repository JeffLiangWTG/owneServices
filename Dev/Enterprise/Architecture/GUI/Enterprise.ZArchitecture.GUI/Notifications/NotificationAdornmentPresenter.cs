using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public class NotificationAdornmentPresenter : NotificationPresenter
	{
		public override void Dispose()
		{
			DetachAdornment();
			base.Dispose();
		}

		protected override void Display()
		{
			AttachAdornment();
			Adornment.Notifications = Notifications;
		}

		protected override void Clear()
		{
			DetachAdornment();
			Adornment.Notifications = NotificationCollection.Empty;
		}

		protected virtual NotificationAdornment CreateAdornment()
		{
			return NotificationAdornmentFactory.Create(Control);
		}

		#region Implementation

		bool adornmentAttached;

		NotificationAdornment Adornment
		{
			get
			{
				if (adornment == null)
				{
					adornment = CreateAdornment();
					adornment.Initialize(Control, this);
				}

				return adornment;
			}
		}
		NotificationAdornment adornment;

		void AttachAdornment()
		{
			if (!adornmentAttached)
			{
				Adornment.Attach();
				adornmentAttached = true;
			}
		}

		void DetachAdornment()
		{
			if (adornmentAttached)
			{
				Adornment.Detach();
				adornmentAttached = false;
			}
		}

		#endregion
	}
}
