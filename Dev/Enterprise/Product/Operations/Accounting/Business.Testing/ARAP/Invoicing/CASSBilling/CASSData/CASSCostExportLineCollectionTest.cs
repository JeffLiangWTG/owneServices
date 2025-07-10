using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CASSCostExportLineCollection))]
	public class CASSCostExportLineCollectionTest : CASSCostLineCollectionTest<CASSCostExportLineCollection>
	{
		protected override CASSCostExportLineCollection GetCollectionToTest()
		{
			return new CASSCostExportLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CASSCostExportLine(Factory, CASSCostLineType.Default);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CASSCostExportLineCollection);
		}
	}
}
