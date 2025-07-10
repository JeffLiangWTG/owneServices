using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ReasonForCancellation))]
	class ReasonForCancellationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<ReasonForCancellationValidation>(new ReasonForCancellation(Factory).Validation);
		}

		public void TestLookups()
		{
			AssertType<ReasonForCancellationLookups>(new ReasonForCancellation(Factory).Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => new ReasonForCancellation(Factory);
	}
}
