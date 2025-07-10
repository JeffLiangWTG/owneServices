using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class AdditionalInformationHelperTest : TestCaseWithFactory
	{
		CNEntryHeaderTestData SetUpData() => CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("2713200000", "00000", "00010", "00423", "00352", "00009", "00005", "99999");
				helper.CreateAdditionalElement("00000", "品名");
				helper.CreateAdditionalElement("00423", "针入度");
				helper.CreateAdditionalElement("00352", "加工方法");
				helper.CreateAdditionalElement("00010", "包装规格");
				helper.CreateAdditionalElement("00009", "GTIN");
				helper.CreateAdditionalElement("00005", "CAS");
				helper.CreateAdditionalElement("99999", "其他");
				Factory.Save();
			});

		public void TestCorrectAdditionalInfoValues()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("2713200000", "00000", "00422", "00069", "99999");
				helper.CreateAdditionalElement("00000", "品名");
				helper.CreateAdditionalElement("00422", "品牌类型");
				helper.CreateAdditionalElement("00069", "出口享惠情况");
				helper.CreateAdditionalElement("99999", "其他");
				Factory.Save();
			}

			);
			item.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			item.JobDeclaration.JE_MessageSubType = DecTypeList.Codes.Both;
			item.JobDeclaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			AssertEquals("Precondition: ", EnteringOrExiting.Entering, invoiceLine.AdditionalInformationHelper.IsEnteringOrExiting);
			AssertEquals("Precondition: ", EnteringOrExiting.Exiting, invoiceLine.AdditionalInformation2Helper.IsEnteringOrExiting);
			invoiceLine.XC_GoodsSpecModel = "1|3|无其他";
			invoiceLine.XC_GoodsSpecModel2 = "1|3|无其他";
			invoiceLine.AdditionalInformationHelper.CorrectAdditionalInfoValues();
			invoiceLine.AdditionalInformation2Helper.CorrectAdditionalInfoValues();
			AssertEquals("Correct Additional Info Values", "1|3|无其他", invoiceLine.XC_GoodsSpecModel);
			AssertEquals("Correct Additional Info Values", "1||无其他", invoiceLine.XC_GoodsSpecModel2);
			invoiceLine.XC_GoodsSpecModel = "1|2";
			invoiceLine.XC_GoodsSpecModel2 = "1|2";
			invoiceLine.AdditionalInformationHelper.CorrectAdditionalInfoValues();
			invoiceLine.AdditionalInformation2Helper.CorrectAdditionalInfoValues();
			AssertEquals("Correct Additional Info Values", "1|3", invoiceLine.XC_GoodsSpecModel);
			AssertEquals("Correct Additional Info Values", "1|2", invoiceLine.XC_GoodsSpecModel2);
		}

		public void TestExtractedIngredient()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "720c28d9e797c0d8408d34da1b0ab981", "97cdf10b7b7091d04586060f0620788e");
				helper.CreateAdditionalElement("97cdf10b7b7091d04586060f0620788e", "填充物成分含量");
				helper.CreateAdditionalElement("720c28d9e797c0d8408d34da1b0ab981", "面料成分含量");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "50|100";
			AssertEquals("Value", "50", invoiceLine.AdditionalInformationHelper.ExtractedIngredient);
		}

		public void TestExtractedSpecification()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "99997", "99998", "01746");
				helper.CreateCustomsTariff("6202131001", "99997", "01746");
				helper.CreateAdditionalElement("99997", "包装规格");
				helper.CreateAdditionalElement("99998", "规格型号");
				helper.CreateAdditionalElement("01746", "规格");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "111|规格：222、型号：333|444";
			AssertEquals("Has 99998", "222", invoiceLine.AdditionalInformationHelper.ExtractedSpecification);

			invoiceLine.JI_Tariff = "6202131001";
			invoiceLine.XC_GoodsSpecModel = "555|666";
			AssertEquals("No 99998", "666", invoiceLine.AdditionalInformationHelper.ExtractedSpecification);
		}

		public void TestExtractedBrand()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "00422", "b2b702b8812a57b174a8f70a7c4faaa1", "baeb884cae1c682fd9c48a9bbc20b849");
				helper.CreateAdditionalElement("00422", "品牌类型");
				helper.CreateAdditionalElement("b2b702b8812a57b174a8f70a7c4faaa1", "品牌(中英文)");
				helper.CreateAdditionalElement("baeb884cae1c682fd9c48a9bbc20b849", "品牌(厂商)");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "A|B|C";
			AssertEquals("Value", "C", invoiceLine.AdditionalInformationHelper.ExtractedBrand);
		}

		public void TestExtractedModel()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "00441", "99998");
				helper.CreateCustomsTariff("6202131001", "01746", "00441");
				helper.CreateAdditionalElement("01746", "规格");
				helper.CreateAdditionalElement("00441", "型号");
				helper.CreateAdditionalElement("99998", "规格型号");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "111|规格：222、型号：333";
			AssertEquals("Has 99998", "333", invoiceLine.AdditionalInformationHelper.ExtractedModel);

			invoiceLine.JI_Tariff = "6202131001";
			invoiceLine.XC_GoodsSpecModel = "444|555";
			AssertEquals("No 99998", "555", invoiceLine.AdditionalInformationHelper.ExtractedModel);
		}

		public void TestExtractedManufactureDates()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("2203000000", "99997", "ef374a392b7934885636cee2f907884a");
				helper.CreateAdditionalElement("99997", "包装规格");
				helper.CreateAdditionalElement("ef374a392b7934885636cee2f907884a", "生产日期");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2203000000";
			invoiceLine.XC_GoodsSpecModel = "500毫升*12瓶/箱|20220211;20220213";
			var extractedManufactureDates = invoiceLine.AdditionalInformationHelper.ExtractedManufactureDates;
			AssertEquals("Count", 2, extractedManufactureDates.Length);
			AssertEquals("1st", new ZDateTime(2022, 02, 11), extractedManufactureDates[0]);
			AssertEquals("2nd", new ZDateTime(2022, 02, 13), extractedManufactureDates[1]);
		}

		public void TestCorrectAdditionalInfoValuesWithADDCVDElements()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("2713200000", "00000", "00422", "00069", "99999", OriginalManufacturerNameCNStrategy.AdditionalElementCode, OriginalManufacturerNameENStrategy.AdditionalElementCode, AntiDumpingDutyRateStrategy.AdditionalElementCode, CountervailingDutyRateStrategy.AdditionalElementCode, MeetsPricePromiseStrategy.AdditionalElementCode);
				helper.CreateAdditionalElement("00000", "品名");
				helper.CreateAdditionalElement("00422", "品牌类型");
				helper.CreateAdditionalElement("00069", "出口享惠情况");
				helper.CreateAdditionalElement("99999", "其他");
				helper.CreateAdditionalElement(OriginalManufacturerNameCNStrategy.AdditionalElementCode, "原厂商中文名称");
				helper.CreateAdditionalElement(OriginalManufacturerNameENStrategy.AdditionalElementCode, "原厂商英文名称");
				helper.CreateAdditionalElement(AntiDumpingDutyRateStrategy.AdditionalElementCode, "反倾销税率");
				helper.CreateAdditionalElement(CountervailingDutyRateStrategy.AdditionalElementCode, "反补贴税率");
				helper.CreateAdditionalElement(MeetsPricePromiseStrategy.AdditionalElementCode, "是否符合价格承诺");
				Factory.Save();
			}

			);
			item.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			item.JobDeclaration.JE_MessageSubType = DecTypeList.Codes.Both;
			item.JobDeclaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			AssertEquals("Precondition: ", EnteringOrExiting.Entering, invoiceLine.AdditionalInformationHelper.IsEnteringOrExiting);
			AssertEquals("Precondition: ", EnteringOrExiting.Exiting, invoiceLine.AdditionalInformation2Helper.IsEnteringOrExiting);
			invoiceLine.XC_GoodsSpecModel = "1|1|无其他|<1><2><3><4><5>";
			invoiceLine.XC_GoodsSpecModel2 = "1|1|无其他|<1><2><3><4><5>";
			invoiceLine.AdditionalInformationHelper.CorrectAdditionalInfoValues();
			invoiceLine.AdditionalInformation2Helper.CorrectAdditionalInfoValues();
			AssertEquals("Correct Additional Info Values", "1|3|无其他|<1><2><3><4><>", invoiceLine.XC_GoodsSpecModel);
			AssertEquals("Correct Additional Info Values", "1|1|无其他", invoiceLine.XC_GoodsSpecModel2);
		}

		public void TestCacheAdditionalElementValues()
		{
			var item = SetUpData();
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|||123-123-123|无其他";
			var additionalElementValues = invoiceLine.AdditionalInformationHelper.AdditionalElementValues;
			AssertSame("AdditionalElementValues should be cached", additionalElementValues, invoiceLine.AdditionalInformationHelper.AdditionalElementValues);
			for (var i = 0; i < additionalElementValues.Count(); i++)
			{
				AssertSame("AdditionalElementValues should be cached", additionalElementValues.ElementAt(i), invoiceLine.AdditionalInformationHelper.AdditionalElementValues.ElementAt(i));
			}

			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|||123-123-123";
			AssertNotEquals("AdditionalElementValues should be reset", additionalElementValues, invoiceLine.AdditionalInformationHelper.AdditionalElementValues);
			invoiceLine.JI_Tariff = "2713200010";
			AssertNotEquals("AdditionalElementValues should be reset", additionalElementValues, invoiceLine.AdditionalInformationHelper.AdditionalElementValues);
			item.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNotEquals("AdditionalElementValues should be reset", additionalElementValues, invoiceLine.AdditionalInformationHelper.AdditionalElementValues);
		}

		public void TestGetAdditionalElementValue()
		{
			var item = SetUpData();
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|||123-123-123|无其他";
			var result = invoiceLine.AdditionalInformationHelper.GetAdditionalElementValue(CASElementStrategy.AdditionalElementCode);
			AssertEquals("CAS value", "123-123-123", result);
		}

		public void TestIsStandardAdditionalInformation()
		{
			var item = SetUpData();
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|||123-123-123|无其他";
			Assert("IsStandard", invoiceLine.AdditionalInformationHelper.IsStandardAdditionalInformation());
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|||123-123-123";
			Assert("IsStandard", invoiceLine.AdditionalInformationHelper.IsStandardAdditionalInformation());
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX||";
			Assert("IsStandard", invoiceLine.AdditionalInformationHelper.IsStandardAdditionalInformation());
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|";
			Assert("IsStandard", invoiceLine.AdditionalInformationHelper.IsStandardAdditionalInformation());
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|";
			Assert("IsStandard", !invoiceLine.AdditionalInformationHelper.IsStandardAdditionalInformation());
			invoiceLine.XC_GoodsSpecModel = "1千克/箱";
			Assert("IsStandard", !invoiceLine.AdditionalInformationHelper.IsStandardAdditionalInformation());
		}

		public void TestGetMergeKeyFromGoodsSpecModel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			TariffView tariff = null;
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				tariff = helper.CreateCustomsTariff("2713200000", "00000", "00422", "00069", "00352", "00010", "00009", "00005", "99999");
				helper.CreateAdditionalElement("00000", "品名");
				helper.CreateAdditionalElement("00422", "品牌类型");
				helper.CreateAdditionalElement("00069", "出口享惠情况");
				helper.CreateAdditionalElement("00352", "加工方法");
				helper.CreateAdditionalElement("00010", "包装规格");
				helper.CreateAdditionalElement("00009", "GTIN");
				helper.CreateAdditionalElement("00005", "CAS");
				helper.CreateAdditionalElement("99999", "其他");
				helper.CreateTariffAttribute("COMMODITYTYPE", "ATP", tariff);
				Factory.Save();
			}

			);
			item.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			item.JobDeclaration.JE_MessageSubType = DecTypeList.Codes.Both;
			item.JobDeclaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			AssertEquals("Precondition: ", EnteringOrExiting.Entering, invoiceLine.AdditionalInformationHelper.IsEnteringOrExiting);
			AssertEquals("Precondition: ", EnteringOrExiting.Exiting, invoiceLine.AdditionalInformation2Helper.IsEnteringOrExiting);
			invoiceLine.XC_GoodsSpecModel = "1|3||1千克/箱|GTIN|CAS|无其他";
			invoiceLine.XC_GoodsSpecModel2 = "1|3||1千克/箱|GTIN|CAS|无其他";
			AssertEquals("1|3||1千克/箱|GTIN|CAS|无其他", invoiceLine.AdditionalInformationHelper.GetMergeKeyFromGoodsSpecModel());
			AssertEquals("1|3|GTIN|CAS", invoiceLine.AdditionalInformation2Helper.GetMergeKeyFromGoodsSpecModel());
			invoiceLine.XC_GoodsSpecModel = "1|3||1千克/箱";
			invoiceLine.XC_GoodsSpecModel2 = "1|3||1千克/箱";
			AssertEquals("1|3||1千克/箱", invoiceLine.AdditionalInformationHelper.GetMergeKeyFromGoodsSpecModel());
			AssertEquals("1|3||", invoiceLine.AdditionalInformation2Helper.GetMergeKeyFromGoodsSpecModel());
			invoiceLine.XC_GoodsSpecModel = "规格型号";
			invoiceLine.XC_GoodsSpecModel2 = "规格型号";
			AssertEquals("", invoiceLine.AdditionalInformationHelper.GetMergeKeyFromGoodsSpecModel());
			AssertEquals("", invoiceLine.AdditionalInformation2Helper.GetMergeKeyFromGoodsSpecModel());
		}

		public void TestGoodsSpecModelMaxLength()
		{
			AssertMaxLength(true, 1300);
			AssertMaxLength(false, 255);

			void AssertMaxLength(bool enableGoLiveDate, int expectedMaxLength)
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CNDecMessageV2020GoLiveDate, Core.Constants.CountryCodes.China, ZDateTime.Today, enableGoLiveDate))
				{
					AssertEquals(expectedMaxLength, AdditionalInformationHelper.GoodsSpecModelMaxLength);
				}
			}
		}

		public void TestGetMergedNameOfGoods()
		{
			var item = SetUpData();
			var entryLine = item.EntryLine;
			var invoiceLine = item.InvoiceLine;
			var invoiceHeader = item.InvoiceHeader;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.JI_NameOfGoods = "品名CUS";
			invoiceLine.JI_NameOfGoods2 = "品名REC";
			var result = AdditionalInformationHelper.GetMergedNameOfGoods(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedNameOfGoods", "品名CUS", result);
			result = AdditionalInformationHelper.GetMergedNameOfGoods(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformation2Helper));
			AssertEquals("GetMergedNameOfGoods", "品名REC", result);
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_Tariff = "2713200000";
			invoiceLine2.JI_NameOfGoods = "品名CUS2";
			invoiceLine2.JI_NameOfGoods2 = "品名REC2";
			entryLine.InvoiceLines.Add(invoiceLine2);
			result = AdditionalInformationHelper.GetMergedNameOfGoods(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedNameOfGoods", "品名CUS等", result);
			result = AdditionalInformationHelper.GetMergedNameOfGoods(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformation2Helper));
			AssertEquals("GetMergedNameOfGoods", "品名REC等", result);
		}

		public void TestGetMergedGoodsSpecModel()
		{
			var item = SetUpData();
			var entryLine = item.EntryLine;
			var invoiceHeader = item.InvoiceHeader;
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX||||无其他";
			invoiceLine.XC_GoodsSpecModel2 = "2千克/箱||YYYYY|||其他1";
			var result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克/箱|XXXXX||||无其他", result);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformation2Helper));
			AssertEquals("GetMergedGoodsSpecModel", "2千克/箱||YYYYY|||其他1", result);
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2713200000";
			invoiceLine2.XC_GoodsSpecModel = "1千克/箱|LLLLL||||其他";
			invoiceLine2.XC_GoodsSpecModel2 = "2千克/箱||DDDDD|||其他2";
			entryLine.InvoiceLines.Add(invoiceLine2);
			var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2713200000";
			invoiceLine3.XC_GoodsSpecModel = "1千克/箱|LLLLL||||其他";
			invoiceLine3.XC_GoodsSpecModel2 = "2千克/箱||AAAAA|||其他3";
			entryLine.InvoiceLines.Add(invoiceLine3);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克/箱|XXXXX/LLLLL||||其他", result);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformation2Helper));
			AssertEquals("GetMergedGoodsSpecModel", "2千克/箱||YYYYY等|||其他1/其他2/其他3", result);
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX||||";
			invoiceLine.XC_GoodsSpecModel2 = "2千克/箱||YYYYY|||";
			invoiceLine2.XC_GoodsSpecModel = "1千克/箱|LLLLL||||";
			invoiceLine2.XC_GoodsSpecModel2 = "2千克/箱||DDDDD|||";
			invoiceLine3.XC_GoodsSpecModel = "1千克/箱|LLLLL||||";
			invoiceLine3.XC_GoodsSpecModel2 = "2千克/箱||AAAAA|||";
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克/箱|XXXXX/LLLLL|", result);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformation2Helper));
			AssertEquals("GetMergedGoodsSpecModel", "2千克/箱||YYYYY等", result);
		}

		public void TestGetMergedGoodsSpecModel_99997()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("3824999990", "00000", "99997", "99999");
				helper.CreateAdditionalElement("00000", "品名");
				helper.CreateAdditionalElement("99997", "包装规格");
				helper.CreateAdditionalElement("99999", "其他");
				Factory.Save();
			}

			);
			var entryLine = item.EntryLine;
			var invoiceHeader = item.InvoiceHeader;
			var invoiceLine1 = item.InvoiceLine;
			invoiceLine1.JI_Tariff = "3824999990";
			invoiceLine1.JI_NameOfGoods = "品名";
			invoiceLine1.XC_GoodsSpecModel = "1千克x6罐/箱|其他";
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3824999990";
			invoiceLine2.JI_NameOfGoods = "品名";
			invoiceLine2.XC_GoodsSpecModel = "1千克x6罐/箱|其他";
			entryLine.InvoiceLines.Add(invoiceLine2);
			var result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克x6罐/箱|其他", result);
			invoiceLine2.XC_GoodsSpecModel = "1千克x12罐/箱|其他";
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克x6罐/箱|其他/1千克x12罐/箱", result);
			var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3824999990";
			invoiceLine3.JI_NameOfGoods = "品名";
			invoiceLine3.XC_GoodsSpecModel = "1千克x24罐/箱|其他";
			entryLine.InvoiceLines.Add(invoiceLine3);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克x6罐/箱|其他/1千克x12罐/箱等", result);
		}

		public void TestGetMergedGoodsSpecModel_99998()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("7318151090", "00000", "99998", "99999");
				helper.CreateAdditionalElement("00000", "品名");
				helper.CreateAdditionalElement("99998", "规格型号");
				helper.CreateAdditionalElement("99999", "其他");
				Factory.Save();
			}

			);
			var entryLine = item.EntryLine;
			var invoiceHeader = item.InvoiceHeader;
			var invoiceLine1 = item.InvoiceLine;
			invoiceLine1.JI_Tariff = "7318151090";
			invoiceLine1.JI_NameOfGoods = "品名";
			invoiceLine1.XC_GoodsSpecModel = "规格：G1、型号：X1|其他";
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7318151090";
			invoiceLine2.JI_NameOfGoods = "品名";
			invoiceLine2.XC_GoodsSpecModel = "规格：G1、型号：X1|其他";
			entryLine.InvoiceLines.Add(invoiceLine2);
			var result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "规格：G1、型号：X1|其他", result);
			invoiceLine2.XC_GoodsSpecModel = "规格：G2、型号：X2";
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "规格：G1、型号：X1|其他/规格：G2、型号：X2", result);
			var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7318151090";
			invoiceLine3.JI_NameOfGoods = "品名";
			invoiceLine3.XC_GoodsSpecModel = "规格：G2、型号：X3|无其他";
			entryLine.InvoiceLines.Add(invoiceLine3);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "规格：G1、型号：X1|其他/规格：G2、型号：X2等", result);
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine4.JI_Tariff = "7318151090";
			invoiceLine4.JI_NameOfGoods = "品名";
			invoiceLine4.XC_GoodsSpecModel = "规格：G3、型号：X3|其他";
			entryLine.InvoiceLines.Add(invoiceLine4);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "规格：G1、型号：X1|其他/规格：G2等、型号：X2等", result);
			invoiceLine3.XC_GoodsSpecModel = "G3|无其他";
			invoiceLine4.XC_GoodsSpecModel = "G4|其他";
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "规格：G1、型号：X1|其他/规格：G2等、型号：X2等", result);
			invoiceLine2.XC_GoodsSpecModel = "G2|其他";
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "规格：G1、型号：X1|其他/G2等", result);
		}

		public void TestGetMergedGoodsSpecModelWithADDCVDElements()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("2713200000", "00000", "00010", "00423", "00352", "00009", "00005", "99999", OriginalManufacturerNameCNStrategy.AdditionalElementCode, OriginalManufacturerNameENStrategy.AdditionalElementCode, AntiDumpingDutyRateStrategy.AdditionalElementCode, CountervailingDutyRateStrategy.AdditionalElementCode, MeetsPricePromiseStrategy.AdditionalElementCode);
				helper.CreateAdditionalElement("00000", "品名");
				helper.CreateAdditionalElement("00423", "针入度");
				helper.CreateAdditionalElement("00352", "加工方法");
				helper.CreateAdditionalElement("00010", "包装规格");
				helper.CreateAdditionalElement("00009", "GTIN");
				helper.CreateAdditionalElement("00005", "CAS");
				helper.CreateAdditionalElement("99999", "其他");
				helper.CreateAdditionalElement(OriginalManufacturerNameCNStrategy.AdditionalElementCode, "原厂商中文名称");
				helper.CreateAdditionalElement(OriginalManufacturerNameENStrategy.AdditionalElementCode, "原厂商英文名称");
				helper.CreateAdditionalElement(AntiDumpingDutyRateStrategy.AdditionalElementCode, "反倾销税率");
				helper.CreateAdditionalElement(CountervailingDutyRateStrategy.AdditionalElementCode, "反补贴税率");
				helper.CreateAdditionalElement(MeetsPricePromiseStrategy.AdditionalElementCode, "是否符合价格承诺");
				Factory.Save();
			}

			);
			item.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine = item.EntryLine;
			var invoiceHeader = item.InvoiceHeader;
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX||||无其他";
			invoiceLine.XC_GoodsSpecModel2 = "2千克/箱||YYYYY|||其他1|<A><B><C><D><E>";
			var result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克/箱|XXXXX||||无其他|<><><><><>", result);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformation2Helper));
			AssertEquals("GetMergedGoodsSpecModel", "2千克/箱||YYYYY|||其他1|<A><B><C><D><E>", result);
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2713200000";
			invoiceLine2.XC_GoodsSpecModel = "1千克/箱|LLLLL||||其他";
			invoiceLine2.XC_GoodsSpecModel2 = "2千克/箱||DDDDD|||其他2|<A><B><C><D><E>";
			entryLine.InvoiceLines.Add(invoiceLine2);
			var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2713200000";
			invoiceLine3.XC_GoodsSpecModel = "1千克/箱|LLLLL||||其他|<><><><><>";
			invoiceLine3.XC_GoodsSpecModel2 = "2千克/箱||AAAAA|||其他3|<A><B><C><D><E>";
			entryLine.InvoiceLines.Add(invoiceLine3);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformationHelper));
			AssertEquals("GetMergedGoodsSpecModel", "1千克/箱|XXXXX/LLLLL||||其他|<><><><><>", result);
			result = AdditionalInformationHelper.GetMergedGoodsSpecModel(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.AdditionalInformation2Helper));
			AssertEquals("GetMergedGoodsSpecModel", "2千克/箱||YYYYY等|||其他1/其他2/其他3|<A><B><C><D><E>", result);
		}
	}
}
