using System;
using CargoWise.EntityFramework;
using CargoWise.Pipes;

namespace Enterprise.BufferManagement.Business
{
	public static class IPipeExtensions
	{
		public static IPipeHandoverWrapper<T> ThreadSentryHandover<T>(this IPipe<T> pipe, Func<T, BusinessObjectFactory> factoryGetter)
		{
			return pipe.AddHandover(
				onClaim: obj =>
				{
					var factory = factoryGetter(obj);
					if (factory != null && !factory.ThreadSentry.IsOwner)
					{
						factory.ThreadSentry.TakeThreadOwnership();
					}
				},
				onRelease: obj =>
				{
					var factory = factoryGetter(obj);
					if (factory != null && factory.ThreadSentry.IsOwner)
					{
						factory.ThreadSentry.RelinquishThreadOwnership();
					}
				});
		}
	}
}
