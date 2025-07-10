using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Matching.Testing
{
	[TestedType(typeof(PayLinesForm))]
	internal class PayLinesFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PayLinesForm(new InvoicingBasePayLineMediator(new APMatchingBase(Factory), Factory.NewWithValidTestData<APInvoice>()));
		}

		public void TestChargeTypeColumnIsOnlyShownForAR()
		{
			using (PayLinesForm form = new PayLinesForm(new InvoicingBasePayLineMediator(new APMatchingBase(Factory), Factory.New<APInvoice>())))
			{
				form.Show();
				Assert("Charge Type column shouldn't be shown for AP", !IsColumnExists(form.InvoiceLinesGrid_ForTestOnly, BaseCharge.Schema.ChargeType));
			}

			using (PayLinesForm form = new PayLinesForm(new InvoicingBasePayLineMediator(new ARMatchingBase(Factory), Factory.New<ARInvoice>())))
			{
				form.Show();
				Assert("Charge Type column should be shown for AR", IsColumnExists(form.InvoiceLinesGrid_ForTestOnly, BaseCharge.Schema.ChargeType));
			}
		}

		bool IsColumnExists(ZGrid grid, string columnName)
		{
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.ColumnName == columnName)
				{
					return true;
				}
			}
			return false;
		}
	}
}
