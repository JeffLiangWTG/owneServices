using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ServiceManager.Business
{
	sealed class HostedServiceSerializableSettings : XmlSerializableSetting
	{
		public string ConfigString { get; set; }

		public int SecondaryProcessesMaxCount { get; set; }
	}
}

