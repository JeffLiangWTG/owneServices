using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// The base class for all WebNotification Providers .
	/// </summary>
	public abstract class ZWebNotificationProvider : ZDivPopup, IContainResources
	{
		public ZWebNotificationProvider(Control control, IEnumerable<INotification> notifications) : base(control)
		{
			this.NotificationControl = control;
			this.Notifications = notifications;
			ErrorMessage = "";
			Initialise();
		}

		protected void Initialise()
		{
			foreach (INotification message in NotificationMessages)
			{
				ErrorMessage += message.Message + "\n";
			}
			if (ErrorMessage.EndsWith("\n"))
			{
				ErrorMessage = ErrorMessage.Substring(0, ErrorMessage.Length - 1);
			}

			this.ID = Guid.NewGuid().ToString();
			this.Attributes.Add("ControlToValidate", NotificationControl.ClientID);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css text should not translated")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			this.Style.Add("DISPLAY", "none");
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			Image errorImage = new Image();
			errorImage.ImageUrl = Icon.FileName;

			errorImage.ToolTip = ErrorMessage;
			Controls.Add(errorImage);
		}

		protected Image ErrorImageControl
		{
			get
			{
				EnsureChildControls();
				return (Image)Controls[0];
			}
		}

		protected abstract IEnumerable<INotification> NotificationMessages { get; }

#if DEBUG
		public IEnumerable<INotification> NotificationMessagesForTesting
		{
			get { return NotificationMessages; }
		}
#endif
		protected string ErrorMessage;
		protected Control NotificationControl;

#if DEBUG
		public Control NotificationControlForTesting
		{
			get { return NotificationControl; }
		}
#endif

		protected IEnumerable<INotification> Notifications;

		#region IContainResources Members

		protected ZPage BasePage
		{
			get { return Page as ZPage; }
		}

		protected abstract string IconResourceName { get; }

		protected ZWebResource Icon
		{
			get
			{
				if (fIcon == null)
				{
					fIcon = new ZWebResource(typeof(ZWebNotificationProvider), IconResourceName, BasePage);
					//required because Notification is added after resources are extracted
					//new ControlResourceManager(BasePage).ExtractResources(((IContainResources)this).Resources);
				}
				return fIcon;
			}
		}
		ZWebResource fIcon;

		ZWebResourceCollection IContainResources.Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(Icon);
				return result;
			}
		}

		#endregion
	}
}
