using System;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void StringEventHandler(object sender, StringEventArgs e);

	/// <summary>
	/// EventArgs for the StringEventHandler. Used when you need to pass a string message through
	/// </summary>
	public class StringEventArgs : EventArgs
	{
		public StringEventArgs(string message)
		{
			fMessage = message;
		}

		public ZString Message
		{
			get { return fMessage; }
		}
		readonly ZString fMessage;
	}
}
