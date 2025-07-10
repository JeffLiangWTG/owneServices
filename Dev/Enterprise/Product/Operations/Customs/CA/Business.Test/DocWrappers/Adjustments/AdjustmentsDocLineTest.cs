using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AdjustmentsDocLine))]
	abstract class AdjustmentsDocLineTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestAdjustmentsDocumentLineMembers();

		public abstract void TestPopulateCalculatedValueForTax();
	}
}
