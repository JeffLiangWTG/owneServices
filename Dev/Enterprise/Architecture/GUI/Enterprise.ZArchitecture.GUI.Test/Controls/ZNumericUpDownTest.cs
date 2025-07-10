using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZNumericUpDownTest : ZControlBaseTestCase<ZNumericUpDown>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "Text" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "BindingItems"; }
		}

		public void TestBindingToZInt()
		{
			var type = typeof(ZInt);
			using (var testForm = new ZChildForm())
			{
				var testControl = CreateAndShowBoundTestControl(testForm, type);
				testControl.Minimum = -100m;

				Dummy.Z0_Number = 5;
				AssertEquals("Value set in GUI when changing Dummy", "5", testControl.Text);

				testControl.UpButton();
				AssertEquals("Value should go up", "6", testControl.Text);
				AssertEquals("Dummy should go up, committed by click", 6, Dummy.Z0_Number);

				testControl.DownButton();
				AssertEquals("Value should go down", "5", testControl.Text);
				AssertEquals("Dummy should go down, committed by click", 5, Dummy.Z0_Number);

				AssertEquals("Should be Zero", 0, testControl.GetTextBinding_ParseResultForTesting("....567..---", type));
				AssertEquals("Should be Zero", 0, testControl.GetTextBinding_ParseResultForTesting("lkasjdhflk", type));
				AssertEquals("Should be -50", -50, testControl.GetTextBinding_ParseResultForTesting("-50", type));
				AssertEquals("Should be -100", -100, testControl.GetTextBinding_ParseResultForTesting("-1-0---0-", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", 100, testControl.GetTextBinding_ParseResultForTesting("105", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", -100, testControl.GetTextBinding_ParseResultForTesting("-105", type));
			}
		}

		public void TestBindingToZShort()
		{
			var type = typeof(ZShort);
			using (var testForm = new ZChildForm())
			{
				var testControl = CreateAndShowBoundTestControl(testForm, type);
				testControl.Minimum = -100m;

				Dummy.Z0_Short = 5;
				AssertEquals("Value set in GUI when changing Dummy", "5", testControl.Text);

				testControl.UpButton();
				AssertEquals("Value should go up", "6", testControl.Text);
				AssertEquals("Dummy should go up, committed by click", (ZShort)6, Dummy.Z0_Short);

				testControl.DownButton();
				AssertEquals("Value should go down", "5", testControl.Text);
				AssertEquals("Dummy should go down, committed by click", (ZShort)5, Dummy.Z0_Short);

				AssertEquals("Should be Zero", ZShort.Zero, testControl.GetTextBinding_ParseResultForTesting("...567---", type));
				AssertEquals("Should be Zero", ZShort.Zero, testControl.GetTextBinding_ParseResultForTesting("lkasjdhflk", type));
				AssertEquals("Should be -50", (ZShort)(-50), testControl.GetTextBinding_ParseResultForTesting("-50", type));
				AssertEquals("Should be -100", (ZShort)(-100), testControl.GetTextBinding_ParseResultForTesting("-1-0---0-", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", (ZShort)100, testControl.GetTextBinding_ParseResultForTesting("105", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", (ZShort)(-100), testControl.GetTextBinding_ParseResultForTesting("-105", type));
			}
		}

		public void TestBindingToZByte()
		{
			var type = typeof(ZByte);
			using (var testForm = new ZChildForm())
			{
				var testControl = CreateAndShowBoundTestControl(testForm, type);
				testControl.Minimum = -100m;

				Dummy.Z0_Byte = 5;
				AssertEquals("Value set in GUI when changing Dummy", "5", testControl.Text);

				testControl.UpButton();
				AssertEquals("Value should go up", "6", testControl.Text);
				AssertEquals("Dummy should go up, committed by click", (ZByte)6, Dummy.Z0_Byte);

				testControl.DownButton();
				AssertEquals("Value should go down", "5", testControl.Text);
				AssertEquals("Dummy should go down, committed by click", (ZByte)5, Dummy.Z0_Byte);

				AssertEquals("Should be Zero", ZByte.Zero, testControl.GetTextBinding_ParseResultForTesting("....567---", type));
				AssertEquals("Should be Zero", ZByte.Zero, testControl.GetTextBinding_ParseResultForTesting("lkasjdhflk", type));
				AssertEquals("Should be 0", ZByte.Zero, testControl.GetTextBinding_ParseResultForTesting("-50", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", (ZByte)100, testControl.GetTextBinding_ParseResultForTesting("105", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", ZByte.Zero, testControl.GetTextBinding_ParseResultForTesting("-105", type));
			}
		}

		public void TestBindingToZDecimal()
		{
			var type = typeof(ZDecimal);
			using (var testForm = new ZChildForm())
			{
				var testControl = CreateAndShowBoundTestControl(testForm, type);
				testControl.Minimum = -100m;

				Dummy.Z0_Decimal = 5;
				AssertEquals("Value set in GUI when changing Dummy", "5", testControl.Text);

				testControl.UpButton();
				AssertEquals("Value should go up", "6", testControl.Text);
				AssertEquals("Dummy should go up, committed by click", (ZDecimal)6, Dummy.Z0_Decimal);

				testControl.DownButton();
				AssertEquals("Value should go down", "5", testControl.Text);
				AssertEquals("Dummy should go down, committed by click", (ZDecimal)5, Dummy.Z0_Decimal);

				AssertEquals("Should be Zero", .567m, testControl.GetTextBinding_ParseResultForTesting("....567..---", type));
				AssertEquals("Should be Zero", ZDecimal.Zero, testControl.GetTextBinding_ParseResultForTesting("lkasjdhflk", type));
				AssertEquals("Should be -50", -50m, testControl.GetTextBinding_ParseResultForTesting("-50", type));
				AssertEquals("Should be -100", -100m, testControl.GetTextBinding_ParseResultForTesting("-1-0---0-", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", 100m, testControl.GetTextBinding_ParseResultForTesting("105", type));
				AssertEquals("Value should be the set to the minimum value if input exceeds minimum value.", -100m, testControl.GetTextBinding_ParseResultForTesting("-105", type));
			}
		}

		public void TestReverseUpDownButtons()
		{
			using (var testForm = new ZChildForm())
			{
				var testControl = CreateAndShowBoundTestControl(testForm, typeof(ZInt));

				AssertEquals("Precondition: ReverseUpDownButtons default", false, testControl.ReverseUpDownButtons);

				testControl.UpButton();
				AssertEquals("Value should go up", "1", testControl.Text);

				testControl.DownButton();
				AssertEquals("Value should go down", "0", testControl.Text);

				testControl.ReverseUpDownButtons = true;

				testControl.DownButton();
				AssertEquals("Value should go up", "1", testControl.Text);

				testControl.UpButton();
				AssertEquals("Value should go down", "0", testControl.Text);
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZNumericUpDown.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZNumericUpDown)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZNumericUpDown).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		ZNumericUpDown CreateAndShowBoundTestControl(ZChildForm testForm, Type typeOfBoundMember)
		{
			var testControl = new ZNumericUpDown();
			if (typeOfBoundMember == typeof(ZByte))
			{
				testControl.BindTo = DummyBusinessObject.Schema.Z0_Byte;
			}

			if (typeOfBoundMember == typeof(ZInt))
			{
				testControl.BindTo = DummyBusinessObject.Schema.Z0_Number;
			}

			if (typeOfBoundMember == typeof(ZShort))
			{
				testControl.BindTo = DummyBusinessObject.Schema.Z0_Short;
			}

			if (typeOfBoundMember == typeof(ZDecimal))
			{
				testControl.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			}

			testForm.Controls.Add(testControl);
			testControl.SetDataBinding(Dummy, testControl.BindTo);
			testForm.Show();

			return testControl;
		}

		public void TestReadOnly()
		{
			using (var testForm = new ZChildForm())
			{
				var testControl = CreateAndShowBoundTestControl(testForm, typeof(ZInt));

				Assert("Not readonly by default", !testControl.ReadOnly);

				testControl.ReadOnly = true;
				Assert("Readonly after Readonly is set", testControl.ReadOnly);

				for (var i = 0; i < testControl.Controls.Count; i++)
				{
					Assert("Child controls Readonly should be set", testControl.Controls[i].GetReadOnly());
				}

				testControl.IsNeverReadOnly = true;
				testControl.ReadOnly = true;
				Assert("not readonly if IsNeverReadOnly is set", !testControl.ReadOnly);

				for (var i = 0; i < testControl.Controls.Count; i++)
				{
					Assert("Child controls Readonly should not be set", !testControl.Controls[i].GetReadOnly());
				}
			}
		}

		protected override void BindControl()
		{
			base.BindControl();
			Control.SetDataBinding(Dummy, DummyBizoSchema.Z0_Decimal.Name);
		}
	}
}
