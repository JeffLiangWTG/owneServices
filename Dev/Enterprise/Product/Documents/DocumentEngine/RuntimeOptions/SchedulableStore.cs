using System;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class SchedulableStore<T> where T : struct
	{
		public T? RunValue { get; set; }
		public DateTime? Schedule { get; set; }
	}
}
