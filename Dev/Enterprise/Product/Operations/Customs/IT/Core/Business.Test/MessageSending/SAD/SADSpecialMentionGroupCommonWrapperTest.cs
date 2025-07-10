using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SADSpecialMentionGroupCommonWrapperTest<TSpecialMentionGroupWrapper> : TestCaseWithFactory
	where TSpecialMentionGroupWrapper : ISpecialMentionGroup
{
	public void TestEori()
	{
		specialMentionGroupWrapper = GetSpecialMentionGroupWrapper(entryLine);
		AssertNotNull("Eori should not be null", specialMentionGroupWrapper.Eori);
		AssertType<SADSpecialMentionEoriInfoWrapper>("Eori type", specialMentionGroupWrapper.Eori);
	}

	public void TestUnloadingData()
	{
		specialMentionGroupWrapper = GetSpecialMentionGroupWrapper(entryLine);
		var unloadingData = specialMentionGroupWrapper.UnloadingData;
		AssertNotNull("UnloadingData should not be null", unloadingData);
		AssertType<SADSpecialMentionUnloadingDataInfoWrapper>("UnloadingData type", unloadingData);

		CombineAssertions("No previous documents -> empty UnloadingData", () =>
		{
			AssertEquals("", unloadingData.CommodityCode);
			AssertNull(unloadingData.Quantity);
			AssertNull(unloadingData.SupplementaryUnit);
		});

		var paDocument = invoiceLine.PreviousDocuments.AddNew();
		paDocument.CSI_Procedure = "A3";
		var rpDocument = invoiceLine.PreviousDocuments.AddNew();
		rpDocument.CSI_Procedure = "2";
		rpDocument.CSI_Quantity = 200m;
		rpDocument.CSI_Quantity2 = 100m;
		rpDocument.CSI_Tariff = "1234568790";
		declaration.ResetApportionedPreviousDocuments();
		unloadingData = specialMentionGroupWrapper.UnloadingData;
		CombineAssertions("1PA and 1RP document -> RP document quantities in UnloadingData", () =>
		{
			AssertEquals("1234568790", unloadingData.CommodityCode);
			AssertEquals(200m, unloadingData.Quantity);
			AssertEquals(100m, unloadingData.SupplementaryUnit);
		});
	}

	public void TestUnloadingDataOnlyOneGenericDocument()
	{
		specialMentionGroupWrapper = GetSpecialMentionGroupWrapper(entryLine);
		var unloadingData = specialMentionGroupWrapper.UnloadingData;
		AssertNotNull("UnloadingData should not be null", unloadingData);
		AssertType<SADSpecialMentionUnloadingDataInfoWrapper>("UnloadingData type", unloadingData);

		CombineAssertions("No previous documents -> empty UnloadingData", () =>
		{
			AssertEquals("", unloadingData.CommodityCode);
			AssertNull(unloadingData.Quantity);
			AssertNull(unloadingData.SupplementaryUnit);
		});

		var paDocument = invoiceLine.PreviousDocuments.AddNew();
		paDocument.CSI_Procedure = "A3";
		declaration.ResetApportionedPreviousDocuments();
		unloadingData = specialMentionGroupWrapper.UnloadingData;
		CombineAssertions("Only one NOT RP previous documents -> empty UnloadingData", () =>
		{
			AssertEquals("", unloadingData.CommodityCode);
			AssertNull(unloadingData.Quantity);
			AssertNull(unloadingData.SupplementaryUnit);
		});
	}

	public void TestUnloadingDataOnlyOneRPDocument()
	{
		specialMentionGroupWrapper = GetSpecialMentionGroupWrapper(entryLine);
		var unloadingData = specialMentionGroupWrapper.UnloadingData;
		AssertNotNull("UnloadingData should not be null", unloadingData);
		AssertType<SADSpecialMentionUnloadingDataInfoWrapper>("UnloadingData type", unloadingData);

		CombineAssertions("No previous documents -> empty UnloadingData", () =>
		{
			AssertEquals("", unloadingData.CommodityCode);
			AssertNull(unloadingData.Quantity);
			AssertNull(unloadingData.SupplementaryUnit);
		});

		var rpDocument = invoiceLine.PreviousDocuments.AddNew();
		rpDocument.CSI_Procedure = "2";
		rpDocument.CSI_Quantity = 200m;
		rpDocument.CSI_Quantity2 = 100m;
		rpDocument.CSI_Tariff = "1234568790";
		declaration.ResetApportionedPreviousDocuments();
		unloadingData = specialMentionGroupWrapper.UnloadingData;
		CombineAssertions("1RP document -> RP document quantities in UnloadingData", () =>
		{
			AssertEquals("1234568790", unloadingData.CommodityCode);
			AssertEquals(200m, unloadingData.Quantity);
			AssertEquals(100m, unloadingData.SupplementaryUnit);
		});
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("An entry line is required", () => GetSpecialMentionGroupWrapper(null));
		AssertNoExceptionThrown("An entry line with at least one invoice lines is required", () => GetSpecialMentionGroupWrapper(entryLine));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	protected abstract TSpecialMentionGroupWrapper GetSpecialMentionGroupWrapper(CusEntryLine entryLine);

	protected JobDeclaration declaration;
	protected CusEntryLine entryLine;
	protected JobComInvoiceLine invoiceLine;
	protected CusEntryHeader entryHeader;
	protected CusEntryInstruction entryInstruction;
	TSpecialMentionGroupWrapper specialMentionGroupWrapper;
}
