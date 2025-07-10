using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationMiscMessageSendingObjectCollection))]
	sealed class JobDeclarationMiscMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationMiscMessageSendingObjectCollection>
	{
		protected override JobDeclarationMiscMessageSendingObjectCollection GetCollectionToTest()
		{
			return new JobDeclarationMiscMessageSendingObjectCollection(Declaration, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}

		public void TestGetEntryLineFilter_5FN()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A1070001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제107조 제1항 해당물품");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageStatus = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_SpecificUseCodeDutyRatePermitNo = "A";
			AssertEquals("CKI_SpecificUseCodeDutyRatePermitNo is not empty", false, invoiceLine1.IsSubjectTo5FN);

			var entryLine2 = entry.MergedLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_SpecificUseCodeDutyRatePermitNo = "";
			invoiceLine2.JI_IsSpecificUseCode = false;
			invoiceLine2.JI_SecondaryPreference = "A093000004";
			invoiceLine2.JI_InstallmentCode = "";
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyExemption, invoiceLine2.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is A", true, invoiceLine2.IsSubjectTo5FN);

			var entryLine3 = entry.MergedLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyReduction, invoiceLine3.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is B", true, invoiceLine3.IsSubjectTo5FN);

			var entryLine4 = entry.MergedLines.AddNew();
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_SecondaryPreference = "";
			invoiceLine4.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.InstallmentPayment, invoiceLine4.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is C", true, invoiceLine4.IsSubjectTo5FN);

			var entryLine5 = entry.MergedLines.AddNew();
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CL = entryLine5.PK;
			invoiceLine5.JI_IsSpecificUseCode = true;
			invoiceLine5.JI_InstallmentCode = "";
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly, invoiceLine5.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is D", true, invoiceLine5.IsSubjectTo5FN);

			var entryLine6 = entry.MergedLines.AddNew();
			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CL = entryLine6.PK;
			invoiceLine6.JI_IsSpecificUseCode = true;
			invoiceLine6.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment, invoiceLine6.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is T", true, invoiceLine6.IsSubjectTo5FN);

			var entryLine7 = entry.MergedLines.AddNew();
			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_CL = entryLine7.PK;
			invoiceLine7.JI_SecondaryPreference = "A1070001";
			invoiceLine7.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.Invalid, invoiceLine7.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is X", false, invoiceLine7.IsSubjectTo5FN);

			var messageSendingObjects = new JobDeclarationMiscMessageSendingObjectCollection(declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			AssertEquals(1, messageSendingObjects.Count);
			var messageSendingObject = messageSendingObjects[0];
			var entryLines = messageSendingObject.MessageSendingEntryLines;
			AssertEquals(5, entryLines.Count);
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyExemption, entryLines[0].DutyReduction);
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyReduction, entryLines[1].DutyReduction);
			AssertEquals(ImportDutyReductionClassificationList.Codes.InstallmentPayment, entryLines[2].DutyReduction);
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly, entryLines[3].DutyReduction);
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment, entryLines[4].DutyReduction);
		}
	}
}
