using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class PropertyIsUniqueInCollectionValidation : ValidationProvider
	{
		/// <summary>
		/// Deprecated. Do not use this method as a BusinessObject should not know about collections it belongs in.
		/// </summary>
		public static void CheckPropertyIsUniqueInCollection(ZPropertyInfo propertyInfo, bool checkEmptyValues = false)
		{
			CheckPropertyIsUniqueInCollection(propertyInfo, GetParentCollectionBusinessObjects(propertyInfo.BizObj), checkEmptyValues);
		}

		public static void CheckPropertyIsUniqueInCollection(ZPropertyInfo propertyInfo, IEnumerable<BusinessObject> collection, bool checkEmptyValues = false, bool ignoreStringCase = true)
		{
			CheckPropertyIsUniqueInCollection(propertyInfo, collection, false, null, checkEmptyValues, ignoreStringCase);
		}

		/// <summary>
		/// Deprecated. Do not use this method as a BusinessObject should not know about collections it belongs in.
		/// </summary>
		public static void CheckPropertyIsUniqueInCollection(ZPropertyInfo propertyInfo, string errorMessage)
		{
			CheckPropertyIsUniqueInCollection(propertyInfo, GetParentCollectionBusinessObjects(propertyInfo.BizObj), errorMessage);
		}

		/// <summary>
		/// Deprecated. Do not use this method as a BusinessObject should not know about collections it belongs in.
		/// </summary>
		public static void CheckPropertyIsUniqueInCollection(ZPropertyInfo propertyInfo, IMultilingualString errorMessage)
		{
			CheckPropertyIsUniqueInCollection(propertyInfo, GetParentCollectionBusinessObjects(propertyInfo.BizObj), errorMessage);
		}

		public static void CheckPropertyIsUniqueInCollection(ZPropertyInfo propertyInfo, IEnumerable<BusinessObject> collection, string errorMessage, bool checkEmptyValues = false, bool ignoreStringCase = true)
		{
			CheckPropertyIsUniqueInCollection(propertyInfo, collection, true, (NoResString)errorMessage, checkEmptyValues, ignoreStringCase);
		}

		public static void CheckPropertyIsUniqueInCollection(ZPropertyInfo propertyInfo, IEnumerable<BusinessObject> collection, IMultilingualString errorMessage, bool checkEmptyValues = false, bool ignoreStringCase = true)
		{
			CheckPropertyIsUniqueInCollection(propertyInfo, collection, true, errorMessage, checkEmptyValues, ignoreStringCase);
		}

		#region Implementation

		protected static IEnumerable<BusinessObject> GetParentCollectionBusinessObjects(BusinessObject businessObject)
		{
			foreach (BusinessObjectCollection collection in businessObject.ParentCollections)
			{
				foreach (BusinessObject next in collection)
				{
					yield return next;
				}
			}
		}

		static void CheckPropertyIsUniqueInCollection(ZPropertyInfo propertyInfo, IEnumerable<BusinessObject> collection, bool overrideErrorMessage, IMultilingualString errorMessage, bool checkEmptyValues, bool ignoreStringCase)
		{
			if (!propertyInfo.Value.IsEmpty || checkEmptyValues)
			{
				if (ignoreStringCase && !(propertyInfo.Value is ZString))
				{
					ignoreStringCase = false;
				}

				if (collection.Any(child => child.PK != propertyInfo.BizObj.PK && !child.IsDeleted &&
					((ignoreStringCase)
					? child[propertyInfo.Name].ToString().Equals(propertyInfo.Value.ToString(), System.StringComparison.OrdinalIgnoreCase)
					: child[propertyInfo.Name].Equals(propertyInfo.Value))
					))
				{
					string error = overrideErrorMessage
						? errorMessage.ToString()
						: MustBeUniqueMessage(propertyInfo.HumanReadableName);

					propertyInfo.AddError(error);
				}
			}
		}

		public static string MustBeUniqueMessage(string propertyDescriptor)
		{
			return Res.GetString("0dbcb874-4596-4659-b55f-a2b21c07e219", "The {0} has been duplicated and must be unique.", propertyDescriptor);
		}

		#endregion
	}
}
