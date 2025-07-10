using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class BusinessObjectDataSourceProvider
	{
		public BusinessObjectDataSourceProvider(BusinessObject businessEntity)
		{
			this.businessEntity = businessEntity;
		}

		public object GetProperty(string propertyName, string orderBy)
		{
			object result = businessEntity;
			foreach (string currentPropertyName in propertyName.Split('.'))
			{
				PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(result)[currentPropertyName];
				if (propertyDescriptor != null)
				{
					result = propertyDescriptor.GetValue(result);
				}
			}
			SortCollection(result, orderBy);

			return result;
		}

		protected void SortCollection(object collection, string orderBy)
		{
			if (!string.IsNullOrEmpty(orderBy))
			{
				IBusinessObjectCollection bizOCollection = collection as IBusinessObjectCollection;
				if (bizOCollection != null)
				{
					string[] orderBySegments = orderBy.Split(' ');
					string parameterName = orderBySegments[0];
					ListSortDirection direction = (orderBySegments.Length > 1 && orderBySegments[1].Trim().ToUpper() == "DESC") ? ListSortDirection.Descending : ListSortDirection.Ascending;
					bizOCollection.ApplySort(new SortInfo(parameterName, direction));
				}
			}
		}

		public readonly BusinessObject businessEntity;
	}
}
