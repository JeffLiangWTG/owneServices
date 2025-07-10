using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class EmailSentNotificationForTest : EmailSentNotification
	{
		public EmailSentNotificationForTest()
		{
			MessageLabel_Exposed = new ZArchitecture.Web.GUI.WebControls.ZTextLabel();
		}

		public void DoPageLoad()
		{
			try
			{
				base.OnLoad(EventArgs.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is QueryStringException)
				{
					throw;
				}
			}
		}

		public ZArchitecture.Web.GUI.WebControls.ZTextLabel MessageLabel_Exposed { get => MessageLabel; set => MessageLabel = value; }

		protected override ZGlobal GetNewTestGlobal()
		{
			var result = new GlobalForTest();
			result.OnCustomSessionStart();
			return result;
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
