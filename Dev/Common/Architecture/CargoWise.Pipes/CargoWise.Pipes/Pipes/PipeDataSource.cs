using System;

namespace CargoWise.Pipes
{
	public class PipeDataSource<T> : IPipeDataSource<T>
	{
		public PipeDataSource(string debugName = "Unnamed PipeDataSource")
		{
			this.key = Guid.NewGuid();
			this.debugName = debugName;
		}

		readonly Guid key;
		string debugName;

		Guid IPipeDataSource.Key
		{
			get { return key; }
		}

		string IPipeDataSource.DebugName
		{
			get { return debugName; }
			set { debugName = value; }
		}

		Type IPipeDataSource.Output
		{
			get { return typeof(T); }
		}
	}
}
