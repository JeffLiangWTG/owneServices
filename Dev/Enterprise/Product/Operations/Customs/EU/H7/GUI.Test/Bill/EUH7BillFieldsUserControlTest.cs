using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7BillFieldsUserControl))]
	sealed class EUH7BillFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumberTextBox()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var localReferenceNumberTextBox = control.FindSingle<ZTextBox>("LocalReferenceNumberTextBox");

				AssertEquals("BindingMember", "LocalReferenceNumber", localReferenceNumberTextBox.GetBindingMember());
			}
		}

		public void TestMovementReferenceNumberTextBox()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var movementReferenceNumberTextBox = control.FindSingle<ZTextBox>("MovementReferenceNumberTextBox");

				AssertEquals("BindingMember", "MovementReferenceNumber", movementReferenceNumberTextBox.GetBindingMember());
			}
		}

		public void TestAdditionalProcedureDropEdit()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var additionalProcedureDropEdit = control.FindSingle<ZDropEdit>("AdditionalProcedureDropEdit");

				AssertEquals("BindingMember", "ABL_Procedure", additionalProcedureDropEdit.GetBindingMember());
			}
		}

		public void TestMessageStatusDropEdit()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var messageStatusDropEdit = control.FindSingle<ZDropEdit>("MessageStatusDropEdit");

				AssertEquals("BindingMember", "ABL_MessageStatus", messageStatusDropEdit.GetBindingMember());
			}
		}

		public void TestAdditionalProcedureCodesUserControl()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var additionalProcedureCodesUserControl = control.FindSingle<AdditionalProcedureCodesUserControl>("AdditionalProcedureCodesUserControl");

				AssertType<AdditionalProcedureCodesUserControl>(additionalProcedureCodesUserControl);
			}
		}

		public void TestContainerUserControl()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var containerUserControl = control.FindSingle<EUH7ContainerUserControl>("ContainerUserControl");

				AssertNotNull(containerUserControl);
			}
		}

		public void TestGoodsValueConvertToLocalCurrencyControl()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var goodsValueConvertToLocalCurrencyControl =
					control.FindSingle<ConvertToLocalCurrencyControl>(c => c.Name == "GoodsValueConvertToLocalCurrencyControl");

				CombineAssertions("", () =>
				{
					AssertEquals("BindToAmount", "ABL_GoodsValue",
						goodsValueConvertToLocalCurrencyControl.BindToAmount);
					AssertEquals("BindToUnit", "ABL_RX_NKGoodsValueCurrency",
						goodsValueConvertToLocalCurrencyControl.BindToUnit);
					AssertEquals("Decimals", 2, goodsValueConvertToLocalCurrencyControl.Decimals);
				});
			}
		}
	}
}
