using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(TransactionNumberSettingForm))]
	sealed class TransactionNumberSettingFormTest : ZFormBasherTest
	{
		public void TestSave()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "GB1";
			branch.GB_GC = company.PK;

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			Factory.Save();

			var bo = new TransactionNumberSettingBO(Factory);

			using (var form = new TransactionNumberSettingFormForTest(bo))
			{
				form.Show();

				AssertEquals("There is no existing number fountains", 0, bo.ExistingTransactionNumberSettingCollection.Count);
				Assert("there are several available ranges", bo.AvailableTransactionNumberSettingCollection.Count > 2);

				form.SelectAvailable("CA1 12345 IMP GB1");
				form.MoveToExistingButton.PerformClick();

				form.SelectAvailable("CA1 12345 DIF");
				form.MoveToExistingButton.PerformClick();

				form.SelectExisting("CA1 12345 IMP GB1");
				EnterValue(form.NextNumberCalcEdit, "30000005");
				EnterValue(form.MinNumberCalcEdit, "30000000");
				EnterValue(form.MaxNumberCalcEdit, "30999999");

				form.SelectExisting("CA1 12345 DIF");
				EnterValue(form.NextNumberCalcEdit, "30000005");
				EnterValue(form.MinNumberCalcEdit, "30000000");
				EnterValue(form.MaxNumberCalcEdit, "30999999");

				form.SelectExisting("CA1 12345 IMP GB1");
				form.MoveToAvailable();

				form.FireSaveButton();
			}

			TransactionNumberSetting impSetting = new TransactionNumberSetting(Factory, "Test", "12345", branch, "IMP", true);
			TransactionNumberSetting difSetting = new TransactionNumberSetting(Factory, "Test", "12345", null, "DIF", true);

			AssertEquals(false, impSetting.IsInitialized);
			AssertEquals(true, difSetting.IsInitialized);
			AssertEquals((ZDecimal)30000000, difSetting.CurrentMinNumber);
			AssertEquals((ZDecimal)30000005, difSetting.CurrentNextNumber);
			AssertEquals((ZDecimal)30999999, difSetting.CurrentMaxNumber);
		}

		protected override Form GetFormToBashCore() => new TransactionNumberSettingForm(new TransactionNumberSettingBO(Factory));

		void EnterValue(ZCalcEdit edit, string value)
		{
			Application.DoEvents();
			edit.Focus();
			edit.Text = value;
			edit.FindForm().GetNextControl(edit, true).Focus();
			Application.DoEvents();
		}

		sealed class TransactionNumberSettingFormForTest : TransactionNumberSettingForm
		{
			public TransactionNumberSettingFormForTest(TransactionNumberSettingBO transactionNumbersBO) : base(transactionNumbersBO)
			{
			}

			internal void SelectExisting(string rangeName)
			{
				int idx = TransactionNumberSettings.ExistingTransactionNumberSettingCollection.OfType<TransactionNumberSetting>().ToList().FindIndex(s => s.RangeName == rangeName);
				if (idx < 0)
				{
					throw new Exception($"Cannot find range with name '{rangeName}'.");
				}
				BindingContext[DataSource, nameof(TransactionNumberSettingBO.ExistingTransactionNumberSettingCollection)].Position = idx;
			}

			internal void SelectAvailable(string rangeName)
			{
				int idx = TransactionNumberSettings.AvailableTransactionNumberSettingCollection.OfType<TransactionNumberSetting>().ToList().FindIndex(s => s.RangeName == rangeName);
				if (idx < 0)
				{
					throw new Exception($"Cannot find range with name '{rangeName}'.");
				}
				BindingContext[DataSource, nameof(TransactionNumberSettingBO.AvailableTransactionNumberSettingCollection)].Position = idx;
			}

			internal void MoveToAvailable()
			{
				MoveToAvailableButton.PerformClick();
			}

			TransactionNumberSettingBO TransactionNumberSettings => (TransactionNumberSettingBO)DataSource;
		}
	}
}
