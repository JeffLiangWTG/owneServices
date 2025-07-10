#if DEBUG
using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBusinessObjectWithList : DummyBusinessObject
	{
		public DummyBusinessObjectWithList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyBusinessObjectInListList List
		{
			get
			{
				if (list == null)
				{
					list = new DummyBusinessObjectInListList(Factory);
				}
				return list;
			}
		}
		DummyBusinessObjectInListList list;
	}

	public class DummyBusinessObjectInListList : BusinessObjectCollection<DummyBusinessObjectInList>
	{
		public DummyBusinessObjectInListList(BusinessObjectFactory factory) : base(factory) { }

		public new DummyBusinessObjectInList this[int i]
		{
			get { return (DummyBusinessObjectInList)Elements[i]; }
		}
	}

	[CodeProperty("Z0_Code")]
	[DescriptionProperty("Z0_Description")]
	public class DummyBusinessObjectInList : DummyBusinessObject
	{
		public DummyBusinessObjectInList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyBusinessObjectInListList LookupList
		{
			get
			{
				if (list == null)
				{
					list = new DummyBusinessObjectInListList(Factory);
				}
				return list;
			}
		}
		DummyBusinessObjectInListList list;

		public DummyBusinessObjectInListList LookupListFromDataBase
		{
			get
			{
				if (listFromDataBase == null)
				{
					var localListFromDataBase = new DummyBusinessObjectInListList(Factory);
					localListFromDataBase.Load();
					listFromDataBase = localListFromDataBase;
				}
				return listFromDataBase;
			}
		}
		DummyBusinessObjectInListList listFromDataBase;
	}
}
#endif
