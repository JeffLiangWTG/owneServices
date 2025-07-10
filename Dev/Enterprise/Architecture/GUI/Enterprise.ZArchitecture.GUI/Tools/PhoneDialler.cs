using System;
using System.ComponentModel;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Environment;

#if !WINZOR
using Enterprise.RemoteDesktopServices.Server;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	public class PhoneDialler
	{
		public void Dial(string number, string protocol)
		{
			if (IsRemoteSessionWithoutRDServices)
			{
				Globals.Message.ShowError(Res.GetString("c2f05e83-4876-4357-80e6-bfc624e0c851", "Phone calls may not be established in a remote session without RD Services"), Res.GetString("8ae56871-72e3-46fa-8376-70959a544ed0", "Error"));
			}
			else
			{
				var callParameter = protocol + ":" + number.Replace(" ", string.Empty);
				try
				{
					StartPhoneCall(callParameter);
				}
				catch (Win32Exception ex)
				{
					Globals.Message.ShowError(ex.Message
							+ System.Environment.NewLine
							+ Res.GetString("d3207581-80bf-4dd2-b232-5a04cfad8b70", "Error establishing phone call")
							+ System.Environment.NewLine
							+ Res.GetString("a850fa91-346c-48e8-86aa-f97fa5f786f1", "URL = {0}", callParameter),
						Res.GetString("8ae56871-72e3-46fa-8376-70959a544ed0", "Error"));
				}
			}
		}

		protected virtual bool IsRemoteSessionWithoutRDServices
		{
			get
			{
#if WINZOR
				return false;
#else
				return ObjectFactory.Get<TerminalService>().IsWTSSession
					&& Array.IndexOf(InitializationMessageHandler.RegisteredRemoteMessageTypes, EnterpriseChannelMessageTypes.WebUrl) == -1;
#endif
			}
		}

		protected virtual void StartPhoneCall(string sipParameter)
		{
			WebUrlLauncher.Launch(sipParameter);
		}
	}
}
