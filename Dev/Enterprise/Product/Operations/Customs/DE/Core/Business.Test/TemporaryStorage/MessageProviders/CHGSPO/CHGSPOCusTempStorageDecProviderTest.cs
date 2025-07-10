using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGSPOCusTempStorageDecProvider))]
	class CHGSPOCusTempStorageDecProviderTest : CusTempStorageDecProviderAbstractTest<CHGSPOCusTempStorageDecProvider>
	{
		public void TestOwnerReferenceNumber()
		{
			TempStorageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			TempStorageDec.STH_OwnerReferenceNumber = "ATB150002930220195875";
			CombineAssertions(() =>
			{
				AssertEquals("Owner Reference Number formatted.", "AT/B/15/000293/02/2019/5875", TempStorageDec.FormattedOwnerReferenceNumber);
				AssertEquals("Formatting not on data layer property", "ATB150002930220195875", ((ICHGSPOTempStorageDec)TempStorageDecWrapped).OwnerReferenceNumber);
			});
		}

		protected override CusTempStorageDec GetTempStorageDecToTest() => Factory.New<CHGSPOCusTempStorageDec>();

		protected override ITempStorageDec GetTempStorageDecWrapped() => new CHGSPOCusTempStorageDecProvider(TempStorageDec);
	}
}
