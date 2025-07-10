using System;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageDecProvider))]
	internal abstract class CusTempStorageDecProviderAbstractTest<T> : DataProviderTestCase<T>
		where T : CusTempStorageDecProvider
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()), new object[] { null });
		}

		public virtual void TestStorageLines()
		{
			TempStorageDec.CusTempStorageLines.AddNew();
			TempStorageDec.CusTempStorageLines.AddNew();
			CombineAssertions(() =>
			{
				var storageLines = TempStorageDecWrapped.StorageLines;
				AssertEquals("Lines Collection", 2, storageLines.Count);
				AssertSame("Cached", TempStorageDecWrapped.StorageLines, storageLines);
			});
		}

		public void TestFirstTempStorageLineDetails()
		{
			var tempStorageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			TempStorageDec.STH_SJH = tempStorageHeader.PK;

			var now = ZDateTime.Now;
			var firstStorageLine = TempStorageDec.CusTempStorageLines.AddNew();
			firstStorageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
			firstStorageLine.TSL_LineNo = 1;
			firstStorageLine.TSL_SystemCreateTimeUtc = now.AddDays(-1);

			var secondStorageLine = TempStorageDec.CusTempStorageLines.AddNew();
			secondStorageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
			secondStorageLine.TSL_LineNo = 2;
			secondStorageLine.TSL_SystemCreateTimeUtc = now;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Owner Reference Type", OwnerReferenceTypeList.Codes.AWB, TempStorageDecWrapped.FirstTempStorageLineDetails.OwnerReferenceType);
				AssertEquals("Line No", 1, TempStorageDecWrapped.FirstTempStorageLineDetails.LineNumber);
			});
		}

		public void TestHeader()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = TempStorageDecWrapped.Header);
			var jobHeader = Factory.New<CusTempStorageJobHeader>();
			jobHeader.CusTempStorageDecs.Add(TempStorageDec);
			AssertSame(TempStorageDecWrapped.Header, TempStorageDecWrapped.Header);
		}

		protected CusTempStorageDec TempStorageDec => tempStorageDec ?? (tempStorageDec = GetTempStorageDecToTest());
		CusTempStorageDec tempStorageDec;

		protected ITempStorageDec TempStorageDecWrapped => tempStorageDecWrapped ?? (tempStorageDecWrapped = GetTempStorageDecWrapped());
		ITempStorageDec tempStorageDecWrapped;

		protected abstract CusTempStorageDec GetTempStorageDecToTest();

		protected abstract ITempStorageDec GetTempStorageDecWrapped();

		protected override T GetProvider() => (T)TempStorageDecWrapped;
	}
}
