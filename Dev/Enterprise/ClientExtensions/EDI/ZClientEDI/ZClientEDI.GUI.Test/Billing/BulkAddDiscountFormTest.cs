using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(BulkAddDiscountForm))]
	public class BulkAddDiscountFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizO = new BulkAddDiscountBizO(System.Array.Empty<BusinessObject>());
			return new BulkAddDiscountForm(bizO);
		}
	}
}
