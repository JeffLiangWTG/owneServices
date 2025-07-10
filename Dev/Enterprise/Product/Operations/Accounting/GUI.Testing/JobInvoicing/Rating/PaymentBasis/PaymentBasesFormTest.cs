using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	[TestedType(typeof(PaymentBasesForm))]
	public class PaymentBasesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_Desc = "Freight";

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = chargeCode.PK;

			var collection = new JobPaymentBasisViewCollection(Factory);
			var basis = collection.AddNew();
			basis.PBS_RX_NKRateCurrency = "AUD";
			basis.PBS_ChargeableAmount = 5;
			basis.PBS_ChargeableUnit = "KG";
			basis.PBS_PerUnitRate = 6m;
			basis.PBS_RateUnit = "KG";
			basis.PBS_AdapterID = "SHP001";
			basis.PBS_JR = charge.PK;
			basis.PBS_ChargeableDescription = "Desc";

			return new PaymentBasesForm(collection, true);
		}
	}
}
