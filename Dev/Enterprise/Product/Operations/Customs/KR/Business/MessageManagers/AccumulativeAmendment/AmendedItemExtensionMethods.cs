using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.DataItemIDAttribute;

namespace Enterprise.Customs.KR.Business
{
	public static class AmendedItemExtensionMethods
	{
		public static ChangeType GetChangeType(this AmendedItemCollection amendedItems)
		{
			if (amendedItems.Cast<AmendedItem>().Any(x => x.ChangeType == ChangeType.DutyTaxRelatedAndNormal))
			{
				return ChangeType.DutyTaxRelatedAndNormal;
			}
			else if (amendedItems.Cast<AmendedItem>().Any(x => x.ChangeType == ChangeType.DutyTaxRelated))
			{
				if (amendedItems.Cast<AmendedItem>().Any(x => x.ChangeType == ChangeType.Normal))
				{
					return ChangeType.DutyTaxRelatedAndNormal;
				}
				return ChangeType.DutyTaxRelated;
			}
			else
			{
				return ChangeType.Normal;
			}
		}

		public static ZString GetID(this IEnumerable<IDInList> idsInList)
		{
			ZString result;
			if (idsInList?.Count() > 0)
			{
				var strBuilder = new ZStringBuilder();
				foreach (var id in idsInList)
				{
					var description = AmendmentIDSupporter.GetIDDescription(id.IDType);
					if (description.IsEmpty)
					{
						throw new Exception("Please specify the required ID description to be displayed.");
					}
					if (string.IsNullOrEmpty(id.IDValue) || id.IDValue == "0")
					{
						strBuilder.Append(description);
					}
					else
					{
						strBuilder.Append(description + " : " + id.IDValue);
					}
				}
				result = strBuilder.ToStringWithDelimiterBetweenAppends(", ");
			}
			else
			{
				result = Res.GetString("9F3E5C31-C355-402F-868A-017AFAE51F02", "Header");
			}
			return result;
		}
	}
}
