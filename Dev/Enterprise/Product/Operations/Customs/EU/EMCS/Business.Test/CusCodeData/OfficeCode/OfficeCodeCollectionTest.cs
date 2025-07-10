using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(OfficeCodeCollection<OfficeCode>))]
	class OfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new OfficeCodeCollection<OfficeCode>(Factory.New<EMCSJobDeclaration>());
	}
}
