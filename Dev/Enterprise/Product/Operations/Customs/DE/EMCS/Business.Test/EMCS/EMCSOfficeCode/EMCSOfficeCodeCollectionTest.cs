using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSOfficeCodeCollection))]
	public class EMCSOfficeCodeCollectionTest : EuOfficeCodeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new EMCSOfficeCodeCollection(Factory.New<EMCSJobDeclaration>());
	}
}
