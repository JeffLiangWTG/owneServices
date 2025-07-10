using System;

namespace Enterprise.Accounting.DataTransfer
{
	public class BoolResponseEventArgs : EventArgs
	{
		public BoolResponseEventArgs(bool response = false)
		{
			Response = response;
		}

		public bool Response { get; set; }
	}
}
