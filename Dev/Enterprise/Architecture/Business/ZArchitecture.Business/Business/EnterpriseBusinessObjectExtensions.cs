using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public static class EnterpriseBusinessObjectExtensions
	{
		public static Logs GetLogs(this BusinessObject businessObject)
		{
			return ((IStmALogParent)businessObject).Logs;
		}

		public static Notes GetNotes(this BusinessObject businessObject)
		{
			return ((IStmNoteParent)businessObject).Notes;
		}

		public static void LoadChildrenForDeletion(this BusinessObject bizObj, IBusiness[] childList = null, Func<BusinessObject, BusinessObject[]> getAdditionalChildren = null)
		{
			if (bizObj != null)
			{
				bizObj.LoadChildEditableObjectsForChild(childList, getAdditionalChildren: getAdditionalChildren, addAdditionalFetchHints: AddFetchForDelete);
			}
		}

		public static void LoadChildrenForDeletion<B>(this IEnumerable<B> bizObjs, IBusiness[] childList = null, Func<BusinessObject, BusinessObject[]> getAdditionalChildren = null)
			where B : BusinessObject
		{
			if (bizObjs != null)
			{
				foreach (var bizObj in bizObjs.ToArray())
				{
					bizObj.LoadChildrenForDeletion(childList, getAdditionalChildren);
				}
			}
		}

		static void AddFetchForDelete(BusinessObject bizObj)
		{
			if (bizObj.IsInDatabase)
			{
				bizObj.FetchStrategy.FetchForDelete();
			}
		}

		public static void DeleteAll<B>(this IEnumerable<B> bizObjsToDelete, bool addFetchHints = false, Func<BusinessObject, BusinessObject[]> getAdditionalChildren = null)
			where B : BusinessObject
		{
			if (bizObjsToDelete != null)
			{
				if (addFetchHints)
				{
					bizObjsToDelete.LoadChildrenForDeletion(getAdditionalChildren: getAdditionalChildren);
				}
				foreach (var bizObjs in bizObjsToDelete.ToArray())
				{
					bizObjs.Delete();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004")]
		public static void DeleteChildren<B>(this BusinessObject parentBizObj, SchemaGuidColumn foreignKey, bool addFetchHints = false, ZQuery additionalQuery = null)
			where B : BusinessObject
		{
			LoadChildren<B>(parentBizObj, foreignKey, additionalQuery)?.DeleteAll(addFetchHints);
		}

		public static B[] LoadChildren<B>(this BusinessObject parentBizObj, SchemaGuidColumn foreignKey, ZQuery additionalQuery = null)
			where B : BusinessObject
		{
			if (parentBizObj != null)
			{
				var query = new ZQuery(foreignKey, parentBizObj.PK);
				if (additionalQuery != null)
				{
					query.AddToFilter(additionalQuery);
				}
				query.FetchOnlyFromLocalCache = !parentBizObj.IsInDatabase;
				return parentBizObj.Factory.Load<B>(query);
			}
			return null;
		}
	}
}
