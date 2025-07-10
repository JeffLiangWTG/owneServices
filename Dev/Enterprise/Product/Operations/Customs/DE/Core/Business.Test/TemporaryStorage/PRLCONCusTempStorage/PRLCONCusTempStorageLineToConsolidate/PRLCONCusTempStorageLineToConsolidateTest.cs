using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONCusTempStorageLineToConsolidate))]
	public class PRLCONCusTempStorageLineToConsolidateTest : CusTempStorageLineTest<PRLCONCusTempStorageLineToConsolidate>
	{
		public void TestSequenceNumberEnabled()
		{
			var lineToConsolidate = GetNewCusTempStorageLine();
			var storageDec = lineToConsolidate.Dec;
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				AssertEquals("REG", true, lineToConsolidate.SequenceNumberEnabled);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				AssertEquals("AWB", false, lineToConsolidate.SequenceNumberEnabled);
			});
		}

		public void TestTSL_OwnerReferenceNumber_AWB()
		{
			var lineToConsolidate = GetNewCusTempStorageLine();
			lineToConsolidate.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			lineToConsolidate.TSL_OwnerReferenceNumber = "ATB150000010320006000";
			AssertEquals("ATB150000010320006000", lineToConsolidate.TSL_OwnerReferenceNumber);
		}

		public void TestTSL_OwnerReferenceNumber_REG()
		{
			var lineToConsolidate = GetNewCusTempStorageLine();
			lineToConsolidate.Dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			lineToConsolidate.TSL_ReferenceNumber = "ATB150000010320006001";
			CombineAssertions(() =>
			{
				AssertEquals("Max Length of ATB No with formatting", 27, lineToConsolidate.FormattedReferenceNumberInfo.MaxLength);
				AssertEquals("Formatted for REG", "AT/B/15/000001/03/2000/6001", lineToConsolidate.FormattedReferenceNumber);
			});
		}

		public void TSL_PackageQty()
		{
			var lineToConsolidate1 = GetNewCusTempStorageLine();
			var storageDec = lineToConsolidate1.Dec;
			var consolidatedLine = storageDec.ConsolidatedLine;

			AssertEquals(5, consolidatedLine.TSL_PackageQtyInfo.MaxLength);
			AssertEquals(0, consolidatedLine.TSL_PackageQty);

			lineToConsolidate1.TSL_PackageQty = 10;
			AssertEquals(10, consolidatedLine.TSL_PackageQty);

			var lineToConsolidated2 = storageDec.CusTempStorageLines.AddNew();
			lineToConsolidated2.TSL_PackageQty = 30;

			AssertEquals(40, consolidatedLine.TSL_PackageQty);
		}

		public void TestConsolidatedLine()
		{
			var lineToConsolidate = GetNewCusTempStorageLine();
			var storageDec = lineToConsolidate.Dec;
			var consolidatedLine = storageDec.ConsolidatedLine;

			AssertEquals(storageDec.PK, consolidatedLine.Dec.PK);

			var pivot = Factory.LoadTop1<CusTempStorageLinePivot>(new ZQuery(CusTempStorageLinePivotSchema.SLR_TSL_FromLine, lineToConsolidate.PK));
			AssertEquals(consolidatedLine.PK, pivot.SLR_TSL_ToLine);
		}

		public void TestDelete()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			storageDec.CusTempStorageLines.AddNew();
			var consolidatedLine = storageDec.ConsolidatedLine;

			storageJobHeader.Delete();

			AssertEquals(true, consolidatedLine.IsDeleted);
		}

		public void TestSetDefaultValues()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			var lineToConsolidated = storageDec.CusTempStorageLines.AddNew();

			AssertEquals("REG", lineToConsolidated.TSL_OwnerReferenceType);
		}

		public void TestTSL_ReferenceNumberLineCaption()
		{
			var lineToConsolidate = GetNewCusTempStorageLine();
			var storageDec = lineToConsolidate.Dec;

			var propertyData = DataBoundResourceStrings.GetDataForProperty(lineToConsolidate.TSL_ReferenceNumberLineInfo);
			AssertEquals("TSL_ReferenceNumberLine Caption", "Reference Line No.", propertyData.Caption);
		}

		public void TestFormattedReferenceNumberCaption()
		{
			var lineToConsolidate = GetNewCusTempStorageLine();

			var propertyData = DataBoundResourceStrings.GetDataForProperty(lineToConsolidate.FormattedReferenceNumberInfo);
			AssertEquals("FormattedReferenceNumber Caption", "Reference", propertyData.Caption);
		}

		protected override PRLCONCusTempStorageLineToConsolidate GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OH_Customer = customer.PK;
			var storageDecs = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			return storageDecs.CusTempStorageLines.AddNew();
		}

		protected override Type GetDecType() => typeof(PRLCONCusTempStorageDec);

		protected override Type GetLookupType() => typeof(PRLCONCusTempStorageLineToConsolidateLookups);

		protected override Type GetValidationType() => typeof(PRLCONCusTempStorageLineToConsolidateValidation);
	}
}
