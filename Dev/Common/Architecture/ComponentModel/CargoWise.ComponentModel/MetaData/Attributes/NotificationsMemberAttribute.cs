using System;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Specifies the member or method for meta-data type MetaDataTypes.Notifications.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class NotificationsMemberAttribute : MetaDataMemberAttribute
	{
		public NotificationsMemberAttribute(string member)
			: base(MetaDataTypes.Notifications, member)
		{
			Argument.NotNull(member, nameof(member));
		}
	}
}
