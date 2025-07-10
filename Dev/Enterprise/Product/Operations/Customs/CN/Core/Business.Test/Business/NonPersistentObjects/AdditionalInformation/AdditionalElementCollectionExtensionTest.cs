using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class AdditionalElementCollectionExtensionTest : TestCaseWithFactory
	{
		public void TestSetProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("00010", "包装规格");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "2713200000";
			var collection = pivot.AdditionalInformationCodes;
			collection.SetNameOfGoods("品名CUS");
			AssertEquals(1, collection.Count);
			collection.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Both, "XXXXX||无其他");
			AssertEquals(3, collection.Count);
			var collectionAddinfo = collection.Cast<AdditionalInformation>();
			AssertHasOneWithData(collectionAddinfo, "00000", "品名CUS");
			AssertHasOneWithData(collectionAddinfo, "00423", "XXXXX");
			AssertHasOneWithData(collectionAddinfo, "99999", "无其他");
			collection.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Both, "XXXXX");
			AssertHasOneWithData(collectionAddinfo, "00423", "XXXXX");
			AssertHasOneWithData(collectionAddinfo, "99999", "");
		}

		void AssertHasOneWithData(IEnumerable<AdditionalInformation> collection, ZString code, ZString data)
		{
			AssertEquals(1, collection.Count(addinfo => addinfo.CY_Code == code));
			AssertEquals(data, collection.FirstOrDefault(addinfo => addinfo.CY_Code == code).CY_Data);
		}

		public void TestGetAndSetSpecModel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00010", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00010", "包装规格");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "2713200000";
			AssertEquals("GetSpecModel", "||", pivot.AdditionalInformationCodes.GetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Both));
			pivot.AdditionalInformationCodes.AddNew("00423", "XXXXX");
			pivot.AdditionalInformationCodes.AddNew("00010", "1千克/箱");
			pivot.AdditionalInformationCodes.AddNew("00352", "YYYYY");
			AssertEquals("GetSpecModel", "1千克/箱|XXXXX|YYYYY", pivot.AdditionalInformationCodes.GetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Both));
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Both, "A|B|C");
			AssertEquals("A", pivot.AdditionalInformationCodes["00010"].CY_Data);
			AssertEquals("B", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("C", pivot.AdditionalInformationCodes["00352"].CY_Data);
		}

		public void TestGetAndSetSpecModelWithADDCVD()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00010", "00423", "00352", "99999", OriginalManufacturerNameCNStrategy.AdditionalElementCode, OriginalManufacturerNameENStrategy.AdditionalElementCode, AntiDumpingDutyRateStrategy.AdditionalElementCode, CountervailingDutyRateStrategy.AdditionalElementCode, MeetsPricePromiseStrategy.AdditionalElementCode);
			helper.CreateAdditionalElement("00010", "包装规格");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("99999", "其他");
			helper.CreateAdditionalElement(OriginalManufacturerNameCNStrategy.AdditionalElementCode, "原厂商中文名称");
			helper.CreateAdditionalElement(OriginalManufacturerNameENStrategy.AdditionalElementCode, "原厂商英文名称");
			helper.CreateAdditionalElement(AntiDumpingDutyRateStrategy.AdditionalElementCode, "反倾销税率");
			helper.CreateAdditionalElement(CountervailingDutyRateStrategy.AdditionalElementCode, "反补贴税率");
			helper.CreateAdditionalElement(MeetsPricePromiseStrategy.AdditionalElementCode, "是否符合价格承诺");
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "2713200000";
			AssertEquals("GetSpecModel", "||||<><><><><>", pivot.AdditionalInformationCodes.GetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering));
			AssertEquals("GetSpecModel", "||", pivot.AdditionalInformationCodes.GetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Exiting));
			pivot.AdditionalInformationCodes.AddNew("00423", "XXXXX");
			pivot.AdditionalInformationCodes.AddNew("00010", "1千克/箱");
			pivot.AdditionalInformationCodes.AddNew("00352", "YYYYY");
			pivot.AdditionalInformationCodes.AddNew("99999", "");
			pivot.AdditionalInformationCodes.AddNew(OriginalManufacturerNameCNStrategy.AdditionalElementCode, "德拉克通信法国集团公司");
			pivot.AdditionalInformationCodes.AddNew(OriginalManufacturerNameENStrategy.AdditionalElementCode, "DrakaComteqFranceSAS");
			pivot.AdditionalInformationCodes.AddNew(AntiDumpingDutyRateStrategy.AdditionalElementCode, "0.129");
			pivot.AdditionalInformationCodes.AddNew(CountervailingDutyRateStrategy.AdditionalElementCode, "");
			pivot.AdditionalInformationCodes.AddNew(MeetsPricePromiseStrategy.AdditionalElementCode, "0");
			AssertEquals("GetSpecModel", "1千克/箱|XXXXX|YYYYY||<德拉克通信法国集团公司><DrakaComteqFranceSAS><0.129><><0>", pivot.AdditionalInformationCodes.GetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering));
			AssertEquals("GetSpecModel", "1千克/箱|XXXXX|YYYYY", pivot.AdditionalInformationCodes.GetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Exiting));
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering, "");
			AssertEquals("", pivot.AdditionalInformationCodes["00010"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes["99999"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameCNStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameENStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[AntiDumpingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[CountervailingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[MeetsPricePromiseStrategy.AdditionalElementCode].CY_Data);
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering, "A|B|C");
			AssertEquals("A", pivot.AdditionalInformationCodes["00010"].CY_Data);
			AssertEquals("B", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("C", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes["99999"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameCNStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameENStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[AntiDumpingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[CountervailingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[MeetsPricePromiseStrategy.AdditionalElementCode].CY_Data);
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering, "A|B|C|D");
			AssertEquals("A", pivot.AdditionalInformationCodes["00010"].CY_Data);
			AssertEquals("B", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("C", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("D", pivot.AdditionalInformationCodes["99999"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameCNStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameENStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[AntiDumpingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[CountervailingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[MeetsPricePromiseStrategy.AdditionalElementCode].CY_Data);
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering, "A|B|C||<><><><><>");
			AssertEquals("A", pivot.AdditionalInformationCodes["00010"].CY_Data);
			AssertEquals("B", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("C", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes["99999"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameCNStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameENStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[AntiDumpingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[CountervailingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[MeetsPricePromiseStrategy.AdditionalElementCode].CY_Data);
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering, "A|B|C|D|<E><><F>");
			AssertEquals("A", pivot.AdditionalInformationCodes["00010"].CY_Data);
			AssertEquals("B", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("C", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("D", pivot.AdditionalInformationCodes["99999"].CY_Data);
			AssertEquals("E", pivot.AdditionalInformationCodes[OriginalManufacturerNameCNStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[OriginalManufacturerNameENStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("F", pivot.AdditionalInformationCodes[AntiDumpingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[CountervailingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[MeetsPricePromiseStrategy.AdditionalElementCode].CY_Data);
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(pivot.UniversalTariff, EnteringOrExiting.Entering, "A|B|C||<D><E><F><><H>");
			AssertEquals("A", pivot.AdditionalInformationCodes["00010"].CY_Data);
			AssertEquals("B", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("C", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes["99999"].CY_Data);
			AssertEquals("D", pivot.AdditionalInformationCodes[OriginalManufacturerNameCNStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("E", pivot.AdditionalInformationCodes[OriginalManufacturerNameENStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("F", pivot.AdditionalInformationCodes[AntiDumpingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("", pivot.AdditionalInformationCodes[CountervailingDutyRateStrategy.AdditionalElementCode].CY_Data);
			AssertEquals("H", pivot.AdditionalInformationCodes[MeetsPricePromiseStrategy.AdditionalElementCode].CY_Data);
		}
	}
}
