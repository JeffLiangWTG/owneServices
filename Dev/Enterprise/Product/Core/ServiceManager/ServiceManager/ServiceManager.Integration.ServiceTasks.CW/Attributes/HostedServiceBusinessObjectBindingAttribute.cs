using System;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Definitions;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	[Serializable]
	[XmlSerializerAssembly("ServiceManager.Integration.ServiceTasks.CW.XmlSerializers")]
	public sealed class HostedServiceBusinessObjectBindingAttribute : AssemblyMetaDataAttribute, IHostedServiceBusinessObjectBinding, IEquatable<HostedServiceBusinessObjectBindingAttribute>
	{
		public HostedServiceBusinessObjectBindingAttribute(string serviceTaskCode, string table, string[] predicates, string queueName)
		{
			ServiceTaskCode = serviceTaskCode;
			Table = table;
			Predicates = predicates;
			QueueName = queueName;
		}

		public HostedServiceBusinessObjectBindingAttribute() { }

		public string ServiceTaskCode { get; set; }

		public string Table { get; set; }

		public string[] Predicates { get; set; }

		public string QueueName { get; set; }

		#region Equality members

		public bool Equals(HostedServiceBusinessObjectBindingAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other)
				   && (ServiceTaskCode, Table, QueueName).Equals((other.ServiceTaskCode, other.Table, other.QueueName))
				   && ((Predicates == null && other.Predicates == null) || EnumerablesExtensions.EqualIgnoringOrder(Predicates, other.Predicates));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is HostedServiceBusinessObjectBindingAttribute other && Equals(other);
		}

		public override int GetHashCode() =>
			(base.GetHashCode(), ServiceTaskCode, Table, string.Join(string.Empty, Predicates ?? []), QueueName).GetHashCode();

		#endregion
	}
}
