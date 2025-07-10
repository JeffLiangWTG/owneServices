using System;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void NumberEventHandler(object sender, NumberEventArgs e);

	/// <summary>
	/// EventArgs for the NumberEventHandler. Used when you need to pass a number through (e.g. for a percentage if updating a percentage bar)
	/// </summary>
	public class NumberEventArgs : EventArgs
	{
		public NumberEventArgs(int number)
		{
			fNumber = number;
		}

		public int Number
		{
			get { return fNumber; }
		}
		readonly int fNumber;
	}
}
