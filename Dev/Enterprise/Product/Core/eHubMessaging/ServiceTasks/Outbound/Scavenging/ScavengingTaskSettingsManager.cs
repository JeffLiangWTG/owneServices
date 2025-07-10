using System;
using System.Xml;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eHub;

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging
{
	public class ScavengingTaskSettingsManager
	{
		internal ScavengingSettingCollection Collection;

		public void Load()
		{
			try
			{
				Collection = GetRegistryValue();
			}
			catch (InvalidOperationException ex)
			{
				if (!ex.IsCriticalException() && ex.InnerException is XmlException)
				{
					Collection = new ScavengingSettingCollection();
					SetRegistryValue(Collection);
				}
				else
				{
					throw;
				}
			}
		}

		public void Save()
		{
			SetRegistryValue(Collection);
		}

		public ScavengingSetting GetScavengingTaskSettings(string taskName)
		{
			var value = Collection.Get(taskName);
			if (value == null)
			{
				value = Collection.AddNew();
				value.TaskName = taskName;
			}
			return value;
		}

		internal virtual ScavengingSettingCollection GetRegistryValue()
		{
			return eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		internal virtual void SetRegistryValue(ScavengingSettingCollection collection)
		{
			eHubMessagingRegistry.Instance.ScavengingTaskSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}
	}
}
