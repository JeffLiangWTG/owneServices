using System;

namespace Enterprise.xTMessaging.Shared
{
	public interface IMsgClientProvider
	{
		IMsgClient MsgClient { get; }

		void TearDown();

		string ErrorMessage { get; }

		string XtToObjFilter { get; }

		TimeSpan XtServerMessageTimeout { get; }
	}
}
