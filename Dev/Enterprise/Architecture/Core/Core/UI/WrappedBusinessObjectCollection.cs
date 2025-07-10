using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Core
{
	[Serializable]
	public class WrappedBusinessObjectCollection : Collection<WrappedBusinessObject>
	{
		public void AddBaseBusinessObjects(IEnumerable<BusinessObject> baseBusinessObjects)
		{
			foreach (var baseBusinessObject in baseBusinessObjects)
			{
				Items.Add(ToWrappedBusinessObject(baseBusinessObject));
			}
		}

		protected virtual WrappedBusinessObject ToWrappedBusinessObject(BusinessObject baseBusinessObject)
		{
			return new WrappedBusinessObject(baseBusinessObject);
		}

#if DEBUG
		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
#endif
		public ArrayList AsArrayList()
		{
			var selectedRows = new ArrayList();
			foreach (var element in Items)
			{
				selectedRows.Add(element.BaseBusinessObject);
			}
			return selectedRows;
		}
	}
}
