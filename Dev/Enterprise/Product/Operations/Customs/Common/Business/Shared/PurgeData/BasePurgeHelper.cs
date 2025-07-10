using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Common
{
	public abstract class BasePurgeHelper<T> where T : BusinessObject
	{
		protected BasePurgeHelper(T parent)
		{
			Argument.NotNull(parent, nameof(parent));

			Parent = parent;
			ParentType = typeof(T);
		}

		protected T Parent { get; }

		protected Type ParentType { get; }

		protected bool GetValue(string property)
		{
			if (string.IsNullOrWhiteSpace(property))
			{
				return true;
			}

			try
			{
				var propertyValue = ReflectionUtil.GetPropertyValue(Parent, property);
				if (propertyValue != null)
				{
					bool result;
					if (bool.TryParse(propertyValue.ToString(), out result))
					{
						return result;
					}

					ZBool resultInZBool;
					if (ZBool.TryParse(propertyValue.ToString(), out resultInZBool))
					{
						return resultInZBool;
					}
				}

				throw new DeveloperNotificationException("This result is not Boolean.");
			}
			catch (Exception e)
			{
				throw new DeveloperNotificationException("This property could not be get. The following errors have ocurred.", e);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1081:DoNotUseSubClassOfTypeofBusinessObjectCollection", Justification = "Baseline")]
		protected void PurgeTheValueOfTargetProperty(string propertyName)
		{
			var targetPropertyInfo = Parent.FindPropertyInfo(propertyName);
			if (targetPropertyInfo != null)
			{
				targetPropertyInfo.ClearValue();
			}
			else
			{
				var propertyType = Parent.GetPropertyType(propertyName);

				if (propertyType.IsSubclassOf(typeof(BusinessObjectCollection)))
				{
					var propertyInfo = ParentType.GetProperty(propertyName);
					if (propertyInfo != null)
					{
						var collection = propertyInfo.GetValue(Parent) as BusinessObjectCollection;
						if (collection != null && collection.AllowRemove)
						{
							collection.RemoveAndDeleteAll();
						}
					}
				}
			}
		}

		#region Purge Source Info

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<PurgeSourceInfo> PurgeSourceInfoList
		{
			get
			{
				if (purgeSourceInfoList == null)
				{
					if (!PurgeSourceCacheInfo.TryGetValue(ParentType, out purgeSourceInfoList))
					{
						purgeSourceInfoList = GetNewPurgeSourceInfoList();
						PurgeSourceCacheInfo.Add(ParentType, purgeSourceInfoList);
					}
				}

				return purgeSourceInfoList;
			}
		}
		IEnumerable<PurgeSourceInfo> purgeSourceInfoList;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected abstract List<PurgeSourceInfo> GetNewPurgeSourceInfoList();

		static Dictionary<Type, IEnumerable<PurgeSourceInfo>> PurgeSourceCacheInfo
		{
			get => purgeSourceCacheInfo ?? (purgeSourceCacheInfo = new Dictionary<Type, IEnumerable<PurgeSourceInfo>>());
		}

		[ThreadStatic]
		static Dictionary<Type, IEnumerable<PurgeSourceInfo>> purgeSourceCacheInfo;

		public sealed class PurgeSourceInfo
		{
			public PurgeSourceInfo(string condition, bool shouldPurge)
			{
				Condition = condition;
				ShouldPurge = shouldPurge;
				Targets = new List<string>();
			}

			public string Condition { get; }

			public bool ShouldPurge { get; }

			public List<string> Targets { get; }
		}

		#endregion
	}
}
