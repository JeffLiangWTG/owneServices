using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalcFindBoxTest : ZControlBaseTestCase<ZCalcFindBox>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "IsVisibleForBinding" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Text"; }
		}

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		public void TestBinding()
		{
			var child = Factory.New<DummyChildBusinessObject>();
			child.Z0_Code = "CHILD";

			using (var testForm = new CurrencyTestForm(Dummy))
			{
				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				testForm.CalcFindBox.UnitFindBox.CodeBox.Focus();
				testForm.CalcFindBox.UnitFindBox.CodeBox.Text = "CHILD";

				testForm.TextBox.Focus();
				Application.DoEvents();

				AssertEquals("FindBox value set to BizObj", Dummy.Z0_Guid, child.PK);

				testForm.CalcFindBox.AmountCalcEdit.Focus();
				testForm.CalcFindBox.AmountCalcEdit.Text = "50" + Core.Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator + "5";

				testForm.TextBox.Focus();
				Application.DoEvents();

				AssertEquals("CalcEdit value set to BizObj", new ZDecimal(50.5M), Dummy.Z0_AnotherDecimal);
			}
		}

		public void TestChangingBindToUnitCauseNoProblem()
		{
			var dummyWithCurrency = new DummyWithCurrency
			{
				DMM_Amount = 100,
				DMM_RX_NKCurrency = "USD",
			};

			using (var form = new ZForm(dummyWithCurrency))
			{
				var calcFindBox = new ZCalcFindBox();
				calcFindBox.BindToAmount = "DMM_Amount";
				calcFindBox.BindToUnit = "DMM_RX_NKCurrency";

				form.Controls.Add(calcFindBox);

				form.Show();

				calcFindBox.BindToUnit = "";

				AssertNoExceptionThrown(() => calcFindBox.BindToUnit = "DMM_RX_NKCurrency");
			}
		}

		public void TestFindBoxType()
		{
			using (var calcFindBox = new ZCalcFindBox())
			{
				calcFindBox.PopupCaption = "Test Caption";
				calcFindBox.PreBoundMaxLength = 5;

				AssertEquals("PreBoundMaxLength set through to UnitFindBox", 5, calcFindBox.UnitFindBox.PreBoundMaxLength);
				AssertEquals("PopupCaption set through to UnitFindBox", "Test Caption", calcFindBox.UnitFindBox.PopupCaption);
				AssertEquals("CalcFindBox FindBoxType default", FindBoxType.Guid, calcFindBox.FindBoxType);
				Assert("CalcFindBox UnitFindBox instantiated type", calcFindBox.UnitFindBox is ZGuidFindBox);

				calcFindBox.FindBoxType = FindBoxType.Code;
				AssertEquals("PreBoundMaxLength set through to UnitFindBox", 5, calcFindBox.UnitFindBox.PreBoundMaxLength);
				AssertEquals("PopupCaption set through to UnitFindBox", "Test Caption", calcFindBox.UnitFindBox.PopupCaption);
				AssertEquals("CalcFindBox FindBoxType after set", FindBoxType.Code, calcFindBox.FindBoxType);
				Assert("CalcFindBox UnitFindBox instantiated type after setting FindBoxType to Code", (calcFindBox.UnitFindBox as ZGuidFindBox) == null);
			}
		}

		public void TestReadOnly()
		{
			var child = Factory.New<DummyChildBusinessObject>();
			using (var form = new CurrencyTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertEquals("ExchangeRateControl is not read only when one of guid or decimal are not", false, form.CalcFindBox.ReadOnly);
				Dummy.Z0_Guid_ReadOnly = true;
				Dummy.Z0_AnotherDecimal_ReadOnly = true;
				Application.DoEvents();
				AssertEquals("ExchangeRateControl is read only when both guid and decimal are read only", true, form.CalcFindBox.ReadOnly);
			}
		}

		protected override void BindControl()
		{
			Control.BindToAmount = DummyBusinessObject.Schema.Z0_Decimal;
			Control.BindToUnit = DummyBusinessObject.Schema.Z0_Guid;
			Control.BindToList = "Collection";
			Control.SetDataBinding(Dummy, DummyBusinessObject.Schema.Z0_Decimal);
		}

		class DummyWithCurrency
		{
			public int DMM_Amount { get; set; }
			public string DMM_RX_NKCurrency { get; set; }
		}
	}
}
