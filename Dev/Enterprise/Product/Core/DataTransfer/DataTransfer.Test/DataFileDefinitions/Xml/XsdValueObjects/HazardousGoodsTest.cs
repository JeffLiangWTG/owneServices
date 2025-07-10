using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.HazardousGoods))]
	sealed class HazardousGoodsTest : ValueObjectTestCase
	{
		public void TestImportWithBlankCode()
		{
			UNDGDataItemProvider provider = new UNDGDataItemProvider(Factory);
			Xsd.HazardousGoods xsdHaz = new HazardousGoods();

			AssertEquals(0, provider.UNDGs.Count);
			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(0, provider.UNDGs.Count);

			xsdHaz.UNDGCode = "0014a";
			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(1, provider.UNDGs.Count);
		}

		public void TestCompileTimeCheck()
		{
			Xsd.HazardousGoods value = null;
			value = new Xsd.HazardousGoodsCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestImportMarinePollutant()
		{
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;
			undg.DG_Code = "9999a";

			var provider = new UNDGDataItemProvider(Factory);
			var xsdHaz = new HazardousGoods();
			xsdHaz.UNDGCode = undg.DG_Code;
			undg.DG_UNNO = "9999";
			undg.DG_Variant = "a";

			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(1, provider.UNDGs.Count);
			AssertEquals(undg.DG_MP, provider.UNDGs[0].DI_MPMarinePollutant);

			xsdHaz.MarinePollutant = UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code;
			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(1, provider.UNDGs.Count);
			AssertEquals("S", provider.UNDGs[0].DI_MPMarinePollutant);

			undg.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(1, provider.UNDGs.Count);
			AssertEquals(UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code, provider.UNDGs[0].DI_MPMarinePollutant);

			xsdHaz.MarinePollutant = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(1, provider.UNDGs.Count);
			AssertEquals("", provider.UNDGs[0].DI_MPMarinePollutant);

			xsdHaz.MarinePollutant = "";
			provider.UNDGs[0].DI_MPMarinePollutant = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;
			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(1, provider.UNDGs.Count);
			AssertEquals(UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code, provider.UNDGs[0].DI_MPMarinePollutant);
		}

		[ExpectNoExceptions]
		public void TestImportTechnicalName()
		{
			UNDGSubstance undg = Factory.New<UNDGSubstance>();
			undg.DG_TechName = "*";
			undg.DG_Code = "9999a";
			undg.DG_UNNO = "9999";
			undg.DG_Variant = "a";

			UNDGDataItemProvider provider = new UNDGDataItemProvider(Factory);
			Xsd.HazardousGoods xsdHaz = new HazardousGoods();
			xsdHaz.UNDGCode = undg.DG_Code;
			xsdHaz.TechnicalName = "some name that is really long, so long in fact that its length is over the one hundred and fifty character limit that has been set to keep storage costs low";

			xsdHaz.ImportSingleItemToUNDGDataItems(() => provider.UNDGs);
			AssertEquals(1, provider.UNDGs.Count);
			AssertEquals("some name that is really long, so long in fact that its length is over the one hundred and fifty character limit that has been set to keep storage cos", provider.UNDGs[0].DI_TechnicalName);
		}

		#region Test Classes

		class UNDGDataItemProvider : NonPersistentBusinessObject, IUNDGDataItemProvider
		{
			public UNDGDataItemProvider(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public UNDGDataItemCollection UNDGs
			{
				get { return uNDGs ?? (uNDGs = new UNDGDataItemCollection(this)); }
			}
			UNDGDataItemCollection uNDGs;

			bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;
		}

		#endregion

		public void TestExportSingleItemFromUNDGDataItems()
		{
			UNDGDataItem item = Factory.New<UNDGDataItem>();
			UNDGSubstance subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "AS";
			subs.DG_UNNO = "AS";
			subs.DG_MP = "M";
			item.DI_DG = subs.PK;
			item.DI_TechnicalName = "TN";
			item.DI_IsLimitedQuantity = true;
			HazardousGoods goods = new HazardousGoods();
			goods.ExportSingleItemFromUNDGDataItems(new UNDGDataItem[] { item });
			AssertEquals("TN", goods.TechnicalName);
			AssertEquals("should use sub's pollutant when master's is empty", "M", goods.MarinePollutant);
			AssertEquals(true, goods.PackedInLimitedQuantity);

			subs.DG_MP = "C";
			item.DI_MPMarinePollutant = "S";
			item.DI_IsLimitedQuantity = false;
			goods = new HazardousGoods();
			goods.ExportSingleItemFromUNDGDataItems(new UNDGDataItem[] { item });
			AssertEquals("S", goods.MarinePollutant);
			AssertEquals(false, goods.PackedInLimitedQuantity);
		}
	}
}
