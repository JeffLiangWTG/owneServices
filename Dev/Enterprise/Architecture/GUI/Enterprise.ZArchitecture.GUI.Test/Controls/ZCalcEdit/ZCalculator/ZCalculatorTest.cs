using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalculatorTest : TestCaseWithFactory
	{
		public void TestBindExtraFieldsDoesNotBlowUp()
		{
			BindCalcEditCore(DummyBizoSchema.Constants.Z0_Decimal); // the textbox will always be bound before constructing the calculator
			using (var calc = new ZCalculatorForTesting(core))
			{
				AssertEquals("This assertion is to ensure BindExtraFields() is tested in case it is moved out of the constructor.", true, calc.IsBindExtraFieldsTested);
			}
		}

		public void TestBindExtraFieldsInGridDoesNotBlowUp()
		{
			var core = GetNewBoundGridColumnCore(DummyDependentBizoSchema.Constants.ZD1_Number);
			using (var calc = new ZCalculatorForTesting(core))
			{
				AssertEquals("This assertion is to ensure BindExtraFields() is tested in case it is moved out of the constructor.", true, calc.IsBindExtraFieldsTested);
			}
		}

		public void TestSetCurrentValue()
		{
			var core = GetNewBoundGridColumnCore(DummyDependentBizoSchema.Constants.ZD1_Number);
			using (var calc = new ZCalculatorForTesting(core))
			{
				var privateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

				var currentValueInfo = typeof(ZCalculator).GetField("currentValue", privateInstanceFlags);
				var calcTextBox = (ZCalculatorTextBox)typeof(ZCalculator).GetField("ValueTextBox", privateInstanceFlags).GetValue(calc);

				calc.SetCurrentValue("10");
				AssertEquals(10m, currentValueInfo.GetValue(calc));
				AssertEquals("10", calcTextBox.Text);

				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
				{
					calc.SetCurrentValue("10.00000");
					AssertEquals(10m, currentValueInfo.GetValue(calc));
					AssertEquals("10", calcTextBox.Text);

					calc.SetCurrentValue("10.000001");
					AssertEquals(10.000001m, currentValueInfo.GetValue(calc));
					AssertEquals("10.000001", calcTextBox.Text);
				}

				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
				{
					calc.SetCurrentValue("10,00000");
					AssertEquals(10m, currentValueInfo.GetValue(calc));
					AssertEquals("10", calcTextBox.Text);

					calc.SetCurrentValue("10,000001");
					AssertEquals(10.000001m, currentValueInfo.GetValue(calc));
					AssertEquals("10,000001", calcTextBox.Text);
				}

				calc.SetCurrentValue("");
				AssertEquals(0m, currentValueInfo.GetValue(calc));
				AssertEquals("0", calcTextBox.Text);
			}
		}

		public void TestZAcceptButtonClick()
		{
			form.Grid.SetDataBinding(null, "");

			var info = new ZCalcEditColumnStyleInfo();
			info.ColumnName = DummyDependentBizoSchema.Constants.ZD1_Number;
			info.Decimals = 2;
			form.Grid.ColumnStyles.Add(info);

			form.Grid.BindTo = "Dependents";
			form.Grid.SetDataBinding(dummy, form.Grid.BindTo, DummyDependentBizoSchema.Constants.TableName);

			var column = form.Grid.Columns[DummyDependentBizoSchema.Constants.ZD1_Number];
			var style = (ZCalcEditColumnStyle)column.ColumnStyle;
			var calEditCore = style.Core;

			using (var calc = calEditCore.Calculator)
			{
				var privateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

				var currentValueInfo = typeof(ZCalculator).GetField("currentValue", privateInstanceFlags);
				var calcTextBox = (ZCalculatorTextBox)typeof(ZCalculator).GetField("ValueTextBox", privateInstanceFlags).GetValue(calc);

				calc.SetCurrentValue("10.12346465464");
				calc.Show();
				((ZButton)calc.Controls["AcceptZButton"]).PerformClick();
				AssertEquals("Text should be rounded", "10.12", calEditCore.TextBox.Text);
			}
		}

		public void TestCorrectResultForAcceptAfterPercent()
		{
			var calcEdit = new ZCalcEdit();
			core.Dispose();
			core = new ZCalcEditCoreForTesting(calcEdit);
			form.Controls.Add(calcEdit);
			form.Grid.SetDataBinding(null, "");

			var info = new ZCalcEditColumnStyleInfoForTesting();
			info.ColumnName = DummyDependentBizoSchema.Constants.ZD1_Number;
			form.Grid.ColumnStyles.Add(info);

			form.Grid.BindTo = "Dependents";
			form.Grid.SetDataBinding(dummy, form.Grid.BindTo, DummyDependentBizoSchema.Constants.TableName);

			var column = form.Grid.Columns[DummyDependentBizoSchema.Constants.ZD1_Number];
			var style = (ZCalcEditColumnStyleForTesting)column.ColumnStyle;
			var calEditCore = style.Core;

			using (var calc = (ZCalculatorForTesting)((ZCalcEditCoreForTesting)calEditCore).Calculator)
			{
				calc.Show();
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad1);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleOperatorKeyPress_Exposed(Keys.Multiply);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad2);
				calc.HandlePercentage_Exposed();
				((ZButton)calc.Controls["AcceptZButton"]).PerformClick();
				AssertEquals("Value should reflect percent of input", "2", calEditCore.TextBox.Text);

				calc.Reset();
				calc.Show();
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad1);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleOperatorKeyPress_Exposed(Keys.Multiply);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad2);
				calc.HandlePercentage_Exposed();
				calc.HandleEqualsKeyPress_Exposed();
				((ZButton)calc.Controls["AcceptZButton"]).PerformClick();
				AssertEquals("Value should reflect percent of input", "200", calEditCore.TextBox.Text);

				calc.Reset();
				calc.Show();
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad1);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleOperatorKeyPress_Exposed(Keys.Multiply);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad2);
				calc.HandleEqualsKeyPress_Exposed();
				((ZButton)calc.Controls["AcceptZButton"]).PerformClick();
				AssertEquals("Value should reflect percent of input", "20", calEditCore.TextBox.Text);

				calc.Reset();
				calc.Show();
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad1);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleOperatorKeyPress_Exposed(Keys.Divide);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad5);
				calc.HandlePercentage_Exposed();
				calc.HandleEqualsKeyPress_Exposed();
				((ZButton)calc.Controls["AcceptZButton"]).PerformClick();
				AssertEquals("Value should reflect percent of input", "2000", calEditCore.TextBox.Text);

				calc.Reset();
				calc.Show();
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad1);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad0);
				calc.HandleOperatorKeyPress_Exposed(Keys.Divide);
				calc.HandleNumberKeyPress_Exposed(Keys.NumPad5);
				calc.HandlePercentage_Exposed();
				((ZButton)calc.Controls["AcceptZButton"]).PerformClick();
				AssertEquals("Value should reflect percent of input", "2000", calEditCore.TextBox.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestSetCurrentValue_InvalidValue()
		{
			using (var core = GetNewBoundGridColumnCore(DummyDependentBizoSchema.Constants.ZD1_Number))
			using (var calc = new ZCalculatorForTesting(core))
			{
				calc.SetCurrentValue("TEST"); // this would previously blow up as TextAsDecimal was called without CanTextBeParsedToDecimal
			}
		}

		public void TestGetLocationForDualMonitors()
		{
			if (CachedScreenInfo.Instance.ScreenInfos.Length < 2)
			{
				Assert("Run this test only on machines with 2 or more monitors", true);
			}

			using (var core = GetNewBoundGridColumnCore(DummyDependentBizoSchema.Constants.ZD1_Number))
			using (var calc = new ZCalculatorForTesting(core))
			{
				foreach (var screenInfo in CachedScreenInfo.Instance.ScreenInfos)
				{
					core.TextBox.Location = new Point(screenInfo.Left + screenInfo.Width / 2, screenInfo.Top + screenInfo.Height / 2);
					var calcLocation = calc.GetLocationExposed();
					Assert("Should popup on same monitor", screenInfo.Contains(calcLocation));
				}
			}
		}

		public void TestCalculatorInGridMarkOwnerForm()
		{
			form.Grid.SetDataBinding(null, "");
			var info = new ZCalcEditColumnStyleInfo();
			info.ColumnName = DummyDependentBizoSchema.Constants.ZD1_Number;
			form.Grid.ColumnStyles.Add(info);
			form.Grid.BindTo = "Dependents";
			form.Grid.SetDataBinding(dummy, form.Grid.BindTo, DummyDependentBizoSchema.Constants.TableName);

			KeySender.PostKeyDown(form.Grid.LastFocusedColumn.EditControl, Keys.F4);
			Application.DoEvents();

			var column = form.Grid.Columns[DummyDependentBizoSchema.Constants.ZD1_Number];
			var style = (ZCalcEditColumnStyle)column.ColumnStyle;

			AssertEquals("ZTestForm", style.Core.Calculator.Owner?.GetType().Name);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			dummy = Factory.New<DummyWithDependentsBusinessObject>();
			dummy.Dependents.AddNew();

			form = new ZTestForm();
			var calcEdit = new ZCalcEdit();
			core = new ZCalcEditCore(calcEdit);
			form.Controls.Add(calcEdit);

			form.Grid.ColumnStyles.Clear();

			form.Show();
		}

		protected override void TearDown()
		{
			form.Close();
			UnbindCalcEditCore();
			core.TextBox.Dispose();
			core.Dispose();
			form.Dispose();

			base.TearDown();
		}

		void BindCalcEditCore(string bindTo)
		{
			UnbindCalcEditCore();
			DataBoundControl.Get(core.TextBox).SetDataBinding(dummy, bindTo);
		}

		void UnbindCalcEditCore()
		{
			DataBoundControl.Get(core.TextBox).SetDataBinding(null, "");
		}

		ZCalcEditCore GetNewBoundGridColumnCore(string columnName)
		{
			form.Grid.SetDataBinding(null, "");

			var info = new ZCalcEditColumnStyleInfo();
			info.ColumnName = columnName;
			form.Grid.ColumnStyles.Add(info);

			form.Grid.BindTo = "Dependents";
			form.Grid.SetDataBinding(dummy, form.Grid.BindTo, DummyDependentBizoSchema.Constants.TableName);

			var column = form.Grid.Columns[DummyDependentBizoSchema.Constants.ZD1_Number];
			var style = (ZCalcEditColumnStyle)column.ColumnStyle;
			var result = style.Core;

			return result;
		}

		DummyWithDependentsBusinessObject dummy;
		ZTestForm form;
		ZCalcEditCore core;

		#endregion

		#region class ZCalculator for Testing

		class ZCalculatorForTesting : ZCalculator
		{
			public ZCalculatorForTesting(ZCalcEditCore core)
				: base(core)
			{
			}

			protected override void BindExtraFields()
			{
				base.BindExtraFields();
				bindExtraFieldsTested = true;
			}

			public bool IsBindExtraFieldsTested
			{
				get { return bindExtraFieldsTested; }
			}

			bool bindExtraFieldsTested;

			public Point GetLocationExposed()
			{
				return GetLocation();
			}

			public void HandleNumberKeyPress_Exposed(Keys numberKey)
			{
				HandleNumberKeyPress(numberKey);
			}

			public void HandleOperatorKeyPress_Exposed(Keys operatorKey)
			{
				HandleOperatorKeyPress(operatorKey);
			}

			public void HandleEqualsKeyPress_Exposed()
			{
				HandleEqualsKeyPress();
			}

			public void HandlePercentage_Exposed()
			{
				HandlePercentage();
			}
		}

		#endregion

		#region

		class ZCalcEditCoreForTesting : ZCalcEditCore
		{
			public ZCalcEditCoreForTesting(TextBox editor) : base(editor) { }

			public ZCalcEditCoreForTesting(TextBox editor, ZCalcEditColumnStyleForTesting columnStyle) : base(editor, columnStyle) { }

			public override ZCalculator Calculator
			{
				get
				{
					if (calculator == null)
					{
						calculator =
							new ZCalculatorForTesting(this)
							{
								ShowExtraButtons = showExtraButtons,
								Extra1LabelText = extra1LabelText,
								Extra2LabelText = extra2LabelText
							};
						calculator.CalculationComplete += Calculator_CalculationComplete;
					}

					return calculator;
				}
			}
		}

		class ZCalcEditColumnStyleForTesting : ZCalcEditColumnStyle
		{
			public ZCalcEditColumnStyleForTesting(ZCalcEditColumnStyleInfoForTesting columnInfo) : base(columnInfo) { }

			protected override ZCalcEditCore GetNewCalcEditCore()
			{
				return new ZCalcEditCoreForTesting(TextBox, this);
			}
		}

		class ZCalcEditColumnStyleInfoForTesting : ZCalcEditColumnStyleInfo
		{
			public override Type ColumnStyleType
			{
				get { return typeof(ZCalcEditColumnStyleForTesting); }
			}
		}

		#endregion
	}
}
