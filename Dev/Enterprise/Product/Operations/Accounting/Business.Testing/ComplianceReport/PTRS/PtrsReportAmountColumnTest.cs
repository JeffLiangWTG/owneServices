using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportAmountColumn))]
	public class PtrsReportAmountColumnTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValueProperty()
		{
			var column = GetNewBusinessObject() as PtrsReportAmountColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);

			column.Value = 100.11m;
			AssertEquals(100.11m, column.Value);
		}

		public void TestIHandleValueChanged()
		{
			var column = GetNewBusinessObject() as PtrsReportAmountColumn;
			AssertNotNull(column);

			var handlesValueChanged = column as IHandleValueChanged;
			AssertNotNull(handlesValueChanged);

			Assert(!handlesValueChanged.IsValueChangedHandlerAttached);
			handlesValueChanged.AttachValueChangedHandlerOnlyOnce(new EventHandler(delegate(object sender, EventArgs e) { }));
			Assert(handlesValueChanged.IsValueChangedHandlerAttached);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var column = Factory.NewWithValidTestData<AccTaxReturnColumn>();
			return new PtrsReportAmountColumn(column);
		}
	}
}
