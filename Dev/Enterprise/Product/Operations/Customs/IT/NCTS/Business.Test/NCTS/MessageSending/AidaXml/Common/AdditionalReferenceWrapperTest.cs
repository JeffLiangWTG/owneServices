using System;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class AdditionalReferenceWrapperTest : AdditionalReferenceBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new AdditionalReferenceWrapper(null));
	}

	public override void TestReferenceNumber()
	{
		var additionalReferenceWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IAdditionalReference.ReferenceNumber), additionalReferenceWrapper.ReferenceNumber);

		additionalDocument.CSI_ReferenceNumber = "REF1";
		additionalReferenceWrapper = CreateWrapper();
		AssertEquals(nameof(IAdditionalReference.ReferenceNumber), "REF1", additionalReferenceWrapper.ReferenceNumber);
	}

	public override void TestReferenceType()
	{
		var additionalReferenceWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IAdditionalReference.ReferenceType), additionalReferenceWrapper.ReferenceType);

		additionalDocument.CSI_Code = "Y025";
		additionalReferenceWrapper = CreateWrapper();
		AssertEquals(nameof(IAdditionalReference.ReferenceType), "Y025", additionalReferenceWrapper.ReferenceType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		additionalDocument = Factory.New<NctsAdditionalInfo>();
	}

	NctsAdditionalInfo additionalDocument;

	protected override IAdditionalReference CreateWrapper() => new AdditionalReferenceWrapper(additionalDocument);
}
