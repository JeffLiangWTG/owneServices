using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassificationCollection<CusClassification>))]
	public class CusClassificationCollectionTest : Customs.Business.Testing.BaseClassificationCollectionAbstractTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusClassificationCollection<CusClassification>(Factory);
		}
	}
}
