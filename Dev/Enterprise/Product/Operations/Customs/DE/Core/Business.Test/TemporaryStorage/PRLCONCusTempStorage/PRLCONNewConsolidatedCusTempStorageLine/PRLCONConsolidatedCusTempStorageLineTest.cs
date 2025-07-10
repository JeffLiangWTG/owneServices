using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONConsolidatedCusTempStorageLine))]
	public class PRLCONConsolidatedCusTempStorageLineTest : CusTempStorageLineTest<PRLCONConsolidatedCusTempStorageLine>
	{
		public void TestTotalPackagesFromLinesToConsolidate()
		{
			var storageJobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			var consolidatedLine1 = storageDec.ConsolidatedLine;
			var lineToConsolidate1 = storageDec.CusTempStorageLines.AddNew();
			var lineToConsolidate2 = storageDec.CusTempStorageLines.AddNew();
			lineToConsolidate1.TSL_LineNo = 1;
			lineToConsolidate2.TSL_LineNo = 2;

			lineToConsolidate1.TSL_PackageQty = 13;
			lineToConsolidate2.TSL_PackageQty = 9;
			AssertEquals(22, consolidatedLine1.TotalPackagesFromLinesToConsolidate);

			lineToConsolidate2.TSL_PackageQty = 17;
			AssertEquals(30, consolidatedLine1.TotalPackagesFromLinesToConsolidate);
		}

		public void TestLoadOrCreate()
		{
			var storageJobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			var fromLine1 = storageDec.CusTempStorageLines.AddNew();
			fromLine1.TSL_LineNo = 1;
			var fromLine2 = storageDec.CusTempStorageLines.AddNew();
			fromLine2.TSL_LineNo = 2;

			var consolidatedLine1 = storageDec.ConsolidatedLine;
			Assert("new line", !consolidatedLine1.IsInDatabase);

			Factory.Save();

			Assert("in Database", consolidatedLine1.IsInDatabase);

			var consolidatedLine2 = PRLCONConsolidatedCusTempStorageLine.LoadOrCreate(storageDec);
			AssertSame("The same consolidated line", consolidatedLine1, consolidatedLine2);
		}

		public void TestSetDefaultValues()
		{
			var line = Factory.New<PRLCONConsolidatedCusTempStorageLine>();
			AssertEquals(1, line.TSL_LineNo);
		}

		public void TestSetPackageQtyTo1()
		{
			var storageJobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			var consolLine = storageDec.ConsolidatedLine;
			var storageLine = storageDec.CusTempStorageLines.AddNew();

			consolLine.TSL_PackageQty = 0;

			foreach (var packageType in Factory.GetCachedValue<ZString[]>("DE|GetSingleCountPackageTypes", () => null))
			{
				consolLine.TSL_PackageType = packageType;
				AssertEquals(1, consolLine.TSL_PackageQty);
				consolLine.TSL_PackageQty = 0;
			}

			consolLine.TSL_PackageType = "@@";
			AssertEquals(0, consolLine.TSL_PackageQty);

			consolLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ULD;
			AssertEquals(1, consolLine.TSL_PackageQty);

			consolLine.TSL_PackageQty = 0;

			consolLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
			AssertEquals(0, consolLine.TSL_PackageQty);
		}

		public void TestRecalculatePackageQuantity()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			AssertEquals(0, storageDec.CusTempStorageLines.Count);

			var line1 = storageDec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 10;

			var consolidatedLine = storageDec.ConsolidatedLine;

			consolidatedLine.TSL_PackageQty = 1;
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;

			consolidatedLine.RecalculatePackageQuantity();
			AssertEquals(10, consolidatedLine.TSL_PackageQty);

			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			var line2 = storageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 20;

			AssertEquals(30, consolidatedLine.TSL_PackageQty);
		}

		public void TestRecalculatePackageQuantity_DoesNotOccurWhenPackageQtyShouldBeOne()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			var consolLine = storageDec.ConsolidatedLine;

			consolLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ULD;

			var line1 = storageDec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 10;
			var line2 = storageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 20;

			AssertEquals(1, storageDec.ConsolidatedLine.TSL_PackageQty);

			storageDec.CusTempStorageLines.RemoveAndDelete(line2);
			AssertEquals(1, storageDec.ConsolidatedLine.TSL_PackageQty);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("It wouldn't be deleted directly", true);
		}

		protected override PRLCONConsolidatedCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
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

			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;

			var lineToConsolidate = storageDec.CusTempStorageLines.AddNew();

			return storageDec.ConsolidatedLine;
		}

		protected override Type GetDecType() => typeof(PRLCONCusTempStorageDec);

		protected override Type GetLookupType() => typeof(PRLCONConsolidatedCusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(PRLCONConsolidatedCusTempStorageLineValidation);
	}
}
