using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(SumARegisterForm))]
	class SumARegisterFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			using (var form = new SumARegisterForm(header))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("No Reference", "SumA Register Form", form.FormCaption);
					header.SRH_Reference = "ATB150002110520195876";
					AssertEquals("Reference Entered", "SumA Register Form - AT/B/15/000211/05/2019/5876", form.FormCaption);
				});
			}
		}

		public void TestCheckOnShowPreSaveDialogs()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			using (var form = new SumARegisterForm(header))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.FireSaveButton();
				AssertEquals("At least one line should be entered.", UnitTestUserNotification.Instance.LastMessage.Text);

				var line = header.CusTempStorageRegLines.AddNew();
				line.SRL_LocationOfGoods = "CN";
				line.SRL_LimitDate = ZDateTime.Now.Date;
				line.SRL_LineNumber = 1;

				var transaction = line.CusTempStorageRegLineTransactions.AddNew();
				transaction.SRT_InternalReferenceNumber = "T001";
				transaction.SRT_PackageQty = 10;
				transaction.SRT_GrossWeight = 10m;
				transaction.SRT_PackageQty = 100;

				form.FireSaveButton();
				AssertEquals("Transactions cannot be amended once saved. Do you want to continue saving the transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			Factory.Save();
			var result = new SumARegisterForm(header);
			result.ControllerID = ControllerIDs.Customs.DE.SumARegister;
			return result;
		}
	}
}
