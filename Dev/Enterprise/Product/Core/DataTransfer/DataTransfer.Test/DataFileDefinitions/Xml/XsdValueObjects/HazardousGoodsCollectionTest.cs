using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.HazardousGoodsCollection))]
	sealed class HazardousGoodsCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestExportFromUNDGDataItems()
		{
			HazardousGoodsCollection collection = new HazardousGoodsCollection();
			UNDGDataItem item = Factory.New<UNDGDataItem>();
			UNDGSubstance subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "AS";
			subs.DG_UNNO = "AS";
			subs.DG_MP = "M";
			item.DI_DG = subs.PK;
			item.DI_TechnicalName = "TN";
			item.DI_IsLimitedQuantity = true;
			collection.ExportFromUNDGDataItems(new UNDGDataItem[] { item }, "", null);
			AssertEquals("TN", collection[0].TechnicalName);
			AssertEquals("should use sub's pollutant when master's is empty", "M", collection[0].MarinePollutant);
			AssertEquals(true, collection[0].PackedInLimitedQuantity);
			AssertEquals(true, collection[0].PackedInLimitedQuantitySpecified);

			item.DI_MPMarinePollutant = "S";
			item.DI_IsLimitedQuantity = false;
			subs.DG_MP = "C";
			collection = new HazardousGoodsCollection();
			collection.ExportFromUNDGDataItems(new UNDGDataItem[] { item }, "", null);
			AssertEquals("S", collection[0].MarinePollutant);
			AssertEquals(false, collection[0].PackedInLimitedQuantity);
			AssertEquals(false, collection[0].PackedInLimitedQuantitySpecified);
		}

		public void TestImportMarinePullutant()
		{
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;
			undg.DG_Code = "9999a";
			undg.DG_UNNO = "9999";
			undg.DG_Variant = "a";

			var provider = new UNDGDataItemProvider(Factory);
			var collection = new HazardousGoodsCollection();
			var xsdHaz = collection.AddNew();
			xsdHaz.UNDGCode = undg.DG_Code;

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
			HazardousGoodsCollection collection = new HazardousGoodsCollection();
			Xsd.HazardousGoods xsdHaz = collection.AddNew();
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
	}
}
