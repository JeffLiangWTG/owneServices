using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportNumberColumn))]
	public class PtrsReportNumberColumnTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValueProperty()
		{
			var column = GetNewBusinessObject() as PtrsReportNumberColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);

			column.Value = 111;
			AssertEquals(111, column.Value);
		}

		public void TestIHandleValueChanged()
		{
			var column = GetNewBusinessObject() as PtrsReportNumberColumn;
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
			return new PtrsReportNumberColumn(column);
		}
	}
}
