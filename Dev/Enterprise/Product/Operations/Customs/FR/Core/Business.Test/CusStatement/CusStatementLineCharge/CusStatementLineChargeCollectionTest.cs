using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementLineChargeCollection))]
	class CusStatementLineChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(CusStatementLineChargeCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new CusStatementLineChargeCollection(Factory.New<CusStatementHeader>().ChargesDetail);
	}
}

