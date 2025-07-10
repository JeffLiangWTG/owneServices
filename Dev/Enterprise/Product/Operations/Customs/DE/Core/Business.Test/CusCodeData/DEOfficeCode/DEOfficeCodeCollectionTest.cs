using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(DEOfficeCodeCollection))]
	public class DEOfficeCodeCollectionTest : EuOfficeCodeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<JobDeclaration>();
			return new DEOfficeCodeCollection(parent);
		}
	}
}
