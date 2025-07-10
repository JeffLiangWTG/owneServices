using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class AdditionalReferenceWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new AdditionalReferenceWrapper(null));
	}

	public void TestReferenceType()
	{
		var wrapper = GetNewAdditionalReferenceWrapper();
		AssertEquals(nameof(IAdditionalReference.ReferenceType), "", wrapper.ReferenceType);

		additionalReference.CSI_Code = "00300";
		wrapper = GetNewAdditionalReferenceWrapper();
		AssertEquals(nameof(IAdditionalReference.ReferenceType), "00300", wrapper.ReferenceType);
	}

	public void TestReferenceNumber()
	{
		var wrapper = GetNewAdditionalReferenceWrapper();
		AssertEquals(nameof(IAdditionalReference.ReferenceNumber), "", wrapper.ReferenceNumber);

		additionalReference.CSI_ReferenceNumber = "123456";
		wrapper = GetNewAdditionalReferenceWrapper();
		AssertEquals(nameof(IAdditionalReference.ReferenceNumber), "123456", wrapper.ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		additionalReference = Factory.New<AdditionalInfo>();
	}

	AdditionalInfo additionalReference;

	IAdditionalReference GetNewAdditionalReferenceWrapper() => new AdditionalReferenceWrapper(additionalReference);
}
