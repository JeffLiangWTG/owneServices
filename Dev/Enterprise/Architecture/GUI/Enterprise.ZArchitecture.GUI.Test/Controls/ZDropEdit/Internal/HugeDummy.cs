using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class HugeDummy : DummyBusinessObject
	{
		public HugeDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		SuperDummyCollection supperDummyCollection;
		public SuperDummyCollection SupperDummyCollection
			=> supperDummyCollection ?? (supperDummyCollection = new SuperDummyCollection(Factory));

		public CodeDescriptionPairList SortedInvoiceList
		{
			get
			{
				if (sortedInvoiceList == null)
				{
					sortedInvoiceList = new CodeDescriptionPairList();
					var list = SupperDummyCollection.ToList();
					sortedInvoiceList.AddRange(list);
				}
				return sortedInvoiceList;
			}
		}
		CodeDescriptionPairList sortedInvoiceList;

		public void RefreshSortedInvoiceList()
		{
			sortedInvoiceList = null;
		}
	}
}
