using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Environment
{
	class ConnectingFormManager : ZProcessStatusFormManager<ConnectingForm>
	{
		public ConnectingFormManager()
		{
			InitialDelay = TimeSpan.FromSeconds(4);
			if (System.Environment.UserInteractive)
			{
				Start();
			}
		}
	}
}

