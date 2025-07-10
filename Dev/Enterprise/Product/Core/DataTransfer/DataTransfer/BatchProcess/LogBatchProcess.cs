using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataTransfer.BatchProcessor
{
	public class LoggedDataBatchProcess : BaseLoggedDataBatchProcess
	{
		public LoggedDataBatchProcess(LoggingInformation logger)
			: base(logger)
		{
		}

		#region HighWaterMark

		protected override ZDateTime HighWaterMark
		{
			get
			{
				ZDateTime regoDateTime = new ZDateTime(SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value);
				if (!regoDateTime.IsValid)
				{
					regoDateTime = ZDateTime.UtcNow.Date;
				}
				return regoDateTime;
			}
			set
			{
				if (value.IsValid)
				{
					SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime());
				}
			}
		}

		#endregion

		#region Listeners

		protected override ILogBatchListenerProxy[] Listeners
		{
			get { throw new NotImplementedException(); }
		}

		//protected override ILogBatchListenerProxy[] Listeners
		//{
		//	get
		//	{
		//		if (fListeners == null)
		//		{
		//			//fListeners = ClientHookLoader.Instance.ClientHook.LogBatchProcessListeners;
		//		}
		//		return fListeners;
		//	}
		//}

		//ILogBatchListenerProxy[] fListeners;

		#endregion
	}
}
