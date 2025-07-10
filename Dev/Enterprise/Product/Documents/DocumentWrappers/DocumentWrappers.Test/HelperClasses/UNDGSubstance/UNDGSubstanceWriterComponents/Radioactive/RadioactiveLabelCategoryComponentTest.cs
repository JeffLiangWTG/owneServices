using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class RadioactiveLabelCategoryComponentTest : TestCaseWithFactory
	{
		public void TestRadioactiveLabelCategoryIsNullOrEmpty()
		{
			var dgItem = Factory.New<UNDGDataItem>();
			var wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			var component = new RadioactiveLabelCategoryComponent() as IUNDGSummaryWriterComponent;

			AssertNotEquals("RADIOACTIVE WHITE-I LABEL", component.Write(wrapper));
		}

		public void TestRadioactiveLabelCategoryForWH1() =>
			TestRadioactiveLabelCategoryComponent(RadioactiveLabelCategoryList.Codes.WhiteI, "RADIOACTIVE WHITE-I LABEL");

		public void TestRadioactiveLabelCategoryForYL2() =>
			TestRadioactiveLabelCategoryComponent(RadioactiveLabelCategoryList.Codes.YellowII, "RADIOACTIVE YELLOW-II LABEL");

		public void TestRadioactiveLabelCategoryForYL3() =>
			TestRadioactiveLabelCategoryComponent(RadioactiveLabelCategoryList.Codes.YellowIII, "RADIOACTIVE YELLOW-III LABEL");

		void TestRadioactiveLabelCategoryComponent(string labelCategory, string expectedLabel)
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "000";

			Factory.Save();

			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "000", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();
			var dgItem = Factory.New<UNDGDataItem>();
			dgItem.DI_RadioactiveLabelCategory = labelCategory;
			dgItem.LinkDefault(dgSubstance);

			var wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			var component = new RadioactiveLabelCategoryComponent() as IUNDGSummaryWriterComponent;

			AssertEquals(expectedLabel, component.Write(wrapper));
		}
	}
}
