using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	#region Interface

	public interface IPurgeValueParent
	{
		bool IsPurging { get; set; }

		IPurgeValueHelper PurgeHelper { get; }
	}

	public interface IPurgeValueHelper
	{
		void PurgeAllValues();
		bool HasValueNeededToBePurged();
	}

	#endregion

	public class PurgeValueHelper<T>
		: BasePurgeHelper<T>, IPurgeValueHelper where T : BusinessObject, IPurgeValueParent
	{
		public PurgeValueHelper(T parent)
			: base(parent)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1081:DoNotUseSubClassOfTypeofBusinessObjectCollection", Justification = "Baseline")]
		public bool HasValueNeededToBePurged()
		{
			var result = false;
			foreach (var purgeSourceInfo in PurgeSourceInfoList)
			{
				foreach (var propertyName in purgeSourceInfo.Targets)
				{
					var targetPropertyInfo = Parent.FindPropertyInfo(propertyName);
					if (targetPropertyInfo != null)
					{
						if (!targetPropertyInfo.Value.IsEmpty
							&& (targetPropertyInfo.PropertyType != typeof(ZBool) || (ZBool)targetPropertyInfo.Value))
						{
							result = true;
							break;
						}
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
									if (collection.Count > 0)
									{
										result = true;
										break;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		public void PurgeAllValues()
		{
			try
			{
				if (Parent.IsPurging)
				{
					return;
				}

				Parent.IsPurging = true;

				foreach (var purgeSourceInfo in PurgeSourceInfoList)
				{
					var value = GetValue(purgeSourceInfo.Condition);

					if (value == purgeSourceInfo.ShouldPurge)
					{
						foreach (var propertyName in purgeSourceInfo.Targets)
						{
							PurgeTheValueOfTargetProperty(propertyName);
						}
					}
				}
			}
			finally
			{
				Parent.IsPurging = false;
			}
		}

		#region GetNewPurgeSourceInfoList

		protected override List<PurgeSourceInfo> GetNewPurgeSourceInfoList()
		{
			var result = new List<PurgeSourceInfo>();

			var properties = GetPropertiesWithAttribute();

			foreach (var property in properties)
			{
				var sourceProperty = property.Item1;
				var conditionProperty = property.Item2;
				var shouldPurge = property.Item3;

				var purgeSource = result.FirstOrDefault(c => c.Condition == conditionProperty && c.ShouldPurge == shouldPurge);
				if (purgeSource == null)
				{
					purgeSource = new PurgeSourceInfo(conditionProperty, shouldPurge);
					result.Add(purgeSource);
				}

				purgeSource.Targets.Add(sourceProperty);
			}

			return result;
		}

		IEnumerable<Tuple<string, string, bool>> GetPropertiesWithAttribute()
		{
			var type = ParentType;
			var currentProperties = GetPropertiesFromType(type).ToList();

			var baseType = ParentType.BaseType;
			var typeOfBusinessObject = typeof(BusinessObject);

			while (baseType != null && baseType != typeOfBusinessObject)
			{
				var childProperties = GetPropertiesFromType(baseType);

				foreach (var property in childProperties)
				{
					if (currentProperties.All(c => c.Item1 != property.Item1))
					{
						currentProperties.Add(property);
					}
				}

				baseType = baseType.BaseType;
			}

			return currentProperties;
		}

		IEnumerable<Tuple<string, string, bool>> GetPropertiesFromType(Type type)
		{
			var listFromPurgeValue = from propertyInfo in type.GetProperties()
									 from attribute in propertyInfo.GetCustomAttributes(true)
									 where attribute is PurgeValueAttribute
									 let valueAttribute = (PurgeValueAttribute)attribute
									 select new Tuple<string, string, bool>(propertyInfo.Name, valueAttribute.ConditionPropertyName, true);

			var listFromPurgeValueExcept = from propertyInfo in type.GetProperties()
										   from attribute in propertyInfo.GetCustomAttributes(true)
										   where attribute is PurgeValueExceptAttribute
										   let valueExceptAttribute = (PurgeValueExceptAttribute)attribute
										   select new Tuple<string, string, bool>(propertyInfo.Name, valueExceptAttribute.ConditionPropertyName, false);

			return listFromPurgeValue.Union(listFromPurgeValueExcept);
		}

		#endregion
	}
}
