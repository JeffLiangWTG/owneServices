using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CusTempStorageDecProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestIdentificationIndicator()
		{
			tempStorageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertEquals(TemporaryStorageIdentificationIndicatorList.Codes.AWB, temporaryStorageDecProvider.IdentificationIndicator);
		}

		public void TestAdditionalInformation()
		{
			var additionalInformation = "Additional information";
			tempStorageDec.STH_AdditionalInformation = additionalInformation;
			AssertEquals(additionalInformation, temporaryStorageDecProvider.AdditionalInformation);
		}

		public void TestInterchangeControlReference()
		{
			AssertEquals(EDIInterchange.InterchangeNumberPlaceHolder, temporaryStorageDecProvider.InterchangeControlReference);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals(EDIMessage.SendersReferencePlaceHolder, temporaryStorageDecProvider.MessageIdentifier);
		}

		protected override void SetUp()
		{
			base.SetUp();
			tempStorageDec = Factory.New<CusTempStorageDecForTest>();
			temporaryStorageDecProvider = new CusTempStorageDecProvider(tempStorageDec);
		}
		CusTempStorageDecForTest tempStorageDec;
		ITempStorageDec temporaryStorageDecProvider;
	}
}
