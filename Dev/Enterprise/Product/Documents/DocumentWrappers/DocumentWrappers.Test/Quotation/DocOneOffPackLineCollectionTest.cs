using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocOneOffPackLineCollection))]
	sealed class DocOneOffPackLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocOneOffPackLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var rateOneOffPackLine = Factory.New<RateOneOffPackLine>();
			return DocOneOffPackLine.New(rateOneOffPackLine, Factory);
		}

		protected override DocOneOffPackLineCollection GetCollectionToTest()
		{
			return new DocOneOffPackLineCollection(Factory);
		}
	}
}
