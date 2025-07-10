using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using CargoWise.Definitions;

namespace Enterprise.TimeEngineScheduler.Integration
{
	[Serializable]
	[XmlSerializerAssembly("Enterprise.TimeEngineScheduler.Integration.XmlSerializers")]
	[SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	public sealed class SchedulerActionAttribute : AssemblyMetaDataAttributeWithType, IEquatable<SchedulerActionAttribute>
	{
		public SchedulerActionAttribute(string code, string description, Type type)
			: base(type)
		{
			Code = code;
			Description = description;
		}

		public SchedulerActionAttribute()
		{ }

		public string Code { get; set; }
		public string Description { get; set; }

		#region Equality members

		public bool Equals(SchedulerActionAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && (Code, Description).Equals((other.Code, other.Description));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is SchedulerActionAttribute other && Equals(other);
		}

		public override int GetHashCode() => (base.GetHashCode(), Code, Description).GetHashCode();

		#endregion
	}
}
