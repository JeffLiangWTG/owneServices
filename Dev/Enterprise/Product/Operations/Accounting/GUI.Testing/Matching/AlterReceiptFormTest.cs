using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AlterReceiptForm))]
	public class AlterReceiptFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var result = new AlterReceiptForm(Factory.New<APReceipt>());
			result.ControllerID = ControllerIDs.ZAPReceipt;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			APPay = Factory.NewWithValidTestData<APReceipt>();
		}

		APReceipt GetAPReceiptWithValidTestData()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper();
			helper.SetupPeriods();

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			APReceipt receipt = Factory.NewWithValidTestData<APReceipt>();

			receipt.AH_OH = org.PK;
			receipt.AH_AB = bank.PK;

			receipt.AH_OSExTaxAmount = 10M;
			receipt.AH_ChequeOrReference = "9";
			return receipt;
		}

		APReceipt APPay;

		#endregion

		public void TestValidateAndSaveDoesNotSave()
		{
			APPay = GetAPReceiptWithValidTestData();
			using (AlterReceiptForm payForm = new AlterReceiptForm(APPay))
			{
				Assert("Precondition: Receipt is not already in Database", !APPay.IsInDatabase);
				payForm.ValidateAndSave_ForTestOnly();
				Assert("Receipt should not be in database", !APPay.IsInDatabase);
			}
		}
	}
}
