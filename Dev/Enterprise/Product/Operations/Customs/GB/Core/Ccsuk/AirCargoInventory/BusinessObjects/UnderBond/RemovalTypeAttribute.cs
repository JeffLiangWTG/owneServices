using System;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class RemovalTypeAttribute : Attribute
	{
		public RemovalTypeAttribute(string removalTypeCode)
		{
			RemovalTypeCode = removalTypeCode;
		}

		public readonly string RemovalTypeCode;

		public static RemovalTypeAttribute Get(Type type)
		{
			object[] typeAttributes = type.GetCustomAttributes(typeof(RemovalTypeAttribute), false);
			if (typeAttributes.Length > 0)
			{
				return (RemovalTypeAttribute)typeAttributes[0];
			}
			else
			{
				throw new ArgumentException("RemovalTypeAttribute must be implemented", nameof(type));
			}
		}
	}
}
