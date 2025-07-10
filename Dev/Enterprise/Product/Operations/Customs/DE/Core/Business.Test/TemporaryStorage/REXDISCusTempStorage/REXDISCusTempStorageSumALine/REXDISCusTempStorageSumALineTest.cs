using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageSumALine))]
	class REXDISCusTempStorageSumALineTest : CusTempStorageLineTest<REXDISCusTempStorageSumALine>
	{
		public void TestSequenceNumberAndLineNo()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var rexdis = REXDISCusTempStorageDec.New(header);
			var sumALine = rexdis.CusTempStorageLines.AddNew().SumALine;
			Assert(!sumALine.SequenceNumberEnabled);
			Assert(!sumALine.TSL_ReferenceNumberLineInfo.ReadOnly);
		}

		public void TestReferenceNumber()
		{
			var storageLine = GetNewCusTempStorageLine();
			var storageDec = storageLine.Dec;
			storageLine.ReferenceNumber = "ATB154711120820183302";

			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertEquals("TSL_ReferenceNumber shouldn't be formatted", "ATB154711120820183302", storageLine.TSL_ReferenceNumber);
			AssertEquals("ReferenceNumber shouldn't be formatted for AWB", "ATB154711120820183302", storageLine.ReferenceNumber);

			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertEquals("TSL_ReferenceNumber shouldn't be formatted", "ATB154711120820183302", storageLine.TSL_ReferenceNumber);
			AssertEquals("Formatted for REG", "AT/B/15/471112/08/2018/3302", storageLine.ReferenceNumber);
		}

		public void TestToLineNotNull()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECUSPRL001";

			var storageDec = REXDISCusTempStorageDec.LoadOrCreate(storageJobHeader);
			storageDec.STH_SJH = storageJobHeader.PK;
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch;

			var reExportLine = storageDec.CusTempStorageLines.AddNew();
			AssertNotNull(reExportLine.SumALine);

			var loadedExportLine = Factory.Load<REXDISCusTempStorageReExportLine>(reExportLine.PK);
			AssertEquals(loadedExportLine.SumALine, reExportLine.SumALine);
		}

		public void TestTSL_ReferenceNumberLineCaption()
		{
			var storageLine = GetNewCusTempStorageLine();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(storageLine.TSL_ReferenceNumberLineInfo);
			AssertEquals("TSL_ReferenceNumberLine Caption", "Reference Line No.", propertyData.Caption);
		}

		#region Implementation

		protected override REXDISCusTempStorageSumALine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CUSTEST";
			var presenter = Factory.NewWithValidTestData<OrgAddress>();
			var representative = Factory.NewWithValidTestData<OrgAddress>();

			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECUSPRL001";
			storageJobHeader.SJH_OH_Customer = customer.PK;
			storageJobHeader.SJH_OA_Presenter = presenter.PK;
			storageJobHeader.SJH_OA_Representative = representative.PK;

			var storageDec = REXDISCusTempStorageDec.LoadOrCreate(storageJobHeader);
			storageDec.STH_SJH = storageJobHeader.PK;
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch;

			var reExpLine = storageDec.CusTempStorageLines.AddNew();
			return reExpLine.SumALine;
		}

		protected override Type GetDecType() => typeof(REXDISCusTempStorageDec);

		protected override Type GetLookupType() => typeof(REXDISCusTempStorageSumALineLookups);

		protected override Type GetValidationType() => typeof(REXDISCusTempStorageSumALineValidation);

		#endregion
	}
}
