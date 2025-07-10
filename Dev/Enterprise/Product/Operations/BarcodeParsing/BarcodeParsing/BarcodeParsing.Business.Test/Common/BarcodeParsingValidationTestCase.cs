using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	abstract class BarcodeParsingValidationTestCase : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			dummyBarcodeEnableDisposable = BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(Factory, new DummyBarcodeValidationRulesConsumer(Factory));
		}

		protected override void TearDown()
		{
			base.TearDown();
			dummyBarcodeEnableDisposable?.Dispose();
		}

		IDisposable dummyBarcodeEnableDisposable;

		protected void OverrideDummyBarcodeParsingConsumer(IBarcodeParsingConsumer consumer)
		{
			dummyBarcodeEnableDisposable?.Dispose();
			dummyBarcodeEnableDisposable = BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(Factory, consumer);
		}

		protected BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}

		BarcodeParsingTestHelper helper;
	}
}
