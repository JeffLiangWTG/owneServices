using System;

namespace Enterprise.Customs.KR.Messaging
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
	public sealed class OrganisationDataItemIDAttribute : Attribute
	{
		public OrganisationDataItemIDAttribute(String messageType, RoleType role, string itemID)
		{
			if (string.IsNullOrEmpty(itemID))
			{
				throw new ArgumentException("itemID should have a non empty value");
			}
			MessageType = messageType;
			ItemID = itemID;
			Role = role;
		}

		public readonly string MessageType;
		public readonly string ItemID;
		public readonly RoleType Role;
	}
}
