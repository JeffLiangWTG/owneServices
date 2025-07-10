using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.SWL.Business
{
	public delegate void ShipnetProcessProgressEventHandler(ShipnetProcessor sender, ShipnetProcessProgressChangedEventArgs e);

	public class ShipnetProcessProgressChangedEventArgs : EventArgs
	{
		public ShipnetProcessProgressChangedEventArgs(int count, int total, StmALog log)
		{
			this.Count = count;
			this.Total = total;
			this.Log = log;
		}

		public readonly int Count;
		public readonly int Total;
		public readonly StmALog Log;
	}
}
