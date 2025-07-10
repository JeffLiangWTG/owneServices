using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CompletionCustomsOfficeCollection))]
	public class CompletionCustomsOfficeCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestMaxCount()
		{
			var collection = (CompletionCustomsOfficeCollection)GetCollectionToTest();
			NUnit.Framework.Assert.That(collection.MaxCount, Is.EqualTo(999));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			return new CompletionCustomsOfficeCollection(instruction);
		}
	}
}
