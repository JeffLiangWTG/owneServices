using System.Globalization;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalculatorTextBoxTest : TestCaseWithFactory
	{
		public void TestTextAsDecimal()
		{
			// With default culture (Aussie-based)
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				CalculatorTextBox.Text = "123.456";
				AssertEquals(123.456m, CalculatorTextBox.TextAsDecimal);
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				CalculatorTextBox.Text = "123,456";
				AssertEquals(123.456m, CalculatorTextBox.TextAsDecimal);
			}
		}

		[ExpectNoExceptions]
		public void TestTextAsDecimal_DoesntBlowUpWithInvalidText()
		{
			CalculatorTextBox.Text = "xxx123.456";
			AssertEquals(0.0m, CalculatorTextBox.TextAsDecimal);
		}

		public void TestTextContainsDecimal()
		{
			// With default culture (Aussie-based)
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				CalculatorTextBox.Text = "123.456";
				AssertEquals(true, CalculatorTextBox.TextContainsDecimal);
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				CalculatorTextBox.Text = "123,456";
				AssertEquals(true, CalculatorTextBox.TextContainsDecimal);
			}
		}

		#region Implementation

		ZCalculatorTextBox CalculatorTextBox
		{
			get { return calculatorTextBox ?? (calculatorTextBox = new ZCalculatorTextBox()); }
		}
		ZCalculatorTextBox calculatorTextBox;

		protected override void TearDown()
		{
			base.TearDown();
			if (calculatorTextBox != null)
			{
				calculatorTextBox.Dispose();
			}
		}

		#endregion
	}
}
