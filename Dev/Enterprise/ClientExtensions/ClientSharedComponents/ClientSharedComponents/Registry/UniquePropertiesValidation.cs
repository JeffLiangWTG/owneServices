using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ClientSharedComponents
{
	/// <summary>
	/// Check the uniqueness of two or more properties in a collection.
	/// </summary>
	/// <remarks>Ideally this code should be moved to base and renamed to something like "IsKeyUniqueValidation" and test uniqueness of the key against all the elements 
	/// in the collection, where the key is 1 or more properties (parts) of the element (business object).</remarks>

	public class UniquePropertiesValidation : PropertyIsUniqueInCollectionValidation
	{
		public static void CheckPropertiesAreUniqueInCollection(params ZPropertyInfo[] keyProperties)
		{
			CheckPropertiesAreUniqueInCollection(GetParentCollectionBusinessObjects(keyProperties[0].BizObj), false, null, keyProperties);
		}

		public static void CheckPropertiesAreUniqueInCollection(IEnumerable<BusinessObject> collection, params ZPropertyInfo[] keyProperites)
		{
			CheckPropertiesAreUniqueInCollection(collection, false, null, keyProperites);
		}

		public static void CheckPropertiesAreUniqueInCollection(IEnumerable<BusinessObject> collection, string errorMessage, params ZPropertyInfo[] keyProperites)
		{
			CheckPropertiesAreUniqueInCollection(collection, true, errorMessage, keyProperites);
		}

		#region Implementation
		static void CheckPropertiesAreUniqueInCollection(IEnumerable<BusinessObject> collection, bool overrideErrorMessage, string errorMessage, params ZPropertyInfo[] keyProperites)
		{
			if (keyProperites.Length < 2)
			{
				throw new ArgumentException(ErrorPropertyInfoArgumentExecption);
			}

			foreach (BusinessObject child in collection)
			{
				if (child != keyProperites[0].BizObj)
				{
					if (!IsBizObjUnique(child, keyProperites))
					{
						string error;
						if (overrideErrorMessage)
						{
							error = errorMessage;
						}
						else
						{
							error = String.Format(ErrorDuplicateProperties, GetPropertiesHumanReadableNames(keyProperites));
						}
						AddErrors(error, keyProperites);
						break;
					}
				}
			}
		}

		static bool IsBizObjUnique(BusinessObject child, ZPropertyInfo[] keyProperites)
		{
			bool result = true;
			foreach (ZPropertyInfo propertyInfo in keyProperites)
			{
				result &= child[propertyInfo.Name].Equals(propertyInfo.Value);
			}
			return !result;
		}

		static string GetPropertiesHumanReadableNames(ZPropertyInfo[] keyProperites)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (ZPropertyInfo propertyInfo in keyProperites)
			{
				result.Append(propertyInfo.HumanReadableName);
			}
			return result.ToStringWithDelimiterBetweenAppends(commaDelimiter);
		}

		static void AddErrors(string error, ZPropertyInfo[] keyProperites)
		{
			foreach (ZPropertyInfo propertyInfo in keyProperites)
			{
				propertyInfo.AddError(error);
			}
		}
		#endregion

		const string ErrorDuplicateProperties = "The following properties ({0}) have been duplicated and must be unique.";
		const string ErrorPropertyInfoArgumentExecption = "Unable to compare property uniqueness.  You must pass 2 or more property info's.";
		const string commaDelimiter = ", ";
	}
}
