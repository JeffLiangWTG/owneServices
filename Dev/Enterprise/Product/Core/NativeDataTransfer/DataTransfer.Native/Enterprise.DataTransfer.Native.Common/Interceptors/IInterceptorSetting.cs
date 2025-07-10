using System.Collections.Generic;

namespace Enterprise.DataTransfer.Native.Common.Interceptors
{
	public interface IInterceptorSetting
	{
		bool Enable { get; set; }
		IEntityContext Context { get; }

		IEnumerable<string> EnableList { get; }
		IEnumerable<string> DisableList { get; }

		IInterceptor<IEntitySet> Interceptor { get; }
	}

	public abstract class BaseInterceptorSetting : IInterceptorSetting
	{
		public bool Enable { get; set; }
		public IEntityContext Context { get; set; }
		public IInterceptor<IEntitySet> Interceptor { get; set; }

		public abstract IEnumerable<string> EnableList { get; }
		public abstract IEnumerable<string> DisableList { get; }
	}
}