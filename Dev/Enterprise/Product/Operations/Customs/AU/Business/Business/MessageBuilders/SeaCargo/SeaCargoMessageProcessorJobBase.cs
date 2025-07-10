using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class SeaCargoMessageProcessorJobBase : ICargoMessageProcessorJob
	{
		public bool IsAcceptable
		{
			get
			{
				return IsSea && IsDischargedAtAustralianPort
					&& (!HasOceanBill || !OceanBill.HasErrors);
			}
		}

		public CusSCAOceanBill OceanBill => OceanBillCore;

		public ZGlobalMutex Mutex => MutexCore;

		public ZString JobNumber => JobNumberCore;

		public void SetSACIfRequired()
		{
			SetSACIfRequiredCore();
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		#endregion // IDisposable

		#region Implementation

		protected readonly bool shouldDelaySending;

		protected SeaCargoMessageProcessorJobBase(bool shouldDelaySending = false)
		{
			this.shouldDelaySending = shouldDelaySending;
		}

		protected abstract bool IsDischargedAtAustralianPort { get; }

		protected abstract bool IsSea { get; }

		protected abstract CusSCAOceanBill OceanBillCore { get; }

		protected abstract bool HasOceanBill { get; }

		protected abstract ZGlobalMutex MutexCore { get; }

		protected abstract ZString JobNumberCore { get; }

		protected abstract void SetSACIfRequiredCore();

		#endregion // Implementation
	}
}
