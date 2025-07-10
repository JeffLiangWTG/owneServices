using System;
using CargoWise.Customs.IT.MessageDefinitions.NCTS.Departure.data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class MrnFieldHandlerTest : TestCaseWithFactory
{
	public void TestHandle_StoreMrnNumber()
	{
		var responseMessage = new Data { Mrn = "MRN202209281105AMS", DataTimeStartElab = new DateTime(2024, 5, 7, 17, 0, 0, DateTimeKind.Utc) };
		var dateTime = new ZDateTime(responseMessage.DataTimeStartElab).ToLocalBranchTime();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);

		var handler = new MrnFieldHandler(adapter);
		handler.Handle(responseMessage);

		var mrnEntry = CusEntryNumber.Load(entryHeader, "MRN", "IT", true);
		var regEntry = CusEntryNumber.Load(entryHeader, "REG", "IT", true);

		AssertNotNull("MRN Entry", mrnEntry);
		AssertEquals("MRN Value", responseMessage.Mrn, mrnEntry.CE_EntryNum);
		AssertEquals("Issue Date for MRN", dateTime, mrnEntry.CE_IssueDate);

		AssertNotNull("REG Entry", regEntry);
		AssertEquals("REG Value", "9 -281105A", regEntry.CE_EntryNum);
		AssertEquals("Issue Date for REG", dateTime, regEntry.CE_IssueDate);

		AssertEquals("Entry Status", ITEntryStatusList.Codes.Registered, entryHeader.CH_EntryStatus);
	}

	public void TestHandle_NoMrnValue()
	{
		var responseMessage = new Data();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);

		var handler = new MrnFieldHandler(adapter);
		handler.Handle(responseMessage);

		var mrnEntry = CusEntryNumber.Load(entryHeader, "MRN", "IT", true);
		var regEntry = CusEntryNumber.Load(entryHeader, "REG", "IT", true);

		AssertNull("MRN Entry", mrnEntry);
		AssertNull("REG Entry", regEntry);
	}

	[ExpectNoExceptions]
	public void TestHandle_SetSentEntryLinesCountCall()
	{
		var responseMessage = new Data();
		responseMessage.Mrn = "123";

		var mockAdapter = new Mock<IXmlCustomsLinkedObjectAdapter>();
		mockAdapter.Setup(x => x.SetSentEntryLinesCount());
		var handler = new MrnFieldHandler(mockAdapter.Object);
		handler.Handle(responseMessage);
		mockAdapter.Verify(x => x.SetSentEntryLinesCount(), Times.Once());
	}

	[ExpectNoExceptions]
	public void TestHandle_SetStatusAsRegistered()
	{
		var responseMessage = new Data();
		responseMessage.Mrn = "123";
		responseMessage.DataTimeEndElab = new DateTime(2024, 5, 7, 17, 0, 0, DateTimeKind.Utc);
		var dateTime = new ZDateTime(responseMessage.DataTimeEndElab).ToLocalBranchTime();
		var mockAdapter = new Mock<IXmlCustomsLinkedObjectAdapter>();
		mockAdapter.Setup(x => x.SetStatusAsRegistered(dateTime));
		var handler = new MrnFieldHandler(mockAdapter.Object);
		handler.Handle(responseMessage);
		mockAdapter.Verify(x => x.SetStatusAsRegistered(dateTime), Times.Once());
	}

	protected override void SetUp()
	{
		base.SetUp();
		var jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
	}

	CusEntryHeader entryHeader;
}
