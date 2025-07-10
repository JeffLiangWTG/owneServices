using System;
using System.Collections;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	internal class BusinessObjectsForARow
	{
		public BusinessObjectsForARow(BusinessObject bizO)
		{
			businessObjectOrArrayList = bizO;
		}

		public BusinessObject this[Type typeKey]
		{
			get
			{
				BusinessObject result = null;
				if (hasMultipleObjects)
				{
					foreach (BusinessObject bizO in list)
					{
						if (bizO.GetType() == typeKey)
						{
							result = bizO;
							break;
						}
					}
					if (result == null && IDontMindLoadingASubclassInsteadAttribute.HasAttribute(typeKey))
					{
						foreach (BusinessObject bizO in list)
						{
							if (bizO.GetType().IsSubclassOf(typeKey))
							{
								result = bizO;
								break;
							}
						}
					}
				}
				else
				{
					if ((FirstBizO.GetType() == typeKey) || (IDontMindLoadingASubclassInsteadAttribute.HasAttribute(typeKey) && FirstBizO.GetType().IsSubclassOf(typeKey)))
					{
						result = FirstBizO;
					}
				}
				return result;
			}
		}

		public void Add(BusinessObject bizO1)
		{
			Type key = bizO1.GetType();
#if DEBUG
			var existing = this[key];
			if (existing != null && existing.GetType() == bizO1.GetType())
			{
				throw new ArgumentException("There is already an entry for: " + key.FullName);
			}
#endif
			if (!hasMultipleObjects)
			{
				object bizO = businessObject;
				businessObjectOrArrayList = new ArrayList();
				list.Add(bizO);
				hasMultipleObjects = true;
			}
			list.Add(bizO1);
		}

		public void HandleDeleted(BusinessObject sender)
		{
			if (hasMultipleObjects)
			{
				foreach (BusinessObject bizObj in list)
				{
					if (bizObj != sender)
					{
						bizObj.Delete();
					}
				}
			}
			else
			{
				if (sender != FirstBizO)
				{
					FirstBizO.Delete();
				}
			}
		}

		internal BusinessObject FirstBizO
		{
			get
			{
				if (hasMultipleObjects)
				{
					return (BusinessObject)list[0];
				}
				else
				{
					return businessObject;
				}
			}
		}

		public bool IsSavedByFactory
		{
			get
			{
				bool isSavedByFactory = FirstBizO.IsSavedByFactory;

				if (hasMultipleObjects)
				{
					for (int i = 1; i < list.Count; i++)
					{
						BusinessObject bizo = (BusinessObject)list[i];
						if (bizo.IsSavedByFactory != isSavedByFactory)
						{
							string message = String.Format((NoResString)"Unable to determine if Factory should save record.\r\n{0}.IsSavedByFactory={1}, but {2}.IsSavedByFactory={3}",
								list[0].GetType().FullName, isSavedByFactory, bizo.GetType().FullName, bizo.IsSavedByFactory);
							throw new ApplicationException(message);
						}
					}
				}

				return isSavedByFactory || (FirstBizO.IsInDatabase && HasChangesInAuditDetails);
			}
		}

		bool HasChangesInAuditDetails
		{
			get
			{
				return
					FirstBizO.HasChangesInAuditDetails ||
					hasMultipleObjects && list.Cast<BusinessObject>().Any(bizo => bizo.HasChangesInAuditDetails);
			}
		}

#if DEBUG
		internal bool HasChangesInAuditDetailsForTests
		{
			get { return HasChangesInAuditDetails; }
		}
#endif

		public void UpdateHasChangesForOtherObjectsAroundRow(BusinessObject newBizO)
		{
			if (hasMultipleObjects)
			{
				foreach (BusinessObject bO in list)
				{
					if (bO != newBizO)
					{
						newBizO.HasChanges = ((IBusiness)bO).HasChangesNotIncludingChildren;
						break;
					}
				}
			}
		}

		public void SetHasChanges(bool hasChangesValue)
		{
			if (hasMultipleObjects)
			{
				foreach (BusinessObject bizO in list)
				{
					if (((IBusiness)bizO).HasChangesNotIncludingChildren != hasChangesValue)
					{
						bizO.HasChanges = hasChangesValue;
					}
				}
			}
			else
			{
				FirstBizO.HasChanges = hasChangesValue;
			}
		}

		BusinessObject businessObject
		{
			get { return (BusinessObject)businessObjectOrArrayList; }
		}

		ArrayList list
		{
			get { return (ArrayList)businessObjectOrArrayList; }
		}

		internal BusinessObject[] Values
		{
			get
			{
				if (hasMultipleObjects)
				{
					return (BusinessObject[])list.ToArray(typeof(BusinessObject));
				}
				else
				{
					return new BusinessObject[] { businessObject };
				}
			}
		}

		bool hasMultipleObjects;
		object businessObjectOrArrayList;
	}
}
