using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageReExportLine))]
	class REXDISCusTempStorageReExportLineTest : CusTempStorageLineTest<REXDISCusTempStorageReExportLine>
	{
		public void TestTSL_LineNo()
		{
			var dec = Factory.New<REXDISCusTempStorageDec>();
			var line = dec.CusTempStorageLines.AddNew();
			AssertEquals(true, line.SequenceNumberEnabled);
			AssertEquals(true, line.TSL_LineNoInfo.ReadOnly);
			AssertEquals(1, line.TSL_LineNo);

			var line2 = dec.CusTempStorageLines.AddNew();
			AssertEquals(2, line2.TSL_LineNo);
		}

		public void TestSumALineNewAndChildEditable()
		{
			var dec = Factory.New<REXDISCusTempStorageDec>();
			var reExportLine = dec.CusTempStorageLines.AddNew();
			var sumALine = reExportLine.SumALine;
			AssertNotNull(sumALine);
			Assert(reExportLine.IsRegisteredEditableChildObject(sumALine));
		}

		public void TestSumALineLoad()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_OH_Customer = customer.PK;
			var dec = REXDISCusTempStorageDec.LoadOrCreate(header);
			var reExportLine = dec.CusTempStorageLines.AddNew();
			var sumALine = reExportLine.SumALine;
			Factory.Save();

			var newfactory = new BusinessObjectFactory();
			var reloadedDec = newfactory.Load<REXDISCusTempStorageDec>(dec.PK);
			AssertEquals(sumALine.PK, reloadedDec.CusTempStorageLines[0].SumALine.PK);
		}

		#region Implementation

		protected override REXDISCusTempStorageReExportLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
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

			var reExportLine = storageDec.CusTempStorageLines.AddNew();
			return reExportLine;
		}

		protected override Type GetDecType() => typeof(REXDISCusTempStorageDec);

		protected override Type GetLookupType() => typeof(REXDISCusTempStorageReExportLineLookups);

		protected override Type GetValidationType() => typeof(REXDISCusTempStorageReExportLineValidation);

		#endregion
	}
}
