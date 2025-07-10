using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DSBJobManagementCollection : ActiveBusinessObjectCollection<DSBJobManagement>, IBindingList
	{
		public DSBJobManagementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DSBJobManagementCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		bool IBindingList.AllowRemove
		{
			get { return false; }
		}
	}
}
