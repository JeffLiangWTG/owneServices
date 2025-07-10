using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusExitItemPackageCollection))]
	class CusExitItemPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(CusExitItemPackageCollection);

		protected override CargoWise.EntityFramework.BusinessObjectCollection GetCollectionToTest()
		{
			cusExitItem = cusExitItem ?? Factory.New<CusExitItem>();
			return cusExitItem.Packages;
		}
		CusExitItem cusExitItem;
	}
}
