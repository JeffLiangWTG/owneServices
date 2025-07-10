using System;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZGlobalForApplicationStartTesting : ZGlobalForTesting
	{
		public ZGlobalForApplicationStartTesting()
		{
			HasApplicationStartedSuccessfully = false;
		}

		protected override void Application_Start(object sender, EventArgs e)
		{
			throw new InvalidOperationException();
		}

		internal void Application_Start_Internal(object sender, EventArgs e) => Application_Start(sender, e);
	}
}
