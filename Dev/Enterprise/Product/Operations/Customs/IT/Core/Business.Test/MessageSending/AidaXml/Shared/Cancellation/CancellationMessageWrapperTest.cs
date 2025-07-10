using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class CancellationMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new CancellationMessageWrapper(entryHeader: null, sendingObject));
		AssertExceptionThrown<ArgumentNullException>("When entryHeader.Declaration is null", () => new CancellationMessageWrapper(Factory.New<CusEntryHeader>(), sendingObject));
		AssertExceptionThrown<ArgumentNullException>("When sendingObject is null", () => new CancellationMessageWrapper(entryHeader, sendingObject: null));
	}

	public void TestMrn()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Mrn), "", cancellationMessageWrapper.Mrn);

		entryHeader.MovementReferenceNumberSetter("22CH00000294926586");
		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Mrn), "22CH00000294926586", cancellationMessageWrapper.Mrn);
	}

	public void TestCustomsOffice()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.CustomsOffice), "", cancellationMessageWrapper.CustomsOffice);

		declaration.JE_CustomsOffice = "IT137100";
		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.CustomsOffice), "137100", cancellationMessageWrapper.CustomsOffice);
	}

	public void TestReason()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Reason), "", cancellationMessageWrapper.Reason);

		sendingObject.VOCReason = "A";
		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Reason), "A", cancellationMessageWrapper.Reason);
	}

	public void TestLegislativeReference()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.LegislativeReference), "", cancellationMessageWrapper.LegislativeReference);

		sendingObject.CancellationAndAmendmentLegislativeReference = "1";
		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.LegislativeReference), "1", cancellationMessageWrapper.LegislativeReference);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sendingObject = new JobDeclarationMessageSendingObjectWithNullSubTypeForTest(entryHeader, new JobDeclarationMessageSendingObjectParent(declaration));
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObject sendingObject;

	ICancellation GetNewCancellationMessageWrapper() => new CancellationMessageWrapper(entryHeader, sendingObject);
}
