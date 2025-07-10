using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSCusTempStorageDec))]
	class CUSPCSCusTempStorageDecTest : CusTempStorageDecAbstractTest<CUSPCSCusTempStorageDec>
	{
		public void TestConsolidatedCusTempStorageLine()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var consolidatedLine = storageDec.ConsolidatedCusTempStorageLine;
			consolidatedLine.TSL_OwnerReferenceType = "AAA";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var reloadedStorageDec = anotherFactory.Load<CUSPCSCusTempStorageDec>(storageDec.PK);
			var reloadedConsolidatedLine = reloadedStorageDec.ConsolidatedCusTempStorageLine;
			AssertEquals(consolidatedLine.PK, reloadedConsolidatedLine.PK);
			AssertEquals("AAA", reloadedConsolidatedLine.TSL_OwnerReferenceType);

			reloadedStorageDec.Delete();
			Assert(reloadedConsolidatedLine.IsDeleted);
		}

		public void TestCusTempStorageLines()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			var presenter = Factory.NewWithValidTestData<OrgAddress>();
			var representative = Factory.NewWithValidTestData<OrgAddress>();

			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_GB = GlbBranch.CurrentBranch.PK;
			header.SJH_JobReference = "From1";
			header.SJH_OH_Customer = customer.PK;
			header.SJH_OA_Presenter = presenter.PK;
			header.SJH_OA_Representative = representative.PK;

			var cusTempStorageDec = Factory.New<CUSPCSCusTempStorageDec>();
			cusTempStorageDec.STH_SJH = header.PK;
			cusTempStorageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;

			var line1 = cusTempStorageDec.ConsolidatedCusTempStorageLine;
			line1.TSL_LineNo = 1;
			var line2 = cusTempStorageDec.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			line2.TSL_STH = cusTempStorageDec.PK;

			var line3 = line1.CusTempStorageLinesTo.AddNew();
			line3.TSL_LineNo = 1;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDec = newFactory.Load<CUSPCSCusTempStorageDec>(cusTempStorageDec.PK);
			AssertContainsExactElementsInAnyOrder(new[] { line1.PK, line2.PK }, reloadedDec.CusTempStorageLines.Select(x => x.PK));
		}

		public void TestFormattedOwnerReferenceNumber()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var storageLine = storageDec.ConsolidatedCusTempStorageLine;
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			storageLine.TSL_OwnerReferenceNumber = "ATB150000010320006000";
			AssertEquals("ATB150000010320006000", storageLine.FormattedOwnerReferenceNumber);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			storageLine.TSL_OwnerReferenceNumber = "ATB150000010320006001";
			AssertEquals("Formatted for REG", "AT/B/15/000001/03/2000/6001", storageLine.FormattedOwnerReferenceNumber);
		}

		public void TestSTH_IdentificationIndicator()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var storageDec = GetCusTempStorageDecForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Identification Indicator Empty", ZString.Empty, storageDec.STH_IdentificationIndicator);
				storageDec.ConsolidatedCusTempStorageLine.TSL_LineNo = ZInt.Zero;
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				AssertEquals("Identification Type AWB sets Line No to 1", 1, storageDec.ConsolidatedCusTempStorageLine.TSL_LineNo);
				storageDec.ConsolidatedCusTempStorageLine.TSL_OA_Custodian = orgHeader.MainAddress.PK;
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				AssertEquals("Defaults Line Owner Reference Type", TemporaryStorageIdentificationIndicatorList.Codes.REG, storageDec.ConsolidatedCusTempStorageLine.TSL_OwnerReferenceType);
				AssertEquals("Clears Line Custodian", ZGuid.Empty, storageDec.ConsolidatedCusTempStorageLine.TSL_OA_Custodian);
				storageDec.STH_IdentificationIndicator = "XXX";
				AssertEquals("Line Owner Reference Type statys as invalid indicator does nothing", TemporaryStorageIdentificationIndicatorList.Codes.REG, storageDec.ConsolidatedCusTempStorageLine.TSL_OwnerReferenceType);
			});
		}

		public void TestIsAWBDeclaration()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var storageLine = storageDec.ConsolidatedCusTempStorageLine;
			Assert(!storageLine.IsAWBDeclaration);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			Assert(storageLine.IsAWBDeclaration);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			Assert(!storageLine.IsAWBDeclaration);
		}

		public void TestSetDefaultValues()
		{
			var dec = GetCusTempStorageDecForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("STH_IdentificationIndicator", ZString.Empty, dec.STH_IdentificationIndicator);
				AssertEquals("STH_DeclarationType", TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo, dec.STH_DeclarationType);
			});
		}

		public void TestFetchStrategyGet()
		{
			AssertType<CusTempStorageDecFetchStrategy>(GetCusTempStorageDecForTesting().FetchStrategy);
		}

		public void TestSameConsolidatedCusTempStorageLineIsReturnedWhenThereIsMoreThanOne()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var consolidatedLine = storageDec.ConsolidatedCusTempStorageLine;
			var storageLine1 = storageDec.CusTempStorageLines.AddNew();
			storageLine1.TSL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			Factory.Save();

			CombineAssertions(() =>
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedStorageDec = newFactory.Load<CUSPCSCusTempStorageDec>(storageDec.PK);
				AssertEquals("Load returns the newest one", storageLine1.PK, reloadedStorageDec.ConsolidatedCusTempStorageLine.PK);

				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				storageLine2.TSL_SystemCreateTimeUtc = ZDateTime.Now.AddHours(1);
				Factory.Save();

				newFactory = new BusinessObjectFactory();
				reloadedStorageDec = newFactory.Load<CUSPCSCusTempStorageDec>(storageDec.PK);
				AssertEquals("Load still returns the same one as the extra is 23 hours older", storageLine1.PK, reloadedStorageDec.ConsolidatedCusTempStorageLine.PK);
			});
		}

		public void TestLookupsOverriddenType()
		{
			AssertType<CUSPCSCusTempStorageDecLookups>(GetCusTempStorageDecForTesting().Lookups);
		}

		public void TestValidationOverriddenType()
		{
			AssertType<CUSPCSCusTempStorageDecValidation>(GetCusTempStorageDecForTesting().Validation);
		}

		public void TestReferenceNumberCaption()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(storageDec.ReferenceNumberInfo);
			AssertEquals("Reference Number Caption", "New Reference", propertyData.Caption);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectForTest(factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForTest(Factory);

		protected override CUSPCSCusTempStorageDec GetCusTempStorageDecForTesting() => GetNewBusinessObjectForTest(Factory);

		CUSPCSCusTempStorageDec GetNewBusinessObjectForTest(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CUSTEST";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECUSPRL001";
			storageJobHeader.SJH_OH_Customer = customer.PK;
			storageJobHeader.SJH_OA_Presenter = presenter.PK;
			storageJobHeader.SJH_OA_Representative = representative.PK;

			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;

			return storageDec;
		}

		#endregion
	}
}
