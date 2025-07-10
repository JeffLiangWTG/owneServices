using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZMoneyLabelTest : ZNumericLabelTest
	{
		public virtual void TestBindToCurrecySymbolStringProperty()
		{
			ZLabelTestO.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			ZLabelTestO.BindToCurrencySymbol = "";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("Should be no prefix", ZLabelTestO.Text, TestBizO.Z0_Decimal.ToString(ZLabelTestO.Decimals));

			ZLabelTestO.BindToCurrencySymbol = "Z0_Code";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("Should be a prefix", TestBizO.Z0_Code + TestBizO.Z0_Decimal.ToString(ZLabelTestO.Decimals), ZLabelTestO.Text);

			ZLabelTestO.BindTo = DummyBusinessObject.Schema.Z0_Description;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("Should be empty string", "", ZLabelTestO.Text);

			TestBizO.Z0_Code = ZString.Empty;
			ZLabelTestO.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("Should be no prefix", TestBizO.Z0_Decimal.ToString(ZLabelTestO.Decimals), ZLabelTestO.Text);
		}

		#region Implementation

		protected override ZLabelBase GetNewLabel()
		{
			return new ZMoneyLabel();
		}

		new ZMoneyLabel ZLabelTestO
		{
			get { return base.ZLabelTestO as ZMoneyLabel; }
			set { base.ZLabelTestO = value; }
		}

		#endregion
	}
}
