using System;

namespace CargoWise.Pipes
{
	public class PipeHandoverWrapper<TOutput> : IPipeHandoverWrapper<TOutput>
	{
		public PipeHandoverWrapper(IPipe<TOutput> inner, Action<TOutput> onClaim, Action<TOutput> onRelease)
		{
			this.inner = inner;
			this.onRelease = onRelease;
			this.onClaim = onClaim;
		}

		readonly IPipe<TOutput> inner;
		readonly Action<TOutput> onClaim;
		readonly Action<TOutput> onRelease;
		string debugName;

		#region IPipeDataSource

		string IPipeDataSource.DebugName
		{
			get { return debugName ?? inner.DebugName; }
			set { debugName = value; }
		}

		Guid IPipeDataSource.Key
		{
			get { return inner.Key; }
		}

		Type IPipeDataSource.Output
		{
			get { return inner.Output; }
		}

		#endregion

		#region IPipeHandoverWrapper

		void IPipeHandoverWrapper.OnClaim(object value)
		{
			OnClaim((TOutput)value);
		}

		void IPipeHandoverWrapper.OnRelease(object value)
		{
			OnRelease((TOutput)value);
		}

		public void OnClaim(TOutput value)
		{
			onClaim(value);
		}

		public void OnRelease(TOutput value)
		{
			onRelease(value);
		}

		#endregion

		#region Implementation

		public override string ToString()
		{
			return inner.ToString();
		}

		#endregion
	}
}
