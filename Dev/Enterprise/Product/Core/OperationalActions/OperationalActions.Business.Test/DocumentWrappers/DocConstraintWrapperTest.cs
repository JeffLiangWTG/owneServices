using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocConstraintWrapper))]
	internal sealed class DocConstraintWrapperTest : DocumentWrapperTest
	{
		public void TestName()
		{
			var constraint = new Mock<IFilterConstraint>();
			DocConstraintWrapper wrapper = DocConstraintWrapper.New(constraint.Object);

			constraint.Setup(m => m.Name).Returns("Name");
			AssertEquals("Name", wrapper.Name);
		}

		public void TestDescription()
		{
			var constraint = new Mock<IFilterConstraint>();
			var wrapper = DocConstraintWrapper.New(constraint.Object);
			constraint.Setup(m => m.Description).Returns("Description");

			AssertEquals("Description", wrapper.Description);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return DocConstraintWrapper.New(new FilterCountryConstraint());
		}
		#endregion
	}
}
