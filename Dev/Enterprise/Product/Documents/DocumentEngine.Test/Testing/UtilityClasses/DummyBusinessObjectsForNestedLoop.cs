using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyBusinessObjectLevel0 : ChildDummyBusinessObject
	{
		public DummyBusinessObjectLevel0(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString Level0Only { get; set; }

		DummyBusinessObjectLevel1Collection collectionLevel1;
		public DummyBusinessObjectLevel1Collection CollectionLevel1 => collectionLevel1 ?? (collectionLevel1 = new DummyBusinessObjectLevel1Collection(Factory));
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyBusinessObjectLevel1 : DummyChildBusinessObject
	{
		public DummyBusinessObjectLevel1(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString Level1Only { get; set; }

		public ZString GroupBy { get; set; }

		DummyBusinessObjectLevel2Collection collectionLevel2;
		public DummyBusinessObjectLevel2Collection CollectionLevel2 => collectionLevel2 ?? (collectionLevel2 = new DummyBusinessObjectLevel2Collection(Factory));
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyBusinessObjectLevel1Collection : BusinessObjectCollection<DummyBusinessObjectLevel1>
	{
		public DummyBusinessObjectLevel1Collection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DummyBusinessObjectLevel1Collection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyBusinessObjectLevel2 : DummyChildBusinessObject
	{
		public DummyBusinessObjectLevel2(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		DummyBusinessObjectLevel3Collection collectionLevel3;
		public DummyBusinessObjectLevel3Collection CollectionLevel3 => collectionLevel3 ?? (collectionLevel3 = new DummyBusinessObjectLevel3Collection(Factory));

		DummyBusinessObjectLevel3Collection collectionLevelEmpty;
		public DummyBusinessObjectLevel3Collection CollectionLevelEmpty => collectionLevelEmpty ?? (collectionLevelEmpty = new DummyBusinessObjectLevel3Collection(Factory));
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyBusinessObjectLevel2Collection : BusinessObjectCollection<DummyBusinessObjectLevel2>
	{
		public DummyBusinessObjectLevel2Collection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DummyBusinessObjectLevel2Collection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyBusinessObjectLevel3 : DummyChildBusinessObject
	{
		public DummyBusinessObjectLevel3(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public DummyBusinessObjectLevel2 Parent { get; set; }
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyBusinessObjectLevel3Collection : BusinessObjectCollection<DummyBusinessObjectLevel3>
	{
		public DummyBusinessObjectLevel3Collection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DummyBusinessObjectLevel3Collection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}
	}
}
