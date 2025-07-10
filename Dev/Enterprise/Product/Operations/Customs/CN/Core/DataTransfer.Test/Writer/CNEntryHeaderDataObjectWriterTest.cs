using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using CustomsChargeTypeList = Enterprise.Customs.CN.Business.CustomsChargeTypeList;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNEntryHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateAddInfo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "瑞士", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("AEO", "AEO001", Core.Constants.CountryCodes.Switzerland);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_OH_Importer = orgHeader.PK;

			var overseasPatry = declaration.SupplierDocumentaryAddress;
			overseasPatry.E2_AddressOverride = true;
			overseasPatry.OverseasPartyCodeType = "AEO";
			overseasPatry.OverseasPartyCode = "AEO1";

			var instruction = Factory.New<Business.CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CustomsMessageRemarks = "CustomMessagesRemarks1";

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_MarksAndNumbers = "Marks And Numbers";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_LinePrice = 30;

			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();

			var freightCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			freightCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freightCharge.J7_Amount = 4;
			freightCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			freightCharge.J7_IsDutiable = false;
			freightCharge.J7_IsGSTApplicable = true;
			freightCharge.J7_DistributeBy = "VAL";
			freightCharge.J7_FullOrPartialApportionment = "PAA";

			var insuranceCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			insuranceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			insuranceCharge.J7_IsDutiable = false;
			insuranceCharge.J7_IsGSTApplicable = true;
			insuranceCharge.J7_Amount = 5;
			insuranceCharge.J7_DistributeBy = "VAL";
			insuranceCharge.J7_FullOrPartialApportionment = "PAA";

			var otherFeeCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			otherFeeCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Royalty;
			otherFeeCharge.J7_IsDutiable = false;
			otherFeeCharge.J7_IsGSTApplicable = false;
			otherFeeCharge.J7_Amount = 1;
			otherFeeCharge.J7_DistributeBy = "VAL";
			otherFeeCharge.J7_FullOrPartialApportionment = "PAA";

			Factory.Save();

			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);
			var entryHeader = declaration.CustomsEntryHeaders.Cast<Business.CusEntryHeader>().FirstOrDefault();

			var writer = new CNEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryHeader);

			var addInfos = result.AddInfoCollection;
			AssertHasAddInfo(addInfos, "FreightFeeCurrencyCode", "CNY");
			AssertHasAddInfo(addInfos, "FreightFeeMarkCode", "3");
			AssertHasAddInfo(addInfos, "FreightFeeAmount", "4.0000");
			AssertHasAddInfo(addInfos, "InsuranceFeeCurrencyCode", "CNY");
			AssertHasAddInfo(addInfos, "InsuranceFeeMarkCode", "3");
			AssertHasAddInfo(addInfos, "InsuranceFeeAmount", "5.0000");
			AssertHasAddInfo(addInfos, "OtherFeeCurrencyCode", "CNY");
			AssertHasAddInfo(addInfos, "OtherFeeMarkCode", "3");
			AssertHasAddInfo(addInfos, "OtherFeeAmount", "1.0000");
			AssertHasAddInfo(addInfos, "OverseasPartyCode", "AEO1");
			AssertHasAddInfo(addInfos, "MarksAndNumbers", "Marks And Numbers");
			AssertHasAddInfo(addInfos, "Remarks", "CustomMessagesRemarks1");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "001Y";
			instruction.BillOfLading = "BILL001";
			instruction.CustomsMessageRemarks = null;
			addInfos = writer.GetDataObject(entryHeader).AddInfoCollection;
			AssertHasAddInfo(addInfos, "BillOfLading", "BILL001");
			AssertHasAddInfo(addInfos, "VesselName", "BUNGA DELIMA");
			AssertHasAddInfo(addInfos, "Voyage", "001Y");

			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			declaration.DoMerge(notifier);
			entryHeader = declaration.CustomsEntryHeaders.Cast<Business.CusEntryHeader>().FirstOrDefault();

			writer = new CNEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			result = writer.GetDataObject(entryHeader);

			addInfos = result.AddInfoCollection;
			AssertNoAddInfo(addInfos, "FreightFeeCurrencyCode");
			AssertNoAddInfo(addInfos, "FreightFeeMarkCode");
			AssertNoAddInfo(addInfos, "FreightFeeAmount");
			AssertNoAddInfo(addInfos, "InsuranceFeeCurrencyCode");
			AssertNoAddInfo(addInfos, "InsuranceFeeMarkCode");
			AssertNoAddInfo(addInfos, "InsuranceFeeAmount");
			AssertNoAddInfo(addInfos, "OtherFeeCurrencyCode");
			AssertNoAddInfo(addInfos, "OtherFeeMarkCode");
			AssertNoAddInfo(addInfos, "OtherFeeAmount");
			AssertNoAddInfo(addInfos, "Remarks");
		}

		static void AssertHasAddInfo(IEnumerable<UniversalAddInfo> addInfos, string key, string value)
		{
			AssertEquals("Should have an AddInfo wity key " + key, true, addInfos.Any(addInfo => addInfo.Key.GetValueOrDefault() == key));
			AssertEquals($"AddInfo with Key '{key}' should have Value '{value}'.", value, addInfos.Single(addInfo => addInfo.Key.GetValueOrDefault() == key).Value.GetValueOrDefault());
		}

		static void AssertNoAddInfo(IEnumerable<UniversalAddInfo> addInfos, string key)
		{
			Assert($"AddInfos should NOT have a item with Key '{key}'", !addInfos.Any(addInfo => addInfo.Key.GetValueOrDefault() == key));
		}
	}
}
