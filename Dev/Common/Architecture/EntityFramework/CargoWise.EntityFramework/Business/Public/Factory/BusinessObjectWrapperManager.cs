using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectWrapperManager
	{
		public BusinessObjectWrapperManager()
		{
			wrapperObjectArrays = new Dictionary<BusinessObject, List<BusinessObjectWrapper>>();
		}

		public int GetCount(BusinessObject bizO)
		{
			return GetWrappers(bizO).Count;
		}

		/// <summary>
		/// Returns the internal list of BusinessObjectWrapper objects
		/// Copy before enumerating and removing / adding to the collection
		/// </summary>
		/// <param name="bizO"></param>
		/// <returns></returns>
		internal IList<BusinessObjectWrapper> GetWrappers(BusinessObject bizO)
		{
			List<BusinessObjectWrapper> result;
			if (wrapperObjectArrays.TryGetValue(bizO, out result))
			{
				return result;
			}
			else
			{
				return Array.Empty<BusinessObjectWrapper>();
			}
		}

		#region Implementation

		readonly Dictionary<BusinessObject, List<BusinessObjectWrapper>> wrapperObjectArrays;

		internal void Add(BusinessObjectWrapper wrapper)
		{
			BusinessObject bizO = wrapper.WrappedBusinessObject;
			foreach (BusinessObjectWrapper existingWrapper in GetWrappers(bizO))
			{
				if (existingWrapper.GetType() == wrapper.GetType())
				{
					throw new ApplicationException("A second instance of the same type cannot be added for this object");
				}
			}

			List<BusinessObjectWrapper> list;
			if (!wrapperObjectArrays.TryGetValue(bizO, out list))
			{
				list = new List<BusinessObjectWrapper>();
				wrapperObjectArrays[bizO] = list;
			}
			list.Add(wrapper);
		}

		internal void Remove(BusinessObjectWrapper wrapper)
		{
			BusinessObject bizO = wrapper.WrappedBusinessObject;
			List<BusinessObjectWrapper> list;
			if (wrapperObjectArrays.TryGetValue(bizO, out list))
			{
				list.Remove(wrapper);
			}
		}

		internal void DeleteFor(BusinessObject bizO)
		{
			IList<BusinessObjectWrapper> wrappers = GetWrappers(bizO);
			for (int i = wrappers.Count - 1; i >= 0; i--)
			{
				wrappers[i].Delete();
			}
			wrapperObjectArrays.Remove(bizO);
		}

		#endregion
	}
}
