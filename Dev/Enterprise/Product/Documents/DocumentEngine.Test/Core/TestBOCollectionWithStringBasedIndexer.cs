using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TestBOCollectionWithStringBasedIndexer : TestBOCollection
	{
		public TestBOCollectionWithStringBasedIndexer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TestBO this[string index]
		{
			get
			{
				switch (index)
				{
					case "One":
						return this[0];
					case "Two":
						return this[1];
					case "Three":
						return this[2];
				}
				return null;
			}
		}
	}
}
