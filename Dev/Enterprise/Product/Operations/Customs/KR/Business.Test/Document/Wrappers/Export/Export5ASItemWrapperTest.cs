using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Export5ASItemWrapper))]
	class Export5ASItemWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = new Export5ASItem();
			return new Export5ASItemWrapper(entryLine, Factory);
		}
	}
}
