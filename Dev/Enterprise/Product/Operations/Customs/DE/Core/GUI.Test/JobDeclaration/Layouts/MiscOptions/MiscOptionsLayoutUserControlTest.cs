using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class MiscOptionsLayoutUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestVATAccountNumberDropEdit()
		{
			var vatAccountNumberDropEdit = control.VATAccountNumberDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", vatAccountNumberDropEdit);
				AssertEquals("BindTo", nameof(JobDeclaration.ZG_VATDeferNumber), vatAccountNumberDropEdit.BindTo);
			});
		}

		public void TestVatPaymentPartyDropEdit()
		{
			var vatPaymentPartyDropEdit = control.VatPaymentPartyDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", vatPaymentPartyDropEdit);
				AssertEquals("BindTo", nameof(JobDeclaration.ZG_VATDeferType), vatPaymentPartyDropEdit.BindTo);
			});
		}

		public void TestDutyAccountNumberDropEdit()
		{
			var dutyAccountNumberDropEdit = control.DutyAccountNumberDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", dutyAccountNumberDropEdit);
				AssertEquals("BindTo", nameof(JobDeclaration.JE_DefermentAccountNumber), dutyAccountNumberDropEdit.BindTo);
			});
		}

		public void TestVATClaimBackDropEdit()
		{
			var vatClaimBackDropEdit = control.VATClaimBackDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", vatClaimBackDropEdit);
				AssertEquals("BindTo", nameof(JobDeclaration.JE_VATClaimBack), vatClaimBackDropEdit.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, vatClaimBackDropEdit.CharacterCasing);
			});
		}

		public void TestStatisticStatusDropEdit()
		{
			var statisticStatusDropEdit = control.StatisticStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", statisticStatusDropEdit);
				AssertEquals("BindTo", nameof(JobDeclaration.JE_StatisticStatus), statisticStatusDropEdit.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new MiscOptionsLayoutUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		MiscOptionsLayoutUserControl control;
	}
}
