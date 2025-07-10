using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageBillValidationDecider))]
sealed class TemporaryStorageBillValidationDeciderTest : TemporaryStorageBillValidationDeciderAbstractTest<TemporaryStorageBillValidationDecider>
{
	protected override bool ExpectedIsGrossWeightCheckSupported => false;
	protected override bool ExpectedIsTypeOfBillDocumentCheckSupported => false;

	protected override bool ExpectedIsConsignorOrgPKCheckSupported => false;
	protected override bool ExpectedIsConsigneeOrgPKCheckSupported => false;

	protected override bool ExpectedIsShipperNameCheckSupported => false;
	protected override bool ExpectedIsShipperCountryCheckSupported => false;
	protected override bool ExpectedIsShipperPostcodeCheckSupported => false;
	protected override bool ExpectedIsShipperRegNoTypeCheckSupported => false;

	protected override bool ExpectedIsConsigneeNameCheckSupported => false;
	protected override bool ExpectedIsConsigneeCountryCheckSupported => false;
	protected override bool ExpectedIsConsigneePostcodeCheckSupported => false;
	protected override bool ExpectedIsConsigneeRegNoTypeCheckSupported => false;

	public override void TestIsTypeOfBillDocumentMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		AssertEquals(true, validationDecider.IsTypeOfBillDocumentMandatory.Invoke(null));
	}

	public override void TestIsABL_BillNumberMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		AssertEquals(true, validationDecider.IsABL_BillNumberMandatory.Invoke(null));
	}

	public override void TestAllowDuplicateTypeAndNumber()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		AssertEquals("AllowDuplicateTypeAndNumber should be false", false, validationDecider.AllowDuplicateTypeAndNumber.Invoke(null));
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
