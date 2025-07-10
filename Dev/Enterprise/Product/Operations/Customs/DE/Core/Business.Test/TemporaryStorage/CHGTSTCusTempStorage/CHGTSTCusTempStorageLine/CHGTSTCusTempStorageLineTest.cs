using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGTSTCusTempStorageLine))]
	class CHGTSTCusTempStorageLineTest : CusTempStorageLineTest<CHGTSTCusTempStorageLine>
	{
		public void TestSequenceNumberEnabled()
		{
			var line = Factory.New<CHGTSTCusTempStorageLine>();
			AssertEquals(false, line.SequenceNumberEnabled);
		}

		public void TestTSL_LineNo_ReadOnly()
		{
			var line = Factory.New<CHGTSTCusTempStorageLine>();
			AssertEquals(false, line.TSL_LineNoInfo.ReadOnly);
		}

		public void TestTSL_CustomsStatus_ReadOnly()
		{
			var line = Factory.New<CHGTSTCusTempStorageLine>();
			AssertEquals(true, line.TSL_CustomsStatusInfo.ReadOnly);
		}

		protected override CHGTSTCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OH_Customer = customer.PK;
			var storageDec = storageJobHeader.CHGTSTCusTempStorageDecs.AddNew();
			return storageDec.CusTempStorageLines.AddNew();
		}

		protected override Type GetDecType() => typeof(CHGTSTCusTempStorageDec);

		protected override Type GetLookupType() => typeof(CHGTSTCusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CHGTSTCusTempStorageLineValidation);
	}
}
