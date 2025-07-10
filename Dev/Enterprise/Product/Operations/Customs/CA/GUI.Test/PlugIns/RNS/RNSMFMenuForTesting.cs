using System;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	sealed class RNSMFMenuForTesting : RNSMFMenu
	{
		public RNSMFMenuForTesting(IRNSPlugInSupport plugInSupport)
			: base(plugInSupport)
		{
			OnPopup(EventArgs.Empty);
		}

		internal MenuItem SendMessagesMenuItem => sendMessages;

		internal MenuItem WithdrawMessagesMenuItem => withdrawMessages;

		internal MenuItem ResetToOriginalMenuItem => resetToOriginal;
	}
}
