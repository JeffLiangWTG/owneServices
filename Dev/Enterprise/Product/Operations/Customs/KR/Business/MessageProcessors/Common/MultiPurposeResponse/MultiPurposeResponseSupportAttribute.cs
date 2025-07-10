using System;
using System.Linq;

namespace Enterprise.Customs.KR.Business
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	sealed class MultiPurposeResponseSupportMessageTypeAttribute : Attribute
	{
		public MultiPurposeResponseSupportMessageTypeAttribute(params string[] transmitMessageTypes)
		{
			this.transmitMessageTypes = transmitMessageTypes;
		}

		readonly string[] transmitMessageTypes;

		public bool DoesSupport(string transmitMessageType) => transmitMessageTypes.Contains(transmitMessageType);
	}
}
