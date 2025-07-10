using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVariousCodeLists()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var taxType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, "MSC");
			helper.LoadOrCreateNewCusRateCode(Factory, "YYY", taxType.PK);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method Of Payment");
			var newMoP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(newMoP.PK, "Category", "Dec|Box47");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFee = entryLine.Fees.AddNew();
			AssertContains("YYY", entryLineFee.Lookups.ChargeTypeList.CodesAsString);
			AssertContains("ZZZ", entryLineFee.Lookups.MethodOfPaymentList.CodesAsString);
		}

		public void TestRateOverrideReasonList()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertEquals("ADD, OVR, PRE", entryLineFee.Lookups.RateOverrideReasonList.CodesAsString);
		}
	}
}
