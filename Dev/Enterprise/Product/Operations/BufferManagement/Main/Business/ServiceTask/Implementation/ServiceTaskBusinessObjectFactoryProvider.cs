using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ServiceTaskBusinessObjectFactoryProvider : BusinessObjectFactoryProvider
	{
		public ServiceTaskBusinessObjectFactoryProvider(BusinessObjectFactory startingFactory)
			: base(startingFactory)
		{
		}

		protected override BusinessObjectFactory CreateNew(bool reclaimMemory)
		{
			var currentFactory = Current;
			var newFactory = base.CreateNew(reclaimMemory);

			TransferService<ServiceTaskCodeService>(currentFactory, newFactory);

			return newFactory;
		}

		static void TransferService<TService>(BusinessObjectFactory currentFactory, BusinessObjectFactory newFactory)
			where TService : IService
		{
			var service = currentFactory.ServiceContainer.GetService<TService>();

			if (service != null)
			{
				newFactory.ServiceContainer.AddService(service);
			}
		}
	}
}
