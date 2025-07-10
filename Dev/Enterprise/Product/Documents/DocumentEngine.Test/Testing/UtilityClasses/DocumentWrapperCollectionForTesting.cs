using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestClass]
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	public class DocumentWrapperCollectionForTesting : DocumentWrapperCollection<DocumentWrapperForTesting>
	{
		public DocumentWrapperCollectionForTesting()
			: base(new BusinessObjectFactory())
		{
		}

		public DocumentWrapperCollectionForTesting(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override ZDataTable Table
		{
			get { return new ZDataTable(); }
		}
	}
}
