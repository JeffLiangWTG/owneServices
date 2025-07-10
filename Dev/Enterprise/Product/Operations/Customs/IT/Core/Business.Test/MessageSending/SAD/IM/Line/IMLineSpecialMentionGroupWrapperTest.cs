using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMLineSpecialMentionGroupWrapperTest : SADSpecialMentionGroupCommonWrapperTest<IMLineSpecialMentionGroupWrapper>
{
	public void TestPreviousProcedure()
	{
		wrapper = new IMLineSpecialMentionGroupWrapper(entryLine);
		var previousProcedure = wrapper.PreviousProcedure;
		AssertNotNull("Previous Procedure should not be null", previousProcedure);
		AssertType<SADPreviousAdministrativeReferenceWrapper>("Previous Procedure type", previousProcedure);
		CombineAssertions("No previous documents -> empty PreviousProcedure", () =>
		{
			AssertEquals("", previousProcedure.CustomsOffice);
			AssertEquals(ZDate.Empty, previousProcedure.Date);
			AssertNull(previousProcedure.ItemNumber);
			AssertEquals("", previousProcedure.ReferenceCIN);
			AssertEquals("", previousProcedure.ReferenceNumber);
			AssertEquals("", previousProcedure.Register);
			AssertEquals("", previousProcedure.Series);
		});

		var paDocument = invoiceLine.PreviousDocuments.AddNew();
		paDocument.CSI_Procedure = "A3";
		var rpDocument = invoiceLine.PreviousDocuments.AddNew();
		rpDocument.CSI_Procedure = "2";
		rpDocument.CSI_ReferenceNumber = "1X";
		rpDocument.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		rpDocument.CSI_Status = "A";
		rpDocument.CSI_CustomsOffice = "IT137100";
		rpDocument.CSI_LineNo = 1;
		declaration.ResetApportionedPreviousDocuments();
		previousProcedure = wrapper.PreviousProcedure;
		CombineAssertions("1PA and 1RP document -> RP document in PreviousProcedure", () =>
		{
			AssertEquals("IT137100", previousProcedure.CustomsOffice);
			AssertEquals(new ZDateTime(2020, 01, 01), previousProcedure.Date);
			AssertEquals(1, previousProcedure.ItemNumber);
			AssertEquals("X", previousProcedure.ReferenceCIN);
			AssertEquals("1", previousProcedure.ReferenceNumber);
			AssertEquals("2", previousProcedure.Register);
			AssertEquals("A", previousProcedure.Series);
		});
	}

	public void TestSteelType()
	{
		invoiceLine.ZG_SteelType = "A";

		wrapper = new IMLineSpecialMentionGroupWrapper(entryLine);
		AssertEquals("Steel Type", "A", wrapper.SteelType);

		invoiceLine.ZG_SteelType = "0";
		AssertEquals("Steel Type", "", wrapper.SteelType);
	}

	protected override IMLineSpecialMentionGroupWrapper GetSpecialMentionGroupWrapper(CusEntryLine entryLine) => new IMLineSpecialMentionGroupWrapper(entryLine);

	IMLineSpecialMentionGroupWrapper wrapper;
}
