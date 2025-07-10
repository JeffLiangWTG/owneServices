using System;

namespace Enterprise.DataTransfer.Native.Common.Interceptors
{
	public interface IInterceptor<T>
	{
		Action<T> Function { get; set; }
		IInterceptorSetting InterceptorSetting { get; }
		void Invoke(T entitySet);
	}

	public interface IInterceptor : IInterceptor<IEntitySet> { }

	public abstract class BaseInterceptor : IInterceptor
	{
		protected BaseInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
		{
			InterceptorSetting = setting;
			this.sessionServices = sessionServices;
		}
		protected AncillaryImportServices sessionServices;

		public Action<IEntitySet> Function { get; set; }

		public IInterceptorSetting InterceptorSetting { get; private set; }

		public abstract void Invoke(IEntitySet entitySet);
	}
}