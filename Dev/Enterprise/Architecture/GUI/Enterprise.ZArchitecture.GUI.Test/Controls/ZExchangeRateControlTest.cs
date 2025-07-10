using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZExchangeRateControlTest : TestCaseWithFactory
	{
		public void TestReadOnly()
		{
			using (var form = new ZFormWithExchangeRate(RateDummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				RateDummy.ExchangeRate.Currency = "USD";
				RateDummy.ExchangeRate.IsRateReadOnly = false;
				RateDummy.ExchangeRate.Currency_ReadOnly = false;

				AssertEquals("RateCalcEdit not read only", false, form.ExchangeRateControl.RateCalcEdit.ReadOnly);
				RateDummy.ExchangeRate.IsRateReadOnly = true;
				AssertEquals("RateCalcEdit read only", true, form.ExchangeRateControl.RateCalcEdit.ReadOnly);
				AssertEquals("ExchangeRateControl is not read only when one of Currency or Rate are not", false, form.ExchangeRateControl.ReadOnly);

				AssertEquals("CurrencyFindBox not read only", false, form.ExchangeRateControl.CurrencyFindBox.ReadOnly);
				RateDummy.ExchangeRate.Currency_ReadOnly = true;
				AssertEquals("CurrencyFindBox read only", true, form.ExchangeRateControl.CurrencyFindBox.ReadOnly);
				AssertEquals("ExchangeRateControl is read only when both Currency and Rate are read only", true, form.ExchangeRateControl.ReadOnly);
			}
		}

		public void TestBindsSuccessfully()
		{
			using (var form = new ZFormWithExchangeRate(RateDummy))
			{
				form.Show();
				AssertNotNull(form.ExchangeRateControl);
			}
		}

		public void TestInvalidCurrencyShowsError()
		{
			using (var form = new ZFormWithExchangeRate(RateDummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				form.ExchangeRateControl.CurrencyFindBox.CodeBox.Text = "ZUB";
				form.MyButton.Focus();

				AssertEquals("Value should still be 'ZUB'.", "ZUB", form.ExchangeRateControl.CurrencyFindBox.CodeBox.Text);
			}
		}

		#region IDataBoundControl

		public void TestDataSourceType()
		{
			using (var control = new ZExchangeRateControl())
			{
				AssertEquals(typeof(ZExchangeRate), control.DataSourceType);
			}
		}

		#endregion

		#region Test Classes

		class ZFormWithExchangeRate : ZChildForm
		{
			public ZFormWithExchangeRate(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			public ZExchangeRateControl ExchangeRateControl;
			public Button MyButton;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				ExchangeRateControl = new ZExchangeRateControl();
				ExchangeRateControl.BindTo = "ExchangeRate";
				ExchangeRateControl.Location = new Point(20, 20);
				ExchangeRateControl.Width = 150;
				Controls.Add(ExchangeRateControl);

				MyButton = new Button();
				MyButton.Location = new Point(20, 50);
				Controls.Add(MyButton);

				Size = new Size(200, 200);
			}
		}

		#endregion

		#region Implementation

		DummyBusinessObject Dummy;

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = (DummyBusinessObject)Factory.New(TypeOfDummy);
			AssertNotNull("Dummy should be created!", Dummy);
			RateDummy = Factory.New<DummyWithExchangeRateBusinessObject>();
			AssertNotNull("RateDummy should be created.", RateDummy);
		}

		public void DeleteAllDummies()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();
		}

		protected virtual Type TypeOfDummy { get { return typeof(DummyBusinessObject); } }

		DummyWithExchangeRateBusinessObject RateDummy;

		public DummyZExchangeRate ExchangeRate
		{
			get { return RateDummy.ExchangeRate; }
		}

		#endregion
	}
}
