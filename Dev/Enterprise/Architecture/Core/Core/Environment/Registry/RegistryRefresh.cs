using System;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Environment
{
	public static class RegistryRefresh
	{
		public static int? FrequencyInSeconds
		{
			get
			{
				if (refreshFrequency == null || DateTime.UtcNow > reGetValueTime)
				{
					try
					{
						refreshFrequency = (ObjectFactory.Get<IEntityFrameworkSettings>().RegistryRefreshFrequencyInSeconds);
					}
					catch (InvalidOperationException)
					{
						refreshFrequency = (ObjectFactory.New<IEntityFrameworkSettings>().RegistryRefreshFrequencyInSeconds);
					}

					reGetValueTime = DateTime.UtcNow.AddSeconds((double)refreshFrequency);
				}
				return refreshFrequency;
			}
		}
		static DateTime reGetValueTime;
		static int? refreshFrequency;
	}
}
