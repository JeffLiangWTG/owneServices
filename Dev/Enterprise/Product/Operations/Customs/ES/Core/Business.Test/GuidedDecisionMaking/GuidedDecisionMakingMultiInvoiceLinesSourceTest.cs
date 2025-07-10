using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Testing;

internal class GuidedDecisionMakingMultiInvoiceLinesSourceTest : TestCaseWithFactory
{
	public void TestDestinationStateIsCanaryIsland() => CombineAssertions(() =>
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");
		var noDecInvLine = Factory.New<JobComInvoiceLine>();
		var gdmSourceWrapper = new GuidedDecisionMakingMultiInvoiceLinesSource(noDecInvLine);
		var declaration = Factory.New<JobDeclaration>();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertEquals("InvoiceLine without parent declaration return false", false, noDecInvLine.DestinationStateIsCanaryIsland);
			AssertEquals("gdmSourceWrapper can get the DestinationStateIsCanaryIsland from the invoice line, false when there is no parent declaration", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = "61";
			gdmSourceWrapper = new GuidedDecisionMakingMultiInvoiceLinesSource(line);
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invoice line, true when there is parent declaration and it's import and ZG_DestinationState is a canary island code", true, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.ZG_DestinationState = "12";
			gdmSourceWrapper = new GuidedDecisionMakingMultiInvoiceLinesSource(line);
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invoice line, false when there is parent declaration and it's import but ZG_DestinationState is not a canary island code", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003861";
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, false when JE_CustomsOffice = 'ES003861', Import and not UCC6", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_DestinationState = "61";
			gdmSourceWrapper = new GuidedDecisionMakingMultiInvoiceLinesSource(line);
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invoice line, false when there is parent declaration and it's export, even when ZG_DestinationState is a canary island code", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, false when JE_CustomsOffice = 'ES003861', Export and not UCC6", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, false when JE_CustomsOffice = 'ES003861', Export and UCC6", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, true when JE_CustomsOffice = 'ES003861', Import and UCC6", true, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, false when JE_CustomsOffice = Empty, Import and UCC6", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003541";
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, true when JE_CustomsOffice = 'ES003541', Import and UCC6", true, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003712";
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, false when JE_CustomsOffice = 'ES003712', Import and UCC6", false, gdmSourceWrapper.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES009998";
			AssertEquals("GDMBasic can get the DestinationStateIsCanaryIsland from the invLine, true when JE_CustomsOffice = 'ES009998', Import and UCC6", true, gdmSourceWrapper.DestinationStateIsCanaryIsland);
		}
	});
}
