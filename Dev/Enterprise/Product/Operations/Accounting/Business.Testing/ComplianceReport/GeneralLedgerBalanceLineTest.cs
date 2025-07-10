using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(GeneralLedgerBalanceLine))]
	public class GeneralLedgerBalanceLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new GeneralLedgerBalanceLine(Factory, GetDataRow(Factory));
		}

		internal static DataRow GetDataRow(BusinessObjectFactory factory)
		{
			var report = factory.NewWithValidTestData<AccComplianceReport>();
			var table = GeneralLedgerBalanceLine.GetDataTable(report);
			var result = table.NewRow();

			result[GeneralLedgerBalanceLine.Schema.PK] = Guid.NewGuid();
			table.Rows.Add(result);
			result.AcceptChanges();

			return result;
		}
	}
}
