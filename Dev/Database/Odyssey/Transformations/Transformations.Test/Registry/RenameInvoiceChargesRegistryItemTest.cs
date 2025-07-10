using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RenameInvoiceChargesRegistryItem))]
	class RenameInvoiceChargesRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("InvoiceCharges"));
			AssertEquals(1, Helper.GetStmDataRowCount("InvoiceChargesForExport"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameInvoiceChargesRegistryItem();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("InvoiceCharges", Guid.Empty, Guid.Empty, true);
		}
	}
}
