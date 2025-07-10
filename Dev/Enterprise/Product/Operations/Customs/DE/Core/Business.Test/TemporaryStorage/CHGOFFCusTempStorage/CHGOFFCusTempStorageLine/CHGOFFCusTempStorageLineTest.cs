using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGOFFCusTempStorageLine))]
	class CHGOFFCusTempStorageLineTest : CusTempStorageLineTest<CHGOFFCusTempStorageLine>
	{
		public void TestSequenceNumberEnabled()
		{
			var storageLine = Factory.New<CHGOFFCusTempStorageLine>();
			AssertEquals(false, storageLine.SequenceNumberEnabled);
		}

		public void TestTSL_LineNo_ReadOnly()
		{
			var storageLine = Factory.New<CHGOFFCusTempStorageLine>();
			AssertEquals(false, storageLine.TSL_LineNoInfo.ReadOnly);
		}

		protected override CHGOFFCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OH_Customer = customer.PK;
			var storageDec = storageJobHeader.CHGOFFCusTempStorageDecs.AddNew();
			return storageDec.CusTempStorageLines.AddNew();
		}

		protected override Type GetDecType() => typeof(CHGOFFCusTempStorageDec);

		protected override Type GetLookupType() => typeof(CHGOFFCusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CHGOFFCusTempStorageLineValidation);
	}
}
