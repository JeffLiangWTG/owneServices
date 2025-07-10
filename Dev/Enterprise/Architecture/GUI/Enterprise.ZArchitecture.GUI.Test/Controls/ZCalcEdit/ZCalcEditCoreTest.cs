using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalcEditCoreTest : TestCaseWithFactory
	{
		public void TestSettingBindToTypeForString()
		{
			AssertExceptionThrown<NotSupportedException>("ZCalcEditCore does not support the type <CargoWise.Types.ZString>.", () => CalcEditCore.SetBindToType(typeof(ZString)));
		}

		public void TestMinAndMaxValueSetter_ZDecimal()
		{
			CombineAssertions(() =>
			{
				CalcEditCore.SetBindToType(typeof(ZDecimal));
				AssertEquals("Default Min Value", decimal.MinValue, CalcEditCore.MinValue);
				AssertEquals("Default Max Value", decimal.MaxValue, CalcEditCore.MaxValue);
				CalcEditCore.MaxValue = 100m;
				AssertEquals("Max Value Set", 100m, CalcEditCore.MaxValue);
				CalcEditCore.MaxValue = 0m;
				AssertEquals("Max Setting 0 returns Default", decimal.MaxValue, CalcEditCore.MaxValue);
			});
		}

		public void TestMinAndMaxValueSetter_ZInt()
		{
			CombineAssertions(() =>
			{
				CalcEditCore.SetBindToType(typeof(ZInt));
				AssertEquals("Default Min Value", (decimal)int.MinValue, CalcEditCore.MinValue);
				AssertEquals("Default Max Value", (decimal)int.MaxValue, CalcEditCore.MaxValue);
				CalcEditCore.MaxValue = 9000m;
				AssertEquals("Max Value Set", 9000m, CalcEditCore.MaxValue);
				CalcEditCore.MaxValue = 0m;
				AssertEquals("Max Setting 0 returns Default", (decimal)int.MaxValue, CalcEditCore.MaxValue);
			});
		}

		public void TestMinAndMaxValueSetter_ZLong()
		{
			CombineAssertions(() =>
			{
				CalcEditCore.SetBindToType(typeof(ZLong));
				AssertEquals("Default Min Value", (decimal)long.MinValue, CalcEditCore.MinValue);
				AssertEquals("Default Max Value", (decimal)long.MaxValue, CalcEditCore.MaxValue);
				CalcEditCore.MaxValue = 100000m;
				AssertEquals("Max Value Set", 100000m, CalcEditCore.MaxValue);
				CalcEditCore.MaxValue = 0m;
				AssertEquals("Max Setting 0 returns Default", (decimal)long.MaxValue, CalcEditCore.MaxValue);
			});
		}

		public void TestUpdateTextOnDecimalsChanged()
		{
			Assert(CalcEditCore.UpdateTextOnDecimalsChanged);
			AssertEquals(2, CalcEditCore.Decimals);
			CalcEditCore.CalcValue = 3.1415m;
			AssertEquals("3.14", CalcEditCore.TextBox.Text);

			CalcEditCore.Decimals = 4;
			AssertEquals("3.1400", CalcEditCore.TextBox.Text);

			CalcEditCore.UpdateTextOnDecimalsChanged = false;
			CalcEditCore.Decimals = 2;
			AssertEquals("3.1400", CalcEditCore.TextBox.Text);
		}

		public void TestPressDotKeyWhenCalcEditReadOnly()
		{
			CalcEdit.Text = "5";
			CalcEdit.ReadOnly = true;
			CalcEdit.Focus();
			CalcEdit.SendKey('.');
			AssertNotEquals("0.", CalcEdit.Text);
			AssertEquals("5", CalcEdit.Text);
		}

		public void TestDoNothingWhenDataSourceIsNull()
		{
			AssertNoExceptionThrown("Do nothing when dataSource is null", () => CalcEditCore.ShowUnitConverter(null, ""));
			AssertExceptionThrown(typeof(ArgumentException), "DataSource must be BusinessObject, DataSoureType : System.String", () => CalcEditCore.ShowUnitConverter("", ""));
		}

		#region Format

		public void TestFormatStringWithDecimalsBoundToZInt()
		{
			AssertFormatStringBasedOnBindToType(typeof(ZInt));
		}

		public void TestFormatStringWithDecimalsBoundToZShort()
		{
			AssertFormatStringBasedOnBindToType(typeof(ZShort));
		}

		public void TestFormatStringWithDecimalsBoundToZByte()
		{
			AssertFormatStringBasedOnBindToType(typeof(ZByte));
		}

		public void TestFormatStringWithDecimalsBoundToZLong()
		{
			AssertFormatStringBasedOnBindToType(typeof(ZLong));
		}

		public void TestFormat()
		{
			// With default culture (Aussie-based)
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				CalcEditCore.Decimals = 2;

				AssertEquals("0.00", CalcEditCore.Format(null));
				AssertEquals("0.00", CalcEditCore.Format(DBNull.Value));
				AssertEquals("0.00", CalcEditCore.Format("junk"));
				AssertEquals("0.00", CalcEditCore.Format("1" + decimal.MaxValue.ToString()));

				AssertEquals("1.00", CalcEditCore.Format((byte)1));
				AssertEquals("1.00", CalcEditCore.Format(1));
				AssertEquals("1.00", CalcEditCore.Format(1f));
				AssertEquals("1.00", CalcEditCore.Format(1d));
				AssertEquals("1.00", CalcEditCore.Format(1m));
				AssertEquals("1.00", CalcEditCore.Format("1"));
				AssertEquals("1.00", CalcEditCore.Format('1'));

				CalcEditCore.Decimals = 4;
				AssertEquals(decimal.MinValue.ToString() + ".0000", CalcEditCore.Format(decimal.MinValue));
				AssertEquals(decimal.MaxValue.ToString() + ".0000", CalcEditCore.Format(decimal.MaxValue));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				CalcEditCore.Decimals = 2;

				AssertEquals("0,00", CalcEditCore.Format(null));
				AssertEquals("0,00", CalcEditCore.Format(DBNull.Value));
				AssertEquals("0,00", CalcEditCore.Format("junk"));
				AssertEquals("0,00", CalcEditCore.Format("1" + decimal.MaxValue.ToString()));

				AssertEquals("1,00", CalcEditCore.Format((byte)1));
				AssertEquals("1,00", CalcEditCore.Format(1));
				AssertEquals("1,00", CalcEditCore.Format(1f));
				AssertEquals("1,00", CalcEditCore.Format(1d));
				AssertEquals("1,00", CalcEditCore.Format(1m));
				AssertEquals("1,00", CalcEditCore.Format("1"));
				AssertEquals("1,00", CalcEditCore.Format('1'));

				CalcEditCore.Decimals = 4;
				AssertEquals(decimal.MinValue.ToString() + ",0000", CalcEditCore.Format(decimal.MinValue));
				AssertEquals(decimal.MaxValue.ToString() + ",0000", CalcEditCore.Format(decimal.MaxValue));
			}
		}

		public void TestFormatWithDecimalPlacesOverload()
		{
			// With default culture (Aussie-based)
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				AssertEquals("0.000", CalcEditCore.Format(null, 3));
				AssertEquals("0.0", CalcEditCore.Format(DBNull.Value, 1));
				AssertEquals("0", CalcEditCore.Format("junk", 0));
				AssertEquals("0.00000", CalcEditCore.Format("1" + decimal.MaxValue.ToString(), 5));

				AssertEquals("1.000", CalcEditCore.Format((byte)1, 3));
				AssertEquals("1.345", CalcEditCore.Format(1.3445, 3));
				AssertEquals("1.300", CalcEditCore.Format(1.3f, 3));
				AssertEquals("1.0000", CalcEditCore.Format(1d, 4));
				AssertEquals("1.0000", CalcEditCore.Format(1m, 4));
				AssertEquals("1.0000", CalcEditCore.Format("1", 4));
				AssertEquals("1", CalcEditCore.Format('1', 0));

				AssertEquals(decimal.MinValue.ToString() + ".00000", CalcEditCore.Format(decimal.MinValue, 5));
				AssertEquals(decimal.MaxValue.ToString() + ".00", CalcEditCore.Format(decimal.MaxValue, 2));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				AssertEquals("0,000", CalcEditCore.Format(null, 3));
				AssertEquals("0,0", CalcEditCore.Format(DBNull.Value, 1));
				AssertEquals("0", CalcEditCore.Format("junk", 0));
				AssertEquals("0,00000", CalcEditCore.Format("1" + decimal.MaxValue.ToString(), 5));

				AssertEquals("1,000", CalcEditCore.Format((byte)1, 3));
				AssertEquals("1,345", CalcEditCore.Format(1.3445, 3));
				AssertEquals("1,300", CalcEditCore.Format(1.3f, 3));
				AssertEquals("1,0000", CalcEditCore.Format(1d, 4));
				AssertEquals("1,0000", CalcEditCore.Format(1m, 4));
				AssertEquals("1,0000", CalcEditCore.Format("1", 4));
				AssertEquals("1", CalcEditCore.Format('1', 0));

				AssertEquals(decimal.MinValue.ToString() + ",00000", CalcEditCore.Format(decimal.MinValue, 5));
				AssertEquals(decimal.MaxValue.ToString() + ",00", CalcEditCore.Format(decimal.MaxValue, 2));
			}
		}

		public void TestFormatWithBindTypeIsNullAble()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				CalcEditCore.SetBindToType(typeof(ZDecimal?));
				AssertEquals(null, CalcEditCore.Format(null, 3));
			}
		}

		public void TestParseWithBindTypeIsNullAble()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				CalcEditCore.SetBindToType(typeof(ZDecimal));
				AssertEquals(0M, CalcEditCore.Parse(""));

				CalcEditCore.SetBindToType(typeof(ZDecimal?));
				AssertEquals(null, CalcEditCore.Parse(""));
			}
		}

		public void TestParse()
		{
			AssertEquals(0m, CalcEditCore.Parse("junk"));
			AssertEquals(0m, CalcEditCore.Parse("1" + decimal.MaxValue.ToString()));

			AssertEquals(1m, CalcEditCore.Parse((byte)1));
			AssertEquals(1m, CalcEditCore.Parse(1));
			AssertEquals(1m, CalcEditCore.Parse(1f));
			AssertEquals(1m, CalcEditCore.Parse(1d));
			AssertEquals(1m, CalcEditCore.Parse(1m));
			AssertEquals(1m, CalcEditCore.Parse("1"));
			AssertEquals(1m, CalcEditCore.Parse('1'));

			AssertEquals(decimal.MinValue, CalcEditCore.Parse(decimal.MinValue.ToString()));
			AssertEquals(decimal.MaxValue, CalcEditCore.Parse(decimal.MaxValue.ToString()));
		}

		#endregion

		#region FormatFordd
		public void TestFormatForShowEmptyStringForEmptyValueisTrue()
		{
			CalcEdit.ShowEmptyStringForEmptyValue = true;
			// With default culture (Aussie-based)
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				CalcEditCore.Decimals = 2;

				AssertEquals("0.00", CalcEditCore.Format(null));
				AssertEquals("0.00", CalcEditCore.Format(DBNull.Value));
				AssertEquals("", CalcEditCore.Format("junk"));
				AssertEquals("", CalcEditCore.Format("1" + decimal.MaxValue.ToString()));

				AssertEquals("1.00", CalcEditCore.Format((byte)1));
				AssertEquals("1.00", CalcEditCore.Format(1));
				AssertEquals("1.00", CalcEditCore.Format(1f));
				AssertEquals("1.00", CalcEditCore.Format(1d));
				AssertEquals("1.00", CalcEditCore.Format(1m));
				AssertEquals("1.00", CalcEditCore.Format("1"));
				AssertEquals("1.00", CalcEditCore.Format('1'));

				CalcEditCore.Decimals = 4;
				AssertEquals(decimal.MinValue.ToString() + ".0000", CalcEditCore.Format(decimal.MinValue));
				AssertEquals(decimal.MaxValue.ToString() + ".0000", CalcEditCore.Format(decimal.MaxValue));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				CalcEditCore.Decimals = 2;

				AssertEquals("0,00", CalcEditCore.Format(null));
				AssertEquals("0,00", CalcEditCore.Format(DBNull.Value));
				AssertEquals("", CalcEditCore.Format("junk"));
				AssertEquals("", CalcEditCore.Format("1" + decimal.MaxValue.ToString()));

				AssertEquals("1,00", CalcEditCore.Format((byte)1));
				AssertEquals("1,00", CalcEditCore.Format(1));
				AssertEquals("1,00", CalcEditCore.Format(1f));
				AssertEquals("1,00", CalcEditCore.Format(1d));
				AssertEquals("1,00", CalcEditCore.Format(1m));
				AssertEquals("1,00", CalcEditCore.Format("1"));
				AssertEquals("1,00", CalcEditCore.Format('1'));

				CalcEditCore.Decimals = 4;
				AssertEquals(decimal.MinValue.ToString() + ",0000", CalcEditCore.Format(decimal.MinValue));
				AssertEquals(decimal.MaxValue.ToString() + ",0000", CalcEditCore.Format(decimal.MaxValue));
			}
		}

		public void TestFormatWithDecimalPlacesOverloadForShowEmptyStringForEmptyValueisTrue()
		{
			CalcEdit.ShowEmptyStringForEmptyValue = true;
			// With default culture (Aussie-based)
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				AssertEquals("0.000", CalcEditCore.Format(null, 3));
				AssertEquals("0.0", CalcEditCore.Format(DBNull.Value, 1));
				AssertEquals("", CalcEditCore.Format("junk", 0));
				AssertEquals("", CalcEditCore.Format("1" + decimal.MaxValue.ToString(), 5));

				AssertEquals("1.000", CalcEditCore.Format((byte)1, 3));
				AssertEquals("1.345", CalcEditCore.Format(1.3445, 3));
				AssertEquals("1.300", CalcEditCore.Format(1.3f, 3));
				AssertEquals("1.0000", CalcEditCore.Format(1d, 4));
				AssertEquals("1.0000", CalcEditCore.Format(1m, 4));
				AssertEquals("1.0000", CalcEditCore.Format("1", 4));
				AssertEquals("1", CalcEditCore.Format('1', 0));

				AssertEquals(decimal.MinValue.ToString() + ".00000", CalcEditCore.Format(decimal.MinValue, 5));
				AssertEquals(decimal.MaxValue.ToString() + ".00", CalcEditCore.Format(decimal.MaxValue, 2));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				AssertEquals("0,000", CalcEditCore.Format(null, 3));
				AssertEquals("0,0", CalcEditCore.Format(DBNull.Value, 1));
				AssertEquals("", CalcEditCore.Format("junk", 0));
				AssertEquals("", CalcEditCore.Format("1" + decimal.MaxValue.ToString(), 5));

				AssertEquals("1,000", CalcEditCore.Format((byte)1, 3));
				AssertEquals("1,345", CalcEditCore.Format(1.3445, 3));
				AssertEquals("1,300", CalcEditCore.Format(1.3f, 3));
				AssertEquals("1,0000", CalcEditCore.Format(1d, 4));
				AssertEquals("1,0000", CalcEditCore.Format(1m, 4));
				AssertEquals("1,0000", CalcEditCore.Format("1", 4));
				AssertEquals("1", CalcEditCore.Format('1', 0));

				AssertEquals(decimal.MinValue.ToString() + ",00000", CalcEditCore.Format(decimal.MinValue, 5));
				AssertEquals(decimal.MaxValue.ToString() + ",00", CalcEditCore.Format(decimal.MaxValue, 2));
			}
		}

		public void TestFormatWithBindTypeIsNullAbleForShowEmptyStringForEmptyValueisTrue()
		{
			CalcEdit.ShowEmptyStringForEmptyValue = true;
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				CalcEditCore.SetBindToType(typeof(ZDecimal?));
				AssertEquals(null, CalcEditCore.Format(null, 3));
			}
		}
		#endregion

		#region Friendly Column Name

		public void TestFriendlyColumnName()
		{
			BindCalcEditCore(DummyBizoSchema.Constants.Z0_Decimal);
			AssertEquals("ZCalcEditCore.FriendlyColumnName", "Decimal", CalcEditCore.FriendlyColumnName);

			BindCalcEditCore("Dependents." + DummyDependentBizoSchema.Constants.ZD1_Number);
			AssertEquals("ZCalcEditCore.FriendlyColumnName", "DummyDependentBizo|ZD1_Number", CalcEditCore.FriendlyColumnName);

			var t = typeof(ZCalcEditCore);
			using (var textBox = new TestTextBox())
			{
				using (var core = Activator.CreateInstance(t, new object[] { textBox }) as ZCalcEditCore)
				{
					var friendlyColumnName = t.GetProperty("FriendlyColumnName");
					AssertNoExceptionThrown(() => friendlyColumnName.GetValue(core)); //there was an exception
				}
			}
		}

		public void TestFriendlyColumnNameInGrid()
		{
			var info =
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = DummyDependentBizoSchema.Constants.ZD1_Number
				};
			form.Grid.ColumnStyles.Add(info);

			form.Grid.BindTo = "Dependents";
			form.Grid.SetDataBinding(Dummy, form.Grid.BindTo, DummyDependentBizoSchema.Constants.TableName);

			var column = form.Grid.Columns[DummyDependentBizoSchema.Constants.ZD1_Number];
			var style = (ZCalcEditColumnStyle)column.ColumnStyle;
			var coreZ = style.Core;

			AssertEquals("ZCalcEditCore.FriendlyColumnName", "DummyDependentBizo|ZD1_Number", coreZ.FriendlyColumnName.Trim((char)32, (char)31));
		}

		#endregion

		#region Sending Decimal Separator

		public void TestSendingDecimalMinMax()
		{
			using (var form = new Form())
			{
				var textBoxForTabbingTo = new TextBox();
				form.Controls.Add(textBoxForTabbingTo);
				form.Controls.Add(CalcEdit);
				form.Show();

				CalcEditCore.Decimals = 2;

				CalcEdit.Text = "";
				CalcEdit.Focus();
				foreach (var c in decimal.MinValue.ToString())
				{
					CalcEdit.SendKey(c);
				}
				CalcEdit.TabOut();
				AssertEquals(decimal.MinValue.ToString() + ".00", CalcEdit.Text);

				CalcEdit.Text = "";
				CalcEdit.Focus();
				foreach (var c in decimal.MaxValue.ToString())
				{
					CalcEdit.SendKey(c);
				}
				CalcEdit.TabOut();
				AssertEquals(decimal.MaxValue.ToString() + ".00", CalcEdit.Text);
			}
		}

		public void TestSendingDecimalSeparator()
		{
			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				const string numberWithPeriod = "10.25";
				var numberWithNationalSeparator = "10" + EnvProxy.Instance.CurrentCompany.Country.Culture.NumberFormat.NumberDecimalSeparator + "25";

				using (var form = new Form())
				{
					var textBoxForTabbingTo = new TextBox();
					form.Controls.Add(textBoxForTabbingTo);
					form.Controls.Add(CalcEdit);
					form.Show();

					CalcEdit.Text = "";
					CalcEdit.Focus();
					foreach (var c in numberWithNationalSeparator)
					{
						CalcEdit.SendKey(c);
					}
					CalcEdit.TabOut();
					AssertEquals("Should accept national decimal separator", numberWithNationalSeparator, CalcEdit.Text);

					CalcEdit.Text = "";
					CalcEdit.Focus();
					foreach (var c in numberWithPeriod)
					{
						CalcEdit.SendKey(c);
					}
					CalcEdit.TabOut();
					AssertEquals("Should also accept decimal point regardless and convert it to national decimal separator", numberWithNationalSeparator, CalcEdit.Text);
				}
			}
		}

		public void TestSendingNegativeDecimalSeparator()
		{
			using (var form = new Form())
			{
				var textBoxForTabbingTo = new TextBox();
				form.Controls.Add(textBoxForTabbingTo);
				form.Controls.Add(CalcEdit);
				form.Show();

				CalcEdit.Text = "";
				CalcEdit.Focus();
				foreach (var c in "-.45")
				{
					CalcEdit.SendKey(c);
				}
				CalcEdit.TabOut();
				AssertEquals("Should accept number with negative sign then decimal", "-0.45", CalcEdit.Text);

				CalcEdit.Text = "";
				CalcEdit.Focus();
				foreach (var c in ".67")
				{
					CalcEdit.SendKey(c);
				}
				CalcEdit.TabOut();
				AssertEquals("Should accept number with decimal", "0.67", CalcEdit.Text);
			}
		}

		#endregion

		#region Sending Hyphen

		public void TestAllowNegative()
		{
			CalcEditCore.SetBindToType(typeof(ZDecimal));

			using (var form = new Form())
			{
				var textBoxForTabbingTo = new TextBox();
				form.Controls.Add(textBoxForTabbingTo);
				form.Controls.Add(CalcEdit);
				form.Show();

				CalcEdit.AllowNegative = true;
				CalcEdit.Text = "";
				CalcEdit.Focus();
				foreach (var c in "-1")
				{
					CalcEdit.SendKey(c);
				}
				CalcEdit.TabOut();
				AssertEquals("Should allow negative numbers", "-1.00", CalcEdit.Text);

				CalcEdit.AllowNegative = false;
				CalcEdit.Text = "";
				CalcEdit.Focus();
				foreach (var c in "-1")
				{
					CalcEdit.SendKey(c);
				}
				CalcEdit.TabOut();
				AssertEquals("Should not allow negative numbers", "1.00", CalcEdit.Text);
			}
		}

#if !WINZOR
		public void TestPasteAndAllowNegative()
		{
			CalcEditCore.SetBindToType(typeof(ZDecimal));

			using (var form = new Form())
			{
				var textBoxForTabbingTo = new TextBox();
				form.Controls.Add(textBoxForTabbingTo);
				form.Controls.Add(CalcEdit);
				form.Show();

				CalcEdit.AllowNegative = true;
				CalcEdit.Text = "";
				CalcEdit.Focus();
				CalcEdit.Paste("-12.56678");
				CalcEdit.TabOut();
				AssertEquals("Should allow negative numbers", "-12.57", CalcEdit.Text);

				CalcEdit.AllowNegative = false;
				CalcEdit.Text = "";
				CalcEdit.Focus();
				CalcEdit.Paste("-16.78");
				CalcEdit.TabOut();
				AssertEquals("Should not allow negative numbers", "16.78", CalcEdit.Text);
			}
		}
#endif

		#endregion

		#region Properties

		public void TestAllowNull()
		{
			// change made that even when AllowNull == true, should still return 0M.
			Assert("Initially", CalcEditCore.AllowNull);

			CalcEdit.Text = "";
			AssertEquals(0M, CalcEditCore.CalcValue);

			CalcEdit.Text = "blah";
			AssertEquals(0M, CalcEditCore.CalcValue);

			CalcEdit.Text = "0";
			AssertEquals(0M, CalcEditCore.CalcValue);

			CalcEditCore.AllowNull = false;
			Assert(!CalcEditCore.AllowNull);

			CalcEdit.Text = "";
			AssertEquals(0M, CalcEditCore.CalcValue);

			CalcEdit.Text = "blah";
			AssertEquals(0M, CalcEditCore.CalcValue);

			CalcEdit.Text = "0";

			AssertEquals(0M, CalcEditCore.CalcValue);
		}

		public void TestCalcValue()
		{
			// With default culture (Aussie-based)
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				AssertEquals("Initially", 0M, CalcEditCore.CalcValue);

				CalcEditCore.Decimals = 3;
				CalcEdit.Text = GlobaliseNumberString("0.000");
				AssertEquals(0M, CalcEditCore.CalcValue);

				CalcEdit.Text = "blah";
				AssertEquals(0M, CalcEditCore.CalcValue);

				CalcEdit.Text = GlobaliseNumberString("123.456");
				AssertEquals(123.456M, CalcEditCore.CalcValue);

				CalcEditCore.Decimals = 2;
				CalcEditCore.CalcValue = GlobaliseNumberString("123.4567");
				AssertEquals(GlobaliseNumberString("123.46"), CalcEdit.Text);
				AssertEquals(GlobaliseNumberString("123.46"), ((decimal)CalcEditCore.CalcValue).ToString(Culture.CurrentCompanyCountryCulture));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				AssertEquals("Initially", 0M, CalcEditCore.CalcValue);

				CalcEditCore.Decimals = 3;
				CalcEdit.Text = GlobaliseNumberString("0.000");
				AssertEquals(0M, CalcEditCore.CalcValue);

				CalcEdit.Text = "blah";
				AssertEquals(0M, CalcEditCore.CalcValue);

				CalcEdit.Text = GlobaliseNumberString("123.456");
				AssertEquals(123.456M, CalcEditCore.CalcValue);

				CalcEditCore.Decimals = 2;
				CalcEditCore.CalcValue = GlobaliseNumberString("123.4567");
				AssertEquals(GlobaliseNumberString("123.46"), CalcEdit.Text);
				AssertEquals(GlobaliseNumberString("123.46"), ((decimal)CalcEditCore.CalcValue).ToString(Culture.CurrentCompanyCountryCulture));
			}
		}

		public void TestDecimals()
		{
			CalcEditCore.Decimals = 4;
			CalcEditCore.CalcValue = 1.1234;
			AssertEquals(1.1234m, CalcEditCore.CalcValue);
			AssertEquals(GlobaliseNumberString("1.1234"), CalcEdit.Text);
			CalcEditCore.Decimals = 2;
			AssertEquals(GlobaliseNumberString("1.12"), CalcEdit.Text);

			CalcEditCore.Decimals = 9;
			CalcEditCore.CalcValue = 1.12345678912;
			AssertEquals(GlobaliseNumberString("1.123456789"), CalcEdit.Text);
		}

		[ExpectException(typeof(FormatException))]
		public void TestTooManyDecimals()
		{
			CalcEditCore.Decimals = 10;
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Dummy = Factory.New<DummyWithDependentsBusinessObject>();
			Dummy.Dependents.AddNew();

			form = new TestForm();
			CalcEdit = new TestTextBox { ShowGroupSeparators = false, ShowEmptyStringForEmptyValue = false };
			CalcEditCore = new ZCalcEditCore(CalcEdit);
			form.Controls.Add(CalcEdit);

			form.Show();
		}

		protected override void TearDown()
		{
			form.Close();
			UnbindCalcEditCore();
			CalcEditCore.TextBox.Dispose();
			CalcEditCore.Dispose();
			form.Dispose();

			base.TearDown();
		}

		void BindCalcEditCore(string bindTo)
		{
			DataBoundControl.Get(CalcEditCore.TextBox).SetDataBinding(Dummy, bindTo);
		}

		void UnbindCalcEditCore()
		{
			DataBoundControl.Get(CalcEditCore.TextBox).SetDataBinding(null, "");
		}

		void AssertFormatStringBasedOnBindToType(Type bindToType)
		{
			CalcEditCore.SetBindToType(bindToType);
			AssertEquals("200", CalcEditCore.Format("200.23", 2));
		}

		static string GlobaliseNumberString(string number)
		{
			return number.Replace(".", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator);
		}

		DummyWithDependentsBusinessObject Dummy;
		TestForm form;
		TestTextBox CalcEdit;
		ZCalcEditCore CalcEditCore;

		class TestTextBox : ZCalcEdit
		{
			public void TabOut()
			{
				KeySender.PostKeyDown(this, Handle, Keys.Tab);
				Application.DoEvents();
			}

			public void SendKey(char key)
			{
				KeySender.SendKeyPress(this, Handle, key);
			}
		}

		#endregion
	}
}
