using System;
using System.Xml.Serialization;
using CargoWise.Definitions;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
	[Serializable]
	[XmlSerializerAssembly("ServiceManager.Integration.ServiceTasks.CW.XmlSerializers")]
	public sealed class HostedServiceQueueProviderAttribute : AssemblyMetaDataAttributeWithType, IHostedServiceQueueProviderAttribute, IEquatable<HostedServiceQueueProviderAttribute>
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="serviceTaskCode"></param>
		/// <param name="queueProviderType">Must implement IHostedServiceQueueProvider</param>
		public HostedServiceQueueProviderAttribute(string serviceTaskCode, string queueName, Type type) : base(type)
		{
			ServiceTaskCode = serviceTaskCode;
			QueueName = queueName;
		}

		public HostedServiceQueueProviderAttribute()
		{
		}

		public string ServiceTaskCode { get; set; }
		public string QueueName { get; set; }

		#region Equality members

		public bool Equals(HostedServiceQueueProviderAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return base.Equals(other) && (ServiceTaskCode, QueueName).Equals((other.ServiceTaskCode, other.QueueName));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is HostedServiceQueueProviderAttribute other && Equals(other);
		}

		public override int GetHashCode() => (base.GetHashCode(), ServiceTaskCode, QueueName).GetHashCode();

		#endregion
	}
}
