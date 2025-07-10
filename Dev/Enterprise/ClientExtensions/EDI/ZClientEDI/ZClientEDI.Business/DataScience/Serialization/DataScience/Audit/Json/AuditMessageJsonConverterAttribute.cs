using System;

namespace WTG.Serialization.DataScience.Audit
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class AuditMessageJsonConverterAttribute : Attribute
	{
		public AuditMessageJsonConverterAttribute(string messageFormatVersion)
		{
			MessageFormatVersion = messageFormatVersion;
		}

		public string MessageFormatVersion { get; }
	}
}
