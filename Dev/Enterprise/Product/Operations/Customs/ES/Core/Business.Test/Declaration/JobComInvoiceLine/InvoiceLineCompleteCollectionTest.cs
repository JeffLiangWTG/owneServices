using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineCompleteCollection))]
public class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
{
	public void TestCountryOfDestinationIsSetForImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var collection = new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
		var invoiceLine = collection.AddNew();
		AssertEquals(ZString.Empty, invoiceLine.ZG_CountryOfDestination);
	}

	public new void TestStatisticalValueManualOverrideIsSetForImports()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		var collection = new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
		var invoiceLine = collection.AddNew();
		AssertEquals(false, invoiceLine.ZG_StatisticalValueManualOverride);
	}

	public void TestMethodOfPaymentNotDefaultedInInvoiceLines_NoDestinationState()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = ZString.Empty;
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPayment = "J";
		((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "S";
		declaration.JE_OH_Importer = orgHeader.PK;
		var invoiceHeader = declaration.Invoices.AddNew();
		CombineAssertions("For Import", () =>
		{
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted", invoiceLine1, ZString.Empty, ZString.Empty);
		});
		CombineAssertions("For Export", () =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted", invoiceLine1, ZString.Empty, ZString.Empty);
		});
	}
	public void TestMethodOfPaymentDefaultedInInvoiceLines_Mainland()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = "01";
		var invoiceHeader = declaration.Invoices.AddNew();

		CombineAssertions("For Import", () =>
		{
			var orgHeaderWithoutMOP = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = orgHeaderWithoutMOP.PK;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted when no mop in importer", invoiceLine1, ZString.Empty, ZString.Empty);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPayment = "J";
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "S";
			declaration.JE_OH_Importer = orgHeader.PK;
			invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 defaulted when mop in importer", invoiceLine1, "J", ZString.Empty);
		});
		CombineAssertions("For Export", () =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPayment = "R";
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "R";
			declaration.JE_OH_Importer = orgHeader2.PK;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted for export", invoiceLine1, ZString.Empty, ZString.Empty);
		});
	}

	public void TestMethodOfPaymentDefaultedInInvoiceLines_CanaryIsland()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		_ = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain + "C", parent: grouping);
		_ = helper.CreateCusCodeList("ESC", RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "61", "Test 61", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		_ = helper.CreateCusCodeList("ESC", RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "62", "Test 62", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = "61";
		var invoiceHeader = declaration.Invoices.AddNew();

		CombineAssertions("For Import", () =>
		{
			var orgHeaderWithoutMOP = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = orgHeaderWithoutMOP.PK;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted when no mop in importer", invoiceLine1, ZString.Empty, ZString.Empty);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPayment = "J";
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "S";
			declaration.JE_OH_Importer = orgHeader.PK;
			invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 defaulted when mop in importer", invoiceLine1, "J", "S");
		});
		CombineAssertions("For Export", () =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPayment = "R";
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "R";
			declaration.JE_OH_Importer = orgHeader2.PK;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted for export", invoiceLine1, ZString.Empty, ZString.Empty);
		});
	}

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

	protected override BaseJobDeclaration GetMeANewJobDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		return dec;
	}

	protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration)
	{
		var entryInstructions = declaration.CustomsEntryInstructions;
		return entryInstructions.FirstOrDefault() ?? entryInstructions.AddNew();
	}

	void AssertMethodOfPaymentsForInvoiceLine(ZString message, JobComInvoiceLine line, ZString mop, ZString mop2)
	{
		AssertEquals("MethodOfPayment for " + message, mop, line.ZG_MethodOfPayment);
		AssertEquals("MethodOfPayment2 for " + message, mop2, line.ZG_MethodOfPayment2);
	}
}
