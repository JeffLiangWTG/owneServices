using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageBillValidationDecider))]
	sealed class TemporaryStorageBillValidationDeciderTest : TemporaryStorageBillValidationDeciderAbstractTest<TemporaryStorageBillValidationDecider>
	{
		protected override bool ExpectedIsGrossWeightCheckSupported => true;
		protected override bool ExpectedIsTypeOfBillDocumentCheckSupported => true;

		protected override bool ExpectedIsConsignorOrgPKCheckSupported => true;
		protected override bool ExpectedIsConsigneeOrgPKCheckSupported => true;

		protected override bool ExpectedIsShipperNameCheckSupported => true;
		protected override bool ExpectedIsShipperCountryCheckSupported => true;
		protected override bool ExpectedIsShipperPostcodeCheckSupported => true;
		protected override bool ExpectedIsShipperRegNoTypeCheckSupported => true;

		protected override bool ExpectedIsConsigneeNameCheckSupported => true;
		protected override bool ExpectedIsConsigneeCountryCheckSupported => true;
		protected override bool ExpectedIsConsigneePostcodeCheckSupported => true;
		protected override bool ExpectedIsConsigneeRegNoTypeCheckSupported => true;

		public override void TestIsTypeOfBillDocumentMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals(expected: true, validationDecider.IsTypeOfBillDocumentMandatory.Invoke(null));
		}

		public override void TestIsABL_BillNumberMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals(expected: true, validationDecider.IsABL_BillNumberMandatory.Invoke(null));
		}

		public override void TestAllowDuplicateTypeAndNumber()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("AllowDuplicateTypeAndNumber should be false by default", expected: false, validationDecider.AllowDuplicateTypeAndNumber.Invoke(null));
		}

		public override void TestIsConsignorOrgPKMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsConsignorOrgPKMandatory should be true by default", expected: true, validationDecider.IsConsignorOrgPKMandatory.Invoke(null));
		}

		public override void TestIsShipperNameMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsShipperNameMandatory should be true by default", expected: true, validationDecider.IsShipperNameMandatory.Invoke(null));
		}

		public override void TestIsShipperCountryMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsShipperCountryMandatory should be true by default", expected: true, validationDecider.IsShipperCountryMandatory.Invoke(null));
		}

		public override void TestIsShipperPostcodeMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsShipperPostcodeMandatory should be true by default", expected: true, validationDecider.IsShipperPostcodeMandatory.Invoke(null));
		}

		public override void TestIsShipperRegNoTypeMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsShipperRegNoTypeMandatory should be true by default", expected: true, validationDecider.IsShipperRegNoTypeMandatory.Invoke(null));
		}

		public override void TestIsConsigneeOrgPKMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsConsigneeOrgPKMandatory should be true by default", expected: true, validationDecider.IsConsigneeOrgPKMandatory.Invoke(null));
		}

		public override void TestIsConsigneeNameMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsConsigneeNameMandatory should be true by default", expected: true, validationDecider.IsConsigneeNameMandatory.Invoke(null));
		}

		public override void TestIsConsigneeCountryMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsConsigneeCountryMandatory should be true by default", expected: true, validationDecider.IsConsigneeCountryMandatory.Invoke(null));
		}

		public override void TestIsConsigneePostcodeMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsConsigneePostcodeMandatory should be true by default", expected: true, validationDecider.IsConsigneePostcodeMandatory.Invoke(null));
		}

		public override void TestIsConsigneeRegNoTypeMandatory()
		{
			var validationDecider = new TemporaryStorageBillValidationDecider();
			AssertEquals("IsConsigneeRegNoTypeMandatory should be true by default", expected: true, validationDecider.IsConsigneeRegNoTypeMandatory.Invoke(null));
		}
	}
}
