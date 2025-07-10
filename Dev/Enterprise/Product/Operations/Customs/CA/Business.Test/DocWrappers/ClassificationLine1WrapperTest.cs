using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ClassificationLine1Wrapper))]
	sealed class ClassificationLine1WrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var classificationLine =
				new JobComInvoiceLineTest.ExpectedClassificationLine1ForTesting();
			return new ClassificationLine1Wrapper(classificationLine);
		}
	}
}
