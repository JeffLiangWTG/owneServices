using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		[TestDate(2006, 5, 12)]
		public void TestNotifyEffectiveDutyDateDirty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 05, 05);
			AssertEquals("Effective Duty Date", ZDateTime.Today, invoice1.EffectiveDutyDate);
			AssertEquals("Effective Duty Date", ZDateTime.Today, invoice2.EffectiveDutyDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2006, 05, 06);
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Effective Duty Date", new ZDateTime(2006, 05, 06), invoice1.EffectiveDutyDate);
			AssertEquals("Effective Duty Date", new ZDateTime(2006, 05, 06), invoice2.EffectiveDutyDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2006, 05, 04);
			AssertEquals("Effective Duty Date", new ZDateTime(2006, 05, 05), invoice1.EffectiveDutyDate);
			AssertEquals("Effective Duty Date", new ZDateTime(2006, 05, 05), invoice2.EffectiveDutyDate);

			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddDays(1);
			AssertEquals("PreCondition:DateOfFirstArrival is in the future", true, declaration.JE_DateOfFirstArrival.IsInTheFutureDatePartOnly);
			AssertEquals("Effective duty date is today when first arrival date is in the future", ZDateTime.Today, invoice1.EffectiveDutyDate);
			AssertEquals("Effective duty date is today when first arrival date is in the future", ZDateTime.Today, invoice2.EffectiveDutyDate);
		}

		public void TestHasValidCharges()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			InvoiceHeaderActiveCollection headerCollection = testDec.Invoices;
			AssertEquals("No valid charges yet", false, headerCollection.HasAnElementWithValidCharges());

			JobComInvoiceHeader invoice = headerCollection.AddNew();
			AssertEquals("No valid charges yet", false, headerCollection.HasAnElementWithValidCharges());

			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			charge.J7_Amount = 100;
			AssertEquals("No valid charges yet", false, headerCollection.HasAnElementWithValidCharges());

			charge.J7_RX_NKCurrency = "AUD";
			AssertEquals("Valid charges", true, headerCollection.HasAnElementWithValidCharges());
		}

		public void TestDefaultingIncoTermsOnNewInvoiceHeader()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.MiscServ.OM_EXDefaultIncoTerm = "XYZ";
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			AssertEquals("Invoive Header Inco Terms should be defaulted form Supplier", "XYZ", invoice.JZ_IncoTerm);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			AssertEquals("Invoive Header Inco Terms should be FOB for Exwarehouse", Core.Constants.IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);
		}

		public void TestDefaultingDreawbackFieldsOnNewInvoiceHeader()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDec.DrawbackHeaderAssesmentMethod = "A";
			testDec.AddInfo.ZA_EDN_Hidden = "EDN";
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			AssertEquals("Invoive Header fields should not defailt if not drawback", "", invoice.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Invoive Header fields should not defailt if not drawback", "", invoice.AddInfo.ZA_EDN_Hidden);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			AssertEquals("Invoive Header fields should defailt if drawback", "A", invoice2.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Invoive Header fields should defailt if drawback", "EDN", invoice2.AddInfo.ZA_EDN_Hidden);
		}
	}
}
