using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageBillValidationDecider))]
sealed class TemporaryStorageBillValidationDeciderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageBillValidationDeciderAbstractTest<TemporaryStorageBillValidationDecider>
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
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = string.Empty;
			AssertEquals("Should not be mandatory when no items", expected: false, validationDecider.IsTypeOfBillDocumentMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM message type", expected: false, validationDecider.IsTypeOfBillDocumentMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM message type", expected: false, validationDecider.IsTypeOfBillDocumentMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			var item = bill.PackedItems.AddNew();
			AssertEquals("Should be mandatory when not TSM/LAM and items don't have TRA", expected: true, validationDecider.IsTypeOfBillDocumentMandatory.Invoke(bill));
		});
	}

	public override void TestIsABL_BillNumberMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = string.Empty;
			AssertEquals("Should not be mandatory when no items", expected: false, validationDecider.IsABL_BillNumberMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM message type", expected: false, validationDecider.IsABL_BillNumberMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM message type", expected: false, validationDecider.IsABL_BillNumberMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			var item = bill.PackedItems.AddNew();
			AssertEquals("Should be mandatory when not TSM/LAM and items don't have TRA", expected: true, validationDecider.IsTypeOfBillDocumentMandatory.Invoke(bill));
		});
	}

	public override void TestAllowDuplicateTypeAndNumber()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		AssertEquals("AllowDuplicateTypeAndNumber should be false", expected: false, validationDecider.AllowDuplicateTypeAndNumber.Invoke(null));
	}

	public override void TestIsConsignorOrgPKMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsConsignorOrgPKMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM message type", expected: false, validationDecider.IsConsignorOrgPKMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM message type", expected: false, validationDecider.IsConsignorOrgPKMandatory.Invoke(bill));
		});
	}

	public override void TestIsConsigneeOrgPKMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsConsigneeOrgPKMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM message type", expected: false, validationDecider.IsConsigneeOrgPKMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM message type", expected: false, validationDecider.IsConsigneeOrgPKMandatory.Invoke(bill));
		});
	}

	public override void TestIsShipperNameMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsShipperNameMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsShipperNameMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsShipperNameMandatory.Invoke(bill));
		});
	}

	public override void TestIsShipperCountryMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsShipperCountryMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsShipperCountryMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsShipperCountryMandatory.Invoke(bill));
		});
	}

	public override void TestIsShipperPostcodeMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsShipperPostcodeMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsShipperPostcodeMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsShipperPostcodeMandatory.Invoke(bill));
		});
	}

	public override void TestIsShipperRegNoTypeMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsShipperRegNoTypeMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsShipperRegNoTypeMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsShipperRegNoTypeMandatory.Invoke(bill));
		});
	}

	public override void TestIsConsigneeNameMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsConsigneeNameMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsConsigneeNameMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsConsigneeNameMandatory.Invoke(bill));
		});
	}

	public override void TestIsConsigneeCountryMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsConsigneeCountryMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsConsigneeCountryMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsConsigneeCountryMandatory.Invoke(bill));
		});
	}

	public override void TestIsConsigneePostcodeMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsConsigneePostcodeMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsConsigneePostcodeMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsConsigneePostcodeMandatory.Invoke(bill));
		});
	}

	public override void TestIsConsigneeRegNoTypeMandatory()
	{
		var validationDecider = new TemporaryStorageBillValidationDecider();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("Should not be mandatory for TSM", expected: false, validationDecider.IsConsigneeRegNoTypeMandatory.Invoke(bill));

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("Should not be mandatory for LAM", expected: false, validationDecider.IsConsigneeRegNoTypeMandatory.Invoke(bill));

			header.AMA_MessageType = string.Empty;
			AssertEquals("Should be mandatory for standard message type", expected: true, validationDecider.IsConsigneeRegNoTypeMandatory.Invoke(bill));
		});
	}
}
