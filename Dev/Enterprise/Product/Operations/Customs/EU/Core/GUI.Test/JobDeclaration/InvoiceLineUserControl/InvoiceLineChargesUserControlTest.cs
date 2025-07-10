using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class InvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceChargesGrid_HasApplicableForStatisticalValueColumn()
		{
			using (var form = new ZForm())
			using (var userControl = new InvoiceLineChargesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var column = userControl.ChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == Business.Declaration.InvoiceCharge.Schema.J7_IsStatisticalValueApplicable);
				AssertNotNull("Statistical value applicable column must be available", column);
			}
		}

		public void TestBaseGroupChargesGrid_HasApplicableForStatisticalValueColumn()
		{
			using (var form = new ZForm())
			using (var userControl = new InvoiceLineChargesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var column = userControl.ApportionedChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == Business.Declaration.InvoiceCharge.Schema.J7_IsStatisticalValueApplicable);
				AssertNotNull("Statistical value applicable column must be available", column);
			}
		}

		public void TestColumnTypes()
		{
			using (var uc = new InvoiceLineChargesUserControl())
			{
				var map = new Dictionary<string, Type>();
				map.Add("J7_ExchangeRate", typeof(ZCalcEditColumnStyleInfo));
				map.Add("IsJ7_ExchangeRateUserEnterable", typeof(ZCheckBoxColumnStyleInfo));

				foreach (var colName in map.Keys)
				{
					var columnStyle = (from ZGridColumnInfo col in uc.ApportionedChargesGrid.ColumnStyles.ToArray() where col.ColumnName == colName select col).FirstOrDefault();
					Type t = map[colName];
					AssertType(string.Format("ApportionedChargesGrid column of name {0} should have a style of type {1}", colName, t.Name), t, columnStyle);

					columnStyle = (from ZGridColumnInfo col in uc.ChargesGrid.ColumnStyles.ToArray() where col.ColumnName == colName select col).FirstOrDefault();
					t = map[colName];
					AssertType(string.Format("InvoiceChargesGrid column of name {0} should have a style of type {1}", colName, t.Name), t, columnStyle);
				}
			}
		}
	}
}
