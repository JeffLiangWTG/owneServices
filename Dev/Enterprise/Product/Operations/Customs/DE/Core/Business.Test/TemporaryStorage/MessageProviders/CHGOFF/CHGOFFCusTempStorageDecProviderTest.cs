using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGOFFCusTempStorageDecProvider))]
	class CHGOFFCusTempStorageDecProviderTest : CusTempStorageDecProviderAbstractTest<CHGOFFCusTempStorageDecProvider>
	{
		public void TestOwnerReferenceNumber()
		{
			TempStorageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			TempStorageDec.STH_OwnerReferenceNumber = "ATB150002930220195875";
			CombineAssertions(() =>
			{
				AssertEquals("Owner Reference Number formatted.", "AT/B/15/000293/02/2019/5875", TempStorageDec.FormattedOwnerReferenceNumber);
				AssertEquals("Formatting not on data layer property", "ATB150002930220195875", ((ICHGOFFTempStorageDec)TempStorageDecWrapped).OwnerReferenceNumber);
			});
		}

		protected override CusTempStorageDec GetTempStorageDecToTest() => Factory.New<CHGOFFCusTempStorageDec>();

		protected override ITempStorageDec GetTempStorageDecWrapped() => new CHGOFFCusTempStorageDecProvider(TempStorageDec);
	}
}
