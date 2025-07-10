using System;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class DataItemIDAttribute : Attribute
	{
		public enum ChangeType { Normal, DutyTaxRelated, DutyTaxRelatedAndNormal }

		public DataItemIDAttribute(string itemID)
			: this(itemID, ChangeType.Normal, ZString.Empty)
		{
		}

		public DataItemIDAttribute(string itemID, ChangeType changeType)
			: this(itemID, changeType, ZString.Empty)
		{
		}

		public DataItemIDAttribute(string itemID, ChangeType changeType, string customsFeeID)
		{
			if (string.IsNullOrEmpty(itemID))
			{
				throw new ArgumentException("itemID should have a non empty value");
			}
			ItemID = itemID;
			DutyTaxChangeType = changeType;
			CustomsFeeID = customsFeeID;
		}

		public readonly string ItemID;
		public readonly string ItemDescription;
		public readonly ChangeType DutyTaxChangeType;
		public readonly string CustomsFeeID;
	}
}
