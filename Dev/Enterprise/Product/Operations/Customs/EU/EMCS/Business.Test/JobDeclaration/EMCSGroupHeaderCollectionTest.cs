using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSGroupHeaderCollection))]
	sealed class EMCSGroupHeaderCollectionTest : GroupHeaderCollectionTest
	{
		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<EMCSJobDeclaration>();

		protected override BusinessObjectCollection GetCollectionToTest() => new EMCSGroupHeaderCollection((EMCSJobDeclaration)TestDec);
	}
}
