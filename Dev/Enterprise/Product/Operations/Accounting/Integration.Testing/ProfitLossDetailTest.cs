using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	[TestedType(typeof(ProfitLossDetail))]
	public class ProfitLossDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DataRow row = new DataTable().Rows.Add(Array.Empty<object>());
			return new ProfitLossDetail(Factory, row);
		}

		public void TestDecimals()
		{
			ProfitLossDetail profitLossDetail = (ProfitLossDetail)GetNewBusinessObject();
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Decimals, profitLossDetail.Decimals);
		}
	}
}
