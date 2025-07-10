using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestClass]
	sealed class BusinessObjectCollectionForTesting : BusinessObjectCollection<BusinessObjectForTestingWith3ZTypedFields>
	{
		public BusinessObjectCollectionForTesting()
			: base(new BusinessObjectFactory())
		{
		}

		public BusinessObjectCollectionForTesting(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override ZDataTable Table
		{
			get { return new ZDataTable(); }
		}
	}
}
