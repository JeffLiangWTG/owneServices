using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class PreviousDocumentWrapperTest : PreviousDocumentBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new PreviousDocumentWrapper(null));
		AssertNoExceptionThrown("when argument is not null", () => new PreviousDocumentWrapper(previousDocument));
	}

	public override void TestReferenceNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IPreviousDocument.ReferenceNumber), wrapper.ReferenceNumber);

		previousDocument.CSI_ReferenceNumber = "123";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "123", wrapper.ReferenceNumber);
	}

	public override void TestDocumentType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IPreviousDocument.DocumentType), wrapper.DocumentType);

		previousDocument.CSI_Code = "ABC";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IPreviousDocument.DocumentType), "ABC", wrapper.DocumentType);
	}

	public override void TestComplementOfInformation()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IPreviousDocument.ComplementOfInformation), wrapper.ComplementOfInformation);

		previousDocument.CSI_ReferenceNumber2 = "123";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IPreviousDocument.ComplementOfInformation), "123", wrapper.ComplementOfInformation);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		previousDocument = nctsHeader
			.Bills.AddNew()
			.PreviousDocuments.AddNew();
	}

	protected override IPreviousDocument CreateWrapper() => new PreviousDocumentWrapper(previousDocument);

	CommonPreviousDocument previousDocument;
}
