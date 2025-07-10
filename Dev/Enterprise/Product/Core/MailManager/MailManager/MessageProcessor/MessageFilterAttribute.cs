using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.Definitions;
using Enterprise.MailManager.Integration;

namespace Enterprise.MailManager.MessageProcessor
{
	[Serializable]
	[XmlSerializerAssembly("MailManager.Integration.XmlSerializers")]
	public sealed class MessageFilterAttribute : AssemblyMetaDataAttributeWithType, IMessageFilterConfig, IEquatable<MessageFilterAttribute>
	{
		public MessageFilterAttribute(string serviceTaskCode, string tableName, Type type)
			: base(type)
		{
			ServiceTaskCode = serviceTaskCode;
			TableName = tableName;
		}

		public MessageFilterAttribute()
		{ }

		public string ServiceTaskCode { get; set; }
		public string TableName { get; set; }

		#region IEquatable<MessageFilterAttribute>

		public bool Equals(MessageFilterAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && (ServiceTaskCode, TableName).Equals((other.ServiceTaskCode, other.TableName));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is MessageFilterAttribute other && Equals(other);
		}

		public override int GetHashCode() => (base.GetHashCode(), ServiceTaskCode, TableName).GetHashCode();

		#endregion
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	[Serializable]
	public sealed class MessageFilterConditionAttribute : Attribute
	{
		public MessageFilterConditionAttribute(string propertyName, string pattern)
			: this(propertyName, pattern, RegexOptions.None)
		{ }

		public MessageFilterConditionAttribute(string propertyName, string pattern, RegexOptions options)
		{
			PropertyName = propertyName;
			_regex = new Regex(pattern, options);
		}

		public string PropertyName { get; private set; }

		public bool IsMatch(object value)
		{
			return value != null && _regex.IsMatch(value.ToString());
		}

		readonly Regex _regex;
	}
}
