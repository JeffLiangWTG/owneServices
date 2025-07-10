using System;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	abstract class BarcodeParsingBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			dummyBarcodeEnableDisposable = BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			dummyBarcodeEnableDisposable?.Dispose();
		}

		IDisposable dummyBarcodeEnableDisposable;

		protected BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}

		BarcodeParsingTestHelper helper;

		#endregion
	}
}
