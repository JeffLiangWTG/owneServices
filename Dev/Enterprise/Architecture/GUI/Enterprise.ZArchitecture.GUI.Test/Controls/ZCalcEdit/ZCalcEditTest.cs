using System;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalcEditTest : ZControlBaseTestCase<ZCalcEdit>
	{
		public void TestZCalcEditReportObjectDisposedException()
		{
			using (Db.DisposableActionForDbConnection())
			using (var form = new ZChildForm { Name = "FormTest" })
			using (var panel = new ZPanel { Name = "PanelTest" })
			using (var zCalcEdit = new ZCalcEdit())
			{
				form.Controls.Add(panel);
				zCalcEdit.Dispose();
				panel.Controls.Add(zCalcEdit);
				AssertExceptionThrown(typeof(ObjectDisposedException), () => form.Show());

				var expectedMessage = @"Control [name:'' type:'Enterprise.ZArchitecture.ZCalcEdit'] is already disposed.
Wish you can find out why this control has been disposed with the help of the below dispose information.
If you want to see the details of Dispose Information below, please open TrackDisposedAccess in this control.
------------Dispose Information Begin------------
Control Path: FormTest : PanelTest :  (ZCalcEdit)
Disposed Control Path:  (ZCalcEdit)
Control Dispose stack trace:
   at CargoWise.Windows.UI.KTextBox.Dispose(Boolean disposing)
   at Enterprise.ZArchitecture.ZTextBox.Dispose(Boolean disposing)
   at Enterprise.ZArchitecture.ZCalcEdit.Dispose(Boolean isNotFinalizing)";

				AssertEquals(typeof(ObjectDisposedException), ErrorReporter.LastExceptionReported.GetType());
				AssertStartsWith("Exception Message Should Start With", expectedMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestRangeNotSetForDecimal()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.Subtract);
				Application.DoEvents();
				AssertEquals("Negative Value Allowed in Text", "-", form.TestCalcEdit.Text);
			}
		}

		public void TestInputLongDecimal()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				for (var i = 0; i < 19; i++)
				{
					KeySender.PostKeyDown(form.TestCalcEdit, Keys.D1);
				}
				Application.DoEvents();
				AssertEquals("Should Not Exceed Maxlength", 19, form.TestCalcEdit.Text.Length);
			}
		}

		public void TestForByteWithMaxLengthAttritube()
		{
			var dummy = Factory.New<DummyWithByte>();
			ZCalcEditTestForm.BindTo = DummyWithByte.Schema.Z0_Byte;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				for (var i = 0; i < 10; i++)
				{
					KeySender.PostKeyDown(form.TestCalcEdit, Keys.D1);
				}
				Application.DoEvents();
				AssertEquals("Should Not Exceed Maxlength", 1, form.TestCalcEdit.Text.Length);
			}
		}

		[DeveloperOnlyTest]
		public void TestPastedValueShouldNotExceedMaxValue()
		{
			using (var form = new Form())
			{
				var testCalcEdit = new ZCalcEdit();
				testCalcEdit.Text = "";
				testCalcEdit.MaxValue = 100000;
				form.Controls.Add(testCalcEdit);
				form.Show();

				SafeClipboard.SetDataObject("345879.12");
				System.Threading.Thread.Sleep(50);
				KeySender.SendKeyDownToProcessCmdKey(testCalcEdit, (int)(Keys.Control | Keys.V));
				Application.DoEvents();
				AssertEquals("Paste should not exceed max value", "100000", testCalcEdit.Text);

				testCalcEdit.MaxValue = 90.99m;
				KeySender.SendKeyDownToProcessCmdKey(testCalcEdit, (int)(Keys.Control | Keys.V));
				Application.DoEvents();
				AssertEquals("Paste should not exeed max value decimals", "90.99", testCalcEdit.Text);
			}
		}

		public void TestMaxValueForByte()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Byte;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D2);
				Application.DoEvents();
				AssertEquals("Text", "2", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D5);
				Application.DoEvents();
				AssertEquals("Text", "25", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D6);
				Application.DoEvents();
				AssertEquals("Text", "25", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D5);
				Application.DoEvents();
				AssertEquals("Text", "255", form.TestCalcEdit.Text);
			}
		}

		public void TestMinValueForByte()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Byte;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.Subtract);
				Application.DoEvents();
				AssertEquals("Text", "0", form.TestCalcEdit.Text);
			}
		}

		public void TestMinValueForShort()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Short;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.Subtract);
				Application.DoEvents();
				AssertEquals("Text", "-", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D3);
				Application.DoEvents();
				AssertEquals("Text", "-3", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D2);
				Application.DoEvents();
				AssertEquals("Text", "-32", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D7);
				Application.DoEvents();
				AssertEquals("Text", "-327", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D6);
				Application.DoEvents();
				AssertEquals("Text", "-3276", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D9);
				Application.DoEvents();
				AssertEquals("Text", "-3276", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D8);
				Application.DoEvents();
				AssertEquals("Text", "-32768", form.TestCalcEdit.Text);
			}
		}

		public void TestMaxValueForShort()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Short;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D3);
				Application.DoEvents();
				AssertEquals("Text", "3", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D2);
				Application.DoEvents();
				AssertEquals("Text", "32", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D7);
				Application.DoEvents();
				AssertEquals("Text", "327", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D6);
				Application.DoEvents();
				AssertEquals("Text", "3276", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D8);
				Application.DoEvents();
				AssertEquals("Text", "3276", form.TestCalcEdit.Text);

				KeySender.PostKeyDown(form.TestCalcEdit, Keys.D7);
				Application.DoEvents();
				AssertEquals("Text", "32767", form.TestCalcEdit.Text);
			}
		}

		public void TestBindingDecimalPlacesAfterFormShown_ForInt()
		{
			TestBindingDecimalPlacesAfterFormShown(DummyBizoSchema.Z0_AnotherDecimal.Name, DummyBizoSchema.Z0_Number.Name, 2.123m, (ZInt)2, "2.12");
		}

		public void TestBindingDecimalPlacesAfterFormShown_ForDecimal()
		{
			TestBindingDecimalPlacesAfterFormShown(DummyBizoSchema.Z0_AnotherDecimal.Name, DummyBizoSchema.Z0_Decimal.Name, 2.123m, (ZDecimal)2m, "2.12");
		}

		public void TestBindingDecimalPlacesAfterFormShown_ForByte()
		{
			TestBindingDecimalPlacesAfterFormShown(DummyBizoSchema.Z0_AnotherDecimal.Name, DummyBizoSchema.Z0_Byte.Name, 2.123m, (ZByte)2, "2.12");
		}

		public void TestBindingDecimalPlacesAfterFormShown_ForShort()
		{
			TestBindingDecimalPlacesAfterFormShown(DummyBizoSchema.Z0_AnotherDecimal.Name, DummyBizoSchema.Z0_Short.Name, 2.123m, (ZShort)2, "2.12");
		}

		public void TestBindingDecimalPlacesAfterFormShown_ForLong()
		{
			TestBindingDecimalPlacesAfterFormShown(DummyBizoSchema.Z0_AnotherDecimal.Name, DummyBizoSchema.Z0_Long.Name, 2.123m, (ZLong)2L, "2.12");
		}

		public void TestBindingDecimalPlacesAfterFormShown_ForString()
		{
			TestBindingDecimalPlacesAfterFormShown(DummyBizoSchema.Z0_AnotherDecimal.Name, DummyBizoSchema.Z0_VarCharMax.Name, 2.123m, (ZString)"2", "2.12");
		}

		void TestBindingDecimalPlacesAfterFormShown(string bindTo, string bindToDecimalPlaces, object value, object decimalPlaces, string expectedText)
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var form = new ZForm(dummy))
			using (var calcEdit = new ZCalcEdit())
			{
				calcEdit.BindToDecimalPlaces = bindToDecimalPlaces;
				calcEdit.BindTo = bindTo;
				dummy[bindToDecimalPlaces] = decimalPlaces;
				dummy[bindTo] = value;

				form.Controls.Add(calcEdit);
				form.Show();
				AssertEquals("Expected number as shown on the ZCalcEdit", expectedText, calcEdit.Text);
			}
		}

		protected override void BindControl()
		{
			base.BindControl();

			Control.BindTo = DummyBizoSchema.Z0_Decimal.Name;
			Control.SetDataBinding(Dummy, null);
		}

		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "Text", "ReadOnly", "Decimals", "IsVisibleForBinding" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

#if !WINZOR

		[DeveloperOnlyTest]
		public void TestPaste()
		{
			TestControl.Text = "";
			SafeClipboard.SetDataObject("ABC");
			System.Threading.Thread.Sleep(50); // we need to sleep to allow the clipboard to catch up.
			Assert("TryPaste should return false", !((IPastableControl)TestControl).TryPaste());
			AssertEquals("Paste should do nothing when not a valid number", "", TestControl.Text);

			SafeClipboard.SetDataObject(new TextBox());
			System.Threading.Thread.Sleep(50);
			Assert("TryPaste should return false", !((IPastableControl)TestControl).TryPaste());
			AssertEquals("Paste should do nothing when not a string", "", TestControl.Text);

			SafeClipboard.SetDataObject("123");
			System.Threading.Thread.Sleep(50);
			Assert("TryPaste should return true", ((IPastableControl)TestControl).TryPaste());
			AssertEquals("Should paste valid number", GlobaliseNumberString("123.00"), TestControl.Text);

			SafeClipboard.SetDataObject(GlobaliseNumberString("1.2"));
			System.Threading.Thread.Sleep(50);
			Assert("TryPaste should return true", ((IPastableControl)TestControl).TryPaste());
			AssertEquals("Should paste valid number", GlobaliseNumberString("1.20"), TestControl.Text);

			SafeClipboard.SetDataObject("0");
			System.Threading.Thread.Sleep(50);
			Assert("TryPaste should return true", ((IPastableControl)TestControl).TryPaste());
			AssertEquals("Should paste valid number", GlobaliseNumberString("0.00"), TestControl.Text);

			TestControl.Text = "";
			TestControl.ReadOnly = true;
			SafeClipboard.SetDataObject("51");
			System.Threading.Thread.Sleep(50);
			Assert("TryPaste should return false because control is readonly", !((IPastableControl)TestControl).TryPaste());
			AssertEquals("Control should be empty", "", TestControl.Text);

			TestControl.ReadOnly = false;
		}

#endif

		public void TestZStringToNumberWithOutrangedValue()
		{
			var value = new object();
			value = TestControl.GetType().InvokeMember("StringToZDecimal", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, TestControl, new object[] { ((double)decimal.MaxValue * 2).ToString("N") });
			AssertEquals((ZDecimal)0, value);
			value = TestControl.GetType().InvokeMember("StringToZLong", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, TestControl, new object[] { ((double)long.MaxValue + 1).ToString() });
			AssertEquals((ZLong)0, value);
			value = TestControl.GetType().InvokeMember("StringToZInt", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, TestControl, new object[] { ((double)int.MaxValue + 1).ToString() });
			AssertEquals((ZInt)0, value);
			value = TestControl.GetType().InvokeMember("StringToZShort", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, TestControl, new object[] { ((double)short.MaxValue + 1).ToString() });
			AssertEquals((ZShort)0, value);
			value = TestControl.GetType().InvokeMember("StringToZByte", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, TestControl, new object[] { ((double)byte.MaxValue + 1).ToString() });
			AssertEquals((ZByte)0, value);
		}

		public void TestParseDecimalUsesCorrectCulture()
		{
			var valueToParse = "56,789.123";

			var value = TestControl.GetType().InvokeMember("StringToZDecimal", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, TestControl, new object[] { valueToParse });
			AssertEquals((ZDecimal)56789.123, value);

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-ZA")))
			{
				AssertNoExceptionThrown(() => value = TestControl.GetType().InvokeMember("StringToZDecimal",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, TestControl, new object[] { valueToParse }));
				AssertEquals((ZDecimal)0, value);
			}
		}

		public void TestTextBinding_ParseWithDecimalNullAble()
		{
			var args = new ConvertEventArgs("", typeof(ZDecimal));
			var zCalcEdit = new ZCalcEdit();
			zCalcEdit.GetType().GetMethod("TextBinding_Parse", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(zCalcEdit, new object[] { null, args });

			AssertEquals("it will be 0 when is type is not nullable", 0M, args.Value);

			args = new ConvertEventArgs("", typeof(ZDecimal?));
			zCalcEdit.GetType().GetMethod("TextBinding_Parse", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(zCalcEdit, new object[] { null, args });

			zCalcEdit.Dispose();

			AssertEquals("it will be null when is type is nullable", null, args.Value);
		}

		public void TestCalcValue()
		{
			AssertEquals("Initially", 0M, TestControl.CalcValue);

			TestControl.Decimals = 3;
			TestControl.Text = GlobaliseNumberString("0.000");
			AssertEquals(0M, TestControl.CalcValue);

			TestControl.Text = "blah";
			AssertEquals(0M, TestControl.CalcValue);

			TestControl.Text = GlobaliseNumberString("123.456");
			AssertEquals(123.456M, TestControl.CalcValue);

			TestControl.Decimals = 2;
			TestControl.CalcValue = GlobaliseNumberString("123.4567");
			AssertEquals(GlobaliseNumberString("123.46"), TestControl.Text);
			AssertEquals(123.46M, TestControl.CalcValue);
		}

		string GlobaliseNumberString(string number)
		{
			return number.Replace(".", Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator);
		}

		public void TestBindAndUnbind()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			TestControl.BindTo = DummyBizoSchema.Z0_Decimal.Name;
			AssertEquals("Initially", 0, TestControl.DataBindings.Count);

			TestControl.SetDataBinding(dummy, TestControl.BindTo);
			AssertEquals("After initial binding", 2, TestControl.DataBindings.Count);

			TestControl.SetDataBinding(null, "");
			AssertEquals("After unbinding", 0, TestControl.DataBindings.Count);

			TestControl.SetDataBinding(dummy, TestControl.BindTo);
			AssertEquals("After unbinding then rebinding", 2, TestControl.DataBindings.Count);

			TestControl.SetDataBinding(dummy, TestControl.BindTo);
			AssertEquals("After binding again", 2, TestControl.DataBindings.Count);

			TestControl.SetDataBinding(null, "");
			AssertEquals("After unbinding again", 0, TestControl.DataBindings.Count);
		}

		public void TestSetDataBinding_WillCauculateDecimalsBeforeDataBinding()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_AnotherDecimal = 1.333;
			ZCalcEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_AnotherDecimal;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				AssertEquals("Decimals should not be changed with history 1.33 1.330 1.333", "0.000 1.333 ", form.TestCalcEditWithTextChangeHistory.decimalChangeHistory);
			}
		}

		public void TestReadOnly()
		{
			AssertEquals(false, TestControl.ReadOnly);
			TestControl.ReadOnly = false;
			AssertEquals(false, TestControl.ReadOnly);
			TestControl.ReadOnly = true;
			AssertEquals(true, TestControl.ReadOnly);
			TestControl.ReadOnly = false;
			AssertEquals(false, TestControl.ReadOnly);
		}

		public void TestShowEmptyStringForEmptyValue()
		{
			AssertEquals(false, TestControl.ShowEmptyStringForEmptyValue);
			TestControl.ShowEmptyStringForEmptyValue = false;
			AssertEquals(false, TestControl.ShowEmptyStringForEmptyValue);
			TestControl.ShowEmptyStringForEmptyValue = true;
			AssertEquals(true, TestControl.ShowEmptyStringForEmptyValue);
			TestControl.ShowEmptyStringForEmptyValue = false;
			AssertEquals(false, TestControl.ShowEmptyStringForEmptyValue);
		}

		public void TestCalculatorMarkOwnerForm()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = AutoDummyBizo.Schema.Z0_Decimal;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();
				KeySender.PostKeyDown(form.TestCalcEdit, Keys.F4);
				Application.DoEvents();

				AssertEquals("ZCalcEditTestForm", form.TestCalcEdit.Core.Calculator.Owner?.GetType().Name);
			}
		}

		public void TestDecimalsMoreThan9WillBeTruncatedAndReportError()
		{
			ErrorReporter.Clear();
			var dummy = Factory.New<DummyBusinessObject>();
			ZCalcEditTestForm.BindTo = AutoDummyBizo.Schema.Z0_Decimal;
			using (var form = new ZCalcEditTestForm(dummy))
			{
				form.Text = "Test ZCalcEdit form";
				form.TestCalcEdit.Name = "Test ZCalcEdit";
				form.TestCalcEdit.BindToDecimalPlaces = "test decimal places";
				AssertNoExceptionThrown(() => { form.TestCalcEdit.Decimals = 12; });

				var excepted = @"ZCalcEditCore Cannot Have More Than 9 Decimal Places
Top Level Control type: Enterprise.ZArchitecture.GUI.Testing.ZCalcEditTestForm
Top Level Control Heading: Test ZCalcEdit form
Top Level Control Data Source: CargoWise.EntityFramework.Testing.DummyBusinessObject
Parent Control: Enterprise.ZArchitecture.GUI.Testing.ZCalcEditTestForm
Binding Member: Z0_Decimal
Control Type: Enterprise.ZArchitecture.GUI.Testing.TestZCalcEdit
Control Name: Test ZCalcEdit
BindToDecimals property name: test decimal places
value: 12
";
				AssertEquals(excepted, ErrorReporter.LastMessageReported);
				AssertEquals(9, form.TestCalcEdit.Decimals);
			}
			ErrorReporter.Clear();
		}

		#region Test DecimalPlaces loading from metadata

		public void TestDecimalPlacesLoadingFromMetadata()
		{
			AssertDummyWithDecimals(Factory.New<DummyWithDecimals>(), DummyBizoSchema.Z0_AnotherDecimal.Scale, 5, 6, 6);
			AssertDummyWithDecimals(Factory.New<DummyWithMetaData>(), 4, 5, 6, 6);
			AssertDummyWithDecimals(Factory.New<DummyWithMetaDataProperty>(), 4, 5, 6, 6);

			var dummy = Factory.New<DummyWithSpecialMetaDataProperty>();
			AssertDummyWithDecimals(dummy, 4, 5, 6, 6);

			using (var form = new ZDecimalTestForm(dummy))
			{
				form.Show();
				Application.DoEvents();

				for (var i = 0; i < 20; i++)
				{
					dummy.SuperPuperDecimalPlaces = i % 10;
					AssertEquals("Decimals should not change without RefreshBinding", 4, form.ZCalcEdit1.Decimals);
				}

				for (var i = 0; i < 20; i++)
				{
					dummy.SuperPuperDecimalPlaces = i % 10;
					dummy.RefreshBinding();
					AssertEquals("Decimals should have new value", i % 10, form.ZCalcEdit1.Decimals);
				}
			}
		}

		void AssertDummyWithDecimals(DummyWithDecimals dummy, int raw, int gui, int bound, int guiAndBound)
		{
			using (var form = new ZDecimalTestForm(dummy))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(raw, form.ZCalcEdit1.Decimals);
				AssertEquals(gui, form.ZCalcEditWithGui.Decimals);
				AssertEquals(bound, form.ZCalcEditWithBinding.Decimals);
				AssertEquals(guiAndBound, form.ZCalcEditWithGuiAndBinding.Decimals);
			}
		}

		internal class DummyWithByte : DummyBusinessObject
		{
			public DummyWithByte(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[MaxLength(1)]
			public override ZByte Z0_Byte
			{
				get
				{
					return base.Z0_Byte;
				}

				set
				{
					base.Z0_Byte = value;
				}
			}
		}

		internal class DummyWithDecimals : DummyBusinessObject
		{
			public DummyWithDecimals(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZInt BindableDecimalPlaces
			{
				get { return 6; }
			}

			public ZPropertyInfo BindableDecimalPlacesInfo
			{
				get { return GetZPropertyInfo(nameof(BindableDecimalPlaces)); }
			}
		}

		internal class DummyWithMetaData : DummyWithDecimals
		{
			public DummyWithMetaData(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[DecimalPlaces(4)]
			public override ZDecimal Z0_AnotherDecimal
			{
				get { return base.Z0_AnotherDecimal; }
				set { base.Z0_AnotherDecimal = value; }
			}
		}

		internal class DummyWithMetaDataProperty : DummyWithDecimals
		{
			public DummyWithMetaDataProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected int Z0_AnotherDecimal_DecimalPlaces
			{
				get { return 4; }
			}
		}

		internal class DummyWithSpecialMetaDataProperty : DummyWithDecimals
		{
			public DummyWithSpecialMetaDataProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				SuperPuperDecimalPlaces = 4;
			}

			[DecimalPlaces("SuperPuperDecimalPlaces")]
			public override ZDecimal Z0_AnotherDecimal
			{
				get { return base.Z0_AnotherDecimal; }
				set { base.Z0_AnotherDecimal = value; }
			}

			public int SuperPuperDecimalPlaces
			{
				get { return superPuperDecimalPlaces; }
				set { superPuperDecimalPlaces = value; }
			}
			int superPuperDecimalPlaces;
		}

		class ZDecimalTestForm : ZChildForm
		{
			public ZDecimalTestForm(DummyWithDecimals bizo)
				: base(bizo)
			{
				InitializeComponent();
			}

			public ZCalcEdit ZCalcEdit1;
			public ZCalcEdit ZCalcEditWithGui;
			public ZCalcEdit ZCalcEditWithBinding;
			public ZCalcEdit ZCalcEditWithGuiAndBinding;

			new void InitializeComponent()
			{
				ZCalcEdit1 = new ZCalcEdit { BindTo = "Z0_AnotherDecimal" };
				Controls.Add(ZCalcEdit1);

				ZCalcEditWithGui = new ZCalcEdit { BindTo = "Z0_AnotherDecimal", Decimals = 5 };
				Controls.Add(ZCalcEditWithGui);

				ZCalcEditWithBinding = new ZCalcEdit { BindTo = "Z0_AnotherDecimal", BindToDecimalPlaces = "BindableDecimalPlaces" };
				Controls.Add(ZCalcEditWithBinding);

				ZCalcEditWithGuiAndBinding = new ZCalcEdit { BindTo = "Z0_AnotherDecimal", Decimals = 5, BindToDecimalPlaces = "BindableDecimalPlaces" };
				Controls.Add(ZCalcEditWithGuiAndBinding);
			}
		}

		#endregion

		ZCalcEdit TestControl;

		protected override void SetUp()
		{
			TestControl = new ZCalcEdit();
		}

		protected override void TearDown()
		{
			TestControl.Dispose();
			base.TearDown();
		}
	}
}
