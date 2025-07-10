using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSConsolidatedCusTempStorageLine))]
	sealed class CUSPCSConsolidatedCusTempStorageLineTest : CusTempStorageLineTest<CUSPCSConsolidatedCusTempStorageLine>
	{
		public void TestSequenceNumberEnabled()
		{
			var storageLine = Factory.New<CUSPCSConsolidatedCusTempStorageLine>();
			AssertEquals(expected: false, storageLine.SequenceNumberEnabled);
		}

		public void TestFormattedOwnerReferenceNumber()
		{
			var storageLine = GetNewCusTempStorageLine();
			CombineAssertions(() =>
			{
				AssertEquals("Max length for Owner Ref", 44, storageLine.FormattedOwnerReferenceNumberInfo.MaxLength);

				storageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				storageLine.TSL_OwnerReferenceNumber = "ATB150000010320006000";
				AssertEquals("No formatting for type AWB", "ATB150000010320006000", storageLine.FormattedOwnerReferenceNumber);

				storageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				AssertEquals("ATB Formatting for type REG", "AT/B/15/000001/03/2000/6000", storageLine.FormattedOwnerReferenceNumber);
			});
		}

		public void TestTestFormattedOwnerReferenceNumber_Caption()
		{
			var storageLine = Factory.New<CUSPCSConsolidatedCusTempStorageLine>();
			AssertCaption(storageLine.FormattedOwnerReferenceNumberInfo, "Reference");
		}

		public void TestTestFormattedOwnerReferenceNumber_List()
		{
			AssertHasCustomAttribute<ListAttribute>
				(typeof(CUSPCSConsolidatedCusTempStorageLine), nameof(CUSPCSConsolidatedCusTempStorageLine.FormattedOwnerReferenceNumber), includesInherit: false, a => a.ListDataSourceMember == nameof(CUSPCSConsolidatedCusTempStorageLine.Lookups) + "." + nameof(CUSPCSConsolidatedCusTempStorageLineLookups.CusTempStorageRegLineCollection));
		}

		public void TestCusTempStorageLinesTo()
		{
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			var splitStorageLine = consolidatedStorageLine.CusTempStorageLinesTo.AddNew();
			splitStorageLine.TSL_GoodsDescription = "GoodsDes";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var reloadedConsolidatedStorageLine = anotherFactory.Load<CUSPCSConsolidatedCusTempStorageLine>(consolidatedStorageLine.PK);
			AssertEquals(1, reloadedConsolidatedStorageLine.CusTempStorageLinesTo.Count);
			AssertEquals(splitStorageLine.PK, reloadedConsolidatedStorageLine.CusTempStorageLinesTo[0].PK);
			AssertEquals("GoodsDes", reloadedConsolidatedStorageLine.CusTempStorageLinesTo[0].TSL_GoodsDescription);
		}

		public void TestDelete()
		{
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			var splitStorageLine = consolidatedStorageLine.CusTempStorageLinesTo.AddNew();
			splitStorageLine.TSL_GoodsDescription = "GoodsDes";

			Factory.Save();

			AssertEquals(2, Factory.GetDatabaseCount(typeof(CusTempStorageLine)));

			consolidatedStorageLine.Dec.Delete();
			Factory.Save();

			AssertEquals("ConsolidatedStorageLine and all SplitCusTempStorageLines are deleted", 0, Factory.GetDatabaseCount(typeof(CusTempStorageLine)));
		}

		public void TestLoadConsolidatedLine()
		{
			var storageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = storageHeader.CUSPCSCusTempStorageDecs.AddNew();
			var consolidatedStorageLine = storageDec.ConsolidatedCusTempStorageLine;
			var toLine1 = consolidatedStorageLine.CusTempStorageLinesTo.AddNew();
			var toLine2 = consolidatedStorageLine.CusTempStorageLinesTo.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDec = newFactory.Load<CUSPCSCusTempStorageDec>(storageDec.PK);
			var consolidatedLineLoaded = CUSPCSConsolidatedCusTempStorageLine.LoadOrCreate(reloadedDec);
			CombineAssertions(() =>
			{
				AssertEquals("Consolidated Line is Reloaded", consolidatedLineLoaded.PK, consolidatedStorageLine.PK);
				AssertContainsExactElementsInAnyOrder("Line To Collection contains the correct elements", new ZGuid[] { toLine1.PK, toLine2.PK }, consolidatedLineLoaded.CusTempStorageLinesTo.Select(x => x.PK));
			});
		}

		public void TestCreateConsolidatedLine()
		{
			var storageLineCreated = CUSPCSConsolidatedCusTempStorageLine.LoadOrCreate(Factory.New<CUSPCSCusTempStorageDec>());
			AssertNotNull(storageLineCreated);
		}

		public void TestSetTSL_LineNoPopulatesRelatedRegLineProperties()
		{
			_ = PrepareRegHeaderAndRegLineAndSafe();
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			consolidatedStorageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			consolidatedStorageLine.FormattedOwnerReferenceNumber = "ATB123";
			AssertRelatedRegLinePropertiesEmpty("Before setting TSL_LineNo", consolidatedStorageLine);

			consolidatedStorageLine.TSL_LineNo = 2;
			AssertRelatedRegLinePropertiesPopulated("After Setting TSL_LineNo", consolidatedStorageLine);
		}

		public void TestSetTestFormattedOwnerReferenceNumberPopulatesRelatedRegLineProperties()
		{
			_ = PrepareRegHeaderAndRegLineAndSafe();
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			consolidatedStorageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			consolidatedStorageLine.TSL_LineNo = 2;
			AssertRelatedRegLinePropertiesEmpty("Before setting FormattedOwnerReferenceNumber", consolidatedStorageLine);

			consolidatedStorageLine.FormattedOwnerReferenceNumber = "ATB123";
			AssertRelatedRegLinePropertiesPopulated("After setting FormattedOwnerReferenceNumber", consolidatedStorageLine);
		}

		public void TestUpdatePropertiesFromRegLine()
		{
			var regLine = PrepareRegHeaderAndRegLineAndSafe();
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			consolidatedStorageLine.UpdatePropertiesFromRegLine(regLine);
			AssertEquals("UpdatePropertiesFromRegLine with valid regLine - TSL_LineNo", 2, consolidatedStorageLine.TSL_LineNo);
			AssertEquals("UpdatePropertiesFromRegLine with valid regLine - FormattedOwnerReferenceNumber", "ATB123", consolidatedStorageLine.FormattedOwnerReferenceNumber);

			consolidatedStorageLine.UpdatePropertiesFromRegLine(null);
			AssertEquals("UpdatePropertiesFromRegLine with null - TSL_LineNo", 1, consolidatedStorageLine.TSL_LineNo);
			AssertEquals("UpdatePropertiesFromRegLine with null - FormattedOwnerReferenceNumber", ZString.Empty, consolidatedStorageLine.FormattedOwnerReferenceNumber);
		}

		public void TestRelatedRegLineCustomerReference()
		{
			var regLine = PrepareRegHeaderAndRegLineAndSafe();
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			consolidatedStorageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly", true, consolidatedStorageLine.RelatedRegLineCustomerReferenceInfo.ReadOnly);
				AssertCaption(consolidatedStorageLine.RelatedRegLineCustomerReferenceInfo, "Customer Reference");
				AssertEquals("No RelatedRegLine", ZString.Empty, consolidatedStorageLine.RelatedRegLineCustomerReference);

				consolidatedStorageLine.UpdatePropertiesFromRegLine(regLine);
				AssertEquals("Has RelatedRegLine", "InternalReference", consolidatedStorageLine.RelatedRegLineCustomerReference);
			});
		}

		public void TestRelatedRegLineOwnerReference()
		{
			var regLine = PrepareRegHeaderAndRegLineAndSafe();
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			consolidatedStorageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly", true, consolidatedStorageLine.RelatedRegLineOwnerReferenceInfo.ReadOnly);
				AssertCaption(consolidatedStorageLine.RelatedRegLineOwnerReferenceInfo, "Owner Reference");
				AssertEquals("No RelatedRegLine", ZString.Empty, consolidatedStorageLine.RelatedRegLineOwnerReference);

				consolidatedStorageLine.UpdatePropertiesFromRegLine(regLine);
				AssertEquals("Has RelatedRegLine", "ZZZ - ABCDEFG", consolidatedStorageLine.RelatedRegLineOwnerReference);
			});
		}

		public void TestRelatedRegLinePackagesAndPackType()
		{
			var regLine = PrepareRegHeaderAndRegLineAndSafe();
			var regLine2 = regLine.RegHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 3;
			regLine2.SRL_PackagesRemaining = 0;
			regLine2.SRL_PackageType = "CT";
			Factory.Save();

			var consolidatedStorageLine = GetNewCusTempStorageLine();
			consolidatedStorageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly", true, consolidatedStorageLine.RelatedRegLinePackagesAndPackageTypeInfo.ReadOnly);
				AssertCaption(consolidatedStorageLine.RelatedRegLinePackagesAndPackageTypeInfo, "Packages");
				AssertEquals("No RelatedRegLine", ZString.Empty, consolidatedStorageLine.RelatedRegLinePackagesAndPackageType);

				consolidatedStorageLine.UpdatePropertiesFromRegLine(regLine);
				AssertEquals("Has RelatedRegLine: remainingPackages > 0", "5 CT", consolidatedStorageLine.RelatedRegLinePackagesAndPackageType);

				consolidatedStorageLine.UpdatePropertiesFromRegLine(regLine2);
				AssertEquals("Has RelatedRegLine: remainingPackages = 0", "0", consolidatedStorageLine.RelatedRegLinePackagesAndPackageType);
			});
		}

		public void TestRelatedRegLineGoodsDescription()
		{
			var regLine = PrepareRegHeaderAndRegLineAndSafe();
			var consolidatedStorageLine = GetNewCusTempStorageLine();
			consolidatedStorageLine.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly", true, consolidatedStorageLine.RelatedRegLineGoodsDescriptionInfo.ReadOnly);
				AssertCaption(consolidatedStorageLine.RelatedRegLineGoodsDescriptionInfo, "Goods Description");
				AssertEquals("No RelatedRegLine", ZString.Empty, consolidatedStorageLine.RelatedRegLineGoodsDescription);

				consolidatedStorageLine.UpdatePropertiesFromRegLine(regLine);
				AssertEquals("Has RelatedRegLine", "Description", consolidatedStorageLine.RelatedRegLineGoodsDescription);
			});
		}

		protected override CUSPCSConsolidatedCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OH_Customer = customer.PK;

			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			return storageDec.ConsolidatedCusTempStorageLine;
		}

		protected override Type GetDecType() => typeof(CUSPCSCusTempStorageDec);

		protected override Type GetLookupType() => typeof(CUSPCSConsolidatedCusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CUSPCSConsolidatedCusTempStorageLineValidation);

		CusTempStorageRegLine PrepareRegHeaderAndRegLineAndSafe()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
			regHeader.SRH_Reference = "ATB123";
			regHeader.SRH_InternalReference = "InternalReference";
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 2;
			regLine.SRL_OwnerReferenceType = "ZZZ";
			regLine.SRL_OwnerReference = "ABCDEFG";
			regLine.SRL_PackagesRemaining = 5;
			regLine.SRL_PackageType = "CT";
			regLine.SRL_GoodsDescription = "Description";
			regLine.SRL_CustomsStatus = CustomsStatusList.Codes.TST;
			Factory.Save();
			return regLine;
		}

		void AssertRelatedRegLinePropertiesEmpty(string testCaseDescription, CUSPCSConsolidatedCusTempStorageLine consolidatedStorageLine)
		{
			CombineAssertions(testCaseDescription, () =>
			{
				AssertEquals("RelatedRegLineCustomerReference", ZString.Empty, consolidatedStorageLine.RelatedRegLineCustomerReference);
				AssertEquals("RelatedRegLineOwnerReference", ZString.Empty, consolidatedStorageLine.RelatedRegLineOwnerReference);
				AssertEquals("RelatedRegLinePackagesAndPackageType", ZString.Empty, consolidatedStorageLine.RelatedRegLinePackagesAndPackageType);
				AssertEquals("RelatedRegLineGoodsDescription", ZString.Empty, consolidatedStorageLine.RelatedRegLineGoodsDescription);
			});
		}

		void AssertRelatedRegLinePropertiesPopulated(string testCaseDescription, CUSPCSConsolidatedCusTempStorageLine consolidatedStorageLine)
		{
			CombineAssertions(testCaseDescription, () =>
			{
				AssertEquals("RelatedRegLineCustomerReference", "InternalReference", consolidatedStorageLine.RelatedRegLineCustomerReference);
				AssertEquals("RelatedRegLineOwnerReference", "ZZZ - ABCDEFG", consolidatedStorageLine.RelatedRegLineOwnerReference);
				AssertEquals("RelatedRegLinePackagesAndPackageType", "5 CT", consolidatedStorageLine.RelatedRegLinePackagesAndPackageType);
				AssertEquals("RelatedRegLineGoodsDescription", "Description", consolidatedStorageLine.RelatedRegLineGoodsDescription);
			});
		}

		void AssertCaption(ZPropertyInfo info, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} - Caption", caption, propertyData.Caption);
		}
	}
}
