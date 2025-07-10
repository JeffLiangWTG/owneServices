using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Test
{
	[TestedType(typeof(AutoRateInfoWrapper))]
	sealed class AutoRateInfoWrapperTest : GenericWrapperTest
	{
		public void TestAutoRateInfoWrapperProperty()
		{
			var wrapper = GetNewWrapperForTest();

			AssertEquals(100m, wrapper.Amount);
			AssertEquals("", wrapper.AutoRatedForString);
			AssertEquals("", wrapper.CalculationSingleLineDescriptionWithoutChargeCode);
			AssertEquals("test invoice line description", wrapper.InvoiceLineDescription);
		}

		#region Implementation

		public override void TestWrapperMappingsEmpty()
		{
			AutoRateInfoWrapper wrapper = (AutoRateInfoWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapper.Amount", 0m, wrapper.Amount);
			AssertEquals("wrapper.AutoRatedForString", "", wrapper.AutoRatedForString);
			AssertEquals("wrapper.CalculationSingleLineDescriptionWithoutChargeCode", "", wrapper.CalculationSingleLineDescriptionWithoutChargeCode);
			AssertEquals("wrapper.InvoiceLineDescription", "", wrapper.InvoiceLineDescription);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return "Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return GetNewWrapperForTest();
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"AutoRateInformation
======================================================================
Name                                    Type
----------------------------------------------------------------------
Amount                                  Decimal
AutoRatedForString                      String
CalculationSingleLineDescriptionWithoutChargeCode  String
InvoiceLineDescription                  String";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new AutoRateInfoWrapper(new AutoRateInfo(Factory), Factory);
		}

		AutoRateInfoWrapper GetNewWrapperForTest()
		{
			var autoRateInfo = new AutoRateInfo(Factory) { InvoiceLineDescription = "test invoice line description" };
			autoRateInfo.AddFlatPaymentBasis(100m, "S00001234", "AUD");

			return new AutoRateInfoWrapper(autoRateInfo, Factory);
		}

		#endregion

	}
}
