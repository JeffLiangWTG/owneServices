
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(AdditionalProcedureCodeForm))]
	class AdditionalProcedureCodeFormTest : ZFormBasherTest
	{
		public void TestAdditionalProcedureCodeStringOnClose()
		{
			var bill = Factory.New<AsycudaBill>();
			using (var form = new AdditionalProcedureCodeFormForTest(bill))
			{
				var initialItem = bill.AdditionalProcedureCodes.AddNew();
				initialItem.CY_Code = "zzz";
				form.Show();
				var newItem = bill.AdditionalProcedureCodes.AddNew();
				newItem.CY_Code = "yyy";
				AssertEquals("Should have 2 while editing", 2, bill.AdditionalProcedureCodes.Count);

				form.Close();
				AssertEquals("Should update additional procedure code string", "yyy,zzz", bill.AdditionalProcedureCodesAsString);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new AdditionalProcedureCodeForm(bill);
		}

		protected class AdditionalProcedureCodeFormForTest : AdditionalProcedureCodeForm
		{
			public AdditionalProcedureCodeFormForTest(IAdditionalProcedureParent procedureParent)
				: base(procedureParent)
			{
			}

			public new void OnClosing(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}
	}
}
