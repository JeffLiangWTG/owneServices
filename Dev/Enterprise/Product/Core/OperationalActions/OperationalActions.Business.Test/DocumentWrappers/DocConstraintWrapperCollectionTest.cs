using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocConstraintWrapperCollection))]
	internal sealed class DocConstraintWrapperCollectionTest : DocumentWrapperCollectionTest<DocConstraintWrapperCollection>
	{
		public void TestNew()
		{
			var constraint1 = new Mock<IFilterConstraint>();
			var constraint2 = new Mock<IFilterConstraint>();
			var collection = DocConstraintWrapperCollection.New(new [] { constraint1.Object, constraint2.Object });
			AssertEquals(2, collection.Count);
			AssertSame(constraint1.Object, collection[0].WrappedObject);
			AssertSame(constraint2.Object, collection[1].WrappedObject);
		}

		#region Implementation
		protected override DocConstraintWrapperCollection GetNewDocumentWrapperCollection()
		{
			return DocConstraintWrapperCollection.New(System.Array.Empty<IFilterConstraint>());
		}

		protected override object GetNewObjectToWrap()
		{
			return new FilterCompanyConstraint();
		}
		#endregion
	}
}
