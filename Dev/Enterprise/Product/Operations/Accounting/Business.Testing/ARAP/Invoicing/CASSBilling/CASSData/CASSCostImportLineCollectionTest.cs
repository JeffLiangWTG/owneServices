using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.CASSBilling.CASSData
{
	[TestedType(typeof(CASSCostImportLineCollection))]
	public class CASSCostImportLineCollectionTest : CASSCostLineCollectionTest<CASSCostImportLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CASSCostImportLine(Factory, CASSCostLineType.Default);
		}

		protected override CASSCostImportLineCollection GetCollectionToTest()
		{
			return new CASSCostImportLineCollection();
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CASSCostImportLineCollection);
		}
	}
}
