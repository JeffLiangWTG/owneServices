using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.DataItemIDAttribute;

namespace Enterprise.Customs.KR.Business
{
	public class DataProviderComparer<TDataProviderInterface>
	{
		public IEnumerable<AmendedItem> Compare(ZString originalMessageType, TDataProviderInterface originalSnapshot, TDataProviderInterface currentSnapshot, IEnumerable<string> mandatoryItems)
		{
			var entity = new Customs.Business.AccumulativeAmendment.DataProviderComparer<TDataProviderInterface>().Compare(originalSnapshot, currentSnapshot, new KRDataComparer(mandatoryItems));
			if (entity.AmendType != EntityAmendType.NoChange)
			{
				var providerType = typeof(TDataProviderInterface);
				foreach (var amendedItem in GetAmendedItems(entity, originalMessageType, providerType))
				{
					yield return amendedItem;
				}
			}
		}

		IEnumerable<AmendedItem> GetAmendedItems(Entity entity, ZString originalMessageType, Type providerType, List<IDInList> ids = null)
		{
			IDInList id = null;
			var isOrgType = IsOrganisation(providerType);
			if (entity.ID != null)
			{
				if (ids == null)
				{
					ids = new List<IDInList>();
				}
				id = AddIDInList(entity.ID, entity.Type);
				ids.Add(id);
			}

			var amendType = entity.AmendType;
			var fields = entity.Field;
			if (fields != null)
			{
				foreach (var field in fields)
				{
					if (IsAmendedItemRequired(field, amendType))
					{
						ZString itemID;
						ChangeType changeType;
						if (isOrgType)
						{
							var role = (entity.ID != null) ? Enum.GetValues(typeof(RoleType)).Cast<RoleType>().SingleOrDefault(x => x.ToString() == entity.ID) : RoleType.None;
							itemID = OrganisationDataItemIDProvider.GetItemID(originalMessageType, role, field.Name);
							changeType = ChangeType.Normal;
						}
						else
						{
							itemID = DataItemIDProvider.GetItemID(providerType, field.Name);
							changeType = DataItemIDProvider.GetChangeType(providerType, field.Name);
						}
						if (!itemID.IsEmpty)
						{
							yield return new AmendedItem()
							{
								EntityType = providerType.Name,
								AmendType = amendType,
								ChangeType = changeType,
								DataItemID = itemID,
								BeforeValue = (field.Before == SystemNullValue) ? string.Empty : field.Before,
								AfterValue = (field.After == SystemNullValue) ? string.Empty : field.After,
								IDsInList = ids != null ? new List<IDInList>(ids) : null,
							};
						}
					}
				}
			}

			if (entity.SubEntity != null)
			{
				foreach (var subEntity in entity.SubEntity)
				{
					var subEntityPropertyInfo = GetInterfaceMemberPropertyInfo(providerType, subEntity.Name);
					var subEntityProviderType = subEntityPropertyInfo.PropertyType;
					var isIEnumerableType = subEntityProviderType.IsGenericType && subEntityProviderType.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>));
					if (isIEnumerableType)
					{
						subEntityProviderType = subEntityProviderType.GetGenericArguments().FirstOrDefault();
					}

					foreach (var amendedItem in GetAmendedItems(subEntity, originalMessageType, subEntityProviderType, ids))
					{
						yield return amendedItem;
					}
				}
			}

			if (amendType == EntityAmendType.Delete && entity.SubEntity == null && entity.Field == null)
			{
				yield return new AmendedItem()
				{
					EntityType = entity.Type,
					AmendType = entity.AmendType,
					IDsInList = ids != null ? new List<IDInList>(ids) : null,
				};
			}
			if (id != null)
			{
				ids.Remove(id);
			}
		}

		static IDInList AddIDInList(ZString id, ZString type)
		{
			var result = new IDInList();
			result.IDType = type;
			result.IDValue = id;
			return result;
		}

		static bool IsAmendedItemRequired(EntityField field, EntityAmendType amendType)
		{
			return !((amendType == EntityAmendType.Add && string.IsNullOrEmpty(field.After)) ||
				(amendType == EntityAmendType.Delete && string.IsNullOrEmpty(field.Before)));
		}

		static bool IsOrganisation(Type providerType) => providerType == typeof(IOrganization);
		const string SystemNullValue = Customs.Business.AccumulativeAmendment.DataProviderComparer<TDataProviderInterface>.SystemNullValue;

		static PropertyInfo GetInterfaceMemberPropertyInfo(Type type, string name)
		{
			return type.GetProperty(name) ?? type.GetInterfaces().Select(x => x.GetProperty(name)).First();
		}
	}

	public class KRDataComparer : DataItemComparer
	{
		public KRDataComparer(IEnumerable<string> mandatoryItems)
		{
			this.mandatoryDataItems = mandatoryItems;
		}
		readonly IEnumerable<string> mandatoryDataItems;
		public override bool IsTheSame(object originalValue, object currentValue, PropertyInfo propertyInfo)
		{
			var result = base.IsTheSame(originalValue, currentValue, propertyInfo);
			if (result && originalValue == null && currentValue != null)
			{
				var id = propertyInfo.GetCustomAttribute<DataItemIDAttribute>()?.ItemID ?? string.Empty;
				if (!string.IsNullOrEmpty(id) && mandatoryDataItems.Contains(id))
				{
					result = false;
				}
			}
			return result;
		}
	}
}
