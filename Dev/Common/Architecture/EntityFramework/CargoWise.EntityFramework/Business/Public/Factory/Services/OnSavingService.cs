using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IOnSavingServiceBuilder : IService
	{
		IEnumerable<IOnSavingService> Build(IEnumerable<BusinessObject> rows);
	}

	public interface IOnSavingService : IService
	{
		void Apply(IEnumerable<BusinessObject> bizos);
		void OnSaveFailed();
	}

	class OnSavingService : ServiceProviderBase, IService
	{
		internal IReadOnlyList<IOnSavingService> GetOnSavingServices(IEnumerable<BusinessObject> rows)
		{
			var services = new List<IOnSavingService>();
			foreach (var pair in Services)
			{
				if (pair.Value is IOnSavingServiceBuilder serviceBuilder)
				{
					foreach (var service in serviceBuilder.Build(rows))
					{
						services.Add(service);
					}
				}
			}

			return services.AsReadOnly();
		}
	}
}
