using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPRLCusTempStorageDecProvider))]
	class CUSPRLCusTempStorageDecProviderTest : CusTempStorageDecProviderAbstractTest<CUSPRLCusTempStorageDecProvider>
	{
		public void TestATBNumber()
		{
			AssertEquals(ZString.Empty, TempStorageDecWrapped.ATBNumber);
			TempStorageDec.CusEntryNumber.CE_EntryNum = "ATB150000010320006000";
			AssertEquals("ATB150000010320006000", TempStorageDecWrapped.ATBNumber);
		}

		public override void TestStorageLines()
		{
			base.TestStorageLines();
			AssertEquals("Is ICUSPRLTempStorageLine", true, typeof(ICUSPRLTempStorageLine).IsAssignableFrom(TempStorageDecWrapped.StorageLines.First().GetType()));
		}

		public void TestStorageLinesFilter()
		{
			var line1 = TempStorageDec.CusTempStorageLines.AddNew();
			line1.TSL_IsModified = true;
			var line2 = TempStorageDec.CusTempStorageLines.AddNew();
			line2.TSL_IsModified = true;
			var line3 = TempStorageDec.CusTempStorageLines.AddNew();
			line3.TSL_IsModified = false;
			var decWrapper = (ICUSPRLTempStorageDec)new CUSPRLCusTempStorageDecProvider(TempStorageDec, (line) => line.TSL_IsModified);
			AssertEquals(2, decWrapper.StorageLines.Count);
		}

		public void TestStorageLinesDefaultFilter()
		{
			TempStorageDec.CusTempStorageLines.AddNew();
			var line2 = TempStorageDec.CusTempStorageLines.AddNew();
			line2.TSL_CustomsStatus = CustomsStatusList.Codes.TST;
			var decWrapper = (ICUSPRLTempStorageDec)new CUSPRLCusTempStorageDecProvider(TempStorageDec);
			AssertEquals(1, decWrapper.StorageLines.Count);
		}

		protected override CusTempStorageDec GetTempStorageDecToTest() => Factory.New<CUSPRLCusTempStorageDec>();

		protected override ITempStorageDec GetTempStorageDecWrapped() => new CUSPRLCusTempStorageDecProvider(TempStorageDec);

		protected new ICUSPRLTempStorageDec TempStorageDecWrapped => (ICUSPRLTempStorageDec)base.TempStorageDecWrapped;
	}
}
