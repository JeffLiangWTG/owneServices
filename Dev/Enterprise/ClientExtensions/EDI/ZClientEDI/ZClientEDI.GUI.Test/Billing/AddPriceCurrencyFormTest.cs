using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(AddPriceCurrencyForm))]
	public class AddPriceCurrencyFormTest : ZFormBasherTest
	{
		public void TestOkButtonClick()
		{
			var updater = new PriceCurrencyUpdater(Factory);
			var rounding = updater.Rounding.AddNew();
			rounding.PriceBreak = 1;
			rounding.RoundingScale = 0;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = new AddPriceCurrencyForm(updater))
			{
				form.Show();
				Application.DoEvents();
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			AssertEquals("Rounding Scale cannot be zero.", UnitTestUserNotification.Instance.LastMessage.Text);

			rounding.PriceBreak = 1;
			rounding.RoundingScale = 1;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = new AddPriceCurrencyForm(updater))
			{
				form.Show();
				Application.DoEvents();
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}

			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		protected override Form GetFormToBashCore()
		{
			return new AddPriceCurrencyForm(new PriceCurrencyUpdater(Factory));
		}

		#endregion
	}

	[TestedType(typeof(PriceExchangeRate))]
	public class PriceExchangeRateTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PriceExchangeRate(Factory);
		}

		#endregion
	}

	[TestedType(typeof(PriceCurrencyUpdater))]
	public class PriceCurrencyUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PriceCurrencyUpdater(Factory);
		}

		#endregion
	}

	[TestedType(typeof(PriceRounding))]
	public class PriceRoundingTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PriceRounding(Factory);
		}

		#endregion
	}

	[TestedType(typeof(PriceExchangeRateCollection))]
	public class PriceExchangeRateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PriceExchangeRateCollection>
	{
		protected override PriceExchangeRateCollection GetCollectionToTest()
		{
			return new PriceExchangeRateCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PriceExchangeRate(Factory);
		}
	}

	[TestedType(typeof(PriceRoundingCollection))]
	public class PriceRoundingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PriceRoundingCollection>
	{
		protected override PriceRoundingCollection GetCollectionToTest()
		{
			return new PriceRoundingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PriceRounding(Factory);
		}
	}
}
