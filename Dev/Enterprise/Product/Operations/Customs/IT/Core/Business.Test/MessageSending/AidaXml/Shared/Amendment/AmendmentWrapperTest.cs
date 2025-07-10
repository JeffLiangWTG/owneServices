using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class AmendmentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new AmendmentWrapper(entryHeader: null, sendingObject));
		AssertExceptionThrown<ArgumentNullException>("When sendingObject is null", () => new AmendmentWrapper(entryHeader, sendingObject: null));
	}

	public void TestMrn()
	{
		var amendmentWrapper = GetNewAmendmentWrapper();
		AssertEquals(nameof(IDeclarationAmendment.Mrn), "", amendmentWrapper.Mrn);

		entryHeader.MovementReferenceNumberSetter("22CH00000294926586");
		amendmentWrapper = GetNewAmendmentWrapper();
		AssertEquals(nameof(IDeclarationAmendment.Mrn), "22CH00000294926586", amendmentWrapper.Mrn);
	}

	public void TestReason()
	{
		var amendmentWrapper = GetNewAmendmentWrapper();
		AssertEquals(nameof(IDeclarationAmendment.Reason), "", amendmentWrapper.Reason);

		sendingObject.VOCReason = "A";
		amendmentWrapper = GetNewAmendmentWrapper();
		AssertEquals(nameof(IDeclarationAmendment.Reason), "A", amendmentWrapper.Reason);
	}

	public void TestLegislativeReference()
	{
		var amendmentWrapper = GetNewAmendmentWrapper();
		AssertEquals(nameof(IDeclarationAmendment.LegislativeReference), "", amendmentWrapper.LegislativeReference);

		sendingObject.CancellationAndAmendmentLegislativeReference = "1";
		amendmentWrapper = GetNewAmendmentWrapper();
		AssertEquals(nameof(IDeclarationAmendment.LegislativeReference), "1", amendmentWrapper.LegislativeReference);
	}

	public void TestCancelledLines()
	{
		var amendmentWrapper = GetNewAmendmentWrapper();
		AssertNull(nameof(IDeclarationAmendment.CancelledLines), amendmentWrapper.CancelledLines);

		entryHeader.ZG_SentEntryLinesCount = 4;
		entryHeader.MergedLines.AddNew();
		entryHeader.MergedLines.AddNew();
		amendmentWrapper = GetNewAmendmentWrapper();
		AssertContainsExactElementsInExactOrder(nameof(IDeclarationAmendment.CancelledLines), new int[] { 3, 4 }, amendmentWrapper.CancelledLines);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sendingObject = new MockJobDeclarationMessageSendingObject(entryHeader, new JobDeclarationMessageSendingObjectParent(declaration));
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObject sendingObject;

	IDeclarationAmendment GetNewAmendmentWrapper() => new AmendmentWrapper(entryHeader, sendingObject);

	class MockJobDeclarationMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public MockJobDeclarationMessageSendingObject(EU.Business.Declaration.CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
			: base(header, jobDeclarationMessageSendingObjectParent)
		{
		}

		protected override ZString GetMessageSubType() => ZString.Empty;
	}
}
