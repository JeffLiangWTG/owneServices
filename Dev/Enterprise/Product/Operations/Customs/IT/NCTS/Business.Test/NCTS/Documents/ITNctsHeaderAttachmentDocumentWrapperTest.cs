using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using ITNctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(ITNctsHeaderAttachmentDocumentWrapper))]
sealed class ITNctsHeaderAttachmentDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when nctsHeader parameter is null", () => ITNctsHeaderAttachmentDocumentWrapper.New(null, Factory));

		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertExceptionThrown<ArgumentNullException>("Exception expected when factory parameter is null", () => ITNctsHeaderAttachmentDocumentWrapper.New(header, null));
	}

	public void TestLinesWithAttachment()
	{
		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var wrapper = ITNctsHeaderAttachmentDocumentWrapper.New(header, Factory);
		AssertType<ITNctsDepCargoDescAttachmentWrapperCollection>(wrapper.LinesWithAttachment);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		return ITNctsHeaderAttachmentDocumentWrapper.New(header, Factory);
	}

	public void TestJobNumber()
	{
		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_JobReference = "ARG001";

		var wrapper = ITNctsHeaderAttachmentDocumentWrapper.New(header, Factory);

		AssertEquals("ReferenceJob", "ARG001", wrapper.JobNumber);
	}
	public void TestMRN()
	{
		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.ArrivalMrnFromUser = "THEMRN";

		var wrapper = ITNctsHeaderAttachmentDocumentWrapper.New(header, Factory);

		AssertEquals("MRN", "THEMRN", wrapper.MRN);
	}
}
